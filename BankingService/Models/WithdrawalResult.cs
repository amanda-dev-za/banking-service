namespace BankingService.Models
{
    public record WithdrawalResult(bool Success, string Message, string? EventId = null);
}