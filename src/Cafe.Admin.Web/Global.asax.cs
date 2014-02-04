using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using log4net.Config;

namespace Cafe.Admin.Web
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            AttributeRoutingHttpConfig.RegisterRoutes(GlobalConfiguration.Configuration.Routes);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            AttributeRoutingConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            XmlConfigurator.Configure();
        }
    }
}