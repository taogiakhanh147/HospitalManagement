using System;
using System.Collections.Generic;

namespace Hospital.Models
{
    public partial class Invoice
    {
        public Invoice()
        {
            InvoiceDetails = new HashSet<InvoiceDetail>();
        }

        public int InvoiceId { get; set; }
        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }
        public int? CashierId { get; set; }
        public int? PharmacistId { get; set; }
        public int? StatusId { get; set; }
        public int? PaymentMethodId { get; set; }
        public int? DiagnosisId { get; set; }
        public string? Notes { get; set; }
        public decimal? TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? RestoredAt { get; set; }
        public bool? Active { get; set; }

        public virtual staff? Cashier { get; set; }
        public virtual Diagnosis? Diagnosis { get; set; }
        public virtual staff? Doctor { get; set; }
        public virtual Patient? Patient { get; set; }
        public virtual PaymentMethod? PaymentMethod { get; set; }
        public virtual staff? Pharmacist { get; set; }
        public virtual InvoiceStatus? Status { get; set; }
        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; }
    }
}
