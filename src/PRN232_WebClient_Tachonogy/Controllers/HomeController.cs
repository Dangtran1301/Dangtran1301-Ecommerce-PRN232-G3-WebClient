using Microsoft.AspNetCore.Mvc;
using PRN232_WebClient_Tachonogy.Models;
using PRN232_WebClient_Tachonogy.Services.Interfaces;
using System.Diagnostics;

namespace PRN232_WebClient_Tachonogy.Controllers
{
    public class HomeController(
        ILogger<HomeController> logger,
        IProductService productService) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;

        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            try
            {
                // Get latest 3 products
                var filter = new PRN232_WebClient_Tachonogy.DTOs.ProductFilterDto
                {
                    PageIndex = 1,
                    PageSize = 3,
                    OrderBy = "Id",
                    Descending = true // Get newest first
                };

                var result = await productService.GetProductsAsync(filter, cancellationToken);
                if (result.Success && result.Data != null)
                {
                    ViewBag.FeaturedProducts = result.Data.Items.ToList();
                }
                else
                {
                    ViewBag.FeaturedProducts = new List<PRN232_WebClient_Tachonogy.DTOs.ProductDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading featured products");
                ViewBag.FeaturedProducts = new List<PRN232_WebClient_Tachonogy.DTOs.ProductDto>();
            }

            return View();
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
