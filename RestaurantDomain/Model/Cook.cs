using RestaurantDomain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantDomain.Models;

public partial class Cook : Entity
{
    public int RestaurantId { get; set; }

    [Display(Name = "Прізвище")]
    public string Surname { get; set; } = null!;

    [Display(Name = "Дата народження")]
    public DateOnly? DateOfBirth { get; set; }

    [Display(Name = "Ресторан")]
    public virtual Restaurant Restaurant { get; set; } = null!;

    public virtual ICollection<Dish> Dishes { get; set; } = new List<Dish>();
}
