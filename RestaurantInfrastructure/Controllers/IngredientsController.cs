﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantDomain.Models;
using RestaurantInfrastructure;

namespace RestaurantInfrastructure.Controllers
{
    [Authorize]
    public class IngredientsController : Controller
    {
        private readonly RestaurantDbContext _context;

        public IngredientsController(RestaurantDbContext context)
        {
            _context = context;
        }

        //(GET: Ingredients (доступно для всіх авторизованих)
        public async Task<IActionResult> Index()
        {
            return View(await _context.Ingredients.ToListAsync());
        }

        // GET: Ingredients/Details/5 (доступно для всіх авторизованих)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ingredient = await _context.Ingredients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ingredient == null)
            {
                return NotFound();
            }

            return View(ingredient);
        }

        // GET: Ingredients/Create (доступно для Admin і Chef)
        [Authorize(Roles = "Admin,Chef")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Ingredients/Create (доступно для Admin і Chef)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Chef")]
        public async Task<IActionResult> Create([Bind("Id,Name,WeightMeasure")] Ingredient ingredient)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ingredient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ingredient);
        }

        // GET: Ingredients/Edit/5 (доступно для Admin і Chef)
        [Authorize(Roles = "Admin,Chef")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null)
            {
                return NotFound();
            }
            return View(ingredient);
        }

        // POST: Ingredients/Edit/5 (доступно для Admin і Chef)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Chef")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,WeightMeasure")] Ingredient ingredient)
        {
            if (id != ingredient.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ingredient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IngredientExists(ingredient.Id))
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
            return View(ingredient);
        }

        // GET: Ingredients/Delete/5 (доступно для Admin)
        [Authorize(Roles = "Admin,Chef")]

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ingredient = await _context.Ingredients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ingredient == null)
            {
                return NotFound();
            }

            return View(ingredient);
        }

        // POST: Ingredients/Delete/5 (доступно для Admin)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Chef")]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient != null)
            {
                _context.Ingredients.Remove(ingredient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IngredientExists(int id)
        {
            return _context.Ingredients.Any(e => e.Id == id);
        }
    }
}