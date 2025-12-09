using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class SystemSettings
    {
        [Key]
        public int Id { get; set; } // Always 1
        public string SiteName { get; set; } = "SmartFunction";
        public string SupportEmail { get; set; }
        public string SupportPhone { get; set; }
        public string OfficeAddress { get; set; }

        // Finance
        public decimal ServiceCommission { get; set; }
        public decimal PackageCommission { get; set; }
        public decimal CustomerCashback { get; set; }

        // System Control
        public bool MaintenanceMode { get; set; }
    }
}
