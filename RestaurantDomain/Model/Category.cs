using RestaurantDomain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantDomain.Models;

public partial class Category : Entity
{
    [Display(Name = "Опис")]
    public string Description { get; set; } = null!;

    public virtual ICollection<Dish> Dishes { get; set; } = new List<Dish>();
}
