using Microsoft.EntityFrameworkCore;

namespace PWP.Maui.Domain.Entities;

[Index(propertyName: (nameof(Name)), AllDescending = false, IsUnique = true)]
public class TranslationArea
{
    public int Id { get; set; }
    public string? Name { get; set; }
}