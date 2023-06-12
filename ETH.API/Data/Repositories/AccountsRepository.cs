using Dapper;
using ETH.API.Models.Tables;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ETH.API.Data.Repositories
{
    public class AccountsRepository
    {
        private readonly IDbConnection _db;

        public AccountsRepository(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("ETHConnection"));
        }

        public async Task<AccountsTableModel> GetAccountAsync(string address)
        {
            try
            {
                return (await _db.QueryFirstOrDefaultAsync<AccountsTableModel>("GetAccount",
                new
                {
                    address = address
                },
                commandType: CommandType.StoredProcedure
            ));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<AccountsTableModel> GetAccountByValueAsync(decimal value)
        {
            try
            {
                return (await _db.QueryFirstOrDefaultAsync<AccountsTableModel>("GetAccountByValue",
                new
                {
                    value = value
                },
                commandType: CommandType.StoredProcedure
            ));
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task CreateAccountAsync(AccountsTableModel address)
        {
            try
            {
                var p = new DynamicParameters();
                p.Add("address", address.Address);
                p.Add("label", address.Label);
                p.Add("value", address.Value);
                p.Add("state", address.State);
                p.Add("new_identity", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await _db.QueryAsync("CreateAccount", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex) { }
        }

        public async Task UpdateValueAccountAsync(string address, decimal value)
        {
            try
            {
                var p = new DynamicParameters();
                p.Add("address", address);
                p.Add("value", value);

                await _db.QueryAsync("UpdateValueAccount", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex) { }
        }
    }
}
