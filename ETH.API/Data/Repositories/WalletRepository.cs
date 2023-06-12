using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System;
using ETH.API.Models.Tables;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace ETH.API.Data.Repositories
{
    public class WalletRepository
    {
        private readonly IDbConnection _db;

        public WalletRepository(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("ExchangeConnection"));
        }

        public async Task<WalletTableModel> GetUserWalletAsync(string userId, string acronim)
        {
            try
            {
                WalletTableModel result = await _db.QueryFirstOrDefaultAsync<WalletTableModel>("GetUserWalletByAcronim",
                new
                {
                    userId = userId,
                    acronim = acronim
                },
                commandType: CommandType.StoredProcedure
            );

                return result;
            }
            catch (Exception ex) { return null; }
        }

        public async Task UpdateWalletBalanceAsync(int walletId, decimal newValue)
        {
            try
            {
                await _db.ExecuteAsync("UpdateWalletBalance",
                    new
                    {
                        walletId = walletId,
                        newWalletBalance = newValue
                    },
                    commandType: CommandType.StoredProcedure);

            }
            catch (Exception ex) { return; }
        }

        public async Task<List<IncomeWalletTableModel>> GetUserIncomeWalletsAsync(string userId)
        {
            try
            {
                List<IncomeWalletTableModel> result = (List<IncomeWalletTableModel>)(await _db.QueryAsync<IncomeWalletTableModel>("GetUserIncomeWallets",
                new { userId = userId },
                commandType: CommandType.StoredProcedure));

                return result;
            }
            catch (Exception ex) { return null; }
        }

        public async Task<WalletTableModel> CreateUserWalletAsync(WalletTableModel wallet)
        {
            var p = new DynamicParameters();
            p.Add("userId", wallet.UserId);
            p.Add("address", wallet.Address);
            p.Add("currencyAcronim", wallet.CurrencyAcronim);
            p.Add("value", wallet.Value);
            p.Add("new_identity", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await _db.QueryAsync<int>("CreateUserWallet", p, commandType: CommandType.StoredProcedure);

            wallet.Id = p.Get<int>("new_identity");

            return wallet;
        }
    }
}
