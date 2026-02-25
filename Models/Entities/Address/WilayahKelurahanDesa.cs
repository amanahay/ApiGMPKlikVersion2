using System.ComponentModel.DataAnnotations;

namespace ApiGMPKlik.Models.Entities.Address
{
    public class WilayahKelurahanDesa : BaseEntity
    {
        public int KecamatanId { get; set; }

        [StringLength(10)]
        public string KodeKelurahanDesa { get; set; } = string.Empty;

        [StringLength(255)]
        public string Nama { get; set; } = string.Empty;

        [StringLength(20)]
        public string Jenis { get; set; } = string.Empty; // Kelurahan or Desa

        [StringLength(10)]
        public string? KodePos { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int SortOrder { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public virtual WilayahKecamatan Kecamatan { get; set; } = null!;
        public virtual ICollection<WilayahDusun> Dusuns { get; set; } = new List<WilayahDusun>();
    }
}