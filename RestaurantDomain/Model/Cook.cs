using RestaurantDomain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantDomain.Models;

public partial class Cook : Entity
{
    public int RestaurantId { get; set; }

    [Required(ErrorMessage = "Прізвище кухаря є обов'язковим.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Прізвище кухаря повинно містити від 2 до 100 символів.")]
    [RegularExpression(@"^(?=.*[a-zA-Zа-яА-Я]).+$", ErrorMessage = "Прізвище кухаря повинно містити хоча б одну букву.")]
    [Display(Name = "Прізвище")]
    public string Surname { get; set; } = null!;

    [Required(ErrorMessage = "Дата народження є обов'язковою.")]
    [DataType(DataType.Date)]
    [Display(Name = "Дата народження")]
    [CustomValidation(typeof(Cook), nameof(ValidateDateOfBirth))]
    public DateOnly? DateOfBirth { get; set; }

    [Display(Name = "Ресторан")]
    public virtual Restaurant Restaurant { get; set; } = null!;

    public virtual ICollection<Dish> Dishes { get; set; } = new List<Dish>();
    public static ValidationResult? ValidateDateOfBirth(DateOnly? date, ValidationContext context)
    {
        if (date == null)
            return new ValidationResult("Дата народження є обов'язковою.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var minDate = today.AddYears(-70); 
        var maxDate = today.AddYears(-18);  

        if (date < minDate || date > maxDate)
            return new ValidationResult("Дата народження має бути між " + minDate.ToString("yyyy-MM-dd") + " і " + maxDate.ToString("yyyy-MM-dd") + ".");

        return ValidationResult.Success;
    }
}
