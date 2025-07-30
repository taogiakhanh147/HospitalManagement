using System;
using System.Collections.Generic;

namespace Hospital.Models
{
    public partial class Diagnosis
    {
        public Diagnosis()
        {
            Invoices = new HashSet<Invoice>();
        }

        public int DiagnosisId { get; set; }
        public string? DiagnosisName { get; set; }

        public virtual ICollection<Invoice> Invoices { get; set; }
    }
}
