using System;
using System.IO;
using RestSharp;

namespace CafeBackup
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new RestClient("http://cafeserver.aws.af.cm");
            var resources = new[]
                                {
                                    "products", "employees", "tags", "users", "vendors", "times", "debits", "orders"
                                };
            foreach (var resource in resources)
            {
                Console.WriteLine(resource);
                IRestResponse response = client.Execute(new RestRequest(resource + "?noLimit=true", Method.GET));
                File.WriteAllText(resource + ".json", response.Content);    
            }
        }
    }
}