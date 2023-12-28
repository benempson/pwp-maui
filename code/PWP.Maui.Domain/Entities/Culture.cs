using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
namespace PWP.Maui.Domain.Entities;

[Index(propertyName: (nameof(TwoLetterCultureCode)), AllDescending = false, IsUnique = true)]
public class Culture
{
    //https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/data-annotations
    //[NotMapped] for locally calculated fields
    public int Id { get; set; }
    [Required, MaxLength(2)]
    public string? TwoLetterCultureCode { get; set; }
    [Required, MaxLength(5)]
    public string? FullCultureCode { get; set; }
    [Required, MaxLength(2)]
    public string? TwoLetterFlagCode { get; set; }
}
