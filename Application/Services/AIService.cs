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

        // Chatbot Logic
        public async Task<string> GetChatResponseAsync(string userMessage, string role)
        {
            if (string.IsNullOrEmpty(_apiKey))
                return "API Key is missing.";

            string systemPrompt = @"
                You are 'SmartBot', assistant for Smart Function Event Management System.
                Answer about bookings, vendors, packages.
                Be short & clear.";
            return await SendRequestToOpenRouter(systemPrompt, userMessage);
        }

        // Budget Planner
        public async Task<string> GenerateBudgetPlanAsync(string eventType, int guests, decimal budget)
        {
            if (string.IsNullOrEmpty(_apiKey))
                return "API Key is missing.";

            string systemPrompt = "You output ONLY JSON array. No markdown.";

            string userPrompt = $@"
                Create budget plan for '{eventType}' with {guests} guests. 
                Total budget: {budget} LKR.

                Output sample:
                [
                    {{""category"":""Catering"",""amount"":0,""percentage"":0}},
                    {{""category"":""Decoration"",""amount"":0,""percentage"":0}}
                ]";

            return await SendRequestToOpenRouter(systemPrompt, userPrompt);
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
