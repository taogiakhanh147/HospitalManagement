using Hospital.Models;
using Hospital.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;
using System.Text.Json;
using System.Globalization;
using System.Diagnostics;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using DocumentFormat.OpenXml.Bibliography;

namespace Hospital.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly HospitalBillDBContext _context;

        public InvoiceController(HospitalBillDBContext context)
        {
            _context = context;
        }
        public IActionResult Index(int? page, int? pageSize)
        {
            var stopwatch = Stopwatch.StartNew();
            int defaultPageSize = 5;
            int currentPageSize = pageSize ?? defaultPageSize;
            int pageNumber = page ?? 1;

            // Đọc dữ liệu từ store procedure rồi phân trang
            var allInvoices = GetActiveInvoicesFromSTO();
            var pagedInvoices = allInvoices.ToPagedList(pageNumber, currentPageSize);

            // Tối ưu ViewBag - chỉ lấy dữ liệu cần thiết và dùng AsNoTracking
            ViewBag.Patients = _context.Patients
                .AsNoTracking()
                .Select(p => new { p.PatientId, p.FullName })
                .ToList();

            ViewBag.Doctors = _context.staff
                .AsNoTracking()
                .Where(d => d.RoleId == 1)
                .Select(d => new { d.StaffId, d.FullName })
                .ToList();

            ViewBag.Cashiers = _context.staff
                .AsNoTracking()
                .Where(c => c.RoleId == 2)
                .Select(c => new { c.StaffId, c.FullName })
                .ToList();

            ViewBag.Pharmacists = _context.staff
                .AsNoTracking()
                .Where(p => p.RoleId == 3)
                .Select(p => new { p.StaffId, p.FullName })
                .ToList();

            ViewBag.Diagnosis = _context.Diagnoses
                .AsNoTracking()
                .Select(d => new { d.DiagnosisId, d.DiagnosisName })
                .ToList();

            ViewBag.Methods = _context.PaymentMethods
                .AsNoTracking()
                .Select(m => new { m.PaymentMethodId, m.PaymentMethodName })
                .ToList();

            ViewBag.Status = _context.InvoiceStatuses
                .AsNoTracking()
                .Select(s => new { s.StatusId, s.StatusName })
                .ToList();

            ViewBag.Items = _context.Items
                .AsNoTracking()
                .Select(i => new { i.ItemId, i.ItemName })
                .ToList();

            /*ViewBag.InactiveInvoices = inactiveInvoices;*/
            ViewBag.PageSize = currentPageSize;

            stopwatch.Stop();
            Console.WriteLine($"[Performance] Index loaded in {stopwatch.ElapsedMilliseconds} ms");
            return View(pagedInvoices);
        }

        public IActionResult PartialInvoiceTable(int? page, int? pageSize, int? patientId)
        {
            var sw = Stopwatch.StartNew();

            int defaultPageSize = 5;
            int currentPageSize = pageSize ?? defaultPageSize;
            int pageNumber = page ?? 1;

            IEnumerable<InvoiceViewModel> query = GetActiveInvoicesFromSTO();

            if (patientId.HasValue)
            {
                query = query.Where(i => i.PatientId == patientId.Value);
            }

            var invoices = query.ToPagedList(pageNumber, currentPageSize);

            sw.Stop();
            System.Diagnostics.Debug.WriteLine($"[PartialInvoiceTable] Render time: {sw.ElapsedMilliseconds} ms");

            ViewBag.PageSize = currentPageSize;
            ViewBag.PatientId = patientId;

            return PartialView("_PartialInvoiceTable", invoices);
        }

        [HttpPost]
        public IActionResult Save(Invoice invoice)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                if (invoice.InvoiceId == 0)
                {
                    // THÊM MỚI
                    if (!string.IsNullOrWhiteSpace(Request.Form["CreatedAt"]))
                    {
                        if (DateTime.TryParseExact(Request.Form["CreatedAt"], "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                        {
                            invoice.CreatedAt = parsedDate;
                        }
                    }
                    else
                    {
                        invoice.CreatedAt = DateTime.Now;
                    }

                    invoice.Active = true;

                    if (invoice.InvoiceDetails != null)
                    {
                        foreach (var detail in invoice.InvoiceDetails)
                        {
                            detail.InvoiceDetailId = 0; // Để tránh lỗi thêm chi tiết hóa đơn bị lặp nhân đôi
                        }
                    }

                    _context.Invoices.Add(invoice);
                    _context.SaveChanges(); // Tự thêm luôn InvoiceDetails nếu quan hệ đúng
                }
                else
                {
                    // CẬP NHẬT
                    invoice.UpdatedAt = DateTime.Now;

                    var existingInvoice = _context.Invoices
                        .Include(i => i.InvoiceDetails)
                        .FirstOrDefault(i => i.InvoiceId == invoice.InvoiceId);

                    if (existingInvoice == null)
                        return NotFound();

                    _context.Entry(existingInvoice).CurrentValues.SetValues(invoice);
                    existingInvoice.Active = true;
                    _context.Entry(existingInvoice).Property(i => i.CreatedAt).IsModified = false;

                    // Xóa chi tiết hóa đơn cũ
                    _context.InvoiceDetails.RemoveRange(existingInvoice.InvoiceDetails);

                    // Thêm chi tiết hóa đơn mới
                    if (invoice.InvoiceDetails != null)
                    {
                        foreach (var detail in invoice.InvoiceDetails)
                        {
                            if (detail.ItemId > 0 && detail.Quantity > 0)
                            {
                                detail.InvoiceDetailId = 0; // Để tránh lỗi IDENTITY INSERT
                                detail.InvoiceId = invoice.InvoiceId;
                                _context.InvoiceDetails.Add(detail);
                            }
                        }
                    }

                    _context.SaveChanges();
                }

                transaction.Commit();
                TempData["SuccessMessage"] = invoice.InvoiceId == 0 ? "Thêm hóa đơn thành công." : "Cập nhật hóa đơn thành công.";
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lưu dữ liệu.";
                return RedirectToAction("Index");
            }  
        }


        [HttpGet]
        public IActionResult GetInvoiceDetails(int invoiceId)
        {
            var invoiceDetail = _context.InvoiceDetails
                .Where(x => x.InvoiceId == invoiceId)
                .Select(x => new
                {
                    x.InvoiceDetailId,
                    x.ItemId,
                    x.Quantity
                })
                .ToList();
            return Json(invoiceDetail);
        }

        [HttpPost]
        public IActionResult Remove(int invoiceId, int? page, int? pageSize)
        {
            var invoice = _context.Invoices.Find(invoiceId);
            if(invoice == null)
            {
                return NotFound();   
            }
            invoice.Active = false;
            _context.SaveChanges();

            int defaultPageSize = 5;
            int currentPageSize = pageSize ?? defaultPageSize;
            int pageNumber = page ?? 1;

            var invoices = GetActiveInvoicesFromSTO();
            var pagedInvoices = invoices.ToPagedList(pageNumber, currentPageSize);

            ViewBag.PageSize = currentPageSize;

            return PartialView("_PartialInvoiceTable", invoices);
        }

        [HttpPost]
        public IActionResult Restore(int invoiceId, int? page, int? pageSize)
        {
            int defaultPageSize = 5;
            int currentPageSize = pageSize ?? defaultPageSize;
            int pageNumber = page ?? 1;

            var invoice = _context.Invoices.Find(invoiceId);
            if (invoice == null)
            {
                return NotFound();
            }
            invoice.Active = true;
            _context.SaveChanges();

            var inactiveInvoices = _context.Invoices
                .Where(i => i.Active == false)
                .Include(i => i.Patient)
                .Include(i => i.Doctor)
                .Include(i => i.Cashier)
                .Include(i => i.Pharmacist)
                .Include(i => i.Diagnosis)
                .Include(i => i.PaymentMethod)
                .Include(i => i.Status)
                .Select(i => new InvoiceViewModel
                {
                    InvoiceId = i.InvoiceId,
                    PatientId = i.Patient.PatientId,
                    DoctorId = i.Doctor.StaffId,
                    CashierId = i.Cashier.StaffId,
                    PharmacistId = i.Pharmacist.StaffId,
                    DiagnosisId = i.Diagnosis.DiagnosisId,
                    PaymentMethodId = i.PaymentMethod.PaymentMethodId,
                    StatusId = i.Status.StatusId,
                    PatientName = i.Patient.FullName,
                    DoctorName = i.Doctor.FullName,
                    CashierName = i.Cashier.FullName,
                    PharmacistName = i.Pharmacist.FullName,
                    DiagnosisName = i.Diagnosis.DiagnosisName,
                    Notes = i.Notes,
                    PaymentMethodName = i.PaymentMethod.PaymentMethodName,
                    TotalAmount = i.TotalAmount ?? 0,
                    StatusName = i.Status.StatusName,
                    CreatedAt = i.CreatedAt,
                    Active = i.Active ?? true,
                    Details = i.InvoiceDetails.Select(x => new InvoiceDetailViewModel
                    {
                        ItemId = x.ItemId ?? 0,
                        Quantity = x.Quantity ?? 0
                    }).ToList()
                })
                .OrderByDescending(i => i.CreatedAt)
                .ToPagedList(pageNumber, currentPageSize);

            ViewBag.InactiveInvoices = inactiveInvoices;
            ViewBag.PageSize = currentPageSize;
            return PartialView("_PartialViewInactiveTable", inactiveInvoices);
        }

        [HttpGet]
        public IActionResult GeneratePaymentMethodJson()
        {
            var paymentMethods = new List<PaymentMethod>
            {
                new PaymentMethod { PaymentMethodId = 3, PaymentMethodName = "Tiền mặt" },
                new PaymentMethod { PaymentMethodId = 4, PaymentMethodName = "Chuyển khoản" },
                new PaymentMethod { PaymentMethodId = 5, PaymentMethodName = "Thẻ BHYT" }
            };

            var json = JsonSerializer.Serialize(paymentMethods, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "PaymentMethod.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            System.IO.File.WriteAllText(path, json);

            return Content("File PaymentMethod.json đã được tạo thành công.");
        }

        public IActionResult LoadInactiveInvoices(int? page, int? pageSize)
        {
            int defaultPageSize = 5;
            int currentPageSize = pageSize ?? defaultPageSize;
            int pageNumber = page ?? 1;

            var invoices = GetInactiveInvoicesFromSTO();
            var pagedInvoices = invoices.ToPagedList(pageNumber, currentPageSize);

            return PartialView("_PartialViewInactiveTable", pagedInvoices);
        }

        private List<InvoiceViewModel> GetActiveInvoicesFromSTO()
        {
            return _context.ActiveInvoiceReports
                .FromSqlRaw("EXEC GetActiveInvoices")
                .AsNoTracking()
                .ToList();
        }

        private List<InvoiceViewModel> GetInactiveInvoicesFromSTO()
        {
            return _context.ActiveInvoiceReports
                .FromSqlRaw("EXEC GetInactiveInvoices")
                .AsNoTracking()
                .ToList();
        }

        public IActionResult ExportExcel()
        {
            var invoices = GetActiveInvoicesFromSTO();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Hóa đơn");

            // Header
            worksheet.Cell(1, 1).Value = "Mã HD";
            worksheet.Cell(1, 2).Value = "Bệnh nhân";
            worksheet.Cell(1, 3).Value = "Thu ngân";
            worksheet.Cell(1, 4).Value = "Chẩn đoán";
            worksheet.Cell(1, 5).Value = "Tổng tiền";
            worksheet.Cell(1, 6).Value = "Ngày tạo";

            // Dữ liệu
            int row = 2;
            foreach (var i in invoices)
            {
                worksheet.Cell(row, 1).Value = i.InvoiceId;
                worksheet.Cell(row, 2).Value = i.PatientName;
                worksheet.Cell(row, 3).Value = i.CashierName;
                worksheet.Cell(row, 4).Value = i.DiagnosisName;
                worksheet.Cell(row, 5).Value = i.TotalAmount;
                worksheet.Cell(row, 6).Value = i.CreatedAt.ToString("dd/MM/yyyy");
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "HoaDon.xlsx");
        }

        public IActionResult ExportInvoiceReportPdf()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var invoices = _context.Invoices
                .Where(i => i.Active == true)
                .Select(i => new InvoiceReportItemModel
                {
                    InvoiceId = i.InvoiceId,
                    PatientName = i.Patient.FullName,
                    CashierName = i.Cashier.FullName,
                    CreatedAt = i.CreatedAt,
                    TotalAmount = i.TotalAmount ?? 0
                }).ToList();

            var document = new InvoiceReportDocument(invoices);

            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/pdf", "BaoCaoHoaDon.pdf");
        }
    }
}
