using System.ComponentModel.DataAnnotations;

namespace ApiGMPKlik.Models.Entities.Address
{
    public class WilayahKotaKab : BaseEntity
    {
        public int ProvinsiId { get; set; }

        [StringLength(4)]
        public string KodeKotaKabupaten { get; set; } = string.Empty;

        [StringLength(255)]
        public string Nama { get; set; } = string.Empty;

        [StringLength(20)]
        public string Jenis { get; set; } = string.Empty; // Kota or Kabupaten

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int SortOrder { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public virtual WilayahProvinsi Provinsi { get; set; } = null!;
        public virtual ICollection<WilayahKecamatan> Kecamatans { get; set; } = new List<WilayahKecamatan>();
    }
}