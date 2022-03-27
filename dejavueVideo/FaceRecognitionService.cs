using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Newtonsoft.Json;
using dejavueVideo;

namespace dejavueVideo
{

    class FaceRecognitionService
    {
        public const string faceEndPoint = "https://dejavue-face.cognitiveservices.azure.com/";
        public const string faceApiKey = "c55802a679ff422b9cfacde483911851";
        HttpClient _client;
        public FaceRecognitionService()
        {
            //Microsoft.Azure.CognitiveServices.Vision.Face.DetectedFace
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("ocp-apim-subscription-key", faceApiKey);
        }
        public async Task<DetectedFace[]> DetectAsync(Stream imageStream, bool returnFaceId, bool returnFaceLandmarks, IEnumerable<FaceAttributeType> returnFaceAttributes)
        {
            var requestUrl =
              $"{faceEndPoint}/detect?returnFaceId={returnFaceId}" +
              "&returnFaceLandmarks={returnFaceLandmarks}" +
              "&returnFaceAttributes={GetAttributeString(returnFaceAttributes)}";
            return await SendRequestAsync<Stream, DetectedFace[]>(HttpMethod.Post, requestUrl, imageStream);
        }

        async Task<TResponse> SendRequestAsync<TRequest, TResponse>(HttpMethod httpMethod, string requestUrl, TRequest requestBody)
        {
            var request = new HttpRequestMessage(httpMethod, Constants.ServiceEndpointElement);
            request.RequestUri = new Uri(requestUrl);
            if (requestBody != null)
            {
                if (requestBody is Stream)
                {
                    request.Content = new StreamContent(requestBody as Stream);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                }
                else
                {
                    // If the image is supplied via a URL
                    request.Content = new StringContent(JsonConvert.SerializeObject(requestBody/*, s_settings*/), Encoding.UTF8, "application/json");
                }
            }

            HttpResponseMessage responseMessage = await _client.SendAsync(request);
            if (responseMessage.IsSuccessStatusCode)
            {
                string responseContent = null;
                if (responseMessage.Content != null)
                {
                    responseContent = await responseMessage.Content.ReadAsStringAsync();
                }
                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    return JsonConvert.DeserializeObject<TResponse>(responseContent/*, s_settings*/);
                }
                return default(TResponse);
            }
            else
            {
                // fail
                return default(TResponse);
            }
            //return default(TResponse);
        }
        public const string RECOGNITION_MODEL4 = RecognitionModel.Recognition04;
        public static IFaceClient Authenticate()
        {
            Function1.logger.LogInformation($"authenticating with {faceEndPoint}");
            return new FaceClient(new ApiKeyServiceClientCredentials(faceApiKey)) { Endpoint = faceEndPoint };
        }
        public static async Task<DetectedFace[]> DetectFaceExtract(IFaceClient client, List<byte[]> images)
        {
            var detectedFacesAll = new List<DetectedFace>();
            string recognitionModel = RECOGNITION_MODEL4;
            Console.WriteLine("========DETECT FACES========");
            Console.WriteLine();
            Function1.logger.LogInformation($"Starting DetectFaceExtract");
            int i = 0;
            foreach (var image in images)
            {
                IList<DetectedFace> detectedFaces;

                // Detect faces with all attributes from image url.
                //client.Face.Detect
                detectedFaces = await client.Face.DetectWithStreamAsync(new MemoryStream(image),
                        returnFaceAttributes: new List<FaceAttributeType> { FaceAttributeType.Accessories, FaceAttributeType.Age,
                FaceAttributeType.Blur, FaceAttributeType.Emotion, FaceAttributeType.Exposure, FaceAttributeType.FacialHair,
                FaceAttributeType.Gender, FaceAttributeType.Glasses, FaceAttributeType.Hair, FaceAttributeType.HeadPose,
                FaceAttributeType.Makeup, FaceAttributeType.Noise, FaceAttributeType.Occlusion, FaceAttributeType.Smile,
                FaceAttributeType.Smile },
                        // We specify detection model 1 because we are retrieving attributes.
                        detectionModel: DetectionModel.Detection01,
                        recognitionModel: recognitionModel);

                Function1.logger.LogInformation($"{detectedFaces.Count} face(s) detected from image `{i}`.");
                Function1.logger.LogInformation($"happiness are {detectedFaces[i].FaceAttributes.Emotion.Happiness} in image `{i}`.");
                i++;

                for (int d = 0; d < detectedFaces.Count; d++) {
                    detectedFacesAll.Add(detectedFaces[d]);
                }

            }
            return detectedFacesAll.ToArray();
        }
    }
}
