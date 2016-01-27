using System;
using System.Collections.Generic;
using RestSharp;

namespace Cafe.Json
{
    public interface ICafeDataReader
    {
        IEnumerable<T> GetData<T>(string resource, Action<RestRequest> restRequest = null,
            bool noLimit = true) where T : new();
    }
}