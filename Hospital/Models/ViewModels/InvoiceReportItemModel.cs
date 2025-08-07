namespace Hospital.Models.ViewModels
{
    public class InvoiceReportItemModel
    {
        public int InvoiceId { get; set; }
        public string PatientName { get; set; }
        public string CashierName { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
