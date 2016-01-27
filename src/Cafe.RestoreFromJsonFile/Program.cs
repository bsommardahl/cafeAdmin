using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cafe.Data;
using Cafe.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Cafe.RestoreFromJsonFile
{
    class Program
    {
        static void Main(string[] args)
        {
            var backupService = new CafeDataBackup(new JsonFileCafeDataReader("backup.json"));
            backupService.Go();                        
        }
    }

    public class JsonFileCafeDataReader : ICafeDataReader
    {
        readonly string _backupJsonFile;

        public JsonFileCafeDataReader(string backupJsonFile)
        {
            _backupJsonFile = backupJsonFile;
        }

        public IEnumerable<T> GetData<T>(string resource, Action<RestRequest> restRequest = null, bool noLimit = true) where T : new()
        {
            var backup = GetBackupFromJson(GetJsonFromFile(_backupJsonFile));
            var collectionToRead = backup.CafeBackup.Collections.First(x => String.Equals(x.Collection.Replace("/",""), resource, StringComparison.CurrentCultureIgnoreCase));

            var data = collectionToRead.Data.Select(x => x.ToObject<T>()).ToList();
            return data;
        }

        private static BackupResponse GetBackupFromJson(string json)
        {
            var obj = JsonConvert.DeserializeObject<BackupResponse>(json);
            return obj;
        }

        private static string GetJsonFromFile(string p)
        {
            var text = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/backup.json");
            return text;
        }
    }
}
