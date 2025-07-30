using System;
using System.Collections.Generic;

namespace Hospital.Models
{
    public partial class Patient
    {
        public Patient()
        {
            Invoices = new HashSet<Invoice>();
        }

        public int PatientId { get; set; }
        public string? FullName { get; set; }
        public bool? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        public virtual ICollection<Invoice> Invoices { get; set; }
    }
}
