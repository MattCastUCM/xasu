using System;

namespace Xasu.Requests
{
    public class HttpResponse
    {
        public int status { get; set; }
        public String contentType { get; set; }
        public byte[] content { get; set; }
        public DateTime lastModified { get; set; }
        public String etag { get; set; }
        public Exception ex { get; set; }

        /*public HttpResponse(HttpWebResponse webResp)
        {
            status = webResp.StatusCode;
            contentType = webResp.ContentType;
            etag = webResp.Headers.Get("Etag");
            lastModified = webResp.LastModified;

            using (var stream = webResp.GetResponseStream())
            {
                content = ReadFully(stream, (int)webResp.ContentLength);
            }
        }*/
    }
}
