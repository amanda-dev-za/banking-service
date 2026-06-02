using System;
using System.Threading.Tasks;
using BankingService.Messaging;
using BankingService.Models;
using BankingService.Repositories;
using BankingService.Services;
using Microsoft.Extensions.Logging;

namespace BankingService.Services
{
    public class WithdrawalService : IWithdrawalService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<WithdrawalService> _logger;

        public WithdrawalService(
            IAccountRepository accountRepository,
            IEventPublisher eventPublisher,
            ILogger<WithdrawalService> logger)
        {
            _accountRepository = accountRepository;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        public async Task<WithdrawalResult> WithdrawAsync(long accountId, decimal amount)
        {
            _logger.LogInformation(
                "Withdrawal initiated. AccountId={AccountId}, Amount={Amount}",
                accountId, amount);

            bool success = await _accountRepository.TryDeductBalanceAsync(accountId, amount);

            if (!success)
            {
                _logger.LogWarning(
                    "Withdrawal rejected. AccountId={AccountId}, Amount={Amount}",
                    accountId, amount);

                return new WithdrawalResult(false, "Insufficient funds or account not found.");
            }

            var withdrawalEvent = new WithdrawalEvent
            {
                AccountId = accountId,
                Amount = amount,
                Status = "SUCCESSFUL"
            };

            try
            {
                await _eventPublisher.PublishWithdrawalEventAsync(withdrawalEvent);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex,
                    "ALERT: Withdrawal succeeded but event publish failed. " +
                    "AccountId={AccountId}, EventId={EventId}",
                    accountId, withdrawalEvent.EventId);

                return new WithdrawalResult(
                    true,
                    "Withdrawal successful. Event notification delayed.",
                    withdrawalEvent.EventId.ToString());
            }

            _logger.LogInformation(
                "Withdrawal completed. AccountId={AccountId}, EventId={EventId}",
                accountId, withdrawalEvent.EventId);

            return new WithdrawalResult(
                true,
                "Withdrawal successful.",
                withdrawalEvent.EventId.ToString());
        }
    }
}