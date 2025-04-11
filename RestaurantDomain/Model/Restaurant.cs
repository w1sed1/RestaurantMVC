using RestaurantDomain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantDomain.Models;

public partial class Restaurant : Entity
{
    [Required(ErrorMessage = "Назва ресторану є обов'язковою.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Назва ресторану повинна містити від 2 до 100 символів.")]
    [RegularExpression(@"^(вул\.|просп\.|пл\.|бул\.)\s*[a-zA-Zа-яА-ЯїЇіІєЄ\s\-']+\s+\d+[a-zA-Zа-яА-Я]?$",
        ErrorMessage = "Локація повинна містити тип (вул., просп., пл., бул.), назву з букв та номер будинку з цифр")]
    [Display(Name = "Локація")]
    public string Name { get; set; } = null!;
    
    [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Контакти повинні бути у форматі +380xxxxxxxxx")]
    [StringLength(12, MinimumLength = 12, ErrorMessage = "Номер телефону повинен містити рівно 12 символів (+380 і 9 цифр).")]
    [Display(Name = "Контакти")]
    public string? Contacts { get; set; }

    [Display(Name = "Відгуки")]
    public string? Reviews { get; set; }

    public virtual ICollection<Cook> Cooks { get; set; } = new List<Cook>();
}
