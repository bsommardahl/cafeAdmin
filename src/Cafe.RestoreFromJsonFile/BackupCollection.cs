using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Cafe.RestoreFromJsonFile
{
    public class BackupCollection
    {
        public string Collection { get; set; }
        public List<JObject> Data { get; set; }
    }
}
