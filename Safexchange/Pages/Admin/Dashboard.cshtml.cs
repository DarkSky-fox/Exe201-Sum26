using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Safexchange.Models;
using Safexchange.Services;

namespace Safexchange.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly SafexchangeDbContext _context;

        public DashboardModel(SafexchangeDbContext context)
        {
            _context = context;
        }

        public int TotalProducts { get; set; }
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public int PendingVerifications { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ActiveProducts { get; set; }
        public int SoldProducts { get; set; }

        // Chart data
        public List<string> CategoryLabels { get; set; } = new();
        public List<int> CategoryProductCounts { get; set; } = new();

        public List<string> OrderStatusLabels { get; set; } = new();
        public List<int> OrderStatusCounts { get; set; } = new();

        public List<string> MonthLabels { get; set; } = new();
        public List<decimal> MonthlyRevenue { get; set; } = new();
        public List<int> MonthlyOrders { get; set; } = new();

        public List<string> RoleLabels { get; set; } = new();
        public List<int> RoleCounts { get; set; } = new();

        public void OnGet()
        {
            TotalProducts = _context.Products.Count();
            TotalUsers = _context.Users.Count();
            TotalOrders = _context.Orders.Count();
            PendingVerifications = _context.UserVerifications.Count(v => v.Status == "Pending");
            ActiveProducts = _context.Products.Count(p => p.Status != null && p.Status.StatusName == "Active");
            SoldProducts = _context.Products.Count(p => p.Status != null && p.Status.StatusName == "Sold");
            TotalRevenue = _context.Orders.Sum(o => o.TotalAmount);

            // Category distribution
            var categoryData = _context.Products
                .Where(p => p.Category != null)
                .GroupBy(p => p.Category.CategoryName)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToList();
            CategoryLabels = categoryData.Select(x => x.Category).ToList();
            CategoryProductCounts = categoryData.Select(x => x.Count).ToList();

            // Order status distribution
            var orderStatusData = _context.Orders
                .GroupBy(o => o.OrderStatus ?? "Không xác định")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();
            OrderStatusLabels = orderStatusData.Select(x => x.Status).ToList();
            OrderStatusCounts = orderStatusData.Select(x => x.Count).ToList();

            // Monthly data (last 6 months)
            var sixMonthsAgo = DateTime.Now.AddMonths(-5);
            var monthlyData = _context.Orders
                .Where(o => o.CreatedAt >= sixMonthsAgo)
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(x => x.TotalAmount),
                    Count = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            var allMonths = Enumerable.Range(0, 6)
                .Select(i => sixMonthsAgo.AddMonths(i))
                .ToList();

            foreach (var month in allMonths)
            {
                var data = monthlyData.FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month);
                MonthLabels.Add(month.ToString("MM/yyyy"));
                MonthlyRevenue.Add(data?.Revenue ?? 0);
                MonthlyOrders.Add(data?.Count ?? 0);
            }

            // User role distribution
            var roleData = _context.Users
                .GroupBy(u => u.Role ?? "Không xác định")
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToList();
            RoleLabels = roleData.Select(x => x.Role).ToList();
            RoleCounts = roleData.Select(x => x.Count).ToList();
        }
    }
}
