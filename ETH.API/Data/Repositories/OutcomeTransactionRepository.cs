using Dapper;
using ETH.API.Models.Tables;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ETH.API.Data.Repositories
{
    public class OutcomeTransactionRepository
    {
        private readonly IDbConnection _db;

        public OutcomeTransactionRepository(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("ExchangeConnection"));
        }

        public async Task<OutcomeTransactionTableModel> CreateOutcomeTransaction(OutcomeTransactionTableModel outcomeTransaction)
        {
            try
            {
                var p = new DynamicParameters();
                p.Add("id", outcomeTransaction.Id, dbType: DbType.Int64, direction: ParameterDirection.InputOutput);
                p.Add("fromWalletId", outcomeTransaction.FromWalletId);
                p.Add("toAddress", outcomeTransaction.ToAddress);
                p.Add("value", outcomeTransaction.Value);
                p.Add("platformCommission", outcomeTransaction.PlatformCommission);
                p.Add("fixedCommission", outcomeTransaction.FixedCommission);
                p.Add("blockchainCommission", outcomeTransaction.BlockchainCommission);
                p.Add("currencyAcronim", outcomeTransaction.CurrencyAcronim);
                p.Add("state", outcomeTransaction.State);

                await _db.ExecuteAsync("CreateOutcomeTransaction", p, commandType: CommandType.StoredProcedure);
                outcomeTransaction.Id = p.Get<long>("id");

                return outcomeTransaction;
            }
            catch (Exception ex) { return null; }
        }

        public async Task<OutcomeTransactionTableModel> GetOutcomeTransactionAsync(long id)
        {
            try
            {
                return (await _db.QueryFirstOrDefaultAsync<OutcomeTransactionTableModel>("GetOutcomeTransactionById",
                new
                {
                    id = id
                },
                commandType: CommandType.StoredProcedure
            ));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task UpdateStateTransactionAsync(OutcomeTransactionTableModel outcomeTransaction)
        {
            try
            {
                var p = new DynamicParameters();
                p.Add("id", outcomeTransaction.Id);
                p.Add("state", outcomeTransaction.State);
                p.Add("errorText", outcomeTransaction.ErrorText);

                await _db.QueryAsync("UpdateStateOutcomeTransaction", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving transaction, OutcomeTransactionId - {outcomeTransaction.Id}, {ex.Message}");
            }
        }
    }
}
