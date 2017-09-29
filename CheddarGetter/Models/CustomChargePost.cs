namespace CheddarGetter.Models
{
    public class CustomChargePost
    {
        public string CustomerCode { get; set; }
        public string ItemCode { get; set; }
        public string ChargeCode { get; set; }
        public int Quantity { get; set; }
        public int EachAmount { get; set; }
        public string Description { get; set; }
    }
}
