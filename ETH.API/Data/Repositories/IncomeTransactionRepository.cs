using Dapper;
using ETH.API.Models.Tables;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ETH.API.Data.Repositories
{
    public class IncomeTransactionRepository
    {
        private readonly IDbConnection _db;

        public IncomeTransactionRepository(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("ExchangeConnection"));
        }

        public async Task<IncomeTransactionTableModel> CreateIncomeTransactionAsync(
            IncomeTransactionTableModel incomeTransaction)
        {
            try
            {
                var p = new DynamicParameters();
                p.Add("currencyAcronim", incomeTransaction.CurrencyAcronim);
                p.Add("transactionId", incomeTransaction.TransactionId);
                p.Add("amount", incomeTransaction.Amount);
                p.Add("platformCommission", incomeTransaction.PlatformCommission);
                p.Add("transactionFee", incomeTransaction.TransactionFee);
                p.Add("toAddress", incomeTransaction.ToAddress);
                p.Add("date", incomeTransaction.Date);
                p.Add("userId", incomeTransaction.UserId);
                p.Add("walletId", incomeTransaction.WalletId);
                p.Add("new_identity", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await _db.QueryAsync<int>("CreateIncomeTransaction", p, commandType: CommandType.StoredProcedure);

                incomeTransaction.Id = p.Get<int>("new_identity");

                return incomeTransaction;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
