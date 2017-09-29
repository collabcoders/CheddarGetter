using System;
using System.Collections.Generic;

namespace CheddarGetter.Models
{
    public class Subscription
    {
        public Guid ID { get; set; }
        public List<SubscriptionPlan> SubscriptionsPlans { get; set; }
        public string GatewayToken { get; set; }
        public string CCFirstName { get; set; }
        public string CCLastName { get; set; }
        public string CCZip { get; set; }
        public string CCType { get; set; }
        public int? CCLastFour { get; set; }
        public DateTime? CCExpirationDate { get; set; }
        public DateTime? CanceledDateTime { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public List<SubscriptionItem> SubscriptionItems { get; set; }
        public List<Invoice> Invoices { get; set; }
    }
}
