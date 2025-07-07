using AccountBalanceViewer.Application.DTOs;
using AccountBalanceViewer.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountBalanceViewer.Application.Services.Contracts
{
    public interface IAccountBalanceService
    {
        Task<FileUploadResponseDto> UploadBalancesAsync(IFormFile file);
        Task<List<AccountBalanceDto>> GetLatestBalancesAsync();
    }
}
