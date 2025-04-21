namespace GeminiDocVerifier.Models
{
    public class FileUpload
    {
        public IFormFile FormFile{get;set;}

        public List<GeminiResponse> FormResponse{get;set;}
}
}
