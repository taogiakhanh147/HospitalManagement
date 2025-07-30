using System;
using System.Collections.Generic;

namespace Hospital.Models
{
    public partial class staff
    {
        public staff()
        {
            InvoiceCashiers = new HashSet<Invoice>();
            InvoiceDoctors = new HashSet<Invoice>();
            InvoicePharmacists = new HashSet<Invoice>();
        }

        public int StaffId { get; set; }
        public int? RoleId { get; set; }
        public string? FullName { get; set; }
        public bool? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        public virtual Role? Role { get; set; }
        public virtual ICollection<Invoice> InvoiceCashiers { get; set; }
        public virtual ICollection<Invoice> InvoiceDoctors { get; set; }
        public virtual ICollection<Invoice> InvoicePharmacists { get; set; }
    }
}
