using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantDomain.Models;
using RestaurantInfrastructure;

namespace RestaurantInfrastructure.Controllers
{
    public class DishesController : Controller
    {
        private readonly RestaurantDbContext _context;

        public DishesController(RestaurantDbContext context)
        {
            _context = context;
        }

        // GET: Dishes
        public async Task<IActionResult> Index()
        {
            if (_context == null)
            {
                return Problem("Entity set 'RestaurantDbContext' is null.");
            }

            return View(await _context.Dishes
                .Include(d => d.Category)
                .Include(d => d.Cooks)
                .Include(d => d.Ingredients)
                .ToListAsync());
        }

        // GET: Dishes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dish = await _context.Dishes
                .Include(d => d.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dish == null)
            {
                return NotFound();
            }

            return View(dish);
        }

        // GET: Dishes/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Description");
            return View();
        }

        // POST: Dishes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Price,Receipt,Calories,CategoryId,Id")] Dish dish)
        {
            ModelState.Remove("Name");
            ModelState.Remove("Category");

            // Перевірка унікальності назви
            if (await _context.Dishes.AnyAsync(d => d.Name == dish.Name))
            {
                ModelState.AddModelError("Name", "Страва з такою назвою вже існує.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(dish);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Description", dish.CategoryId);
            return View(dish);
        }
        // GET: Dishes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dish = await _context.Dishes
                .Include(d => d.Category)
                .Include(d => d.Cooks)        // Завантажуємо пов’язаних кухарів
                .Include(d => d.Ingredients)  // Завантажуємо пов’язані інгредієнти
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dish == null)
            {
                return NotFound();
            }

            // Передаємо список категорій для вибору
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Description", dish.CategoryId);

            // Передаємо список інгредієнтів для вибору (MultiSelectList)
            ViewData["Ingredients"] = new MultiSelectList(_context.Ingredients, "Id", "Name", dish.Ingredients.Select(i => i.Id));

            // Передаємо список кухарів для вибору (MultiSelectList)
            ViewData["Cooks"] = new MultiSelectList(_context.Cooks, "Id", "Surname", dish.Cooks.Select(c => c.Id));

            return View(dish);
        }
        // POST: Dishes/Edit/5
        // POST: Dishes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,Receipt,Calories,CategoryId")] Dish dish, int[] selectedIngredients, int[] selectedCooks)
        {
            if (id != dish.Id)
            {
                return NotFound();
            }
            ModelState.Remove("Name");
            ModelState.Remove("Category");
            if (ModelState.IsValid)
            {
                try
                {
                    // Завантажуємо страву з пов’язаними інгредієнтами та кухарями
                    var dishToUpdate = await _context.Dishes
                        .Include(d => d.Ingredients)
                        .Include(d => d.Cooks)
                        .FirstOrDefaultAsync(d => d.Id == id);

                    if (dishToUpdate == null)
                    {
                        return NotFound();
                    }

                    // Оновлюємо основні поля страви
                    dishToUpdate.Name = dish.Name;
                    dishToUpdate.Price = dish.Price;
                    dishToUpdate.Receipt = dish.Receipt;
                    dishToUpdate.Calories = dish.Calories;
                    dishToUpdate.CategoryId = dish.CategoryId;

                    // Оновлюємо інгредієнти
                    dishToUpdate.Ingredients.Clear();  // Очищаємо поточні інгредієнти
                    if (selectedIngredients != null)
                    {
                        var ingredientsToAdd = await _context.Ingredients
                            .Where(i => selectedIngredients.Contains(i.Id))
                            .ToListAsync();
                        foreach (var ingredient in ingredientsToAdd)
                        {
                            dishToUpdate.Ingredients.Add(ingredient);
                        }
                    }

                    // Оновлюємо кухарів
                    dishToUpdate.Cooks.Clear();  // Очищаємо поточних кухарів
                    if (selectedCooks != null)
                    {
                        var cooksToAdd = await _context.Cooks
                            .Where(c => selectedCooks.Contains(c.Id))
                            .ToListAsync();
                        foreach (var cook in cooksToAdd)
                        {
                            dishToUpdate.Cooks.Add(cook);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DishExists(dish.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Якщо валідація не пройшла, повертаємо форму з даними
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Description", dish.CategoryId);
            ViewData["Ingredients"] = new MultiSelectList(_context.Ingredients, "Id", "Name", selectedIngredients);
            ViewData["Cooks"] = new MultiSelectList(_context.Cooks, "Id", "Surname", selectedCooks);
            return View(dish);
        }
        // GET: Dishes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dish = await _context.Dishes
                .Include(d => d.Category)
                .Include(d => d.Cooks)        // Завантажуємо пов’язаних кухарів
                .Include(d => d.Ingredients)  // Завантажуємо пов’язані інгредієнти
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dish == null)
            {
                return NotFound();
            }

            return View(dish);
        }
        // POST: Dishes/Delete/5
        // POST: Dishes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dish = await _context.Dishes
                .Include(d => d.Cooks)        // Завантажуємо пов’язаних кухарів
                .Include(d => d.Ingredients)  // Завантажуємо пов’язані інгредієнти
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dish != null)
            {
                // Очищаємо зв’язки з кухарями
                dish.Cooks.Clear();

                // Очищаємо зв’язки з інгредієнтами
                dish.Ingredients.Clear();

                // Видаляємо страву
                _context.Dishes.Remove(dish);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        private bool DishExists(int id)
        {
            return _context.Dishes.Any(e => e.Id == id);
        }
    }
}