//
// API.AI .NET SDK - client-side libraries for API.AI
// =================================================
//
// Copyright (C) 2015 by Speaktoit, Inc. (https://www.speaktoit.com)
// https://www.api.ai
//
// ***********************************************************************************************************************
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
// an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.
//
// ***********************************************************************************************************************

using System;
using System.Configuration;
using System.Net;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ApiAiSDK.Model;
using ApiAiSDK.Http;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AiApiSdk.Logging;

namespace ApiAiSDK
{
    public class AIDataService
    {
        private readonly AIConfiguration config;

        public string SessionId { get; }

        public AIDataService(AIConfiguration config)
        {
            this.config = config;

            if (string.IsNullOrEmpty(config.SessionId))
            {
                SessionId = Guid.NewGuid().ToString();
            }
            else
            {
                SessionId = config.SessionId;
            }
        }

        public AIResponse Request(AIRequest request)
        {
            request.Language = config.Language.code;
            request.Timezone = TimeZone.CurrentTimeZone.StandardName;
            request.SessionId = SessionId;
            
            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create(config.RequestUrl);
                httpRequest.Method = "POST";
                httpRequest.ContentType = "application/json; charset=utf-8";
                httpRequest.Accept = "application/json";
                        
                httpRequest.Headers.Add("Authorization", "Bearer " + config.ClientAccessToken);
                        
                var jsonSettings = new JsonSerializerSettings
                { 
                    NullValueHandling = NullValueHandling.Ignore
                };
            
                var jsonRequest = JsonConvert.SerializeObject(request, Formatting.None, jsonSettings);

                if (config.DebugLog)
                {
                    Debug.WriteLine("Request: " + jsonRequest);
                }

                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    streamWriter.Write(jsonRequest);
                    streamWriter.Close();
                }

                var httpResponse = httpRequest.GetResponse() as HttpWebResponse;
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    if (config.DebugLog)
                    {
                        Debug.WriteLine("Response: " + result);
                    }

                    var aiResponse = JsonConvert.DeserializeObject<AIResponse>(result);

                    CheckForErrors(aiResponse);

                    return aiResponse;
                }

            }
            catch (Exception e)
            {
                throw new AIServiceException(e);
            }
        }

       

        public AIResponse VoiceRequestOld(Stream voiceStream, RequestExtras requestExtras = null)
        {
            var request = new AIRequest();
            request.Language = config.Language.code;
            request.Timezone = TimeZone.CurrentTimeZone.StandardName;
            request.SessionId = SessionId;

            if (requestExtras != null)
            {
                requestExtras.CopyTo(request);
            }

            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create(config.RequestUrl);
                httpRequest.Method = "POST";
                httpRequest.Accept = "application/json";
                
                httpRequest.Headers.Add("Authorization", "Bearer " + config.ClientAccessToken);
                
                var jsonSettings = new JsonSerializerSettings
                { 
                    NullValueHandling = NullValueHandling.Ignore
                };
                
                var jsonRequest = JsonConvert.SerializeObject(request, Formatting.None, jsonSettings);

                if (config.DebugLog)
                {
                    Debug.WriteLine("Request: " + jsonRequest);
                }

                var multipartClient = new MultipartHttpClient(httpRequest);
                multipartClient.connect();

                multipartClient.addStringPart("request", jsonRequest);
                multipartClient.addFilePart("voiceData", "voice.wav", voiceStream);

                multipartClient.finish();

                var responseJsonString = multipartClient.getResponse();

                if (config.DebugLog)
                {
                    Debug.WriteLine("Response: " + responseJsonString);
                }

                var aiResponse = JsonConvert.DeserializeObject<AIResponse>(responseJsonString);

                CheckForErrors(aiResponse);

                return aiResponse;

            }
            catch (Exception e)
            {
                throw new AIServiceException(e);
            }
        }

        public AIResponse VoiceRequest(Stream voiceStream, RequestExtras requestExtras = null)
        {
            var request = new AIRequest();
            request.Language = config.Language.code;
            request.Timezone = TimeZone.CurrentTimeZone.StandardName;
            request.SessionId = SessionId;

            if (requestExtras != null)
            {
                requestExtras.CopyTo(request);
            }

            try
            {
                //var multipartClient = new MultipartHttpClient(httpRequest);
                HttpClient httpClient = new HttpClient(new LoggingHandler(new HttpClientHandler()));

                MultipartFormDataContent form = new MultipartFormDataContent();
                var headers = httpClient.DefaultRequestHeaders;
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                headers.Authorization = new AuthenticationHeaderValue("Bearer", config.ClientAccessToken);

                //JSON Request
                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };
                var jsonRequest = JsonConvert.SerializeObject(request, Formatting.None, jsonSettings);

                if (config.DebugLog)
                {
                    Debug.WriteLine("Request: " + jsonRequest);
                }


                StringContent jsonPart = new StringContent(jsonRequest.ToString());
                jsonPart.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
                jsonPart.Headers.ContentDisposition.Name = "request";
                jsonPart.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
     

                //AUDIO
                StreamContent filePart = new StreamContent(voiceStream);
                filePart.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
                filePart.Headers.ContentDisposition.Name = "voiceData";
                filePart.Headers.ContentDisposition.FileName = "voice.wav";
                filePart.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

                form.Add(jsonPart);
                form.Add(filePart);

                var response = httpClient.PostAsync(config.RequestUrl, form).Result;

                response.EnsureSuccessStatusCode();
                httpClient.Dispose();
                string sd = response.Content.ReadAsStringAsync().Result;

                var aiResponse = JsonConvert.DeserializeObject<AIResponse>(sd);

                CheckForErrors(aiResponse);

                return aiResponse;

            }
            catch (Exception e)
            {
                throw new AIServiceException(e);
            }
        }

        public bool ResetContexts()
        {
            var cleanRequest = new AIRequest("empty_query_for_resetting_contexts");
            cleanRequest.ResetContexts = true;
            try {
                var response = Request(cleanRequest);
                return !response.IsError;
            } catch (AIServiceException e) {
                Debug.WriteLine("Exception while contexts clean." + e);
                return false;
            }
        }

        static void CheckForErrors(AIResponse aiResponse)
        {
            if (aiResponse == null)
            {
                throw new AIServiceException("API.AI response parsed as null. Check debug log for details.");
            }

            if (aiResponse.IsError)
            {
                throw new AIServiceException(aiResponse);
            }
        }
    }
}
