
namespace AccountBalanceViewer.Domain.Entities
{
    public class AccountBalance 
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Month { get; set; } = string.Empty; 
        public string MonthNormalized { get; set; } = string.Empty;
    }
}
