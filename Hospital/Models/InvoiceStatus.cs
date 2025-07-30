using System;
using System.Collections.Generic;

namespace Hospital.Models
{
    public partial class InvoiceStatus
    {
        public InvoiceStatus()
        {
            Invoices = new HashSet<Invoice>();
        }

        public int StatusId { get; set; }
        public string? StatusName { get; set; }

        public virtual ICollection<Invoice> Invoices { get; set; }
    }
}
