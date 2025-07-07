
using AccountBalanceViewer.Application.DTOs;
using AccountBalanceViewer.Application.Services;
using AccountBalanceViewer.Application.Services.Contracts;
using AccountBalanceViewer.Domain.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OfficeOpenXml;
using System.Text;
using Xunit;

namespace AccountBalanceViewer.Test
{

    public class AccountBalanceServiceTests
    {
        private readonly Mock<IAccountBalanceRepository> _repoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<AccountBalanceService>> _loggerMock;
        private readonly AccountBalanceService _service;

        public AccountBalanceServiceTests()
        {
            _repoMock = new Mock<IAccountBalanceRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<AccountBalanceService>>();

            _service = new AccountBalanceService(_repoMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task UploadBalancesAsync_ValidTsvFile_ReturnsSuccess()
        {
            // Arrange
            var fileContent = new StringBuilder();
            fileContent.AppendLine("Account Balances for January");
            fileContent.AppendLine("Marketing\t10000");
            fileContent.AppendLine("R&D\t5000");

            var file = CreateMockFile("balances.tsv", fileContent.ToString());

            _mapperMock.Setup(m => m.Map<List<AccountBalance>>(It.IsAny<List<AccountBalanceDto>>()))
                .Returns(new List<AccountBalance> { new(), new() });

            _repoMock.Setup(r => r.UploadBalancesAsync(It.IsAny<List<AccountBalance>>()))
                .ReturnsAsync(2);

            // Act
            var result = await _service.UploadBalancesAsync(file);

            // Assert
            Assert.Equal("Success", result.Status);
            Assert.Equal(2, result.AffectedRows);
        }

        [Fact]
        public async Task UploadBalancesAsync_EmptyFile_ReturnsFailed()
        {
            var file = CreateMockFile("empty.tsv", "");

            var result = await _service.UploadBalancesAsync(file);

            Assert.Equal("Failed", result.Status);
            Assert.Equal(0, result.AffectedRows);
        }

        [Fact]
        public async Task UploadBalancesAsync_InvalidAmount_SkipsInvalidLines()
        {
            var content = new StringBuilder();
            content.AppendLine("Account Balances for February");
            content.AppendLine("IT\tabc"); // invalid amount
            content.AppendLine("Legal\t500");

            var file = CreateMockFile("invalid.tsv", content.ToString());

            _mapperMock.Setup(m => m.Map<List<AccountBalance>>(It.IsAny<List<AccountBalanceDto>>()))
                .Returns(new List<AccountBalance> { new() });

            _repoMock.Setup(r => r.UploadBalancesAsync(It.IsAny<List<AccountBalance>>()))
                .ReturnsAsync(1);

            var result = await _service.UploadBalancesAsync(file);

            Assert.Equal("Success", result.Status);
            Assert.Equal(1, result.AffectedRows);
        }

        [Fact]
        public async Task UploadBalancesAsync_InvalidMonth_ThrowsFormatException()
        {
            var content = new StringBuilder();
            content.AppendLine("Account Balances for Someday");
            content.AppendLine("Legal\t100");

            var file = CreateMockFile("badmonth.tsv", content.ToString());

            var ex = await Assert.ThrowsAsync<FormatException>(() => _service.UploadBalancesAsync(file));
            Assert.Contains("Unrecognized month format", ex.Message);
        }

        [Fact]
        public async Task UploadBalancesAsync_ValidExcelFile_ReturnsSuccess()
        {
            // Arrange
            var file = CreateMockExcelFile("January", new Dictionary<string, decimal>
            {
                { "Marketing", 10000 },
                { "R&D", 5000 }
            });

            _mapperMock.Setup(m => m.Map<List<AccountBalance>>(It.IsAny<List<AccountBalanceDto>>()))
                .Returns(new List<AccountBalance> { new(), new() });

            _repoMock.Setup(r => r.UploadBalancesAsync(It.IsAny<List<AccountBalance>>()))
                .ReturnsAsync(2);

            // Act
            var result = await _service.UploadBalancesAsync(file);

            // Assert
            Assert.Equal("Success", result.Status);
            Assert.Equal(2, result.AffectedRows);
        }


        private IFormFile CreateMockFile(string fileName, string content)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            return new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };
        }

        private IFormFile CreateMockExcelFile(string monthName, Dictionary<string, decimal> data)
        {
            var stream = new MemoryStream();

            using (var package = new ExcelPackage(stream))
            {
                 

                package.Workbook.Worksheets.Add("Sheet1");
                var worksheet = package.Workbook.Worksheets[0];

                // Row 1: header with month name
                worksheet.Cells[1, 1].Value = $"Account Balances for {monthName}";

                // Row 2+: account name and amount
                int row = 2;
                foreach (var entry in data)
                {
                    worksheet.Cells[row, 1].Value = entry.Key;
                    worksheet.Cells[row, 2].Value = entry.Value;
                    row++;
                }

                package.Save(); // Don't use SaveAs(stream) here
            }

            stream.Position = 0; // Reset stream to beginning

            return new FormFile(stream, 0, stream.Length, "file", "balances.xlsx")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
        }


    }

}

