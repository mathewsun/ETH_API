﻿using System;

namespace ETH.API.Models.Tables
{
    public class IncomeTransactionTableModel
    {
        public long Id { get; set; }
        public string CurrencyAcronim { get; set; }
        public string TransactionHash { get; set; }
        /// <summary>
        /// In ETH
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// In ETH
        /// </summary>
        public decimal TransactionFee { get; set; }
        public decimal? PlatformCommission { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public DateTime CreatedDate { get; set; }
        public double Date { get; set; }
        public string UserId { get; set; }
        public int WalletId { get; set; }
    }
}
