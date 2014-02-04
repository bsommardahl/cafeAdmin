using System.Configuration;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Cafe.Admin.Web.App_Start;
using log4net;
using log4net.Config;

namespace Cafe.Admin.Web
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            AttributeRoutingHttpConfig.RegisterRoutes(GlobalConfiguration.Configuration.Routes);
            MvcFilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            ApiFilterConfig.RegisterFilters(GlobalConfiguration.Configuration.Filters);
            AttributeRoutingConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            XmlConfigurator.Configure();

            var connectionString = ConfigurationManager.ConnectionStrings["CafeReport.Properties.Settings.CafeConnectionString"];
            ILog Log = LogManager.GetLogger(this.GetType());
            Log.Info("Using connection string: " + connectionString);
        }
    }
}