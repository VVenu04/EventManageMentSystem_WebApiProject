using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Admin
{
    public class SystemSettingsDto
    {
        public string SiteName { get; set; }
        public string SupportEmail { get; set; }

        // 🚨 FIX: இந்த வரி விடுபட்டிருந்தது, இதைச் சேர்க்கவும்
        public string SupportPhone { get; set; }

        public string OfficeAddress { get; set; }
        public decimal ServiceCommission { get; set; }
        public decimal PackageCommission { get; set; }
        public decimal CustomerCashback { get; set; }
        public bool MaintenanceMode { get; set; }
    }
}
