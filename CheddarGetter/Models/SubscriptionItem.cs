using System;

namespace CheddarGetter.Models
{
    public class SubscriptionItem
    {
        public Guid ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? ModifiedDateTime { get; set; }

    }
}
