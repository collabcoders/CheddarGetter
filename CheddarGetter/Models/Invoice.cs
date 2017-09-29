using System;
using System.Collections.Generic;

namespace CheddarGetter.Models
{
    public class Invoice
    {
        public Guid ID { get; set; }
        public int Number { get; set; }
        public string Type { get; set; }
        public DateTime BillingDateTime { get; set; }
        public Guid? PaidTransactionId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public List<Charge> Charges { get; set; }
        public List<Transaction> Transactions { get; set; }

    }
}
