using Microsoft.AspNetCore.Http;

namespace WordToPdf_Producer.Models
{
    public class WordToPdfFile
    {
        public string Email { get; set; }
        public IFormFile File { get; set; }
    }
}
