using AccountBalanceViewer.Application.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccountBalanceViewer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountBalanceController : ControllerBase
    {
        private readonly IAccountBalanceService _accountBalanceService;
        private readonly ILogger<AccountBalanceController> _logger;

        private readonly string[] _allowedExcelTypes = { ".xlsx" };
        private readonly string[] _allowedTextTypes = { ".tsv", ".txt" };

        public AccountBalanceController(IAccountBalanceService accountBalanceService, 
            ILogger<AccountBalanceController> logger)
        {
            _accountBalanceService = accountBalanceService;
            _logger = logger;
        }

        [HttpGet("latest")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetLatestAsync()
        {
            try
            {
                var balances = await _accountBalanceService.GetLatestBalancesAsync();
                return Ok(balances);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occured while fetching account balances from Db, Error : {ex.Message}", ex);
                return StatusCode(500, "An error occured while connecting to the Database. Contact Sys Admin");
            }
            
        }

        [HttpPost("upload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_allowedExcelTypes.Contains(extension) && !_allowedTextTypes.Contains(extension))
                return BadRequest("Invalid file type. Only .xlsx and .tsv/.txt are allowed.");

            try
            {
                var result = await _accountBalanceService.UploadBalancesAsync(file);
                return Ok(result);
            }
            catch(Exception ex)
            {
                _logger.LogError($"An error occured while uploading data to Db, Error : {ex.Message}", ex);
                return StatusCode(500, "An error occured while uploading data to DB. Contact Sys Admin");
            }
        }
    }
}
