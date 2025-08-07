namespace Hospital.Models.ViewModels
{
    public class InvoiceViewModel
    {
        public int InvoiceId { get; set; }

        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int CashierId { get; set; }
        public int PharmacistId { get; set; }
        public int DiagnosisId { get; set; }
        public int PaymentMethodId { get; set; }
        public int StatusId { get; set; }

        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string CashierName { get; set; }
        public string PharmacistName { get; set; }
        public string DiagnosisName { get; set; }
        public string PaymentMethodName { get; set; }
        public string StatusName { get; set; }

        public bool Active { get; set; }

        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<InvoiceDetailViewModel> Details { get; set; }
    }

}
