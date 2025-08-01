using X.PagedList;
namespace Hospital.Models.ViewModels
{
    public class InvoiceIndexViewModel
    {
        public InvoiceViewModel InvoiceForm { get; set; } = new InvoiceViewModel();
        public IPagedList<InvoiceViewModel> InvoiceList { get; set; }
    }
}
