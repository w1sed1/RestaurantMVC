using RestaurantDomain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantDomain.Models;

public partial class Restaurant : Entity
{
    [Display(Name = "Локація")]
    public string Name { get; set; } = null!;

    [Display(Name = "Контакти")]
    public string? Contacts { get; set; }

    [Display(Name = "Відгуки")]
    public string? Reviews { get; set; }

    public virtual ICollection<Cook> Cooks { get; set; } = new List<Cook>();
}
