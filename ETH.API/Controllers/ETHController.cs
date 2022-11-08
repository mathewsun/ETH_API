using ETH.API.Data.Repositories;
using ETH.API.Models;
using ETH.API.Models.Enum;
using ETH.API.Models.Tables;
using Microsoft.AspNetCore.Mvc;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Web3.Accounts.Managed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

using System;
using Moralis.Web3Api.Interfaces;
using Moralis.Web3Api;
using Moralis.Web3Api.Models;
using Transaction = Moralis.Web3Api.Models.Transaction;
using System.Reflection.Emit;
using Microsoft.OpenApi.Extensions;
using ETH.API.Services;
using Nethereum.ABI;
using Nethereum.Contracts;
using System.Threading;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using Nethereum.BlockchainProcessing.BlockStorage.Entities.Mapping;
using ETH.API.Extensions;

namespace ETH.API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ETHController : ControllerBase
    {
        private OutcomeTransactionRepository _outcomeTransactionRepository;
        private AccountsRepository _accountRepository;
        private WalletRepository _walletRepository;
        private TransactionETHRepository _transactionETHRepository;
        private IncomeTransactionRepository _incomeTransactionRepository;
        private MoralisService _moralisService;


        public ETHController(OutcomeTransactionRepository outcomeTransactionRepository,
            AccountsRepository accountRepository,
            WalletRepository walletRepository,
            TransactionETHRepository transactionETHRepository,
            IncomeTransactionRepository incomeTransactionRepository,
            MoralisService moralisService)
        {
            _outcomeTransactionRepository = outcomeTransactionRepository;
            _accountRepository = accountRepository;
            _walletRepository = walletRepository;
            _transactionETHRepository = transactionETHRepository;
            _moralisService = moralisService;
            _incomeTransactionRepository = incomeTransactionRepository;
        }

        [HttpGet]
        public async Task<string> GetNewAddress(string label)
        {
            var web3 = new Web3("http://192.168.1.86:8545");
            var address = await web3.Personal.NewAccount.SendRequestAsync(label);

            await _accountRepository.CreateAccountAsync(new AccountsTableModel
            {
                Address = address,
                Label = label,
                Value = 0,
                State = (int)AccountState.Created
            });

            return address;
        }

        [HttpGet]
        public async Task<List<IncomeTransactionTableModel>> GetNewTransactions(string userId)
        {
            var result = new List<IncomeTransactionTableModel>();
            try
            {
                var incomeWallets = await _walletRepository.GetUserIncomeWalletsAsync(userId);
                foreach (var incomeWallet in incomeWallets)
                {
                    var newTransactions = await _moralisService.GetTransactions(incomeWallet.Address);
                    foreach (var transaction in newTransactions)
                    {
                        if (transaction.ToAddress.ToLower() == incomeWallet.Address.ToLower())
                        {
                            var operationResult = await _transactionETHRepository.SaveTransactionETHAsync(transaction);

                            var value = decimal.Parse(transaction.Value);

                            if (value > 0)
                            {
                                var wallet = await _walletRepository.GetUserWalletAsync(userId, "ETH");

                                decimal gasPrice = decimal.Parse(transaction.GasPrice);
                                decimal gas = decimal.Parse(transaction.Gas);
                                decimal fee = gasPrice * gas;

                                var incomeTransaction = new IncomeTransactionTableModel()
                                {
                                    CurrencyAcronim = "ETH",
                                    TransactionHash = transaction.Hash,
                                    Amount = value.WeiToEth(),
                                    TransactionFee = fee.WeiToEth(),
                                    PlatformCommission = null,
                                    FromAddress = transaction.FromAddress,
                                    ToAddress = transaction.ToAddress,
                                    CreatedDate = DateTime.Parse(transaction.BlockTimestamp),
                                    Date = 0,//todo
                                    UserId = userId,
                                    WalletId = wallet.Id,
                                };
                                result.Add(incomeTransaction);
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                    var balance = await _moralisService.GetBalance(incomeWallet.Address);
                    await _accountRepository.UpdateValueAccountAsync(incomeWallet.Address, balance);
                }
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        [HttpGet]
        public async Task<ExecuteTransactionModel> ExecuteTransaction(long outcomeTransactionId)
        {
            OutcomeTransactionTableModel tr = await _outcomeTransactionRepository.GetOutcomeTransactionAsync(outcomeTransactionId);
            try
            {
                AccountsTableModel account;
                if (tr.FixedCommission.HasValue)
                {
                    var resultValueToSearch = (tr.Value + tr.FixedCommission.Value).EthToWei();
                    account = await _accountRepository.GetAccountByValueAsync(resultValueToSearch);// берём адрес, учитывая комисию
                }
                else
                {
                    account = await _accountRepository.GetAccountByValueAsync(tr.Value);
                }

                if (account != null)
                {
                    var accountWeb3 = new ManagedAccount(account.Address.Trim(), account.Label.Trim());
                    var web3 = new Web3(accountWeb3, "http://192.168.1.86:8545");

                    var transaction = await web3.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(tr.ToAddress, tr.Value);
                    var trBlockchain = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(transaction.TransactionHash);
                    decimal commission = trBlockchain.Gas.ToLong() * trBlockchain.GasPrice.ToLong();

                    var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address);
                    account.Value = balance.ToLong();
                    await _accountRepository.UpdateValueAccountAsync(account.Address, account.Value.ToString());

                    return new ExecuteTransactionModel()
                    {
                        IsSuccess = true,
                        OutcomeTransactionState = (int)OutcomeTransactionStateEnum.Finished,
                        CommissionBlockchain = commission.WeiToEth(),
                        Value = tr.Value,// in eth
                        FromAddressBlockchain = account.Address,
                        ToAddressBlockchain = tr.ToAddress,
                        TransactionHash = transaction.TransactionHash,
                        ErrorText = null
                    };
                }
                else
                {
                    return new ExecuteTransactionModel()
                    {
                        IsSuccess = false,
                        OutcomeTransactionState = (int)OutcomeTransactionStateEnum.Error,
                        ErrorText = "Operation must be performed manually" // нету счёта с которого можно выветси 1 транзакцией
                    };
                }
            }
            catch (Exception ex)
            {
                return new ExecuteTransactionModel()
                {
                    IsSuccess = false,
                    OutcomeTransactionState = (int)OutcomeTransactionStateEnum.Error,
                    ErrorText = ex.Message
                };
            }
        }
    }
}
