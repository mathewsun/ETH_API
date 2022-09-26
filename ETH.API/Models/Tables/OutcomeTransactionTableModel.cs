using System;

namespace ETH.API.Models.Tables
{
    public class OutcomeTransactionTableModel
    {
        public long Id { get; set; }
        public int FromWalletId { get; set; }
        public string ToAddress { get; set; }
        public decimal Value { get; set; }
        public decimal? FixedCommission { get; set; }
        public decimal? BlockchainCommission { get; set; }
        public decimal? PlatformCommission { get; set; }
        public DateTime CreateDate { get; set; }
        public string CurrencyAcronim { get; set; }
        public int State { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public string ErrorText { get; set; }
    }
}
