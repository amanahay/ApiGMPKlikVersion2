using ApiGMPKlik.DTOs.DataPrice;
using ApiGMPKlik.Models.DataPrice;
using AutoMapper;

namespace ApiGMPKlik.Mappings
{
    public class DataPriceRangeMappingProfile : Profile
    {
        public DataPriceRangeMappingProfile()
        {
            // Entity -> Response DTO
            CreateMap<DataPriceRange, DataPriceRangeResponseDto>();

            // Create DTO -> Entity
            CreateMap<CreateDataPriceRangeDto, DataPriceRange>();

            // Update DTO -> Entity (ReverseMap untuk two-way mapping jika diperlukan)
            CreateMap<UpdateDataPriceRangeDto, DataPriceRange>();

            // Entity -> Dropdown DTO (optimasi manual jika perlu)
            CreateMap<DataPriceRange, DataPriceRangeDropdownDto>();
        }
    }
}