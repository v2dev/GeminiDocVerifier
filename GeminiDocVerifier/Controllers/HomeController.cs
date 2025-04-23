using System.Diagnostics;
using System.Text;
using GeminiDocVerifier.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json;

namespace GeminiDocVerifier.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHostEnvironment _hostEnvironment;

        public HomeController(ILogger<HomeController> logger, IHostEnvironment hostEnvironment)
        {            
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(FileUpload model)
        {
            model.FilePath = await SplitFile(model);
            model.FileName = model.FormFile.FileName;
            model.ContentType = model.FormFile.ContentType;
            model.Json = await GetGeminiResponse(model);
            model.FormResponse = JsonConvert.DeserializeObject<dynamic>(model.Json);
            return View(model);
        }

        public async Task<string> GetGeminiResponse(FileUpload model)
        {
            HttpClient _client = new HttpClient();
            _client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
            HttpResponseMessage _response = null;
            if (model.FormFile.Length > 0)
            {
                var fileBytes = new byte[model.FormFile.Length];
                using (var ms = new MemoryStream())
                {
                    model.FormFile.CopyTo(ms);
                    fileBytes = ms.ToArray();
                }
                var fileContent = Convert.ToBase64String(fileBytes);

                //var prompt = "Detect and analyze multiple documents within the provided file, process their content, extract relevant data, identify the document type, and generate a structured hierarchical JSON for each. Combine all responses into an array. Response {self.payStubTemplate} format if document type is paystub or payslip. Response {self.w2DocTemplate} format if document type is W2. If document type is not paystub or W2, return {self.unknownDocTemplate}."

                var prompt = "Detect and analyze multiple documents within the provided file, process their content, extract relevant data, identify the document type and page number, and generate a structured hierarchical JSON for each. Combine all responses into an array.";
                var jsonFile = "{'generationConfig':{'response_mime_type':'application/json'},'contents':[{'parts':[{'text':'" + prompt + "'},{'inline_data':{'mime_type':'" + model.FormFile.ContentType + "','data':'" + fileContent + "'}}]}]}";
                HttpContent content = new StringContent(jsonFile, Encoding.UTF8, "application/json");
                //gemini-2.5-pro-exp-03-25
                _response = _client.PostAsync("v1beta/models/gemini-2.0-flash:generateContent?key=AIzaSyBEhuDK2hxLpvYoMKOwAj1t8fVMOqhCnYE", content).Result;
            }

            var _responseData = _response.Content.ReadAsStringAsync().Result;

            var _json = JsonConvert.DeserializeObject<dynamic>(_responseData);

            var _text = _json?.candidates[0]?.content?.parts[0].text;

            return _text?.ToString();
        }

        public async Task<string> SplitFile(FileUpload model)
        {
            string _datetime = DateTime.Now.ToString("yyyyMMddHHmmss");            
            if (model.FormFile.Length > 0)
            {
                var _path = @$"{_hostEnvironment.ContentRootPath}\wwwroot\images\{_datetime}";
                Directory.CreateDirectory(_path);

                string filePathWithFileName = $@"{_path}\{model.FormFile.FileName}";

                using (Stream fileStream = new FileStream(filePathWithFileName, FileMode.Create))
                {
                    await model.FormFile.CopyToAsync(fileStream);
                }

                if (model.FormFile.ContentType.ToLower().Contains("pdf"))
                {
                    var pdf = new PdfDocument(filePathWithFileName);
                    for (int _counter = 0; _counter < pdf.Pages.Count; ++_counter)
                    {
                        var pdf_page = pdf.CopyPage(_counter);
                        pdf_page.SaveAs(@$"{_path}\page_{_counter+1}.pdf");
                    }
                }
            }
            return $"/images/{_datetime}";
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
