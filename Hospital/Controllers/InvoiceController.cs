using Hospital.Models;
using Hospital.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace Hospital.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly HospitalBillDBContext _context;

        public InvoiceController(HospitalBillDBContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var invoices = _context.Invoices
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
                    Details = i.InvoiceDetails.Select(x => new InvoiceDetailViewModel
                    {
                        ItemId = x.ItemId ?? 0,
                        Quantity = x.Quantity ?? 0
                    }).ToList()
                }).ToList();
            ViewBag.Patients = _context.Patients.ToList();
            ViewBag.Doctors = _context.staff.Where(d => d.RoleId == 1).ToList();
            ViewBag.Cashiers = _context.staff.Where(c => c.RoleId == 2).ToList();
            ViewBag.Pharmacists = _context.staff.Where(p => p.RoleId == 3).ToList();
            ViewBag.Diagnosis = _context.Diagnoses.ToList();
            ViewBag.Methods = _context.PaymentMethods.ToList();
            ViewBag.Status = _context.InvoiceStatuses.ToList();
            ViewBag.Items = _context.Items.ToList();
            return View(invoices);
        }

        [HttpPost]
        public IActionResult Save(Invoice invoice, List<int> itemIds, List<int> quantities)
        {
            if (invoice.InvoiceId == 0)
            {
                invoice.CreatedAt = invoice.CreatedAt == default ? DateTime.Now : invoice.CreatedAt;
                _context.Invoices.Add(invoice);
                _context.SaveChanges();
            }
            else
            {
                invoice.UpdatedAt = DateTime.Now;
                _context.Invoices.Update(invoice);
                _context.SaveChanges();

                var oldDetails = _context.InvoiceDetails.Where(d => d.InvoiceId == invoice.InvoiceId);
                _context.InvoiceDetails.RemoveRange(oldDetails);
                _context.SaveChanges();
            }

            foreach(var detail in invoice.InvoiceDetails)
            {
                if (detail.ItemId > 0 && detail.Quantity > 0)
                {
                    detail.InvoiceId = invoice.InvoiceId;
                    _context.InvoiceDetails.Add(detail);
                }
            }
            _context.SaveChanges();
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
    }
}
