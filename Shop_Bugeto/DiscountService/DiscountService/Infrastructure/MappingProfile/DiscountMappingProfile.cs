using AutoMapper;
using DiscountService.Model.Entities;
using DiscountService.Model.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscountService.Infrastructure.MappingProfile
{
    public class DiscountMappingProfile:Profile
    {
        public DiscountMappingProfile()
        {
            CreateMap<DiscountCode, DiscountDto>().ReverseMap();

        }
    }
}
