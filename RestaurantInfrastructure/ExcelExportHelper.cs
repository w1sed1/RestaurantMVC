using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RestaurantDomain.Models;
using RestaurantInfrastructure;
using System.IO;
using System.Threading.Tasks;

namespace RestaurantInfrastructure
{
    public class ExcelExportHelper
    {
        public async Task<MemoryStream> ExportToExcelAsync(RestaurantDbContext context, bool exportCategories, bool exportCooks, bool exportDishes, bool exportIngredients, bool exportRestaurants)
        {
            using var package = new ExcelPackage();

            if (exportCategories)
                await ExportCategoriesAsync(package, context);

            if (exportCooks)
                await ExportCooksAsync(package, context);

            if (exportDishes)
                await ExportDishesAsync(package, context);

            if (exportIngredients)
                await ExportIngredientsAsync(package, context);

            if (exportRestaurants)
                await ExportRestaurantsAsync(package, context);

            var stream = new MemoryStream(package.GetAsByteArray());
            return stream;
        }

        private async Task ExportCategoriesAsync(ExcelPackage package, RestaurantDbContext context)
        {
            var worksheet = package.Workbook.Worksheets.Add("Категорії");
            worksheet.Cells[1, 1].Value = "Опис";
            var categories = await context.Categories.ToListAsync();
            for (int i = 0; i < categories.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = categories[i].Description;
            }
            worksheet.Cells.AutoFitColumns();
        }

        private async Task ExportCooksAsync(ExcelPackage package, RestaurantDbContext context)
        {
            var worksheet = package.Workbook.Worksheets.Add("Кухарі");
            worksheet.Cells[1, 1].Value = "Прізвище";
            worksheet.Cells[1, 2].Value = "Дата народження";
            worksheet.Cells[1, 3].Value = "Ресторан";
            var cooks = await context.Cooks.Include(c => c.Restaurant).ToListAsync();
            for (int i = 0; i < cooks.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = cooks[i].Surname;
                worksheet.Cells[i + 2, 2].Value = cooks[i].DateOfBirth?.ToString("yyyy-MM-dd");
                worksheet.Cells[i + 2, 3].Value = cooks[i].Restaurant?.Name ?? "Немає ресторану";
            }
            worksheet.Cells.AutoFitColumns();
        }

        private async Task ExportDishesAsync(ExcelPackage package, RestaurantDbContext context)
        {
            var worksheet = package.Workbook.Worksheets.Add("Страви");
            worksheet.Cells[1, 1].Value = "Назва";
            worksheet.Cells[1, 2].Value = "Ціна";
            worksheet.Cells[1, 3].Value = "Рецепт";
            worksheet.Cells[1, 4].Value = "Калорії";
            worksheet.Cells[1, 5].Value = "Категорія";
            var dishes = await context.Dishes.Include(d => d.Category).ToListAsync();
            for (int i = 0; i < dishes.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = dishes[i].Name;
                worksheet.Cells[i + 2, 2].Value = dishes[i].Price;
                worksheet.Cells[i + 2, 3].Value = dishes[i].Receipt;
                worksheet.Cells[i + 2, 4].Value = dishes[i].Calories;
                worksheet.Cells[i + 2, 5].Value = dishes[i].Category?.Description ?? "Немає категорії";
            }
            worksheet.Cells.AutoFitColumns();
        }

        private async Task ExportIngredientsAsync(ExcelPackage package, RestaurantDbContext context)
        {
            var worksheet = package.Workbook.Worksheets.Add("Інгредієнти");
            worksheet.Cells[1, 1].Value = "Назва";
            worksheet.Cells[1, 2].Value = "Вага(гр)/об'єм(мл)";
            worksheet.Cells[1, 3].Value = "Калорії";
            var ingredients = await context.Ingredients.ToListAsync();
            for (int i = 0; i < ingredients.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = ingredients[i].Name;
                worksheet.Cells[i + 2, 2].Value = ingredients[i].WeightMeasure;
                worksheet.Cells[i + 2, 3].Value = ingredients[i].Calories;
            }
            worksheet.Cells.AutoFitColumns();
        }

        private async Task ExportRestaurantsAsync(ExcelPackage package, RestaurantDbContext context)
        {
            var worksheet = package.Workbook.Worksheets.Add("Ресторани");
            worksheet.Cells[1, 1].Value = "Локація";
            worksheet.Cells[1, 2].Value = "Контакти";
            worksheet.Cells[1, 3].Value = "Відгуки";
            var restaurants = await context.Restaurants.ToListAsync();
            for (int i = 0; i < restaurants.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = restaurants[i].Name;
                worksheet.Cells[i + 2, 2].Value = restaurants[i].Contacts;
                worksheet.Cells[i + 2, 3].Value = restaurants[i].Reviews;
            }
            worksheet.Cells.AutoFitColumns();
        }
    }
}