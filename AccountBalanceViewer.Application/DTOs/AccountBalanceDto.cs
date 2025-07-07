using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountBalanceViewer.Application.DTOs
{
    public class AccountBalanceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Month { get; set; } = string.Empty;
        public string MonthNormalized { get; set; } = string.Empty;

    }
}
