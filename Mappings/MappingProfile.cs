using ApiGMPKlik.DTOs;
using ApiGMPKlik.Models;
using ApiGMPKlik.Shared;
using AutoMapper;

namespace ApiGMPKlik.Mappings
{
    /// <summary>
    /// AutoMapper profile untuk mapping antara entities dan DTOs
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Branch mappings
            CreateMap<Branch, BranchDto>();
            CreateMap<CreateBranchDto, Branch>();
            CreateMap<UpdateBranchDto, Branch>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // UserProfile mappings
            CreateMap<UserProfile, UserProfileDto>();
            CreateMap<CreateUserProfileDto, UserProfile>();
            CreateMap<UpdateUserProfileDto, UserProfile>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // UserSecurity mappings
            CreateMap<UserSecurity, UserSecurityDto>();
            CreateMap<UpdateUserSecurityDto, UserSecurity>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ReferralTree mappings
            CreateMap<ReferralTree, ReferralTreeDto>();
            CreateMap<CreateReferralTreeDto, ReferralTree>();
            CreateMap<UpdateReferralTreeDto, ReferralTree>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // TreeNodeDto mappings
            CreateMap<ReferralTreeNodeDto, TreeNodeDto<ReferralTreeNodeData>>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => new ReferralTreeNodeData
                {
                    UserId = src.UserId,
                    UserName = src.UserName,
                    Email = src.Email,
                    FullName = src.FullName,
                    CommissionPercent = src.CommissionPercent,
                    JoinedAt = src.JoinedAt,
                    IsActive = src.IsActive,
                    DirectReferrals = src.DirectReferrals,
                    TotalDescendants = src.TotalDescendants
                }));
        }
    }
}
