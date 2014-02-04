using System.Web;
using System.Web.Mvc;

namespace Cafe.Admin.Web
{
    public class MvcFilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new HandleErrorAttribute());

            filters.Add(new LoggingExceptionFilter());
        }
    }
}