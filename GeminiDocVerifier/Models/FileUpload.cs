namespace GeminiDocVerifier.Models
{
    public class FileUpload
    {
        public IFormFile FormFile { get; set; }

        public string FilePath { get; set; }

        public string ContentType { get; set; }

        public string FileName { get; set; }

        public dynamic FormResponse { get; set; }

        public string Json { get; set; }
    }
}
