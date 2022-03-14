using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace dejavueVideo
{
    public static class Function1
    {
        public static ILogger logger;
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            logger = log;
            try
            {
                var form = await req.ReadFormAsync();
                var file = form.Files["file"];
                var res = new StringBuilder();
                using (var stream = file.OpenReadStream())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        await BlobFace.PerformFaceRecognition(memoryStream.ToArray());
                    }
                }
                return new OkObjectResult($"{file.FileName} was POSTed to the function and has the content {res}");
            }
            catch
            {
                return new BadRequestObjectResult("Bad file");
            }
        }
    }
}
