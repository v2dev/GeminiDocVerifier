using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using GeminiDocVerifier.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Matching;
using Newtonsoft.Json;

namespace GeminiDocVerifier.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(FileUpload model)
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

                var jsonFile = "{'generationConfig':{'response_mime_type':'application/json'},'contents':[{'parts':[{'text':'Get all the fields and values based on document type with page'},{'inline_data':{'mime_type':'application/pdf','data':'" + fileContent + "'}}]}]}";
                HttpContent content = new StringContent(jsonFile, Encoding.UTF8, "application/json");
                _response = _client.PostAsync("v1beta/models/gemini-2.0-flash:generateContent?key=AIzaSyBEhuDK2hxLpvYoMKOwAj1t8fVMOqhCnYE", content).Result;
            }

            var _responseData = _response.Content.ReadAsStringAsync().Result;

            var _json = JsonConvert.DeserializeObject<dynamic>(_responseData);

            var _text = _json?.candidates[0]?.content?.parts[0].text;

            var _parsedPDFjson = JsonConvert.DeserializeObject<List<GeminiResponse>>(_text?.ToString());
            model.FormResponse = _parsedPDFjson;

            return View(model);
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
