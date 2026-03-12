using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xasu.Exceptions;
using Xasu.Util;

namespace Xasu.Requests
{
    public class HttpRequestHandler : IHttpRequestHandler
    {
        readonly HttpClient _httpClient;

        public HttpRequestHandler() 
        {
            _httpClient = new HttpClient();
        }

        public virtual async Task<HttpResponse> SendRequest(HttpRequest myRequest, IProgress<float> progress = null)
        {
            bool isSimvaStatements = IsSimvaStatements(myRequest);

            // Set URL
            if (string.IsNullOrEmpty(myRequest.url))
            {
                throw new ArgumentNullException("RequestsUtility.DoRequest needs the final URL to make que request (without the query parameters)");
            }

            // Set query params
            string qs = string.Empty;
            qs = AppendParamsToExistingQueryString(qs, myRequest.queryParams);
            if (!string.IsNullOrEmpty(qs))
            {
                myRequest.url += "?" + qs;
            }

            // Await auth
            if (myRequest.authorization != null)
            {
                await myRequest.authorization.UpdateParamsForAuth(myRequest);
            }

            // Perform request
            HttpResponse result = null;
            try
            {
                if (myRequest.policy != null)
                {
                    result = await myRequest.policy.ExecuteAsync(
                        async (_) =>
                        {
                            return await DoRequest(myRequest);
                        }, new CancellationToken(), true);
                }
                else
                {
                    result = await DoRequest(myRequest);
                }

                if (isSimvaStatements)
                {
                    var jArray = JArray.Parse(Encoding.UTF8.GetString(myRequest.content));
                    var idsArray = new JArray();
                    foreach (JObject state in jArray)
                    {
                        idsArray.Add(state.GetValue("id").ToString());
                    }
                    result.content = Encoding.UTF8.GetBytes(idsArray.ToString());
                }
            }
            catch (APIException ex)
            {
                XasuTracker.Log(string.Format("[REQUESTS ({0})] I've seen API exceptions here... ", Thread.CurrentThread.ManagedThreadId));
                result = ex.Response;
            }
            catch (NetworkException)
            {
                XasuTracker.Log(string.Format("[REQUESTS ({0})] I've seen network exceptions here... ", Thread.CurrentThread.ManagedThreadId));
                throw;
            }
            return result;
        }

        public virtual string AppendParamsToExistingQueryString(string currentQueryString, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            foreach (KeyValuePair<String, String> entry in parameters)
            {
                if (!string.IsNullOrEmpty(currentQueryString))
                {
                    currentQueryString += "&";
                }
                currentQueryString += System.Net.WebUtility.UrlEncode(entry.Key) + "=" + System.Net.WebUtility.UrlEncode(entry.Value);
            }

            return currentQueryString;
        }


        protected virtual async Task<HttpResponse> DoRequest(HttpRequest webRequest, IProgress<float> progress = null)
        {
            HttpRequestMessage requestMsg = webRequest.ToHttpRequestMessage();

            XasuTracker.Log(string.Format("[REQUESTS ({2})] {1} Requesting \"{0}\"", webRequest.url, webRequest.method, Thread.CurrentThread.ManagedThreadId));

            HttpResponse httpResponse = null;
            try
            {
                HttpResponseMessage asyncReq = await _httpClient.SendAsync(requestMsg);
                // TODO: Notify progress

                NetworkInfo.Worked();

                httpResponse = new HttpResponse()
                {
                    status = (int)asyncReq.StatusCode,
                    content = await asyncReq.Content.ReadAsByteArrayAsync(),
                    contentType = asyncReq.Content.Headers.ContentType?.ToString(),
                    etag = asyncReq.Headers.ETag?.Tag
                };

                // API / Http Exception
                if (!asyncReq.IsSuccessStatusCode)
                {
                    //XasuTracker.LogError($"{(int)asyncReq.StatusCode} {asyncReq.Content.ToString()} {httpResponse}");
                    XasuTracker.LogWarning(JsonConvert.SerializeObject(requestMsg));
                    XasuTracker.LogError(JsonConvert.SerializeObject(asyncReq));
                    throw new APIException((int)asyncReq.StatusCode, asyncReq.Content.ToString(), httpResponse);
                }
                else
                {
                    XasuTracker.Log(string.Format("[REQUESTS ({4})] {1} Request to \"{0}\" succedded ({2}): \"{3}\"",
                    webRequest.url, webRequest.method, httpResponse.status, asyncReq.Content.ToString(), Thread.CurrentThread.ManagedThreadId));

                }
            }
            // Network Error Exception
            catch (HttpRequestException ex)
            {
                NetworkInfo.Failed();
                throw new NetworkException(ex.Message);
            }

            
            return httpResponse;
        }



        protected virtual bool IsSimvaStatements(HttpRequest myRequest)
        {
            // Simva Special cases
            if (XasuTracker.TrackerConfig != null && XasuTracker.TrackerConfig.Simva)
            {
                // statements endpoint is in /result
                if (myRequest.url.EndsWith("statements"))
                {
                    myRequest.url = myRequest.url.Replace("statements", "result");
                    return true;
                }
            }

            return false;
        }


    }
}
