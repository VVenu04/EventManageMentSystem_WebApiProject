// File: Application/Services/AIService.cs
using Application.Interface.IService;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AIService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _apiKey = config["OpenAISettings:ApiKey"];
        }

        public async Task<string> GetChatResponseAsync(string userMessage, string role)
        {
            // ... (Chat Logic) ...
            return await SendRequestToOpenAI(new
            {
                model = "gpt-3.5-turbo",
                messages = new[] {
                    new { role = "system", content = "You are a helpful assistant." },
                    new { role = "user", content = userMessage }
                }
            });
        }

        // 🚨 NEW: Budget Planner Logic
        public async Task<string> GenerateBudgetPlanAsync(string eventType, int guests, decimal budget)
        {
            string prompt = $@"
                Act as an expert Event Planner. 
                Create a budget breakdown for a '{eventType}' with {guests} guests and a total budget of {budget} LKR.
                
                Provide the response in strictly JSON format like this:
                [
                  {{ ""category"": ""Catering"", ""amount"": 200000, ""percentage"": 40 }},
                  {{ ""category"": ""Decoration"", ""amount"": 100000, ""percentage"": 20 }}
                ]
                Do not add any other text, just the JSON array.
            ";

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[] {
                    new { role = "system", content = "You are a financial assistant that outputs only raw JSON." },
                    new { role = "user", content = prompt }
                }
            };

            return await SendRequestToOpenAI(requestBody);
        }

        private async Task<string> SendRequestToOpenAI(object requestBody)
        {
            if (string.IsNullOrEmpty(_apiKey)) return "API Key is missing.";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode) return $"Error: {response.StatusCode}";

                using var doc = JsonDocument.Parse(jsonResponse);
                var output = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

                // Clean Markdown
                return output?.Replace("```json", "").Replace("```", "").Trim() ?? "[]";
            }
            catch (Exception ex) { return $"Exception: {ex.Message}"; }
        }
    }
}