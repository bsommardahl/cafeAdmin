using System;
using Cafe.Data;
using Cafe.Json;
using RestSharp;

namespace CafeReport
{
    class Program
    {
        static void Main(string[] args)
        {
            var backup = new CafeDataBackup(new CafeDataReader(new RestClient("https://gringo-cafe-server.herokuapp.com")));
            backup.Go();
            Console.WriteLine("Done.");
            //Console.ReadKey();
        }
    }
}