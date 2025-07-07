using AccountBalanceViewer.Application.DTOs;
using AccountBalanceViewer.Application.Services.Contracts;
using AccountBalanceViewer.Domain.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System.Globalization;


namespace AccountBalanceViewer.Application.Services
{
    public class AccountBalanceService : IAccountBalanceService
    {
        private readonly IAccountBalanceRepository _accountBalanceRepository;
        private IMapper _mapper;
        private ILogger<AccountBalanceService> _logger;

        public AccountBalanceService(IAccountBalanceRepository accountBalanceRepository, 
            IMapper mapper, ILogger<AccountBalanceService> logger)
        {
            _accountBalanceRepository = accountBalanceRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<AccountBalanceDto>> GetLatestBalancesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching Account Balances from the DB Started.");
                var accountBalancesFromDb = await _accountBalanceRepository.GetLatestBalancesAsync();
                _logger.LogInformation("Fetching Account Balances from the DB Finished.");
                return _mapper.Map<List<AccountBalanceDto>>(accountBalancesFromDb);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occured while fetching account balances from Db, Error : {ex.Message}", ex);
                throw;
            }
        }
                
        public async Task<FileUploadResponseDto> UploadBalancesAsync(IFormFile file)
        {
            try
            {

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var entries = new List<AccountBalanceDto>();
                string monthName = "";
                string normalizedMonth = "";

                using var stream = file.OpenReadStream();

                if (extension == ".xlsx")
                {
                    ExcelPackage.License.SetNonCommercialPersonal("Krishantha Maduranga");
                    using var package = new ExcelPackage(stream);                    
                    var worksheet = package.Workbook.Worksheets[0];

                    // First line (cell A1) contains: "Account Balances for January"
                    string headerText = worksheet.Cells[1, 1].Text;
                    monthName = ExtractMonthFromHeader(headerText);
                    normalizedMonth = NormalizeMonth(monthName);

                    // Data starts from row 2
                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        string name = worksheet.Cells[row, 1].Text.Trim();
                        string amountStr = worksheet.Cells[row, 2].Text.Replace(",", "").Trim();

                        if (decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount))
                        {
                            entries.Add(new AccountBalanceDto
                            {
                                Name = name,
                                Amount = amount,
                                Month = monthName,
                                MonthNormalized = normalizedMonth
                            });
                        }
                    }
                }
                else // .tsv or .txt
                {
                    using var reader = new StreamReader(stream);
                    string? line;
                    bool isFirstLine = true;

                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (isFirstLine)
                        {
                            monthName = ExtractMonthFromHeader(line);
                            normalizedMonth = NormalizeMonth(monthName);
                            isFirstLine = false;
                            continue;
                        }

                        var parts = line.Split('\t');
                        if (parts.Length >= 2)
                        {
                            string name = parts[0].Trim();
                            string amountStr = parts[1].Replace(",", "").Trim();

                            if (decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount))
                            {
                                entries.Add(new AccountBalanceDto
                                {
                                    Name = name,
                                    Amount = amount,
                                    Month = monthName,
                                    MonthNormalized = normalizedMonth
                                });
                            }
                        }
                    }
                }

                if (!entries.Any())
                    return new FileUploadResponseDto 
                    { 
                        Status = "Failed",
                        AffectedRows = 0,
                        Message = "No valid data found."
                    };

                var newEntries = _mapper.Map<List<AccountBalance>>(entries);
                var rowsInserted = await _accountBalanceRepository.UploadBalancesAsync(newEntries);

                return new FileUploadResponseDto
                {
                    Status = "Success",
                    AffectedRows = rowsInserted,
                    Message = "Data uploaded successfully."
                };

            }
            catch(Exception ex)
            {
                _logger.LogError($"An error occured while uploading data to Db, Error : {ex.Message}", ex);
                throw;
            }
        }

        private string ExtractMonthFromHeader(string header)
        {
            // Expected format: "Account Balances for January"
            var parts = header.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Last(); // Assumes month is the last word
        }

        private string NormalizeMonth(string monthName)
        {
            var currentYear = DateTime.UtcNow.Year;

            if (DateTime.TryParseExact(monthName, "MMMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var monthDate))
            {
                return $"{currentYear}-{monthDate.Month:D2}";
            }

            throw new FormatException($"Unrecognized month format: {monthName}");
        }
    }
}
