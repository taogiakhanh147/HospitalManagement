using System;
using System.Collections.Generic;

namespace Hospital.Models
{
    public partial class PaymentMethod
    {
        public PaymentMethod()
        {
            Invoices = new HashSet<Invoice>();
        }

        public int PaymentMethodId { get; set; }
        public string? PaymentMethodName { get; set; }

        public virtual ICollection<Invoice> Invoices { get; set; }
    }
}
