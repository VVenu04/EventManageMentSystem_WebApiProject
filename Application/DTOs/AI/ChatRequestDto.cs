using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTOs.AI
{
    public class ChatRequestDto
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
