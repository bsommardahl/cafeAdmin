using System;
using System.IO;
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
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            new CafeDataBackup("http://cafeserver.aws.af.cm").Go();
            return Json(new {success = true, message = stringWriter.GetStringBuilder().ToString()}, JsonRequestBehavior.AllowGet);
        }
    }
}