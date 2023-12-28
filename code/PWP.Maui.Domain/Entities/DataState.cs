using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PWP.Maui.Domain.Entities;

[Index(propertyName: (nameof(Type)), AllDescending = false, IsUnique = true)]
public class DataState
{
    public int Id { get; set; }
    [Required, MaxLength(10)]
    public string? Type { get; set; }
    [Required, MaxLength(50)]
    public string? StateHash { get; set; }
}
