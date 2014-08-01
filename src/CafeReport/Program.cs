using System;
using Cafe.Data;

namespace CafeReport
{
    class Program
    {
        static void Main(string[] args)
        {
            var backup = new CafeDataBackup("http://cafeserver.aws.af.cm");
            backup.Go();
            Console.WriteLine("Done.");
            //Console.ReadKey();
        }
    }
}