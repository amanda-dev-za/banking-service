using System.Threading.Tasks;
using BankingService.Models;

namespace BankingService.Services
{
    public interface IWithdrawalService
    {
        Task<WithdrawalResult> WithdrawAsync(long accountId, decimal amount);
    }
}