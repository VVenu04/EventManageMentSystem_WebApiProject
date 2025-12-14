using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IService
{
    public interface IAIService
    {
        Task<string> GetChatResponseAsync(string userMessage, string role);

        // 2. Budget Plan உருவாக்குதல்
        Task<string> GenerateBudgetPlanAsync(string eventType, int guests, decimal budget);
        Task<string> GenerateServiceDescriptionAsync(string serviceName, string category);
    }
}
