using System;

namespace ETH.API.Models.Tables
{
    /// <summary>
    /// Кошелёк внутренних переводов только внутри нашей системы. В блокчейне не создаётся.
    /// </summary>
    public class WalletTableModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal Value { get; set; }
        public string CurrencyAcronim { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUpdate { get; set; }
        public string Address { get; set; }
    }
}
