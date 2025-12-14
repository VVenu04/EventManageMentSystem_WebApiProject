using Application.Interface.IRepo;
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
        private readonly string _baseUrl;

        public AIService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _apiKey = config["OpenRouterSettings:ApiKey"]; // ⭐ NEW KEY NAME

            // ⭐ OpenRouter API Endpoint (FREE)
            _baseUrl = "https://openrouter.ai/api/v1/chat/completions";
        }


        public async Task<string> GetChatResponseAsync(string userMessage, string role)
        {
            if (string.IsNullOrEmpty(_apiKey))
                return "API Key is missing.";

            // 🚨 STRICT SYSTEM PROMPT (AI-க்குக் கடுமையான கட்டுப்பாடுகள்)
            string systemPrompt = @"
        You are 'SmartBot', the exclusive AI assistant for the *'Smart Function' Event Management System* in Sri Lanka.

        ✅ *YOUR ALLOWED TOPICS:*
        1. *Event Planning:* Weddings, Birthdays, Puberty ceremonies, Corporate events.
        2. *Vendor Services:* Catering, Decoration, Photography, Cakes, Halls, Sounds/Lights.
        3. *Platform Features:* Booking process, Payments, Packages, Vendor collaboration.
        4. *Budgeting:* Helping users estimate event costs in *LKR (Sri Lankan Rupees)*.

        ⛔ *STRICT RESTRICTIONS (DO NOT ANSWER):*
        - *DO NOT* answer questions about Politics, Sports, Movies, Math, Coding, or General Knowledge.
        - *DO NOT* write essays or stories unrelated to events.
        - If the user asks anything outside of Event Management, reply with this exact phrase:
          'I apologize, but I am designed only to assist with the Smart Function Event Management System.'

        *CONTEXT & TONE:*
        - Location Context: Jaffna, Sri Lanka.
        - Currency: LKR.
        - Tone: Professional, Polite, and Short.
    ";

            return await SendRequestToOpenRouter(systemPrompt, userMessage);
        }


        // Budget Planner
        public async Task<string> GenerateBudgetPlanAsync(string eventType, int guests, decimal budget)
        {
            if (string.IsNullOrEmpty(_apiKey))
                return "API Key is missing.";

            // 🚨 SYSTEM PROMPT: AI-க்குக் கடுமையான கட்டளை
            string systemPrompt = @"
        You are an expert Event Budget Planner using LKR currency.
        
        CRITICAL RULES:
        1. Output ONLY a raw JSON array. Do not use Markdown (```json).
        2. You MUST allocate the budget ONLY for these 3 specific categories: 'Catering', 'Decoration', 'Cake'.
        3. Do NOT add any other categories like Transport, Venue, or Photography.
        4. Ensure the total amount equals the provided budget.
    ";

            // 🚨 USER PROMPT: விவரங்கள்
            string userPrompt = $@"
        Create a budget plan for a '{eventType}' with {guests} guests. 
        Total Budget: {budget} LKR.

        Allocate the funds logically among 'Catering', 'Decoration', and 'Cake' based on the event type.

        Required Output Format:
        [
            {{ ""category"": ""Catering"", ""amount"": 0, ""percentage"": 0 }},
            {{ ""category"": ""Decoration"", ""amount"": 0, ""percentage"": 0 }},
            {{ ""category"": ""Cake"", ""amount"": 0, ""percentage"": 0 }}
        ]";

            return await SendRequestToOpenRouter(systemPrompt, userPrompt);
        }

        public async Task<string> GenerateServiceDescriptionAsync(string serviceName, string category)
        {
            if (string.IsNullOrEmpty(_apiKey)) return "API Key is missing.";

            // Prompt Engineering (AI-க்கு கட்டளை)
            string prompt = $@"
                Act as a professional marketing copywriter for an Event Management Platform.
                Write a short, attractive, and professional service description (approx 30-40 words).
                
                Service Details:
                - Name: {serviceName}
                - Category: {category}
                
                Tone: Premium, Trustworthy, and Exciting.
                Output: Only the description text. No quotes.
            ";

            var requestBody = new
            {
                model = "google/gemini-2.0-flash-exp:free", // அல்லது உங்களுக்கு பிடித்த மாடல்
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful marketing assistant." },
                    new { role = "user", content = prompt }
                }
            };

            return await SendRequestToOpenRouter(requestBody);
        }

        // ⭐ OpenRouter Request Method
        private async Task<string> SendRequestToOpenRouter(string systemPrompt, string userMessage)
        {
            // Required OpenRouter Headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://yourapp.com");
            _httpClient.DefaultRequestHeaders.Add("X-Title", "Smart Function System");

            var requestBody = new
            {
                model = "deepseek/deepseek-chat", // ⭐ Free model
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userMessage }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(_baseUrl, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return $"OpenRouter Error: {response.StatusCode} - {jsonResponse}";
                }

                using var doc = JsonDocument.Parse(jsonResponse);

                var text = doc.RootElement
                              .GetProperty("choices")[0]
                              .GetProperty("message")
                              .GetProperty("content")
                              .GetString();

                return text?.Trim() ?? "No response.";
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }
        private async Task<string> SendRequestToOpenRouter(object requestBody)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:5018");
            _httpClient.DefaultRequestHeaders.Add("X-Title", "SmartFunction");

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(_baseUrl, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode) return "AI Service Unavailable.";

                using var doc = JsonDocument.Parse(jsonResponse);
                if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var text = choices[0].GetProperty("message").GetProperty("content").GetString();

                    // Clean up any Markdown or JSON formatting if AI adds it
                    return text?.Replace("```json", "").Replace("```", "").Trim() ?? "No content.";
                }
                return "No response.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

    }
}