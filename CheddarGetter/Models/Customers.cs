using System.Collections.Generic;

namespace CheddarGetter.Models
{
    public class Customers
    {
        public List<Customer> CustomerList { get; set; }
        public List<CGError> ErrorList { get; set; }
    }
}
