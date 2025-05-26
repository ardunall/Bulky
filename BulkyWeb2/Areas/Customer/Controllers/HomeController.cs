using BulkyWeb2.Models;
using BulkyWeb2.Repository;
using BulkyWeb2.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BulkyWeb2.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitofwork;
        private readonly ICacheService _cacheService;
        private const string PRODUCTS_CACHE_KEY = "products_list";

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, ICacheService cacheService)
        {
            _logger = logger;
            _unitofwork = unitOfWork;
            _cacheService = cacheService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Try to get products from cache
                var cachedProducts = await _cacheService.GetAsync<IEnumerable<Product>>(PRODUCTS_CACHE_KEY);
                
                if (cachedProducts != null)
                {
                    _logger.LogInformation("Products retrieved from cache. Count: {Count}", cachedProducts.Count());
                    return View(cachedProducts);
                }

                // If not in cache, get from database
                var productList = _unitofwork.Product.GetAll(inculdeProperties: "Category");
                _logger.LogInformation("Products retrieved from database. Count: {Count}", productList.Count());
                
                // Cache the products for 1 minute
                await _cacheService.SetAsync(PRODUCTS_CACHE_KEY, productList, TimeSpan.FromMinutes(1));
                _logger.LogInformation("Products cached for 1 minute");

                return View(productList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving products");
                // In case of error, get products directly from database
                var productList = _unitofwork.Product.GetAll(inculdeProperties: "Category");
                return View(productList);
            }
        }

        public IActionResult Details(int productId)
        {
            Product product = _unitofwork.Product.Get(u=>u.Id== productId, inculdeProperties: "Category");
            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
