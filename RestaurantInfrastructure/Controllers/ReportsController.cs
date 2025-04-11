// RestaurantInfrastructure/Controllers/ReportsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RestaurantDomain.Models;
using RestaurantInfrastructure;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantInfrastructure.Controllers
{
    public class ReportsController : Controller
    {
        private readonly RestaurantDbContext _context;

        public ReportsController(RestaurantDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Export()
        {
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

            using var package = new ExcelPackage();

            if (exportCategories)
            {
                var worksheet = package.Workbook.Worksheets.Add("Категорії");
                var categories = await _context.Categories.ToListAsync();
                worksheet.Cells[1, 1].Value = "Опис";
                for (int i = 0; i < categories.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = categories[i].Description;
                }
                worksheet.Cells.AutoFitColumns();
            }

            if (exportCooks)
            {
                var worksheet = package.Workbook.Worksheets.Add("Кухарі");
                var cooks = await _context.Cooks.Include(c => c.Restaurant).ToListAsync();
                worksheet.Cells[1, 1].Value = "Прізвище";
                worksheet.Cells[1, 2].Value = "Дата народження";
                worksheet.Cells[1, 3].Value = "Ресторан";
                for (int i = 0; i < cooks.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = cooks[i].Surname;
                    worksheet.Cells[i + 2, 2].Value = cooks[i].DateOfBirth?.ToString("yyyy-MM-dd");
                    worksheet.Cells[i + 2, 3].Value = cooks[i].Restaurant?.Name ?? "Немає ресторану";
                }
                worksheet.Cells.AutoFitColumns();
            }

            if (exportDishes)
            {
                var worksheet = package.Workbook.Worksheets.Add("Страви");
                var dishes = await _context.Dishes.Include(d => d.Category).ToListAsync();
                worksheet.Cells[1, 1].Value = "Назва";
                worksheet.Cells[1, 2].Value = "Ціна";
                worksheet.Cells[1, 3].Value = "Рецепт";
                worksheet.Cells[1, 4].Value = "Калорії";
                worksheet.Cells[1, 5].Value = "Категорія";
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

            if (exportIngredients)
            {
                var worksheet = package.Workbook.Worksheets.Add("Інгредієнти");
                var ingredients = await _context.Ingredients.ToListAsync();
                worksheet.Cells[1, 1].Value = "Назва";
                worksheet.Cells[1, 2].Value = "Вага(гр)/об'єм(мл)";
                worksheet.Cells[1, 3].Value = "Калорії";
                for (int i = 0; i < ingredients.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = ingredients[i].Name;
                    worksheet.Cells[i + 2, 2].Value = ingredients[i].WeightMeasure;
                    worksheet.Cells[i + 2, 3].Value = ingredients[i].Calories;
                }
                worksheet.Cells.AutoFitColumns();
            }

            if (exportRestaurants)
            {
                var worksheet = package.Workbook.Worksheets.Add("Ресторани");
                var restaurants = await _context.Restaurants.ToListAsync();
                worksheet.Cells[1, 1].Value = "Локація";
                worksheet.Cells[1, 2].Value = "Контакти";
                worksheet.Cells[1, 3].Value = "Відгуки";
                for (int i = 0; i < restaurants.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = restaurants[i].Name;
                    worksheet.Cells[i + 2, 2].Value = restaurants[i].Contacts;
                    worksheet.Cells[i + 2, 3].Value = restaurants[i].Reviews;
                }
                worksheet.Cells.AutoFitColumns();
            }

            var stream = new MemoryStream(package.GetAsByteArray());
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

            using var stream = new MemoryStream();
            await importFile.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);

            var errors = new List<string>();

            // Тимчасові списки для зберігання нових записів
            var newCategories = new List<Category>();
            var newCooks = new List<Cook>();
            var newDishes = new List<Dish>();
            var newIngredients = new List<Ingredient>();
            var newRestaurants = new List<Restaurant>();

            // Збираємо всі назви для перевірки унікальності
            var allNames = new HashSet<string>();

            // Додаємо існуючі назви з бази даних
            var existingCategoryNames = await _context.Categories.Select(c => c.Description).ToListAsync();
            var existingCookSurnames = await _context.Cooks.Select(c => c.Surname).ToListAsync();
            var existingDishNames = await _context.Dishes.Select(d => d.Name).ToListAsync();
            var existingIngredientNames = await _context.Ingredients.Select(i => i.Name).ToListAsync();
            var existingRestaurantNames = await _context.Restaurants.Select(r => r.Name).ToListAsync();

            allNames.UnionWith(existingCategoryNames);
            allNames.UnionWith(existingCookSurnames);
            allNames.UnionWith(existingDishNames);
            allNames.UnionWith(existingIngredientNames);
            allNames.UnionWith(existingRestaurantNames);

            // Імпорт категорій
            var categoriesSheet = package.Workbook.Worksheets["Категорії"];
            if (categoriesSheet != null)
            {
                for (int row = 2; row <= categoriesSheet.Dimension.Rows; row++)
                {
                    try
                    {
                        var description = categoriesSheet.Cells[row, 1].GetValue<string>();

                        if (string.IsNullOrWhiteSpace(description))
                        {
                            errors.Add($"Рядок {row} у 'Категорії': Опис не може бути порожнім.");
                            continue;
                        }

                        // Перевірка унікальності назви
                        if (allNames.Contains(description))
                        {
                            errors.Add($"Рядок {row} у 'Категорії': Опис '{description}' уже існує в базі даних.");
                            continue;
                        }

                        allNames.Add(description);
                        newCategories.Add(new Category { Description = description });
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Рядок {row} у 'Категорії': Помилка обробки даних ({ex.Message}).");
                    }
                }
            }

            // Імпорт кухарів
            var cooksSheet = package.Workbook.Worksheets["Кухарі"];
            if (cooksSheet != null)
            {
                for (int row = 2; row <= cooksSheet.Dimension.Rows; row++)
                {
                    try
                    {
                        var surname = cooksSheet.Cells[row, 1].GetValue<string>();
                        var dateOfBirthStr = cooksSheet.Cells[row, 2].GetValue<string>();
                        var restaurantName = cooksSheet.Cells[row, 3].GetValue<string>();

                        if (string.IsNullOrWhiteSpace(surname))
                        {
                            errors.Add($"Рядок {row} у 'Кухарі': Прізвище не може бути порожнім.");
                            continue;
                        }

                        // Перевірка унікальності прізвища
                        if (allNames.Contains(surname))
                        {
                            errors.Add($"Рядок {row} у 'Кухарі': Прізвище '{surname}' уже існує в базі даних.");
                            continue;
                        }

                        if (!DateOnly.TryParse(dateOfBirthStr, out var dateOfBirth))
                        {
                            errors.Add($"Рядок {row} у 'Кухарі': Некоректний формат дати народження ({dateOfBirthStr}).");
                            continue;
                        }

                        int? restaurantId = null;
                        if (!string.IsNullOrWhiteSpace(restaurantName) && restaurantName != "Немає ресторану")
                        {
                            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.Name == restaurantName);
                            if (restaurant == null)
                            {
                                errors.Add($"Рядок {row} у 'Кухарі': Ресторан '{restaurantName}' не знайдено.");
                                continue;
                            }
                            restaurantId = restaurant.Id;
                        }

                        allNames.Add(surname);
                        newCooks.Add(new Cook
                        {
                            Surname = surname,
                            DateOfBirth = dateOfBirth,
                            RestaurantId = restaurantId ?? 0
                        });
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Рядок {row} у 'Кухарі': Помилка обробки даних ({ex.Message}).");
                    }
                }
            }

            // Імпорт страв
            var dishesSheet = package.Workbook.Worksheets["Страви"];
            if (dishesSheet != null)
            {
                for (int row = 2; row <= dishesSheet.Dimension.Rows; row++)
                {
                    try
                    {
                        var name = dishesSheet.Cells[row, 1].GetValue<string>();
                        var priceStr = dishesSheet.Cells[row, 2].GetValue<string>();
                        var receipt = dishesSheet.Cells[row, 3].GetValue<string>();
                        var caloriesStr = dishesSheet.Cells[row, 4].GetValue<string>();
                        var categoryDescription = dishesSheet.Cells[row, 5].GetValue<string>();

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            errors.Add($"Рядок {row} у 'Страви': Назва не може бути порожньою.");
                            continue;
                        }

                        // Перевірка унікальності назви
                        if (allNames.Contains(name))
                        {
                            errors.Add($"Рядок {row} у 'Страви': Назва '{name}' уже існує в базі даних.");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(receipt))
                        {
                            errors.Add($"Рядок {row} у 'Страви': Рецепт не може бути порожнім.");
                            continue;
                        }

                        // Валідація ціни (Price)
                        if (!decimal.TryParse(priceStr, out var price))
                        {
                            errors.Add($"Рядок {row} у 'Страви': Ціна '{priceStr}' має бути числом.");
                            continue;
                        }

                        if (price <= 0)
                        {
                            errors.Add($"Рядок {row} у 'Страви': Ціна '{price}' має бути більше 0.");
                            continue;
                        }

                        // Валідація калорій (Calories)
                        if (!int.TryParse(caloriesStr, out var calories))
                        {
                            errors.Add($"Рядок {row} у 'Страви': Калорії '{caloriesStr}' мають бути числом.");
                            continue;
                        }

                        if (calories <= 0)
                        {
                            errors.Add($"Рядок {row} у 'Страви': Калорії '{calories}' мають бути більше 0.");
                            continue;
                        }

                        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Description == categoryDescription);
                        if (category == null)
                        {
                            errors.Add($"Рядок {row} у 'Страви': Категорія '{categoryDescription}' не знайдена.");
                            continue;
                        }

                        allNames.Add(name);
                        newDishes.Add(new Dish
                        {
                            Name = name,
                            Price = price,
                            Receipt = receipt,
                            Calories = calories,
                            CategoryId = category.Id
                        });
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Рядок {row} у 'Страви': Помилка обробки даних ({ex.Message}).");
                    }
                }
            }

            // Імпорт інгредієнтів
            var ingredientsSheet = package.Workbook.Worksheets["Інгредієнти"];
            if (ingredientsSheet != null)
            {
                for (int row = 2; row <= ingredientsSheet.Dimension.Rows; row++)
                {
                    try
                    {
                        var name = ingredientsSheet.Cells[row, 1].GetValue<string>();
                        var weightMeasureStr = ingredientsSheet.Cells[row, 2].GetValue<string>();
                        var caloriesStr = ingredientsSheet.Cells[row, 3].GetValue<string>();

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            errors.Add($"Рядок {row} у 'Інгредієнти': Назва не може бути порожньою.");
                            continue;
                        }

                        // Перевірка унікальності назви
                        if (allNames.Contains(name))
                        {
                            errors.Add($"Рядок {row} у 'Інгредієнти': Назва '{name}' уже існує в базі даних.");
                            continue;
                        }

                        // Валідація ваги/об'єму (WeightMeasure)
                        if (!decimal.TryParse(weightMeasureStr, out var weightMeasure))
                        {
                            errors.Add($"Рядок {row} у 'Інгредієнти': Вага/об'єм '{weightMeasureStr}' має бути числом.");
                            continue;
                        }

                        if (weightMeasure <= 0)
                        {
                            errors.Add($"Рядок {row} у 'Інгредієнти': Вага/об'єм '{weightMeasure}' має бути більше 0.");
                            continue;
                        }

                        // Валідація калорій (Calories)
                        if (!int.TryParse(caloriesStr, out var calories))
                        {
                            errors.Add($"Рядок {row} у 'Інгредієнти': Калорії '{caloriesStr}' мають бути числом.");
                            continue;
                        }

                        if (calories <= 0)
                        {
                            errors.Add($"Рядок {row} у 'Інгредієнти': Калорії '{calories}' мають бути більше 0.");
                            continue;
                        }

                        allNames.Add(name);
                        newIngredients.Add(new Ingredient
                        {
                            Name = name,
                            WeightMeasure = weightMeasure.ToString(), // Зберігаємо як рядок, якщо це потрібно
                            Calories = calories
                        });
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Рядок {row} у 'Інгредієнти': Помилка обробки даних ({ex.Message}).");
                    }
                }
            }

            // Імпорт ресторанів
            var restaurantsSheet = package.Workbook.Worksheets["Ресторани"];
            if (restaurantsSheet != null)
            {
                for (int row = 2; row <= restaurantsSheet.Dimension.Rows; row++)
                {
                    try
                    {
                        var name = restaurantsSheet.Cells[row, 1].GetValue<string>();
                        var contacts = restaurantsSheet.Cells[row, 2].GetValue<string>();
                        var reviews = restaurantsSheet.Cells[row, 3].GetValue<string>();

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            errors.Add($"Рядок {row} у 'Ресторани': Локація не може бути порожньою.");
                            continue;
                        }

                        // Перевірка унікальності назви
                        if (allNames.Contains(name))
                        {
                            errors.Add($"Рядок {row} у 'Ресторани': Локація '{name}' уже існує в базі даних.");
                            continue;
                        }

                        allNames.Add(name);
                        newRestaurants.Add(new Restaurant
                        {
                            Name = name,
                            Contacts = contacts,
                            Reviews = reviews
                        });
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Рядок {row} у 'Ресторани': Помилка обробки даних ({ex.Message}).");
                    }
                }
            }

            // Якщо є помилки, нічого не зберігаємо
            if (errors.Any())
            {
                TempData["Error"] = string.Join("<br>", errors);
                return RedirectToAction("Index");
            }

            // Якщо помилок немає, додаємо всі записи до бази і зберігаємо
            try
            {
                _context.Categories.AddRange(newCategories);
                _context.Cooks.AddRange(newCooks);
                _context.Dishes.AddRange(newDishes);
                _context.Ingredients.AddRange(newIngredients);
                _context.Restaurants.AddRange(newRestaurants);

                await _context.SaveChangesAsync();
                TempData["Success"] = "Дані успішно імпортовано.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Помилка при збереженні даних: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}