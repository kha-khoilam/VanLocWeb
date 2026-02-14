using Microsoft.EntityFrameworkCore;
using VanLocWeb.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace VanLocWeb.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<AdminUser> AdminUsers { get; set; } = null!;
        public DbSet<NewsItem> NewsItems { get; set; } = null!;
        public DbSet<EventItem> EventItems { get; set; } = null!;
        public DbSet<FinanceTransaction> FinanceTransactions { get; set; } = null!;
        public DbSet<Member> Members { get; set; } = null!;
        public DbSet<EventRegistration> EventRegistrations { get; set; } = null!;
        public DbSet<SiteStats> SiteStats { get; set; } = null!;
        public DbSet<MediaItem> MediaItems { get; set; } = null!;
        public DbSet<ContactMessage> ContactMessages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Value converter for List<string> to store as JSON in DB
            var listConverter = new ValueConverter<List<string>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null) ?? "[]",
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            // Value converter for Dictionary<string, int>
            var dictConverter = new ValueConverter<Dictionary<string, int>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null) ?? "{}",
                v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, int>());

            modelBuilder.Entity<NewsItem>()
                .Property(e => e.Images)
                .HasConversion(listConverter);

            modelBuilder.Entity<EventItem>()
                .Property(e => e.Images)
                .HasConversion(listConverter);

            modelBuilder.Entity<SiteStats>()
                .Property(e => e.DailyVisits)
                .HasConversion(dictConverter);

            // SiteStats only needs one row, but for simplicity we'll keep it as a table
            modelBuilder.Entity<SiteStats>().HasKey(s => s.Id);
            // Let's add an ID to SiteStats in AppModels.cs or just use a dummy key.
            // Fixed in AppModels.cs below.
        }
    }
}
