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
        private const string OLLAMA_API_URL = "http://ollama:11434/api/generate";
        private const int REQUEST_TIMEOUT_SECONDS = 30;
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
                bookInfo.AppendLine($"{products.Count} kitap, {products.Select(p => p.Category?.Name ?? "Kategorisiz").Distinct().Count()} kategori bulunmaktadır.");
                bookInfo.AppendLine($"Fiyat aralığı: {products.Min(p => p.Price):C} - {products.Max(p => p.Price):C}");
                
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
                response.AppendLine("📚 Mevcut Kitaplarımız:");
                response.AppendLine("===================");

                foreach (var product in products.OrderBy(p => p.Category?.Name).ThenBy(p => p.Title))
                {
                    response.AppendLine($"\n📖 {product.Title}");
                    response.AppendLine($"✍️ Yazar: {product.Author}");
                    response.AppendLine($"📑 Kategori: {product.Category?.Name ?? "Kategorisiz"}");
                    response.AppendLine($"💰 Fiyat: {product.Price:C}");
                    response.AppendLine("-------------------");
                }

                return response.ToString();
            }
            catch (Exception ex)
            {
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
                response.AppendLine("📑 Kategorilerimiz:");
                response.AppendLine("=================");

                foreach (var category in categories)
                {
                    response.AppendLine($"\n📚 {category.Key}");
                    response.AppendLine($"Kitap Sayısı: {category.Count()}");
                    response.AppendLine($"Fiyat Aralığı: {category.Min(p => p.Price):C} - {category.Max(p => p.Price):C}");
                    response.AppendLine("-------------------");
                }

                return response.ToString();
            }
            catch (Exception ex)
            {
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
                response.AppendLine($"📚 {categoryName} Kategorisindeki Kitaplar:");
                response.AppendLine("================================");

                foreach (var product in products.OrderBy(p => p.Title))
                {
                    response.AppendLine($"\n📖 {product.Title}");
                    response.AppendLine($"✍️ Yazar: {product.Author}");
                    response.AppendLine($"💰 Fiyat: {product.Price:C}");
                    response.AppendLine("-------------------");
                }

                return response.ToString();
            }
            catch (Exception ex)
            {
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
                response.AppendLine($"💰 {minPrice:C} - {maxPrice:C} Fiyat Aralığındaki Kitaplar:");
                response.AppendLine("================================");

                foreach (var product in products)
                {
                    response.AppendLine($"\n📖 {product.Title}");
                    response.AppendLine($"✍️ Yazar: {product.Author}");
                    response.AppendLine($"📑 Kategori: {product.Category?.Name ?? "Kategorisiz"}");
                    response.AppendLine($"💰 Fiyat: {product.Price:C}");
                    response.AppendLine("-------------------");
                }

                return response.ToString();
            }
            catch (Exception ex)
            {
                return "Fiyat aralığındaki kitaplar alınırken hata oluştu: " + ex.Message;
            }
        }

        public async Task<string> GetResponseFromOllama(string message)
        {
            try
            {
                _logger.LogInformation("Starting request to Ollama API. Message: {Message}", message);

                // Boş mesaj kontrolü
                if (string.IsNullOrWhiteSpace(message))
                {
                    return "Üzgünüm, boş bir mesaj gönderdiniz. Lütfen bir soru sorun veya yardım için 'yardım' yazın.";
                }

                message = message.Trim().ToLower();

                // Kitap listesi için özel yanıtlar
                if (message == "hangi kitaplar var" || 
                    message == "kitapları göster" || 
                    message == "kitaplar" || 
                    message == "tüm kitaplar")
                {
                    return await GetDetailedBookList();
                }

                // Kategori listesi için özel yanıtlar
                if (message == "kategoriler" || 
                    message == "hangi kategoriler var" || 
                    message == "kategori listesi")
                {
                    return await GetCategoryList();
                }

                // Kategori sorgulama
                if (message.StartsWith("kategori:") || message.StartsWith("kategori "))
                {
                    var categoryName = message.Split(':')[1].Trim();
                    if (string.IsNullOrWhiteSpace(categoryName))
                        categoryName = message.Substring(9).Trim();
                    return GetBooksByCategory(categoryName);
                }

                // Fiyat aralığı sorgulama
                if (message.StartsWith("fiyat aralığı:") || message.StartsWith("fiyat araligi:"))
                {
                    var range = message.Split(':')[1].Trim().Split('-');
                    if (range.Length == 2 && 
                        double.TryParse(range[0].Trim(), out double minPrice) && 
                        double.TryParse(range[1].Trim(), out double maxPrice))
                    {
                        return GetBooksByPriceRange(minPrice, maxPrice);
                    }
                    return "Lütfen fiyat aralığını şu formatta girin: 'fiyat aralığı: 50-200'";
                }

                // En ucuz ve en pahalı kitaplar
                if (message == "en ucuz kitaplar")
                {
                    var products = _unitOfWork.Product.GetAll(inculdeProperties: "Category")
                        .OrderBy(p => p.Price)
                        .Take(5)
                        .ToList();
                    var minPrice = products.First().Price;
                    var maxPrice = products.Last().Price;
                    return GetBooksByPriceRange(minPrice, maxPrice);
                }

                if (message == "en pahalı kitaplar")
                {
                    var products = _unitOfWork.Product.GetAll(inculdeProperties: "Category")
                        .OrderByDescending(p => p.Price)
                        .Take(5)
                        .ToList();
                    var minPrice = products.Last().Price;
                    var maxPrice = products.First().Price;
                    return GetBooksByPriceRange(minPrice, maxPrice);
                }

                // Yardım komutu
                if (message == "yardım")
                {
                    return @"Size nasıl yardımcı olabilirim? 🤝

1. Kitap Listesi:
   - 'kitaplar' - Tüm kitapları listeler
   - 'hangi kitaplar var' - Tüm kitapları gösterir

2. Kategori İşlemleri:
   - 'kategoriler' - Tüm kategorileri listeler
   - 'kategori: Roman' - Roman kategorisindeki kitapları gösterir

3. Fiyat Sorguları:
   - 'en ucuz kitaplar' - En ucuz 5 kitabı gösterir
   - 'en pahalı kitaplar' - En pahalı 5 kitabı gösterir
   - 'fiyat aralığı: 50-200' - 50TL ile 200TL arası kitapları gösterir

Her türlü sorunuz için bana doğal dille sorabilirsiniz. Size yardımcı olmaktan mutluluk duyarım! 📚";
                }

                // Temel selamlaşma yanıtları
                if (message == "merhaba" || message == "selam" || message == "hi" || message == "hello")
                {
                    return "Merhaba! 👋 Size nasıl yardımcı olabilirim? Kitaplarımız hakkında bilgi almak için 'yardım' yazabilirsiniz.";
                }

                if (message == "nasılsın" || message == "naber" || message == "napıyorsun")
                {
                    return "İyiyim, teşekkür ederim! 😊 Size kitaplarımız hakkında yardımcı olmak için buradayım. Nasıl yardımcı olabilirim?";
                }

                // Eğer özel komutlarla eşleşmezse, Ollama API'yi kullan
                var bookInfo = await GetBookInformation();
                var systemPrompt = $"Sen bir kitap mağazası müşteri temsilcisisin. Türkçe yanıt ver. Mağaza bilgileri: {bookInfo}";

                var requestBody = new
                {
                    model = "llama2",
                    prompt = $"{systemPrompt}\n\nKullanıcı: {message}\nAsistan:",
                    stream = false,
                    options = new { temperature = 0.7, top_k = 40, max_tokens = MAX_TOKENS }
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
                        return "Üzgünüm, şu anda size yardımcı olamıyorum. Lütfen 'yardım' yazarak kullanılabilir komutları görüntüleyin.";
                    }

                    var responseObject = JsonSerializer.Deserialize<OllamaResponse>(responseString);
                    return responseObject?.response?.Trim() ?? 
                           "Üzgünüm, yanıt üretemiyorum. Lütfen 'yardım' yazarak kullanılabilir komutları görüntüleyin.";
                }
                catch (Exception)
                {
                    return "Üzgünüm, şu anda size yardımcı olamıyorum. Lütfen 'yardım' yazarak kullanılabilir komutları görüntüleyin.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error");
                return "Beklenmeyen bir hata oluştu. Lütfen 'yardım' yazarak kullanılabilir komutları görüntüleyin.";
            }
        }
    }

    public class OllamaResponse
    {
        public string response { get; set; }
    }
} 