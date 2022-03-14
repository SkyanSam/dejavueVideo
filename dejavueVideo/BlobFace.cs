using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System.Text.Json;
//using OpenCvSharp;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Microsoft.Extensions.Logging;
namespace dejavueVideo
{
    class BlobFace
    {
        //public const string apiurl = "";
        //public Stream myBlob;
        //public int id;
        //public List<string> attributes;
        public static async Task PerformFaceRecognition(byte[] image)
        {
            var client = FaceRecognitionService.Authenticate();
            var attributes = new List<string>();
            var detectFaceTask = Task.Run(
                     () =>
                     {
                         var frames = new List<byte[]>();
                         frames.Add(image);
                        return FaceRecognitionService.DetectFaceExtract(client, frames);
                     });
            await detectFaceTask.ContinueWith(antecedent =>
            {
                var faces = antecedent.Result;
                Function1.logger.LogInformation($"Getting emotion information from {faces.Length} faces");
                for (int g = 0; g < faces.Length; g++)
                {
                    Function1.logger.LogInformation($"loop g {g}");
                    var smile = faces[g].FaceAttributes.Smile;
                    if (smile.HasValue && !attributes.Contains("smile"))
                        attributes.Add("smile");
                    var emotion = faces[g].FaceAttributes.Emotion;
                    if (emotion.Contempt > 0 && !attributes.Contains("contempt")) attributes.Add("contempt");
                    if (emotion.Neutral > 0 && !attributes.Contains("neutral")) attributes.Add("neutral");
                }
                Function1.logger.LogInformation($"Got {attributes.Count} attributes");
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
            await Post(attributes);
        }
        public static async Task Post(List<string> attributes)
        {
            var feedback = new Feedback(attributes.ToArray());
            string feedbackText = JsonSerializer.Serialize(feedback);
            Function1.logger.LogInformation($"Serialized text: {feedbackText}");
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    "https://sakerunbackend.azurewebsites.net/sendfeedback",
                     new StringContent(feedbackText, Encoding.UTF8, "application/json"));
                Function1.logger.LogInformation($"Sent response to client @ https://sakerunbackend.azurewebsites.net/sendfeedback");
            }
            Function1.logger.LogInformation($"COMPLETE! :)");
        }
        
    }
    [System.Serializable]
    public class Feedback
    {
        public string[] feedback;
        public Feedback(string[] _f)
        {
            feedback = _f;
        }
    }
}
