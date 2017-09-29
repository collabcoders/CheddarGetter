using System;
using System.Collections.Generic;

namespace CheddarGetter.Models
{
    public class Customer
    {
        public Guid ID { get; set; }
        public string Code { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string Notes { get; set; }
        public string Email { get; set; }
        public string GatewayToken { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime ModifiedDateTime { get; set; }
        public List<Subscription> Subscriptions { get; set; }

        public string AdditionalMetaData { get; set; }
    }
}
