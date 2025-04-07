using RestaurantDomain.Model;
using System.ComponentModel.DataAnnotations;

namespace RestaurantDomain.Models;

public partial class Ingredient : Entity
{
    [Display(Name = "Назва")]
    public string Name { get; set; } = null!;

    [Display(Name = "Вага/об'єм")]
    public string? WeightMeasure { get; set; }

    [Display(Name = "Калорії")]
    public int? Calories { get; set; }

    public virtual ICollection<Dish> Dishes { get; set; } = new List<Dish>();
}
