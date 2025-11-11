using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RigShop.Models;
using RigShop.Web.Models;
using System.Diagnostics;

namespace RigShop.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        // HOME PAGE -
        public async Task<IActionResult> Index()
        {
            try
            {
                // just get the latest products
                var allProducts = await _db.Products
                    .Include(p => p.Category)
                    .OrderByDescending(p => p.Id)
                    .Take(9)
                    .ToListAsync();

                // Use first 6 products for deals, rest for offers
                var todaysDeals = allProducts.Take(6).ToList();
                var offersForYou = allProducts.Skip(6).Take(3).ToList();

                ViewBag.TodaysDeals = todaysDeals;
                ViewBag.OffersForYou = offersForYou;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page products");
                ViewBag.TodaysDeals = new List<Product>();
                ViewBag.OffersForYou = new List<Product>();
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