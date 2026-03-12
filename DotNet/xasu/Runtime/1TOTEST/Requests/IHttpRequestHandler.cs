using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xasu.Requests
{
    public interface IHttpRequestHandler
    {
        Task<HttpResponse> SendRequest(HttpRequest myRequest, IProgress<float> progress = null);

        string AppendParamsToExistingQueryString(string currentQueryString, IEnumerable<KeyValuePair<string, string>> parameters);
    }
}
