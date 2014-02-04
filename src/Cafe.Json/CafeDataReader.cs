using System;
using RestSharp;

namespace Cafe.Json
{
    public class CafeDataReader
    {
        readonly RestClient _client;

        public CafeDataReader(RestClient client)
        {
            _client = client;
        }

        public T GetData<T>(string resource, Action<RestRequest> restRequest = null,
                            bool noLimit = true) where T : new()
        {
            var request = new RestRequest(resource, Method.GET);
            if (restRequest != null) restRequest(request);
            if (noLimit)
                request.AddParameter("noLimit", "true");
            IRestResponse<T> response = _client.Execute<T>(request);
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            T data = response.Data;
            return data;
        }
    }
}