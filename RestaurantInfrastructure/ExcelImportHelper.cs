using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RestaurantDomain.Models;
using RestaurantInfrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantInfrastructure
{
    public class ExcelImportHelper
    {
        private readonly RestaurantDbContext _context;

        public ExcelImportHelper(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, List<string> Errors)> ImportFromExcelAsync(IFormFile importFile)
        {
            var errors = new List<string>();

            using var stream = new MemoryStream();
            await importFile.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);

            // Збираємо всі назви для перевірки унікальності
            var allNames = new HashSet<string>();
            allNames.UnionWith(await _context.Categories.Select(c => c.Description).ToListAsync());
            allNames.UnionWith(await _context.Cooks.Select(c => c.Surname).ToListAsync());
            allNames.UnionWith(await _context.Dishes.Select(d => d.Name).ToListAsync());
            allNames.UnionWith(await _context.Ingredients.Select(i => i.Name).ToListAsync());
            allNames.UnionWith(await _context.Restaurants.Select(r => r.Name).ToListAsync());

            // Тимчасові списки для нових записів
            var newCategories = new List<Category>();
            var newCooks = new List<Cook>();
            var newDishes = new List<Dish>();
            var newIngredients = new List<Ingredient>();
            var newRestaurants = new List<Restaurant>();

            // Імпорт кожної сутності
            await ImportCategoriesAsync(package, allNames, newCategories, errors);
            await ImportRestaurantsAsync(package, allNames, newRestaurants, errors);
            await ImportCooksAsync(package, allNames, newCooks, errors);
            await ImportDishesAsync(package, allNames, newDishes, errors);
            await ImportIngredientsAsync(package, allNames, newIngredients, errors);

            // Збереження даних у базу, якщо немає помилок
            if (errors.Any())
            {
                return (false, errors);
            }

            await SaveImportedDataAsync(newCategories, newCooks, newDishes, newIngredients, newRestaurants);
            return (true, errors);
        }

        private async Task ImportCategoriesAsync(ExcelPackage package, HashSet<string> allNames, List<Category> newCategories, List<string> errors)
        {
            var sheet = package.Workbook.Worksheets["Категорії"];
            if (sheet == null) return;

            for (int row = 2; row <= sheet.Dimension.Rows; row++)
            {
                try
                {
                    var description = sheet.Cells[row, 1].GetValue<string>();
                    if (string.IsNullOrWhiteSpace(description))
                    {
                        errors.Add($"Рядок {row} у 'Категорії': Опис не може бути порожнім.");
                        continue;
                    }

                    if (allNames.Contains(description))
                    {
                        errors.Add($"Рядок {row} у 'Категорії': Опис '{description}' уже існує.");
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

        private async Task ImportRestaurantsAsync(ExcelPackage package, HashSet<string> allNames, List<Restaurant> newRestaurants, List<string> errors)
        {
            var sheet = package.Workbook.Worksheets["Ресторани"];
            if (sheet == null) return;

            for (int row = 2; row <= sheet.Dimension.Rows; row++)
            {
                try
                {
                    var name = sheet.Cells[row, 1].GetValue<string>();
                    var contacts = sheet.Cells[row, 2].GetValue<string>();
                    var reviews = sheet.Cells[row, 3].GetValue<string>();

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        errors.Add($"Рядок {row} у 'Ресторани': Назва не може бути порожньою.");
                        continue;
                    }

                    if (allNames.Contains(name))
                    {
                        errors.Add($"Рядок {row} у 'Ресторани': Назва '{name}' уже існує.");
                        continue;
                    }

                    // Валідація формату назви (адреси)
                    var nameRegex = @"^(вул\.|просп\.|пл\.|бул\.)\s*[a-zA-Zа-яА-ЯїЇіІєЄ\s\-']+\s+\d+[a-zA-Zа-яА-Я]?$";
                    if (!System.Text.RegularExpressions.Regex.IsMatch(name, nameRegex))
                    {
                        errors.Add($"Рядок {row} у 'Ресторани': Некоректний формат адреси '{name}'.");
                        continue;
                    }

                    // Валідація контактів, якщо вони вказані
                    if (!string.IsNullOrWhiteSpace(contacts))
                    {
                        var contactRegex = @"^\+380\d{9}$";
                        if (!System.Text.RegularExpressions.Regex.IsMatch(contacts, contactRegex))
                        {
                            errors.Add($"Рядок {row} у 'Ресторани': Некоректний формат контактів '{contacts}'.");
                            continue;
                        }
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

        private async Task ImportCooksAsync(ExcelPackage package, HashSet<string> allNames, List<Cook> newCooks, List<string> errors)
        {
            var sheet = package.Workbook.Worksheets["Кухарі"];
            if (sheet == null) return;

            for (int row = 2; row <= sheet.Dimension.Rows; row++)
            {
                try
                {
                    var surname = sheet.Cells[row, 1].GetValue<string>();
                    var dateOfBirthStr = sheet.Cells[row, 2].GetValue<string>();
                    var restaurantName = sheet.Cells[row, 3].GetValue<string>();

                    if (string.IsNullOrWhiteSpace(surname))
                    {
                        errors.Add($"Рядок {row} у 'Кухарі': Прізвище не може бути порожнім.");
                        continue;
                    }

                    if (allNames.Contains(surname))
                    {
                        errors.Add($"Рядок {row} у 'Кухарі': Прізвище '{surname}' уже існує.");
                        continue;
                    }

                    if (!DateOnly.TryParse(dateOfBirthStr, out var dateOfBirth))
                    {
                        errors.Add($"Рядок {row} у 'Кухарі': Некоректний формат дати народження '{dateOfBirthStr}'.");
                        continue;
                    }

                    // Валідація дати народження
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    var minDate = today.AddYears(-70);
                    var maxDate = today.AddYears(-18);
                    if (dateOfBirth < minDate || dateOfBirth > maxDate)
                    {
                        errors.Add($"Рядок {row} у 'Кухарі': Дата народження має бути між {minDate:yyyy-MM-dd} і {maxDate:yyyy-MM-dd}.");
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

        private async Task ImportDishesAsync(ExcelPackage package, HashSet<string> allNames, List<Dish> newDishes, List<string> errors)
        {
            var sheet = package.Workbook.Worksheets["Страви"];
            if (sheet == null) return;

            for (int row = 2; row <= sheet.Dimension.Rows; row++)
            {
                try
                {
                    var name = sheet.Cells[row, 1].GetValue<string>();
                    var priceStr = sheet.Cells[row, 2].GetValue<string>();
                    var receipt = sheet.Cells[row, 3].GetValue<string>();
                    var caloriesStr = sheet.Cells[row, 4].GetValue<string>();
                    var categoryDescription = sheet.Cells[row, 5].GetValue<string>();

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        errors.Add($"Рядок {row} у 'Страви': Назва не може бути порожньою.");
                        continue;
                    }

                    if (allNames.Contains(name))
                    {
                        errors.Add($"Рядок {row} у 'Страви': Назва '{name}' уже існує.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(receipt))
                    {
                        errors.Add($"Рядок {row} у 'Страви': Рецепт не може бути порожнім.");
                        continue;
                    }

                    // Валідація ціни
                    if (!decimal.TryParse(priceStr, out var price) || price < 0.01m)
                    {
                        errors.Add($"Рядок {row} у 'Страви': Ціна '{priceStr}' має бути числом ≥ 0.01.");
                        continue;
                    }

                    // Валідація калорій
                    if (!int.TryParse(caloriesStr, out var calories) || calories <= 0)
                    {
                        errors.Add($"Рядок {row} у 'Страви': Калорії '{caloriesStr}' мають бути числом > 0.");
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

        private async Task ImportIngredientsAsync(ExcelPackage package, HashSet<string> allNames, List<Ingredient> newIngredients, List<string> errors)
        {
            var sheet = package.Workbook.Worksheets["Інгредієнти"];
            if (sheet == null) return;

            for (int row = 2; row <= sheet.Dimension.Rows; row++)
            {
                try
                {
                    var name = sheet.Cells[row, 1].GetValue<string>();
                    var weightMeasure = sheet.Cells[row, 2].GetValue<string>();
                    var caloriesStr = sheet.Cells[row, 3].GetValue<string>();

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        errors.Add($"Рядок {row} у 'Інгредієнти': Назва не може бути порожньою.");
                        continue;
                    }

                    if (allNames.Contains(name))
                    {
                        errors.Add($"Рядок {row} у 'Інгредієнти': Назва '{name}' уже існує.");
                        continue;
                    }

                    // Валідація калорій
                    if (!int.TryParse(caloriesStr, out var calories) || calories <= 0)
                    {
                        errors.Add($"Рядок {row} у 'Інгредієнти': Калорії '{caloriesStr}' мають бути числом > 0.");
                        continue;
                    }

                    // Валідація ваги/об'єму, якщо вказано
                    if (!string.IsNullOrWhiteSpace(weightMeasure) && decimal.TryParse(weightMeasure, out var weight) && weight < 0)
                    {
                        errors.Add($"Рядок {row} у 'Інгредієнти': Вага/об'єм '{weightMeasure}' не може бути від'ємним.");
                        continue;
                    }

                    allNames.Add(name);
                    newIngredients.Add(new Ingredient
                    {
                        Name = name,
                        WeightMeasure = weightMeasure,
                        Calories = calories
                    });
                }
                catch (Exception ex)
                {
                    errors.Add($"Рядок {row} у 'Інгредієнти': Помилка обробки даних ({ex.Message}).");
                }
            }
        }

        private async Task SaveImportedDataAsync(List<Category> categories, List<Cook> cooks, List<Dish> dishes, List<Ingredient> ingredients, List<Restaurant> restaurants)
        {
            if (categories.Any())
                _context.Categories.AddRange(categories);

            if (restaurants.Any())
                _context.Restaurants.AddRange(restaurants);

            if (cooks.Any())
                _context.Cooks.AddRange(cooks);

            if (dishes.Any())
                _context.Dishes.AddRange(dishes);

            if (ingredients.Any())
                _context.Ingredients.AddRange(ingredients);

            await _context.SaveChangesAsync();
        }
    }
}