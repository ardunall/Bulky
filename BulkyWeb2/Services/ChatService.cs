using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using BulkyWeb2.Models;
using BulkyWeb2.Repository.IRepository;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace BulkyWeb2.Services
{
    public class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ChatService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private const string OLLAMA_API_URL = "http://ollama:11434/api/chat";
        private const int REQUEST_TIMEOUT_SECONDS = 60;
        private const int MAX_TOKENS = 250;

        public ChatService(
            HttpClient httpClient, 
            ILogger<ChatService> logger, 
            IUnitOfWork unitOfWork)
        {
            _httpClient = httpClient;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _httpClient.Timeout = TimeSpan.FromSeconds(REQUEST_TIMEOUT_SECONDS);
        }

        private async Task<string> GetBookInformation()
        {
            try 
            {
                _logger.LogInformation("Retrieving books from database...");
                var products = _unitOfWork.Product.GetAll(inculdeProperties: "Category").ToList();
                _logger.LogInformation($"Successfully retrieved {products.Count} books from database");
                
                if (products.Count == 0)
                {
                    _logger.LogWarning("No books found in the database");
                    return "Veritabanında henüz hiç kitap bulunmamaktadır.";
                }
                
                var bookInfo = new StringBuilder();
                bookInfo.Append($"{products.Count} kitap, {products.Select(p => p.Category?.Name ?? "Kategorisiz").Distinct().Count()} kategori");
                
                return bookInfo.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting book information from database");
                return "Kitap bilgileri alınırken hata oluştu: " + ex.Message;
            }
        }

        private async Task<string> GetDetailedBookList()
        {
            try
            {
                var products = _unitOfWork.Product.GetAll(inculdeProperties: "Category").ToList();
                if (products.Count == 0) return "Henüz kitap bulunmamaktadır.";

                var response = new StringBuilder();
                foreach (var product in products.OrderBy(p => p.Category?.Name).ThenBy(p => p.Title))
                {
                    response.Append($"{product.Title} - {product.Author} - {product.Category?.Name ?? "Kategorisiz"} - {product.Price:C}; ");
                }

                return response.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting detailed book list");
                return "Kitap listesi alınırken hata oluştu: " + ex.Message;
            }
        }

        private async Task<string> GetCategoryList()
        {
            try
            {
                var products = _unitOfWork.Product.GetAll(inculdeProperties: "Category").ToList();
                var categories = products.GroupBy(p => p.Category?.Name ?? "Kategorisiz")
                                      .OrderBy(g => g.Key);

                var response = new StringBuilder();
                foreach (var category in categories)
                {
                    response.Append($"{category.Key}: {category.Count()} kitap; ");
                }

                return response.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting category list");
                return "Kategori listesi alınırken hata oluştu: " + ex.Message;
            }
        }

        private string GetBooksByCategory(string categoryName)
        {
            try
            {
                var products = _unitOfWork.Product.GetAll(inculdeProperties: "Category")
                    .Where(p => (p.Category?.Name ?? "Kategorisiz").Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!products.Any())
                    return $"'{categoryName}' kategorisinde kitap bulunamadı.";

                var response = new StringBuilder();
                foreach (var product in products.OrderBy(p => p.Title))
                {
                    response.Append($"{product.Title} - {product.Author} - {product.Price:C}; ");
                }

                return response.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting books by category");
                return "Kategori kitapları alınırken hata oluştu: " + ex.Message;
            }
        }

        private string GetBooksByPriceRange(double minPrice, double maxPrice)
        {
            try
            {
                var products = _unitOfWork.Product.GetAll(inculdeProperties: "Category")
                    .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
                    .OrderBy(p => p.Price)
                    .ToList();

                if (!products.Any())
                    return $"{minPrice:C} - {maxPrice:C} fiyat aralığında kitap bulunamadı.";

                var response = new StringBuilder();
                foreach (var product in products)
                {
                    response.Append($"{product.Title} - {product.Author} - {product.Category?.Name ?? "Kategorisiz"} - {product.Price:C}; ");
                }

                return response.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting books by price range");
                return "Fiyat aralığındaki kitaplar alınırken hata oluştu: " + ex.Message;
            }
        }

        public async Task<string> GetResponseFromOllama(string message)
        {
            try
            {
                _logger.LogInformation("Starting request to Ollama API. Message: {Message}", message);

                if (string.IsNullOrWhiteSpace(message))
                {
                    return "Üzgünüm, boş bir mesaj gönderdiniz. Lütfen bir soru sorun.";
                }

                message = message.Trim().ToLower();
                
                var bookInfo = await GetBookInformation();
                var systemPrompt = $"You are a customer representative at a bookstore. Respond in English. Store information: {bookInfo}";

                var requestBody = new
                {
                    model = "mistral",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = message }
                    },
                    stream = false
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(REQUEST_TIMEOUT_SECONDS));
                
                try
                {
                    using var httpResponse = await _httpClient.PostAsync(OLLAMA_API_URL, content, cts.Token);
                    var responseString = await httpResponse.Content.ReadAsStringAsync();
                    
                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        return "Ollama servisine bağlanılamıyor. Lütfen Ollama'nın çalıştığından emin olun. Ollama'yı kurmak için: https://ollama.ai/download";
                    }

                    var responseObject = JsonSerializer.Deserialize<OllamaResponse>(responseString);
                    return responseObject?.message?.content?.Trim() ?? 
                        "Üzgünüm, yanıt üretemiyorum. Lütfen tekrar deneyin.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while communicating with Ollama API");
                    return "Üzgünüm, şu anda size yardımcı olamıyorum. Lütfen daha sonra tekrar deneyin.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetResponseFromOllama");
                return "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
            }
        }
    }

    public class OllamaResponse
{
    public Message message { get; set; }
}

public class Message
{
    public string role { get; set; }
    public string content { get; set; }
}
} 