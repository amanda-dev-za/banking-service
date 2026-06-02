using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BankingService.Repositories;

namespace BankingService.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<AccountRepository> _logger;

        public AccountRepository(IConfiguration config, ILogger<AccountRepository> logger)
        {
            _connectionString = config.GetConnectionString("BankingDb");
            _logger = logger;
        }

        public async Task<bool> TryDeductBalanceAsync(long accountId, decimal amount)
        {
            const string sql = @"
                UPDATE accounts
                SET    balance = balance - @Amount
                WHERE  id      = @AccountId
                AND    balance >= @Amount";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync(
                IsolationLevel.ReadCommitted);

            try
            {
                int rowsAffected = await connection.ExecuteAsync(
                    sql,
                    new { Amount = amount, AccountId = accountId },
                    transaction: transaction);

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Balance deduction attempt. AccountId={AccountId}, Amount={Amount}, Success={Success}",
                    accountId, amount, rowsAffected > 0);

                return rowsAffected > 0;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}