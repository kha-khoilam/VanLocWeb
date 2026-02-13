using System;
using System.Collections.Generic;

namespace VanLocWeb.Models
{
    public enum AccessLevel
    {
        Public,
        Internal
    }

    public enum AdminRole
    {
        SuperAdmin,
        ContentManager,
        MemberManager,
        FinanceManager
    }

    public class AdminUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public AdminRole Role { get; set; } = AdminRole.SuperAdmin;
    }

    public class NewsItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public List<string> Images { get; set; } = new();
        public DateTime PublishDate { get; set; }
        public AccessLevel Visibility { get; set; } = AccessLevel.Public;
    }

    public class EventItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public List<string> Images { get; set; } = new();
        public AccessLevel Visibility { get; set; } = AccessLevel.Public;
    }

    public class FinanceTransaction
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = "Income"; // Income or Expense
        public string VoucherUrl { get; set; } = string.Empty;
        public AccessLevel Visibility { get; set; } = AccessLevel.Public;
    }

    public class Member
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Village { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public string Occupation { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = "Active"; // Active, Pending, Inactive
    }

    public class EventRegistration
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int NumberOfAttendees { get; set; }
        public string Note { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
    }

    public class SiteStats
    {
        public int Id { get; set; }
        public int TotalVisits { get; set; } = 0;
        public Dictionary<string, int> DailyVisits { get; set; } = new Dictionary<string, int>();
    }

    public class MediaItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Type { get; set; } = "Image"; // Image or Video
        public string Year { get; set; } = DateTime.Now.Year.ToString();
        public string Topic { get; set; } = "Họp mặt";
        public AccessLevel Visibility { get; set; } = AccessLevel.Public;
    }

    public class ContactMessage
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentDate { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
    }

    public class DashboardViewModel
    {
        public int TotalMembers { get; set; }
        public decimal TotalFunds { get; set; }
        public int TotalVisits { get; set; }
        public int PendingMembers { get; set; }
        public List<NewsItem> RecentNews { get; set; } = new();
        public List<FinanceTransaction> RecentTransactions { get; set; } = new();
    }
}
