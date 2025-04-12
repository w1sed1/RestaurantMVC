using Microsoft.AspNetCore.Mvc;
using RestaurantInfrastructure;
using System.Threading.Tasks;

namespace RestaurantInfrastructure.Controllers
{
    public class ReportsController : Controller
    {
        private readonly RestaurantDbContext _context;
        private readonly ExcelExportHelper _exportHelper;
        private readonly ExcelImportHelper _importHelper;

        public ReportsController(RestaurantDbContext context)
        {
            _context = context;
            _exportHelper = new ExcelExportHelper();
            _importHelper = new ExcelImportHelper(context);
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Export()
        {
            // Отримуємо вибір користувача
            bool exportCategories = Request.Form["exportCategories"] == "on";
            bool exportCooks = Request.Form["exportCooks"] == "on";
            bool exportDishes = Request.Form["exportDishes"] == "on";
            bool exportIngredients = Request.Form["exportIngredients"] == "on";
            bool exportRestaurants = Request.Form["exportRestaurants"] == "on";

            if (!exportCategories && !exportCooks && !exportDishes && !exportIngredients && !exportRestaurants)
            {
                TempData["Error"] = "Виберіть хоча б одну таблицю для експорту.";
                return RedirectToAction("Index");
            }

            // Викликаємо експорт
            var stream = await _exportHelper.ExportToExcelAsync(_context, exportCategories, exportCooks, exportDishes, exportIngredients, exportRestaurants);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RestaurantData.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile importFile)
        {
            if (importFile == null || importFile.Length == 0)
            {
                TempData["Error"] = "Будь ласка, завантажте файл для імпорту.";
                return RedirectToAction("Index");
            }

            // Викликаємо імпорт
            var (success, errors) = await _importHelper.ImportFromExcelAsync(importFile);
            if (!success && errors.Any())
            {
                TempData["Error"] = string.Join("; ", errors);
                return RedirectToAction("Index");
            }

            TempData["Success"] = "Дані успішно імпортовано.";
            return RedirectToAction("Index");
        }
    }
}