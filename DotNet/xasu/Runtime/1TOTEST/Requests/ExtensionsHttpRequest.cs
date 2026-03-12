using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Xasu.Requests
{
    public static class ExtensionsHttpRequest
    {
        public static HttpRequestMessage ToHttpRequestMessage(this HttpRequest myRequest)
        {
            HttpRequestMessage request = new HttpRequestMessage();

            // TODO: Check if calling this before works correctly
            if (myRequest.form != null)
            {
                string formUrlEncoded = myRequest.form
                    .Select(kv => string.Format("{0}={1}", kv.Key, kv.Value))
                    .Aggregate((p1, p2) => string.Format("{0}&{1}", p1, p2));
                myRequest.contentType = "application/x-www-form-urlencoded";
                myRequest.content = Encoding.UTF8.GetBytes(formUrlEncoded);
            }

            switch (myRequest.method.ToUpper())
            {
                case "GET":
                    request.Method = HttpMethod.Get;
                    break;
                case "POST":
                    request.Method = HttpMethod.Post;
                    // TODO: Check
                    request.Content = new ByteArrayContent(myRequest.content);
                    //request.Content = new FormUrlEncodedContent(new Dictionary<string, string>());
                    break;
                case "PUT":
                    request.Method = HttpMethod.Put;
                    request.Content = new ByteArrayContent(myRequest.content);
                    break;
                case "DELETE":
                    request.Method = HttpMethod.Delete;
                    break;
            }
            request.RequestUri = new Uri(myRequest.url);

            //if (myRequest.form != null)
            //{
            //    string formUrlEncoded = myRequest.form
            //        .Select(kv => string.Format("{0}={1}", kv.Key, kv.Value))
            //        .Aggregate((p1, p2) => string.Format("{0}&{1}", p1, p2));
            //    myRequest.contentType = "application/x-www-form-urlencoded";
            //    myRequest.content = Encoding.UTF8.GetBytes(formUrlEncoded);
            //}

            // Set content type
            string contentType = myRequest.GetContentType();
            if (myRequest.content != null && myRequest.content.ToString().Length > 0
                && !string.IsNullOrEmpty(contentType) && request.Content != null)
            {
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            }

            // Set other headers
            myRequest.headers["Content-Type"] = contentType;
            foreach (var header in myRequest.headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return request;
        }

        public static string GetContentType(this HttpRequest req)
        {
            string contentType = "application/octet-stream";
            if (!string.IsNullOrEmpty(req.contentType))
            {
                contentType = req.contentType;
            }
            else if (req.headers.ContainsKey("Content-Type"))
            {
                contentType = req.headers["Content-Type"];
            }

            return contentType;
        }
    }
}
