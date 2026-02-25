using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiGMPKlik.Models.DataPrice;

public class DataPriceRange : BaseEntity
{
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MinPrice { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MaxPrice { get; set; }

    [Required]
    [StringLength(3)]
    public string Currency { get; set; } = "IDR";

    [StringLength(7)]
    public string? Color { get; set; }

    [StringLength(50)]
    public string? Icon { get; set; }

    [StringLength(50)]
    public string? Category { get; set; }

    public int? OrganizationId { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Code { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public int SortOrder { get; set; }
}