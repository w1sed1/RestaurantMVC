﻿using System;
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
    public class CooksController : Controller
    {
        private readonly RestaurantDbContext _context;

        public CooksController(RestaurantDbContext context)
        {
            _context = context;
        }

        // GET: Cooks
        public async Task<IActionResult> Index()
        {
            var restaurantDbContext = _context.Cooks
                .Include(c => c.Restaurant)
                .Include(c => c.Dishes); // Додаємо Include для страв
            return View(await restaurantDbContext.ToListAsync());
        }

        // GET: Cooks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cook = await _context.Cooks
                .Include(c => c.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cook == null)
            {
                return NotFound();
            }

            return View(cook);
        }

        // GET: Cooks/Create
        // GET: Cooks/Create
        public IActionResult Create()
        {
            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", null);  // Дозволяємо null як вибране значення
            return View();
        }

        // POST: Cooks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RestaurantId,Surname,DateOfBirth,Id")] Cook cook)
        {
            ModelState.Remove("Surname");
            ModelState.Remove("Restaurant");
            if (ModelState.IsValid)
            {
                _context.Add(cook);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", cook.RestaurantId);
            return View(cook);
        }
        // GET: Cooks/Edit/5
        // GET: Cooks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cook = await _context.Cooks
                .Include(c => c.Restaurant)
                .Include(c => c.Dishes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cook == null)
            {
                return NotFound();
            }

            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", cook.RestaurantId);
            ViewData["Dishes"] = new MultiSelectList(_context.Dishes, "Id", "Name", cook.Dishes.Select(d => d.Id));
            return View(cook);
        }

        // POST: Cooks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Surname,DateOfBirth,RestaurantId")] Cook cook, int[] selectedDishes)
        {
            if (id != cook.Id)
            {
                return NotFound();
            }
            ModelState.Remove("Surname");
            ModelState.Remove("Restaurant");
            if (ModelState.IsValid)
            {
                try
                {
                    var cookToUpdate = await _context.Cooks
                        .Include(c => c.Dishes)
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (cookToUpdate == null)
                    {
                        return NotFound();
                    }

                    cookToUpdate.Surname = cook.Surname;
                    cookToUpdate.DateOfBirth = cook.DateOfBirth;
                    cookToUpdate.RestaurantId = cook.RestaurantId;

                    cookToUpdate.Dishes.Clear();
                    if (selectedDishes != null)
                    {
                        var dishesToAdd = await _context.Dishes
                            .Where(d => selectedDishes.Contains(d.Id))
                            .ToListAsync();
                        foreach (var dish in dishesToAdd)
                        {
                            cookToUpdate.Dishes.Add(dish);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CookExists(cook.Id))
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

            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", cook.RestaurantId);
            ViewData["Dishes"] = new MultiSelectList(_context.Dishes, "Id", "Name", selectedDishes);
            return View(cook);
        }
        // GET: Cooks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cook = await _context.Cooks
                .Include(c => c.Restaurant)
                .Include(c => c.Dishes)  // Завантажуємо пов’язані страви
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cook == null)
            {
                return NotFound();
            }

            return View(cook);
        }
        // POST: Cooks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cook = await _context.Cooks
                .Include(c => c.Dishes)  // Завантажуємо пов’язані страви
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cook != null)
            {
                // Очищаємо зв’язки зі стравами
                cook.Dishes.Clear();

                // Видаляємо кухаря
                _context.Cooks.Remove(cook);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        private bool CookExists(int id)
        {
            return _context.Cooks.Any(e => e.Id == id);
        }
    }
}
