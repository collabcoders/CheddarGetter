using System;

namespace CheddarGetter.Models
{
    public class PlanItem
    {
        public Guid ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public double QuantityIncluded { get; set; }
        public bool IsPeriodic { get; set; }
        public float OverageAmount { get; set; }
        public DateTime CreatedDateTime { get; set; }

    }
}
