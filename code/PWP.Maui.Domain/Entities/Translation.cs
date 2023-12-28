using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PWP.Maui.Domain.Entities;

public class Translation
{
    public int Id { get; set; }
    public int CultureId { get; set; }
    [ForeignKey("CultureId")]
    public Culture? Culture { get; set; }
    [Required, MaxLength(15)]
    public int AreaId { get; set; }
    [ForeignKey("AreaId")]
    public TranslationArea? Area { get; set; }
    [Required, MaxLength(50)]
    public string? Key { get; set; }
    [Required, MaxLength(4000)]
    public string? Value { get; set; }
}