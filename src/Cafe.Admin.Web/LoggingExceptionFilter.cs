using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Web;
using System.Web.Mvc;
using log4net;

namespace Cafe.Admin.Web
{
    public class LoggingExceptionFilter : IExceptionFilter
    {
        #region IExceptionFilter Members

        public void OnException(ExceptionContext filterContext)
        {
            Exception exception = filterContext.Exception;

            if (exception is ValidationException)
                return;

            if (exception is HttpException &&
                exception.Message.Contains("File does not exist"))
                return;

            string userName = string.Format("User: {0}",
                                            HttpContext.Current.Session == null
                                                ? "empty"
                                                : HttpContext.Current.Session["Username"]);
            string remoteIp = "IP Address: " + GetIP();
            string pageUrl = "Page url: " + HttpContext.Current.Request.Url;
            string userAgent = "Browser: " + HttpContext.Current.Request.UserAgent;

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("##### User Details #####");
            sb.AppendLine();
            sb.AppendLine(userName);
            sb.AppendLine(remoteIp);
            sb.AppendLine(pageUrl);
            sb.AppendLine(userAgent);
            sb.AppendLine();

            sb.AppendLine("##### Exception Details #####");
            ILog Log = LogManager.GetLogger(filterContext.Controller.ControllerContext.Controller.GetType());

            Log.Error(sb.ToString(), exception);
        }

        #endregion

        public static string GetIP()
        {
            string ip =
                HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            return ip;
        }
    }
}