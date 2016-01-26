using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;

namespace Cafe.Json
{
    public class CafeDataReader : ICafeDataReader
    {
        readonly RestClient _client;

        public CafeDataReader(RestClient client)
        {
            _client = client;
        }

        public IEnumerable<T> GetData<T>(string resource, Action<RestRequest> restRequest = null,
                            bool noLimit = true) where T : new()
        {
            var request = new RestRequest(resource, Method.GET);
            if (restRequest != null) restRequest(request);
            if (noLimit)
                request.AddParameter("noLimit", "true");
            IRestResponse<List<T>> response = _client.Execute<List<T>>(request);
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            if(response.StatusCode!=HttpStatusCode.OK)
            {
                throw new Exception(response.ErrorMessage);
            }
            List<T> data = response.Data;
            return data;
        }
    }
}