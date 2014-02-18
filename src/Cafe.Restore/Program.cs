using System;
using System.Collections.Generic;
using Cafe.Data;
using Cafe.Json;
using RestSharp;
using RestSharp.Serializers;

namespace Cafe.Restore
{
    class Program
    {
        static RestClient restClient;

        static void Main(string[] args)
        {
            restClient = new RestClient("http://cafeserver.aws.af.cm");
            
            using (var dc = new CafeDataContext())
            {
                RestoreFromSQL(dc.Products, "/products", x => new ProductJson
                                                                  {
                                                                      _id = x._id,
                                                                      LocationId = x.LocationId,
                                                                      Name = x.Name,
                                                                      Price = Convert.ToDouble(x.Price),
                                                                      Priority = x.Priority,
                                                                      Tag = x.Tag,
                                                                      TaxRate = Convert.ToDouble(x.TaxRate)
                                                                  });

                RestoreFromSQL(dc.Tags, "/tags", x => new TagJson
                                                          {
                                                              _id = x._id,
                                                              Name = x.Name,
                                                              LocationId = x.LocationId
                                                          });

                RestoreFromSQL(dc.Employees, "/employees", x => new EmployeeJson
                                                                    {
                                                                        _id = x._id,
                                                                        Name = x.Name,
                                                                        LocationId = x.LocationId
                                                                    });

                RestoreFromSQL(dc.Vendors, "/vendors", x => new VendorJson
                                                                {
                                                                    _id = x._id,
                                                                    Name = x.Name,
                                                                    LocationId = x.LocationId,
                                                                    Priority = x.Priority
                                                                });
            }

            Console.WriteLine("Done.");
            Console.ReadKey();
        }

        static void RestoreFromSQL<T1, T2>(IEnumerable<T2> objects, string resource, Func<T2, T1> map) where T1 : new()
        {
            Console.WriteLine(resource);
            restClient.Execute(new RestRequest(resource, Method.DELETE));

            foreach (T2 obj in objects)
            {
                var request = new RestRequest(resource, Method.POST)
                                  {
                                      RequestFormat = DataFormat.Json,
                                      JsonSerializer = new JsonSerializer()
                                  };
                T1 mappedObj = map.Invoke(obj);
                request.AddBody(mappedObj);
                IRestResponse<T1> response = restClient.Execute<T1>(request);
                if (response.ErrorException != null) throw response.ErrorException;
                Console.Write(".");
            }
            Console.WriteLine("!");
        }
    }
}