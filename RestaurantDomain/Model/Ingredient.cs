using RestaurantDomain.Model;
using System.ComponentModel.DataAnnotations;

namespace RestaurantDomain.Models;

public partial class Ingredient : Entity
{
    [Required(ErrorMessage = "Назва інгредієнта є обов'язковою.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Назва інгредієнта повинна містити від 2 до 100 символів.")]
    [RegularExpression(@"^(?=.*[a-zA-Zа-яА-Я]).+$", ErrorMessage = "Назва інгредієнта повинна містити хоча б одну букву.")]
    [Display(Name = "Назва")]
    public string Name { get; set; } = null!;

    [Range(0, 10000, ErrorMessage = "Вага повинна бути в межах від 0 до 10000.")]
    [Display(Name = "Вага(гр)/об'єм(мл)")]
    public string? WeightMeasure { get; set; }

    [Required(ErrorMessage = "Калорійність є обов'язковою.")]
    [Range(0, 10000, ErrorMessage = "Калорійність повинна бути в межах від 0 до 10000.")]
    [Display(Name = "Калорії")]
    public int? Calories { get; set; }

    public virtual ICollection<Dish> Dishes { get; set; } = new List<Dish>();
}
