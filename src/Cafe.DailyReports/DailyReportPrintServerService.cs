namespace Cafe.DailyReports
{
    using System;
    using System.Configuration;

    using Nancy.Hosting.Self;
    using Nancy.TinyIoc;
    using Nancy.ViewEngines.Razor;

    using log4net;

    public class DailyReportPrintServerService
    {
        #region Fields

        private readonly ILog _log;

        private readonly int _port;

        private NancyHost _nancyHost;

        #endregion

        #region Constructors and Destructors

        public DailyReportPrintServerService(int port, ILog log)
        {
            this._port = port;
            this._log = log;
        }

        #endregion

        #region Public Methods and Operators

        public void Start()
        {
            string url = "http://localhost:" + this._port;
            string host = ConfigurationManager.AppSettings["host"];
            if (host != null)
            {
                url = host;
            }

            try
            {
                this._nancyHost =
                    new NancyHost(
                        new HostConfiguration
                            {
                                UrlReservations = new UrlReservations { CreateAutomatically = true },
                                RewriteLocalhost = true,
                            },
                        new Uri(url));
                this._nancyHost.Start();
                //TinyIoCContainer.Current.Register<RazorViewEngine>();
                Console.WriteLine("Listening at {0}...", url);
            }
            catch (Exception ex)
            {
                this._log.Error("Could not start daily report print server.", ex);
                throw ex;
            }
        }

        public void Stop()
        {
            this._nancyHost.Dispose();
        }

        #endregion
    }
}