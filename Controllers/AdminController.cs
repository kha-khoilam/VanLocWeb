using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VanLocWeb.Models;

namespace VanLocWeb.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly VanLocWeb.Services.DataService _dataService;
        private readonly VanLocWeb.Services.PdfService _pdfService;

        public AdminController(VanLocWeb.Services.DataService dataService, VanLocWeb.Services.PdfService pdfService)
        {
            _dataService = dataService;
            _pdfService = pdfService;
        }

        public IActionResult Dashboard()
        {
            var stats = _dataService.GetStats();
            var members = _dataService.GetAllMembers();
            var finance = _dataService.GetAllFinance();
            var recentMembers = members.OrderByDescending(m => m.JoinDate).Take(5).ToList();
            var recentFinance = finance.OrderByDescending(f => f.Date).Take(5).ToList();

            ViewBag.Stats = stats;
            ViewBag.TotalMembers = members.Count;
            ViewBag.TotalFunds = finance.Where(f => f.Type == "Income").Sum(f => f.Amount) - finance.Where(f => f.Type == "Expense").Sum(f => f.Amount);
            ViewBag.RecentMembers = recentMembers;
            ViewBag.RecentFinance = recentFinance;

            return View();
        }

        public IActionResult Index() => RedirectToAction("Dashboard");

        public IActionResult News()
        {
            var news = _dataService.GetAllNews();
            return View(news);
        }

        [HttpGet]
        public IActionResult CreateNews()
        {
            return View(new NewsItem { PublishDate = DateTime.Now });
        }

        [HttpPost]
        public IActionResult CreateNews(NewsItem item, List<IFormFile> imageFiles)
        {
            item.Images = new List<string>();
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            if (imageFiles != null && imageFiles.Count > 0)
            {
                foreach (var file in imageFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadDir, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        item.Images.Add("/uploads/" + fileName);
                    }
                }
                if (item.Images.Any()) item.ImageUrl = item.Images.First();
            }

            if (item.PublishDate == default) item.PublishDate = DateTime.Now;

            var news = _dataService.GetAllNews();
            item.Id = news.Any() ? news.Max(n => n.Id) + 1 : 1;
            news.Add(item);
            _dataService.SaveNews(news);
            return RedirectToAction("News");
        }

        [HttpGet]
        public IActionResult EditNews(int id)
        {
            var item = _dataService.GetAllNews().FirstOrDefault(n => n.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        public IActionResult EditNews(NewsItem item, List<IFormFile> imageFiles, List<string> existingImages)
        {
            var news = _dataService.GetAllNews();
            var existing = news.FirstOrDefault(n => n.Id == item.Id);
            if (existing == null) return NotFound();

            item.Images = existingImages ?? new List<string>();

            if (imageFiles != null && imageFiles.Count > 0)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                foreach (var file in imageFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadDir, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        item.Images.Add("/uploads/" + fileName);
                    }
                }
            }

            if (item.Images.Any()) item.ImageUrl = item.Images.First();
            else item.ImageUrl = string.Empty;

            if (item.PublishDate == default) item.PublishDate = DateTime.Now;

            news.Remove(existing);
            news.Add(item);
            _dataService.SaveNews(news);
            return RedirectToAction("News");
        }

        [HttpPost]
        public IActionResult DeleteNews(int id)
        {
            var news = _dataService.GetAllNews();
            var item = news.FirstOrDefault(n => n.Id == id);
            if (item != null)
            {
                news.Remove(item);
                _dataService.SaveNews(news);
            }
            return RedirectToAction("News");
        }

        public IActionResult Events()
        {
            var events = _dataService.GetAllEvents();
            return View(events);
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            return View(new EventItem { StartDate = DateTime.Now.AddDays(7), EndDate = DateTime.Now.AddDays(7).AddHours(4) });
        }

        [HttpPost]
        public IActionResult CreateEvent(EventItem item, List<IFormFile> imageFiles)
        {
            item.Images = new List<string>();
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            if (imageFiles != null && imageFiles.Count > 0)
            {
                foreach (var file in imageFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadDir, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        item.Images.Add("/uploads/" + fileName);
                    }
                }
                if (item.Images.Any()) item.ImageUrl = item.Images.First();
            }

            var events = _dataService.GetAllEvents();
            item.Id = events.Any() ? events.Max(e => e.Id) + 1 : 1;
            events.Add(item);
            _dataService.SaveEvents(events);
            return RedirectToAction("Events");
        }

        [HttpGet]
        public IActionResult EditEvent(int id)
        {
            var item = _dataService.GetAllEvents().FirstOrDefault(e => e.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        public IActionResult EditEvent(EventItem item, List<IFormFile> imageFiles, List<string> existingImages)
        {
            var events = _dataService.GetAllEvents();
            var existing = events.FirstOrDefault(e => e.Id == item.Id);
            if (existing == null) return NotFound();

            item.Images = existingImages ?? new List<string>();

            if (imageFiles != null && imageFiles.Count > 0)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                foreach (var file in imageFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadDir, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        item.Images.Add("/uploads/" + fileName);
                    }
                }
            }

            if (item.Images.Any()) item.ImageUrl = item.Images.First();
            else item.ImageUrl = string.Empty;

            events.Remove(existing);
            events.Add(item);
            _dataService.SaveEvents(events);
            return RedirectToAction("Events");
        }

        [HttpPost]
        public IActionResult DeleteEvent(int id)
        {
            var events = _dataService.GetAllEvents();
            var item = events.FirstOrDefault(e => e.Id == id);
            if (item != null)
            {
                events.Remove(item);
                _dataService.SaveEvents(events);
            }
            return RedirectToAction("Events");
        }

        public IActionResult Members(string search, string status)
        {
            var members = _dataService.GetAllMembers();
            if (!string.IsNullOrEmpty(search))
            {
                members = members.Where(m => m.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrEmpty(status))
            {
                members = members.Where(m => m.Status == status).ToList();
            }

            ViewData["Search"] = search;
            ViewData["Status"] = status;
            return View(members);
        }

        [HttpPost]
        public IActionResult ApproveMember(int id)
        {
            var members = _dataService.GetAllMembers();
            var member = members.FirstOrDefault(m => m.Id == id);
            if (member != null)
            {
                member.Status = "Active";
                member.IsActive = true;
                _dataService.SaveMembers(members);
            }
            return RedirectToAction("Members");
        }

        [HttpPost]
        public IActionResult DeleteMember(int id)
        {
            var members = _dataService.GetAllMembers();
            var member = members.FirstOrDefault(m => m.Id == id);
            if (member != null)
            {
                members.Remove(member);
                _dataService.SaveMembers(members);
            }
            return RedirectToAction("Members");
        }

        public IActionResult Finance()
        {
            var finance = _dataService.GetAllFinance();
            return View(finance);
        }

        [HttpGet]
        public IActionResult CreateTransaction()
        {
            return View(new FinanceTransaction { Date = DateTime.Now });
        }

        [HttpPost]
        public IActionResult CreateTransaction(FinanceTransaction item, IFormFile? voucherFile)
        {
            if (voucherFile != null && voucherFile.Length > 0)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "vouchers");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(voucherFile.FileName);
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    voucherFile.CopyTo(stream);
                }
                item.VoucherUrl = "/uploads/vouchers/" + fileName;
            }

            var finance = _dataService.GetAllFinance();
            item.Id = finance.Any() ? finance.Max(f => f.Id) + 1 : 1;
            finance.Add(item);
            _dataService.SaveFinance(finance);
            return RedirectToAction("Finance");
        }

        [HttpPost]
        public IActionResult DeleteTransaction(int id)
        {
            var finance = _dataService.GetAllFinance();
            var item = finance.FirstOrDefault(f => f.Id == id);
            if (item != null)
            {
                finance.Remove(item);
                _dataService.SaveFinance(finance);
            }
            return RedirectToAction("Finance");
        }

        public IActionResult Media()
        {
            var media = _dataService.GetAllMedia();
            return View(media);
        }

        public IActionResult ExportProfessionalReport()
        {
            var finance = _dataService.GetAllFinance();
            var pdfBytes = _pdfService.GenerateFinanceReport(finance, "BÁO CÁO TỔNG HỢP THU CHI NĂM 2025");
            return File(pdfBytes, "application/pdf", $"BaoCaoTaiChinh_{DateTime.Now:yyyyMMdd}.pdf");
        }
        public IActionResult ExportParticipants(int id)
        {
            var ev = _dataService.GetAllEvents().FirstOrDefault(e => e.Id == id);
            if (ev == null) return NotFound();

            var regs = _dataService.GetAllRegistrations().Where(r => r.EventId == id).ToList();
            var pdfBytes = _pdfService.GenerateParticipantList(regs, ev.Title);
            return File(pdfBytes, "application/pdf", $"DanhSachDangKy_{id}.pdf");
        }

        public IActionResult Users()
        {
            var admins = _dataService.GetAllAdmins();
            return View(admins);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateUser(string username, string password, string fullName, AdminRole role)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin");
                return View();
            }

            var admins = _dataService.GetAllAdmins();
            if (admins.Any(u => u.Username == username))
            {
                ModelState.AddModelError("", "Tên đăng nhập đã tồn tại");
                return View();
            }

            var newUser = new AdminUser
            {
                Id = admins.Any() ? admins.Max(u => u.Id) + 1 : 1,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FullName = fullName,
                Role = role
            };

            admins.Add(newUser);
            _dataService.SaveAdmins(admins);
            return RedirectToAction("Users");
        }

        public IActionResult Messages()
        {
            var messages = _dataService.GetAllMessages();
            return View(messages);
        }
    }
}
