using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Admin
{
    public class AdminDashboardDto
    {
        public int TotalVendors { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; } // Total Payments
        public List<DashboardActivityDto> RecentActivities { get; set; }
        public decimal AdminCashBack { get; set; }

    }

    public class DashboardActivityDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Time { get; set; }
        public string Type { get; set; } // "Vendor", "Booking", "Payment"
        public string Icon { get; set; } // FontAwesome Icon Class
        public string BgColor { get; set; } // Tailwind Class
    }
}
