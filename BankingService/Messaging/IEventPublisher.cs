using System.Threading.Tasks;
using BankingService.Models;

namespace BankingService.Messaging
{
    public interface IEventPublisher
    {
        Task PublishWithdrawalEventAsync(WithdrawalEvent withdrawalEvent);
    }
}