using Hospital.Models;
using Hospital.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;
using System.Text.Json;
using System.IO;
using System.Globalization;

namespace Hospital.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly HospitalBillDBContext _context;

        public InvoiceController(HospitalBillDBContext context)
        {
            _context = context;
        }
        public IActionResult Index(int? page)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var invoices = _context.Invoices
                .Where(i => i.Active == true)
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
                .ToPagedList(pageNumber, pageSize);

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
                .ToPagedList(pageNumber, pageSize);

            ViewBag.Patients = _context.Patients.ToList();
            ViewBag.Doctors = _context.staff.Where(d => d.RoleId == 1).ToList();
            ViewBag.Cashiers = _context.staff.Where(c => c.RoleId == 2).ToList();
            ViewBag.Pharmacists = _context.staff.Where(p => p.RoleId == 3).ToList();
            ViewBag.Diagnosis = _context.Diagnoses.ToList();
            ViewBag.Methods = _context.PaymentMethods.ToList();
            ViewBag.Status = _context.InvoiceStatuses.ToList();
            ViewBag.Items = _context.Items.ToList();
            ViewBag.InactiveInvoices = inactiveInvoices;
            return View(invoices);
        }

        [HttpPost]
        public IActionResult Save(Invoice invoice)
        {
            if (invoice.InvoiceId == 0)
            {
                // THÊM MỚI
                invoice.CreatedAt = DateTime.Now;
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

            return RedirectToAction("Index");
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
        public IActionResult Remove(int invoiceId)
        {
            var invoice = _context.Invoices.Find(invoiceId);
            if(invoice == null)
            {
                return NotFound();   
            }
            invoice.Active = false;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Restore(int invoiceId)
        {
            var invoice = _context.Invoices.Find(invoiceId);
            if (invoice == null)
            {
                return NotFound();
            }
            invoice.Active = true;
            _context.SaveChanges();
            return RedirectToAction("Index");
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

            return Content("✅ File PaymentMethod.json đã được tạo thành công.");
        }

    }
}
