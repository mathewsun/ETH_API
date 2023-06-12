using ETH.API.Data.Repositories;
using ETH.API.Models.Enum;
using ETH.API.Models.Tables;
using Nethereum.Web3;
using System.Threading.Tasks;

namespace ETH.API.Services
{
    public class RequestService
    {
        private OutcomeTransactionRepository _outcomeTransactionRepository;
        private AccountsRepository _accountRepository;
        private WalletRepository _walletRepository;
        private TransactionETHRepository _transactionETHRepository;
        private IncomeTransactionRepository _incomeTransactionRepository;
        private MoralisService _moralisService;

        public RequestService(OutcomeTransactionRepository outcomeTransactionRepository,
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
    }
}
