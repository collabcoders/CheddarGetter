using System;
using System.Collections.Generic;

namespace CheddarGetter.Models
{
    public class SubscriptionPlan
    {
        public Guid ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int TrialDays { get; set; }
        public string BillingFrequency { get; set; }
        public string BillingFrequencyPer { get; set; }
        public string BillingFrequencyUnit { get; set; }
        public string BillingFrequencyQuantity { get; set; }
        public string SetupChargeCode { get; set; }
        public float SetupChargeAmount { get; set; }
        public string RecurringChargeCode { get; set; }
        public float RecurringChargeAmount { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public List<PlanItem> PlanItems { get; set; }
    }
}
