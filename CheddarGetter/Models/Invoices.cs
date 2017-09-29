using System.Collections.Generic;

namespace CheddarGetter.Models
{
    public class Invoices
    {
        public List<Invoice> InvoiceList { get; set; }
        public List<CGError> ErrorList { get; set; }
    }
}
