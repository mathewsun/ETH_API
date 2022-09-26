namespace ETH.API.Models
{
    public class ExecuteTransactionModel
    {
        public string TransactionHash { get; set; }
        public string FromAddressBlockchain { get; set; }
        public string ToAddressBlockchain { get; set; }
        public decimal? Value { get; set; }
        public decimal? CommissionBlockchain { get; set; }
        public int OutcomeTransactionState { get; set; }
        public string ErrorText { get; set; }
        public bool IsSuccess { get; set; }
    }
}
