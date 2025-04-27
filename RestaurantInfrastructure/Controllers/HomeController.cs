using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantInfrastructure;
using RestaurantInfrastructure.Models;
using System.Diagnostics;
using System.Linq;

namespace RestaurantInfrastructure.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RestaurantDbContext _context;

        public HomeController(ILogger<HomeController> logger, RestaurantDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [AllowAnonymous] // Дозволяємо доступ для всіх
        public IActionResult Index()
        {
            var dishCategories = _context.Dishes
                .Include(d => d.Category)
                .GroupBy(d => d.Category.Description)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToList();

            var pieChartLabels = dishCategories.Select(c => c.Category).ToArray();
            var pieChartData = dishCategories.Select(c => c.Count).ToArray();

            var cookRestaurants = _context.Cooks
                .Include(c => c.Restaurant)
                .GroupBy(c => c.Restaurant.Name)
                .Select(g => new { Restaurant = g.Key, Count = g.Count() })
                .ToList();

            var barChartLabels = cookRestaurants.Select(r => r.Restaurant).ToArray();
            var barChartData = cookRestaurants.Select(r => r.Count).ToArray();

            ViewBag.PieChartLabels = pieChartLabels;
            ViewBag.PieChartData = pieChartData;
            ViewBag.BarChartLabels = barChartLabels;
            ViewBag.BarChartData = barChartData;

            return View();
        }

        [Authorize] // Інші дії доступні лише авторизованим користувачам
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