using System.ComponentModel.DataAnnotations;

namespace ApiGMPKlik.Models.Entities.Address
{
    public class WilayahRt : BaseEntity
    {
        public int RwId { get; set; }

        [StringLength(10)]
        public string Nama { get; set; } = string.Empty; // Numeric 001-999

        public int SortOrder { get; set; }

        // Navigation
        public virtual WilayahRw Rw { get; set; } = null!;
    }
}