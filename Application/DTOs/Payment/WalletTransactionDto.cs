using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Payment
{
    public class WalletTransactionDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } // "credit" (Green) or "debit" (Red)
        public DateTime Date { get; set; }
        public string Status { get; set; }
    }
}
