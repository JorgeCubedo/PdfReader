using System.Net;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionAppTest
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("Function1")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }

    public class ReadFile
    {
        private readonly ILogger _logger;

        public ReadFile(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ReadFile>();
        }

        [Function("ReadFile")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody;
            using (StreamReader reader = new StreamReader(req.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }
            var response = req.CreateResponse(HttpStatusCode.OK);

            RequestFileModel? requestFileModel = JsonConvert.DeserializeObject<RequestFileModel>(requestBody);
            RequestFileModel fileModel = requestFileModel;

            if (string.IsNullOrEmpty(fileModel.File))
            {
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.WriteString("No File Specified");
                return response;
            }
            
            string file = fileModel.File;

            if (file.Contains(","))
                file = file.Split(',')[1];

            byte[] pdfBytes = Convert.FromBase64String(file);

            string filePath = Path.Combine(Path.GetTempPath(), "output.pdf");

            await File.WriteAllBytesAsync(filePath, pdfBytes);

            Console.WriteLine($"Path: {filePath}");
            
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            string emailPattern = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";
            string postalPattern = "\\b\\d{5}\\b";

            using (PdfReader reader = new PdfReader(filePath))
            {
                using (PdfDocument document = new(reader))
                {
                    for (int i = 1; i <= document.GetNumberOfPages(); i++)
                    {
                        string content = PdfTextExtractor.GetTextFromPage(document.GetPage(i));
                        MatchCollection collection = Regex.Matches(content, postalPattern);

                        foreach(var c in collection)
                        {
                            Console.WriteLine($"Postal: {c}");
                            response.WriteString($"Postal: {c}");
                        }

                        //Console.WriteLine(content);
                    }
                }
            }

            return response;
        }
    }
}
