using System;

namespace ETH.API.Models.Tables
{
    public class AccountsTableModel
    {
        public long Id { get; set; }
        public string Address { get; set; }
        public string Label { get; set; }
        public decimal Value { get; set; }
        public int State { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}
