using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VanLocWeb.Models;

namespace VanLocWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly VanLocWeb.Services.DataService _dataService;

        public HomeController(ILogger<HomeController> logger, VanLocWeb.Services.DataService dataService)
        {
            _logger = logger;
            _dataService = dataService;
        }

        public IActionResult Index()
        {
            var stats = _dataService.GetStats();
            var members = _dataService.GetAllMembers();
            var finance = _dataService.GetAllFinance();

            ViewBag.TotalVisits = stats.TotalVisits;
            ViewBag.TotalMembers = members.Count;
            ViewBag.TotalFunds = finance.Where(f => f.Type == "Income").Sum(f => f.Amount) - finance.Where(f => f.Type == "Expense").Sum(f => f.Amount);

            ViewBag.RecentNews = _dataService.GetAllNews().OrderByDescending(n => n.PublishDate).Take(3).ToList();
            ViewBag.RecentEvents = _dataService.GetAllEvents().OrderByDescending(e => e.StartDate).Take(2).ToList();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult News(string category, string search, bool @internal = false)
        {
            ViewData["Category"] = category;
            ViewData["Search"] = search;

            var news = _dataService.GetAllNews();
            if (!@internal)
            {
                news = news.Where(n => n.Visibility == AccessLevel.Public).ToList();
            }

            return View(news);
        }

        public IActionResult NewsDetail(int id)
        {
            var item = _dataService.GetAllNews().FirstOrDefault(n => n.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        public IActionResult Events()
        {
            var events = _dataService.GetAllEvents();
            return View(events);
        }

        public IActionResult EventDetail(int id)
        {
            var ev = _dataService.GetAllEvents().FirstOrDefault(e => e.Id == id);
            if (ev == null) return NotFound();
            return View(ev);
        }

        public IActionResult Members(string search, string village, string job)
        {
            ViewData["Search"] = search;
            ViewData["Village"] = village;
            ViewData["Job"] = job;

            var members = _dataService.GetAllMembers().AsEnumerable();

            if (!string.IsNullOrEmpty(search))
            {
                members = members.Where(m => m.FullName.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(village))
            {
                members = members.Where(m => m.Village == village);
            }

            if (!string.IsNullOrEmpty(job))
            {
                members = members.Where(m => m.Occupation.Contains(job, StringComparison.OrdinalIgnoreCase));
            }

            return View(members.ToList());
        }

        public IActionResult Join()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Join(Member member)
        {
            var members = _dataService.GetAllMembers();
            member.Id = members.Any() ? members.Max(m => m.Id) + 1 : 1;
            member.JoinDate = DateTime.Now;
            member.Status = "Pending";
            member.IsActive = false;

            members.Add(member);
            _dataService.SaveMembers(members);

            TempData["Message"] = "Đăng ký của bạn đã được gửi thành công. Vui lòng chờ Ban chấp hành xét duyệt.";
            return RedirectToAction("Join");
        }

        public IActionResult Finance(bool @internal = false)
        {
            var transactions = _dataService.GetAllFinance();
            if (!@internal)
            {
                transactions = transactions.Where(t => t.Visibility == AccessLevel.Public).ToList();
            }
            return View(transactions);
        }

        public IActionResult Gallery(string year, string topic)
        {
            ViewData["Year"] = year;
            ViewData["Topic"] = topic;
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(ContactMessage message)
        {
            var messages = _dataService.GetAllMessages();
            message.Id = messages.Any() ? messages.Max(m => m.Id) + 1 : 1;
            message.SentDate = DateTime.Now;
            message.IsRead = false;

            messages.Add(message);
            _dataService.SaveMessages(messages);

            TempData["Message"] = "Cảm ơn bạn đã liên hệ. Chúng tôi sẽ phản hồi trong thời gian sớm nhất.";
            return RedirectToAction("Contact");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
