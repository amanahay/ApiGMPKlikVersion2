using System.ComponentModel.DataAnnotations;

namespace ApiGMPKlik.Models.Entities.Address
{
    public class WilayahProvinsi : BaseEntity
    {
        [StringLength(2)]
        public string KodeProvinsi { get; set; } = string.Empty;

        [StringLength(255)]
        public string Nama { get; set; } = string.Empty;

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        [StringLength(50)]
        public string? Timezone { get; set; }

        public int SortOrder { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<WilayahKotaKab> KotaKabs { get; set; } = new List<WilayahKotaKab>();
    }
}