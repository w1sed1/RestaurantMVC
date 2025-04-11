using RestaurantDomain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantDomain.Models;

public partial class Dish : Entity
{
    [Required(ErrorMessage = "Назва страви є обов'язковою.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Назва страви повинна містити від 2 до 100 символів.")]
    [RegularExpression(@"^(?=.*[a-zA-Zа-яА-Я]).+$", ErrorMessage = "Назва страви повинна містити хоча б одну букву.")]
    [Display(Name = "Назва")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Ціна є обов'язковою.")]
    [Range(0.01, 10000, ErrorMessage = "Ціна повинна бути в межах від 0.01 грн до 10000 грн.")]
    [Display(Name = "Ціна(грн)")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Рецепт є обов'язковим.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Рецепт повинен містити від 10 до 500 символів.")]
    [Display(Name = "Рецепт")]
    public string? Receipt { get; set; }

    [Required(ErrorMessage = "Калорійність є обов'язковою.")]
    [Range(0, 10000, ErrorMessage = "Калорійність повинна бути в межах від 0 ккал до 10000 ккал.")]
    [Display(Name = "Калорії")]
    public int? Calories { get; set; }

    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Категорія є обов'язковою.")]
    [Display(Name = "Категорія")]
    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Cook> Cooks { get; set; } = new List<Cook>();

    public virtual ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
}
