using AccountBalanceViewer.Application.Services.Contracts;
using AccountBalanceViewer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AccountBalanceViewer.Persistence.Repositories
{
    public class AccountBalanceRepository : IAccountBalanceRepository
    {
        private readonly AppDbContext _dbContext;
        public AccountBalanceRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<AccountBalance>> GetLatestBalancesAsync()
        {
            var latestMonthNormalized = await _dbContext.AccountBalances.MaxAsync(m => m.MonthNormalized);
            return await _dbContext.AccountBalances
                .Where(b => b.MonthNormalized == latestMonthNormalized)
                .ToListAsync();
        }

        public async Task<int> UploadBalancesAsync(List<AccountBalance> balances)
        {

            string monthNormalized = balances.First().MonthNormalized;

            var existingAccounts = await _dbContext.AccountBalances
                    .Where(a => a.MonthNormalized == monthNormalized)
                    .ToListAsync();

            if (existingAccounts.Any())
            {
                _dbContext.AccountBalances.RemoveRange(existingAccounts);
            }

            await _dbContext.AccountBalances.AddRangeAsync(balances);
            return  await _dbContext.SaveChangesAsync();
        }
    }
}
