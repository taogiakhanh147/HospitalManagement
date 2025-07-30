using System;
using System.Collections.Generic;

namespace Hospital.Models
{
    public partial class InvoiceDetail
    {
        public int InvoiceDetailId { get; set; }
        public int? InvoiceId { get; set; }
        public int? ItemId { get; set; }
        public int? Quantity { get; set; }

        public virtual Invoice? Invoice { get; set; }
        public virtual Item? Item { get; set; }
    }
}
