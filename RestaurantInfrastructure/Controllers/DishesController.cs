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
                .Include(d => d.Ingredients) // Завантажуємо поточні інгредієнти
                .Include(d => d.Category)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dish == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Description", dish.CategoryId);
            // Передаємо список усіх інгредієнтів для вибору
            ViewData["Ingredients"] = new MultiSelectList(_context.Ingredients, "Id", "Name", dish.Ingredients.Select(i => i.Id));
            return View(dish);
        }

        // POST: Dishes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Price,Receipt,Calories,CategoryId,Id")] Dish dish, int[] selectedIngredients)
        {
            if (id != dish.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Name");
            ModelState.Remove("Category");

            // Перевірка унікальності назви (виключаємо поточну страву)
            if (await _context.Dishes.AnyAsync(d => d.Name == dish.Name && d.Id != dish.Id))
            {
                ModelState.AddModelError("Name", "Страва з такою назвою вже існує.");
            }

            // Перевірка, чи вибрано хоча б один інгредієнт
            if (selectedIngredients == null || !selectedIngredients.Any())
            {
                ModelState.AddModelError("selectedIngredients", "Будь ласка, виберіть хоча б один інгредієнт для страви.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var dishToUpdate = await _context.Dishes
                        .Include(d => d.Ingredients)
                        .FirstOrDefaultAsync(d => d.Id == id);

                    if (dishToUpdate == null)
                    {
                        return NotFound();
                    }

                    dishToUpdate.Name = dish.Name;
                    dishToUpdate.Price = dish.Price;
                    dishToUpdate.Receipt = dish.Receipt;
                    dishToUpdate.Calories = dish.Calories;
                    dishToUpdate.CategoryId = dish.CategoryId;

                    dishToUpdate.Ingredients.Clear();
                    if (selectedIngredients != null)
                    {
                        var ingredientsToAdd = await _context.Ingredients
                            .Where(i => selectedIngredients.Contains(i.Id))
                            .ToListAsync();
                        foreach (var ingredient in ingredientsToAdd)
                        {
                            if (!dishToUpdate.Ingredients.Any(i => i.Id == ingredient.Id))
                            {
                                dishToUpdate.Ingredients.Add(ingredient);
                            }
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

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Description", dish.CategoryId);
            ViewData["Ingredients"] = new MultiSelectList(_context.Ingredients, "Id", "Name", selectedIngredients);
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
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dish == null)
            {
                return NotFound();
            }

            return View(dish);
        }

        // POST: Dishes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dish = await _context.Dishes.FindAsync(id);
            if (dish != null)
            {
                _context.Dishes.Remove(dish);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DishExists(int id)
        {
            return _context.Dishes.Any(e => e.Id == id);
        }
    }
}