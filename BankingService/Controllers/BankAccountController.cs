using System;
using System.Threading.Tasks;
using BankingService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BankingService.Controllers
{
    [ApiController]
    [Route("bank")]
    public class BankAccountController : ControllerBase
    {
        private readonly IWithdrawalService _withdrawalService;
        private readonly ILogger<BankAccountController> _logger;

        public BankAccountController(
            IWithdrawalService withdrawalService,
            ILogger<BankAccountController> logger)
        {
            _withdrawalService = withdrawalService;
            _logger = logger;
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw(
            [FromQuery] long accountId,
            [FromQuery] decimal amount)
        {
            if (accountId <= 0)
                return BadRequest(new { error = "Invalid account ID." });

            if (amount <= 0)
                return BadRequest(new { error = "Amount must be greater than zero." });

            if (decimal.Round(amount, 2) != amount)
                return BadRequest(new { error = "Amount cannot have more than 2 decimal places." });

            try
            {
                var result = await _withdrawalService.WithdrawAsync(accountId, amount);

                if (!result.Success)
                    return UnprocessableEntity(new { error = result.Message });

                return Ok(new { message = result.Message, eventId = result.EventId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception in Withdraw. AccountId={AccountId}", accountId);

                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }
    }
}