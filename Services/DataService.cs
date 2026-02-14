using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VanLocWeb.Models;
using VanLocWeb.Data;

namespace VanLocWeb.Services
{
    public class DataService
    {
        private readonly AppDbContext _context;

        public DataService(AppDbContext context)
        {
            _context = context;
        }

        public void InitializeDatabase()
        {
            _context.Database.Migrate();

            if (!_context.AdminUsers.Any())
            {
                _context.AdminUsers.Add(new AdminUser
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    FullName = "Hội Trưởng (Admin)",
                    Role = AdminRole.SuperAdmin
                });
            }

            // Sample data removed - admins can create their own news items via admin panel

            if (!_context.SiteStats.Any())
            {
                _context.SiteStats.Add(new SiteStats { TotalVisits = 1245 });
            }

            _context.SaveChanges();
        }

        public List<NewsItem> GetAllNews()
        {
            try { return _context.NewsItems.ToList(); }
            catch { return new List<NewsItem>(); }
        }
        public void SaveNews(List<NewsItem> news)
        {
            _context.NewsItems.ExecuteDelete();
            _context.NewsItems.AddRange(news);
            _context.SaveChanges();
        }

        public void AddMember(Member member)
        {
            try
            {
                _context.Members.Add(member);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding member: " + ex.Message);
                throw;
            }
        }

        public void AddContactMessage(ContactMessage message)
        {
            try
            {
                _context.ContactMessages.Add(message);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding contact message: " + ex.Message);
                throw;
            }
        }

        public List<EventItem> GetAllEvents()
        {
            try { return _context.EventItems.ToList(); }
            catch { return new List<EventItem>(); }
        }
        public void SaveEvents(List<EventItem> events)
        {
            _context.EventItems.ExecuteDelete();
            _context.EventItems.AddRange(events);
            _context.SaveChanges();
        }

        public List<Member> GetAllMembers()
        {
            try { return _context.Members.ToList(); }
            catch { return new List<Member>(); }
        }
        public void SaveMembers(List<Member> members)
        {
            _context.Members.ExecuteDelete();
            _context.Members.AddRange(members);
            _context.SaveChanges();
        }

        public List<FinanceTransaction> GetAllFinance()
        {
            try { return _context.FinanceTransactions.ToList(); }
            catch { return new List<FinanceTransaction>(); }
        }
        public void SaveFinance(List<FinanceTransaction> finance)
        {
            _context.FinanceTransactions.ExecuteDelete();
            _context.FinanceTransactions.AddRange(finance);
            _context.SaveChanges();
        }

        public SiteStats GetStats()
        {
            try { return _context.SiteStats.FirstOrDefault() ?? new SiteStats(); }
            catch { return new SiteStats(); }
        }

        public List<AdminUser> GetAllAdmins()
        {
            try { return _context.AdminUsers.ToList(); }
            catch { return new List<AdminUser>(); }
        }
        public void SaveAdmins(List<AdminUser> admins)
        {
            _context.AdminUsers.ExecuteDelete();
            _context.AdminUsers.AddRange(admins);
            _context.SaveChanges();
        }

        public List<EventRegistration> GetAllRegistrations()
        {
            try { return _context.EventRegistrations.ToList(); }
            catch { return new List<EventRegistration>(); }
        }
        public void SaveRegistrations(List<EventRegistration> reg)
        {
            _context.EventRegistrations.ExecuteDelete();
            _context.EventRegistrations.AddRange(reg);
            _context.SaveChanges();
        }

        public List<MediaItem> GetAllMedia()
        {
            try { return _context.MediaItems.ToList(); }
            catch { return new List<MediaItem>(); }
        }
        public void SaveMedia(List<MediaItem> media)
        {
            _context.MediaItems.ExecuteDelete();
            _context.MediaItems.AddRange(media);
            _context.SaveChanges();
        }

        public List<ContactMessage> GetAllMessages()
        {
            try { return _context.ContactMessages.ToList(); }
            catch { return new List<ContactMessage>(); }
        }
        public void SaveMessages(List<ContactMessage> messages)
        {
            _context.ContactMessages.ExecuteDelete();
            _context.ContactMessages.AddRange(messages);
            _context.SaveChanges();
        }

        public AdminUser? ValidateUser(string username, string password)
        {
            try
            {
                var user = _context.AdminUsers.FirstOrDefault(a => a.Username == username);
                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    return user;
                }
            }
            catch { }
            return null;
        }

        public void IncrementVisit()
        {
            // Wrapper for backward compatibility
            AddSiteVisit();
        }

        public void AddSiteVisit()
        {
            try
            {
                var stats = _context.SiteStats.FirstOrDefault();
                if (stats == null)
                {
                    // Initialize if not exists
                    stats = new SiteStats { TotalVisits = 0, DailyVisits = new Dictionary<string, int>() };
                    _context.SiteStats.Add(stats);
                    _context.SaveChanges(); // Save first to get ID/Key if needed, though here Key is TotalVisits which is weird.
                    // Re-fetch or just use it.
                }

                stats.TotalVisits++;
                var today = DateTime.UtcNow.ToString("yyyy-MM-dd"); // Use UTC date

                if (stats.DailyVisits.ContainsKey(today))
                {
                    stats.DailyVisits[today]++;
                }
                else
                {
                    stats.DailyVisits[today] = 1;
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // Silently fail locally so the app keeps running
                Console.WriteLine("Could not increment visit: " + ex.Message);
            }
        }

        public DashboardViewModel GetDashboardData()
        {
            var members = GetAllMembers();
            var finance = GetAllFinance();
            var stats = GetStats();
            var news = GetAllNews();

            return new DashboardViewModel
            {
                TotalMembers = members.Count,
                TotalFunds = finance.Where(f => f.Type == "Income").Sum(f => f.Amount) - finance.Where(f => f.Type == "Expense").Sum(f => f.Amount),
                TotalVisits = stats.TotalVisits,
                PendingMembers = members.Count(m => m.Status == "Pending"),
                RecentNews = news.OrderByDescending(n => n.PublishDate).Take(5).ToList(),
                RecentTransactions = finance.OrderByDescending(f => f.Date).Take(5).ToList()
            };
        }
    }
}
