using AccountBalanceViewer.Application.DTOs;
using AccountBalanceViewer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountBalanceViewer.Application.Services.Contracts
{
    public interface IAccountBalanceRepository
    {
        Task<List<AccountBalance>> GetLatestBalancesAsync();
        Task<int> UploadBalancesAsync(List<AccountBalance> balances);
    }
}
