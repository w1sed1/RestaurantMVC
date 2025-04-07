using RestaurantDomain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantDomain.Models;

public partial class Dish : Entity
{
    [Display(Name = "Назва")]
    public string Name { get; set; } = null!; // Додаємо поле для назви страви

    [Display(Name = "Ціна")]
    public decimal Price { get; set; }

    [Display(Name = "Рецепт")]
    public string? Receipt { get; set; }

    [Display(Name = "Калорії")]
    public int? Calories { get; set; }

    public int CategoryId { get; set; }

    [Display(Name = "Категорія")]
    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Cook> Cooks { get; set; } = new List<Cook>();

    public virtual ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
}
