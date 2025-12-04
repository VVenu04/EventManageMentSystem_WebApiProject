using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.AI
{
    public class BudgetRequestDto
    {
        public string EventType { get; set; } // e.g. Wedding
        public int GuestCount { get; set; }   // e.g. 500
        public decimal TotalBudget { get; set; } // e.g. 500000
    }
}
