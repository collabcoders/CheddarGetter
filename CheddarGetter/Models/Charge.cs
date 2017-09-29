using System;

namespace CheddarGetter.Models
{
    public class Charge
    {
        public Guid? ID { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public int Quantity { get; set; }
        public float EachAmount { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
