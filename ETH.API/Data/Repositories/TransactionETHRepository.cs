using Dapper;
using Microsoft.Extensions.Configuration;
using Moralis.Web3Api.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ETH.API.Data.Repositories
{
    public class TransactionETHRepository
    {
        private readonly IDbConnection _db;

        public TransactionETHRepository(IConfiguration configuration)
        {
            _db = new SqlConnection(configuration.GetConnectionString("ETHConnection"));
        }

        public async Task<string> SaveTransactionETHAsync(Transaction transaction)
        {
            try
            {
                var operationResult = await _db.QueryFirstOrDefaultAsync<string>("InsertOrUpdateTransaction",
                    new
                    {
                        hash = transaction.Hash,
                        nonce = transaction.Nonce,
                        transactionIndex = transaction.TransactionIndex,
                        fromAddress = transaction.FromAddress,
                        toAddress = transaction.ToAddress,
                        value = transaction.Value,
                        gas = transaction.Gas,
                        gasPrice = transaction.GasPrice,
                        input = transaction.Input,
                        receiptCumulativeGasUsed = transaction.ReceiptCumulativeGasUsed,
                        receiptGasUsed = transaction.ReceiptGasUsed,
                        receiptContractAddress = transaction.ReceiptContractAddress,
                        receiptRoot = transaction.ReceiptRoot,
                        receiptStatus = transaction.ReceiptStatus,
                        blockTimestamp = transaction.BlockTimestamp,
                        blockNumber = transaction.BlockNumber,
                        blockHash = transaction.BlockHash,
                    },
                    commandType: CommandType.StoredProcedure);

                return operationResult;
            }
            catch (Exception ex) { return "ERROR"; }
        }

        public async Task<Transaction> GetTransactionETHAsync(string address)
        {
            try
            {
                var transaction = await _db.QueryFirstOrDefaultAsync<Transaction>("GetLastTransaction",
                    new
                    {
                        address = address,
                    },
                    commandType: CommandType.StoredProcedure);

                return transaction;
            }
            catch (Exception ex) { return null; }
        }
    }
}
