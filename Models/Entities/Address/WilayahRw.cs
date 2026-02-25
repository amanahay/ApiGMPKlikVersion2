using System.ComponentModel.DataAnnotations;

namespace ApiGMPKlik.Models.Entities.Address
{
    public class WilayahRw : BaseEntity
    {
        public int DusunId { get; set; }

        [StringLength(10)]
        public string Nama { get; set; } = string.Empty; // Numeric 001-999

        public int SortOrder { get; set; }

        // Navigation
        public virtual WilayahDusun Dusun { get; set; } = null!;
        public virtual ICollection<WilayahRt> Rts { get; set; } = new List<WilayahRt>();
    }
}