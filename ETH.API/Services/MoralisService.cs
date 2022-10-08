using ETH.API.Data.Repositories;
using Moralis.Web3Api;
using Moralis.Web3Api.Interfaces;
using Moralis.Web3Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transaction = Moralis.Web3Api.Models.Transaction;

namespace ETH.API.Services
{
    //https://docs.moralis.io/reference/getwallettransactions
    public class MoralisService
    {
        private TransactionETHRepository _transactionETHRepository;
        private IWeb3Api _apiClient;
        private string _moralisApiKey = "wR0Id2UZ9zA6jSQ4gUyxjW5weRUCOFIjQTmD8so8g30LYJez01oTbtTigdcwWPOa";

        public MoralisService(TransactionETHRepository transactionETHRepository)
        {
            _transactionETHRepository = transactionETHRepository;
            MoralisClient.Initialize(true, _moralisApiKey);
            _apiClient = MoralisClient.Web3Api;
        }


        public async Task<List<Transaction>> GetTransactions(string address)
        {
            try
            {
                var lastTransaction = await _transactionETHRepository.GetTransactionETHAsync(address);

                if (lastTransaction != null)
                {
                    TransactionCollection collection = await _apiClient.Account.GetTransactions(address.ToLower(), ChainList.eth,
                    fromDate: lastTransaction.BlockTimestamp);

                    var result = collection.Result.Where(x => x.Hash != lastTransaction.Hash).ToList();
                    return result;
                }
                else
                {
                    TransactionCollection collection = await _apiClient.Account.GetTransactions(address.ToLower(), ChainList.eth);
                    return collection.Result;
                }
            }
            catch (Exception exp)
            {
                //error
            }
            return new List<Transaction>();
        }

        public async Task<string> GetBalance(string address)
        {
            try
            {
                var balance = await _apiClient.Account.GetNativeBalance(address.Trim().ToLower(), ChainList.eth);
                return balance.Balance;
            }
            catch (Exception exp)
            {
                //error
            }
            return null;
        }


        public async Task<BlockTransaction> GetTransaction(string trHash)
        {
            try
            {
                BlockTransaction blockTransaction = await _apiClient.Native.GetTransaction(trHash, ChainList.eth);
                return blockTransaction;
            }
            catch (Exception exp)
            {
                //error
            }
            return new BlockTransaction();
        }

    }
}
