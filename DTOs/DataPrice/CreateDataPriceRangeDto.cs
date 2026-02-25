using System.ComponentModel.DataAnnotations;

namespace ApiGMPKlik.DTOs.DataPrice;

public class CreateDataPriceRangeDto
{
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "MinPrice harus >= 0")]
    public decimal MinPrice { get; set; }

    [Required]
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

public class UpdateDataPriceRangeDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "MinPrice harus >= 0")]
    public decimal MinPrice { get; set; }

    [Required]
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

    public byte[]? RowVersion { get; set; }
}

public class DataPriceRangeResponseDto
{
    public int Id { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public string? Category { get; set; }
    public int? OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public byte[]? RowVersion { get; set; }
}

public class DataPriceRangeDropdownDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string DisplayText => $"{Name} ({Currency} {MinPrice:N0} - {MaxPrice:N0})";
}

public class DataPriceRangePagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; } = "asc";
}