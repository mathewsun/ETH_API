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
        private RequestService _requestService;

        public ETHController(OutcomeTransactionRepository outcomeTransactionRepository,
            AccountsRepository accountRepository,
            WalletRepository walletRepository,
            TransactionETHRepository transactionETHRepository,
            IncomeTransactionRepository incomeTransactionRepository,
            MoralisService moralisService,
            RequestService requestService)
        {
            _outcomeTransactionRepository = outcomeTransactionRepository;
            _accountRepository = accountRepository;
            _walletRepository = walletRepository;
            _transactionETHRepository = transactionETHRepository;
            _moralisService = moralisService;
            _incomeTransactionRepository = incomeTransactionRepository;
            _requestService = requestService;
        }

        [HttpGet]
        public async Task<string> GetNewAddress(string label)
        {
            return await _requestService.GetNewAddress(label);
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

                            decimal value = decimal.Parse(transaction.Value);

                            value = value / 1000000000000000000;

                            if (value > 0)
                            {
                                var wallet = await _walletRepository.GetUserWalletAsync(userId, "ETH");

                                if (wallet == null)
                                {
                                    wallet = await _walletRepository.CreateUserWalletAsync(new WalletTableModel()
                                    {
                                        UserId = userId,
                                        CurrencyAcronim = "ETH",
                                        Value = 0,
                                        Address = await _requestService.GetNewAddress(userId)
                                    });
                                }

                                decimal gasPrice = decimal.Parse(transaction.GasPrice);
                                decimal gas = decimal.Parse(transaction.Gas);
                                decimal fee = gasPrice * gas;
                                fee = fee / 1000000000000000000;

                                var incomeTransaction = new IncomeTransactionTableModel()
                                {
                                    CurrencyAcronim = "ETH",
                                    TransactionHash = transaction.Hash,
                                    Amount = value,
                                    TransactionFee = fee,
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
                    var moralisBalance = await _moralisService.GetBalance(incomeWallet.Address);
                    decimal realBalance = Convert.ToDecimal(moralisBalance);
                    realBalance = realBalance / 1000000000000000000;

                    await _accountRepository.UpdateValueAccountAsync(incomeWallet.Address, realBalance);
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

                    //var transaction = await web3.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(tr.ToAddress, tr.Value);

                    var transactionHash = await web3.Personal.SignAndSendTransaction.SendRequestAsync(new TransactionInput()
                    {
                        From = account.Address,
                        To = tr.ToAddress,
                        GasPrice = new HexBigInteger(new BigInteger(20000000000)),
                        Gas = new HexBigInteger(new BigInteger(21000)),
                        Value = new HexBigInteger(new BigInteger(tr.Value.EthToWei())),
                        Data = ""
                    },
                    account.Label);


                    var trBlockchain = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(transactionHash);
                    //var trBlockchain = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(transaction.TransactionHash);
                    decimal commission = trBlockchain.Gas.ToLong() * trBlockchain.GasPrice.ToLong();

                    var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address);
                    account.Value = balance.ToLong();
                    await _accountRepository.UpdateValueAccountAsync(account.Address, account.Value);

                    return new ExecuteTransactionModel()
                    {
                        IsSuccess = true,
                        OutcomeTransactionState = (int)OutcomeTransactionStateEnum.Finished,
                        CommissionBlockchain = commission.WeiToEth(),
                        Value = tr.Value,// in eth
                        FromAddressBlockchain = account.Address,
                        ToAddressBlockchain = tr.ToAddress,
                        TransactionHash = transactionHash,
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
