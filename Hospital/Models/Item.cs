using System;
using System.Collections.Generic;

namespace Hospital.Models
{
    public partial class Item
    {
        public Item()
        {
            InvoiceDetails = new HashSet<InvoiceDetail>();
        }

        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemType { get; set; }
        public decimal? UnitPrice { get; set; }

        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; }
    }
}
