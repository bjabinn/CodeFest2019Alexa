using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET;
using Alexa.NET.Response;
using System.Net;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;

namespace GetTweetsCodeFest
{
    public static class GetTweetsCodeFest
    {
        [FunctionName("GetTweetsCodeFest")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {

            //log.LogInformation("C# HTTP trigger function processed a request.");

            string json = await req.ReadAsStringAsync();
            SkillRequest skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);

            var requestType = skillRequest.GetRequestType();

            SkillResponse response = null;

            switch (requestType)
            {
                case Type _ when requestType == typeof(LaunchRequest):
                    response = ResponseBuilder.Tell("Bienvenidos a CodeFest 2019");
                    response.Response.ShouldEndSession = false;
                    return new OkObjectResult(response);

                case Type _ when requestType == typeof(IntentRequest):
                    var intentRequest = (IntentRequest)skillRequest.Request;                    
                    string mensaje = string.Empty;
                    
                    switch (intentRequest.Intent.Name)
                    {
                        case "DevolverTweets":
                            int numOfTweets;
                            _ = Int32.TryParse(intentRequest.Intent.Slots["NumTweets"].Value, out numOfTweets);
                            if (numOfTweets > 10)
                            {
                                mensaje = "Uffff Eso es mucho compañero. Pideme un número más pequeño.";
                            }
                            else
                            {
                                using var client = new HttpClient
                                {
                                    BaseAddress = new Uri("https://codefesttwitterapi.azurewebsites.net")
                                };
                                client.DefaultRequestHeaders.Add("User-Agent", "C# console program");
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                var url = "/api/Twitter";
                                HttpResponseMessage response1 = await client.GetAsync(url);
                                response1.EnsureSuccessStatusCode();
                                var resp = await response1.Content.ReadAsStringAsync();

                                List<Tweet> contributors = JsonConvert.DeserializeObject<List<Tweet>>(resp);
                                

                                //return name != null ? (ActionResult)new OkObjectResult($"Hello, {name}") : new BadRequestObjectResult("Please pass a name on the query string or in the request body");

                                //mensaje = "";
                            }
                                                        
                            break;
                        case "DevolverTweetMasAlegre":
                            mensaje = "Lo siento, no hay ningun twit alegre. ¿Será culpa de las charlas?";
                            break;
                        case "DevolverTweetMasTriste":
                            mensaje = "Tu si que eres triste. ¿Has venido obligado?";
                            break;
                    }

                    response = ResponseBuilder.Tell(mensaje);
                    response.Response.ShouldEndSession = false;
                    return new OkObjectResult(response);

                case Type _ when requestType == typeof(Error):
                    response = ResponseBuilder.Tell("Algo le pasa hoy a tu boca, ¿puedes repetir? ");
                    response.Response.ShouldEndSession = false;
                    return new OkObjectResult(response);

                default:
                    response = ResponseBuilder.Tell("Upppssss algo desconocido sucedió. Efecto demo");
                    response.Response.ShouldEndSession = false;
                    return new OkObjectResult(response);
            }            
        }
    }
}
