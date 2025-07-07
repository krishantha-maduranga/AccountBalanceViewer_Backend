using AccountBalanceViewer.Application.DTOs;
using AccountBalanceViewer.Domain.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountBalanceViewer.Application.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AccountBalance, AccountBalanceDto>().ReverseMap();
            
        }
    }
}
