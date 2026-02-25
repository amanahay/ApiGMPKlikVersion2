using System.ComponentModel.DataAnnotations;

namespace ApiGMPKlik.Models.Entities.Address
{
    public class WilayahDusun : BaseEntity
    {
        public int KelurahanDesaId { get; set; }

        [StringLength(255)]
        public string Nama { get; set; } = string.Empty;

        public int SortOrder { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public virtual WilayahKelurahanDesa KelurahanDesa { get; set; } = null!;
        public virtual ICollection<WilayahRw> Rws { get; set; } = new List<WilayahRw>();
    }
}