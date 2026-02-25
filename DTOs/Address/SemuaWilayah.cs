using System.ComponentModel.DataAnnotations;

namespace ApiGMPKlik.DTOs.Address
{
    public class CreateProvinsiDto
    {
        [Required, StringLength(2, MinimumLength = 2)]
        [RegularExpression(@"^\d{2}$", ErrorMessage = "Kode harus 2 digit angka")]
        public string KodeProvinsi { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string Nama { get; set; } = string.Empty;

        [Range(-90, 90)]
        public decimal? Latitude { get; set; }

        [Range(-180, 180)]
        public decimal? Longitude { get; set; }

        [StringLength(50)]
        public string? Timezone { get; set; }

        public int SortOrder { get; set; }
        public string? Notes { get; set; }
    }

    // Update
    public class UpdateProvinsiDto : CreateProvinsiDto
    {
        [Required]
        public int Id { get; set; }
    }

    // Read
    public class ProvinsiReadDto
    {
        public int Id { get; set; }
        public string KodeProvinsi { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Timezone { get; set; }
        public int SortOrder { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // Dropdown
    public class ProvinsiDropdownDto
    {
        public int Id { get; set; }
        public string KodeProvinsi { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
    }

    // List
    public class ProvinsiListDto
    {
        public int Id { get; set; }
        public string KodeProvinsi { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int KotaKabCount { get; set; }
    }



    public class CreateKotaKabDto
    {
        [Required]
        public int ProvinsiId { get; set; }

        [Required, StringLength(4, MinimumLength = 4)]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Kode harus 4 digit angka")]
        public string KodeKotaKabupaten { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string Nama { get; set; } = string.Empty;

        [Required, StringLength(20)]
        [RegularExpression(@"^(Kota|Kabupaten)$", ErrorMessage = "Jenis harus Kota atau Kabupaten")]
        public string Jenis { get; set; } = string.Empty;

        [Range(-90, 90)]
        public decimal? Latitude { get; set; }

        [Range(-180, 180)]
        public decimal? Longitude { get; set; }

        public int SortOrder { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateKotaKabDto : CreateKotaKabDto
    {
        [Required]
        public int Id { get; set; }
    }

    public class KotaKabReadDto
    {
        public int Id { get; set; }
        public string KodeKotaKabupaten { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        public string Jenis { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int SortOrder { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }

        public int ProvinsiId { get; set; }
        public string ProvinsiNama { get; set; } = string.Empty;
        public string ProvinsiKode { get; set; } = string.Empty;
    }

    public class KotaKabDropdownDto
    {
        public int Id { get; set; }
        public string KodeKotaKabupaten { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        public string Jenis { get; set; } = string.Empty;
        public int ProvinsiId { get; set; }
    }

    public class KotaKabListDto
    {
        public int Id { get; set; }
        public string KodeKotaKabupaten { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        public string Jenis { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string ProvinsiNama { get; set; } = string.Empty;
        public int KecamatanCount { get; set; }
    }


    public class CreateKecamatanDto
    {
        [Required]
        public int KotaKabupatenId { get; set; }

        [Required, StringLength(7, MinimumLength = 7)]
        [RegularExpression(@"^\d{7}$", ErrorMessage = "Kode harus 7 digit angka")]
        public string KodeKecamatan { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string Nama { get; set; } = string.Empty;

        [Range(-90, 90)]
        public decimal? Latitude { get; set; }

        [Range(-180, 180)]
        public decimal? Longitude { get; set; }

        public int SortOrder { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateKecamatanDto : CreateKecamatanDto
    {
        [Required]
        public int Id { get; set; }
    }

    public class KecamatanReadDto
    {
        public int Id { get; set; }
        public string KodeKecamatan { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int SortOrder { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }

        public int KotaKabupatenId { get; set; }
        public string KotaKabNama { get; set; } = string.Empty;
        public string ProvinsiNama { get; set; } = string.Empty;
    }

    public class KecamatanDropdownDto
    {
        public int Id { get; set; }
        public string KodeKecamatan { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        public int KotaKabupatenId { get; set; }
    }

    public class KecamatanListDto
    {
        public int Id { get; set; }
        public string KodeKecamatan { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string KotaKabNama { get; set; } = string.Empty;
        public string ProvinsiNama { get; set; } = string.Empty;
        public int KelurahanDesaCount { get; set; }
    }

    public class CreateKelurahanDesaDto
    {
        [Required]
        public int KecamatanId { get; set; }

        [Required, StringLength(10, MinimumLength = 10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Kode harus 10 digit angka")]
        public string KodeKelurahanDesa { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string Nama { get; set; } = string.Empty;

        [Required, StringLength(20)]
        [RegularExpression(@"^(Desa|Kelurahan)$", ErrorMessage = "Jenis harus Desa atau Kelurahan")]
        public string Jenis { get; set; } = string.Empty;

        [StringLength(10)]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Kode Pos harus 5 digit")]
        public string? KodePos { get; set; }

        [Range(-90, 90)]
        public decimal? Latitude { get; set; }

        [Range(-180, 180)]
        public decimal? Longitude { get; set; }

        public int SortOrder { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateKelurahanDesaDto : CreateKelurahanDesaDto
    {
        [Required]
        public int Id { get; set; }
    }

    public class KelurahanDesaReadDto
    {
        public int Id { get; set; }
        public string KodeKelurahanDesa { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        public string Jenis { get; set; } = string.Empty;
        public string? KodePos { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int SortOrder { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }

        public int KecamatanId { get; set; }
        public string KecamatanNama { get; set; } = string.Empty;
        public string KotaKabNama { get; set; } = string.Empty;
        public string ProvinsiNama { get; set; } = string.Empty;
    }

    public class KelurahanDesaDropdownDto
    {
        public int Id { get; set; }
        public string KodeKelurahanDesa { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        public string Jenis { get; set; } = string.Empty;
        public int KecamatanId { get; set; }
    }

    public class KelurahanDesaListDto
    {
        public int Id { get; set; }
        public string KodeKelurahanDesa { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        public string Jenis { get; set; } = string.Empty;
        public string? KodePos { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string KecamatanNama { get; set; } = string.Empty;
        public string KotaKabNama { get; set; } = string.Empty;
        public int DusunCount { get; set; }
    }
    public class CreateDusunDto
    {
        [Required]
        public int KelurahanDesaId { get; set; }

        [Required, StringLength(255)]
        public string Nama { get; set; } = string.Empty;

        public int SortOrder { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateDusunDto : CreateDusunDto
    {
        [Required]
        public int Id { get; set; }
    }

    public class DusunReadDto
    {
        public int Id { get; set; }
        public string Nama { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }

        public int KelurahanDesaId { get; set; }
        public string KelurahanDesaNama { get; set; } = string.Empty;
        public string KecamatanNama { get; set; } = string.Empty;
    }

    public class DusunDropdownDto
    {
        public int Id { get; set; }
        public string Nama { get; set; } = string.Empty;
        public int KelurahanDesaId { get; set; }
    }

    public class DusunListDto
    {
        public int Id { get; set; }
        public string Nama { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string KelurahanDesaNama { get; set; } = string.Empty;
        public string KecamatanNama { get; set; } = string.Empty;
        public int RwCount { get; set; }
    }

    public class CreateRwDto
    {
        [Required]
        public int DusunId { get; set; }

        [Required, StringLength(10)]
        [RegularExpression(@"^\d{1,3}$", ErrorMessage = "RW harus angka 1-999")]
        public string Nama { get; set; } = string.Empty; // 001, 002, etc

        public int SortOrder { get; set; }
    }

    public class UpdateRwDto : CreateRwDto
    {
        [Required]
        public int Id { get; set; }
    }

    public class RwReadDto
    {
        public int Id { get; set; }
        public string Nama { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }

        public int DusunId { get; set; }
        public string DusunNama { get; set; } = string.Empty;
        public string KelurahanDesaNama { get; set; } = string.Empty;
    }

    public class RwDropdownDto
    {
        public int Id { get; set; }
        public string Nama { get; set; } = string.Empty;
        public int DusunId { get; set; }
    }

    public class RwListDto
    {
        public int Id { get; set; }
        public string Nama { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string DusunNama { get; set; } = string.Empty;
        public string KelurahanDesaNama { get; set; } = string.Empty;
        public int RtCount { get; set; }
    }

    public class CreateRtDto
    {
        [Required]
        public int RwId { get; set; }

        [Required, StringLength(10)]
        [RegularExpression(@"^\d{1,3}$", ErrorMessage = "RT harus angka 1-999")]
        public string Nama { get; set; } = string.Empty;

        public int SortOrder { get; set; }
    }

    public class UpdateRtDto : CreateRtDto
    {
        [Required]
        public int Id { get; set; }
    }

    public class RtReadDto
    {
        public int Id { get; set; }
        public string Nama { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }

        public int RwId { get; set; }
        public string RwNama { get; set; } = string.Empty;
        public string DusunNama { get; set; } = string.Empty;
        public string KelurahanDesaNama { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
    }

    public class RtDropdownDto
    {
        public int Id { get; set; }
        public string Nama { get; set; } = string.Empty;
        public int RwId { get; set; }
    }

    public class RtListDto
    {
        public int Id { get; set; }
        public string Nama { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string RwNama { get; set; } = string.Empty;
        public string DusunNama { get; set; } = string.Empty;
        public string KelurahanDesaNama { get; set; } = string.Empty;
    }

    public class AddressSearchRequestDto
    {
        public string Keyword { get; set; } = string.Empty;
        public int Limit { get; set; } = 7;
    }

    public class AddressSearchResultDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // Provinsi, Kota/Kab, Kecamatan, Kelurahan/Desa
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public int Level { get; set; } // 1=Provinsi, 2=KotaKab, 3=Kecamatan, 4=KelurahanDesa
    }
}
