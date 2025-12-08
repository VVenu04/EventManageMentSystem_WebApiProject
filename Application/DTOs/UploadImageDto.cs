using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class UploadImageDto
    {
        [Required]
        public IFormFile Photo { get; set; }
    }
}
