using System.ComponentModel.DataAnnotations;

namespace ApiGMPKlik.Models.Entities.Address
{
    public class WilayahKecamatan : BaseEntity
    {
        public int KotaKabupatenId { get; set; }

        [StringLength(7)]
        public string KodeKecamatan { get; set; } = string.Empty;

        [StringLength(255)]
        public string Nama { get; set; } = string.Empty;

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int SortOrder { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public virtual WilayahKotaKab KotaKab { get; set; } = null!;
        public virtual ICollection<WilayahKelurahanDesa> KelurahanDesas { get; set; } = new List<WilayahKelurahanDesa>();
    }
}