using System.Threading.Tasks;

namespace BankingService.Repositories
{
    public interface IAccountRepository
    {
        Task<bool> TryDeductBalanceAsync(long accountId, decimal amount);
    }
}