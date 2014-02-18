using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using AttributeRouting.Web.Mvc;
using Cafe.Data;

namespace Cafe.Admin.Web.Controllers
{
    public class BackupController : Controller
    {
        [GET("/backup/trigger")]
        public ActionResult TriggerBackup()
        {
            return this.Json(new {success = true, message = this.Backup()}, JsonRequestBehavior.AllowGet);
        }

        private string Backup()
        {
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            new CafeDataBackup("http://cafeserver.aws.af.cm").Go();
            var output = stringWriter.GetStringBuilder().ToString();
            return output;
        }
    }
}