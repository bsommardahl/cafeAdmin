namespace Cafe.DailyReports
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Drawing.Printing;
    using System.Linq;
    using System.Management;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using Cafe.Data;

    using Nancy;
    using Nancy.ModelBinding;

    public class DailyReportModule : NancyModule
    {
        #region Static Fields

        private static DateTime dataImported;

        #endregion

        #region Fields

        private readonly string printerDeviceName = ConfigurationManager.AppSettings["printerName"];

        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["CafeReport.Properties.Settings.CafeConnectionString"].ToString();

        private readonly TaskScheduler sta = new StaTaskScheduler(1);

        #endregion

        #region Constructors and Destructors

        public DailyReportModule()
        {
            this.Get["/daily"] = o => this.ViewDailyReport();

            this.Get["/print/daily"] = o => this.PrintDailyReport();
        }

        #endregion

        #region Methods

        private static IEnumerable<ProductModel> Products(
            IQueryable<Order> orders, IEnumerable<Product> products, string orderBy)
        {
            List<IGrouping<string, OrderItem>> list = orders.SelectMany(x => x.OrderItems).GroupBy(x => x.Name).ToList();

            IOrderedEnumerable<IGrouping<string, OrderItem>> orderedEnumerable;
            if (orderBy == "product")
            {
                orderedEnumerable = list.OrderBy(x => x.Key);
            }
            else if (orderBy == "tag")
            {
                orderedEnumerable = list.OrderBy(x => x.First().Tags);
            }
            else if (orderBy == "sales")
            {
                orderedEnumerable = list.OrderBy(x => x.Count() * x.First().Price);
            }
            else
            {
                orderedEnumerable = list.OrderBy(x => x.Count());
            }

            IEnumerable<ProductModel> productModels = orderedEnumerable.Select(
                x =>
                    {
                        OrderItem orderItem = x.First();
                        int quantity = x.Count();
                        Product product = products.FirstOrDefault(p => p.Name == orderItem.Name);
                        if (product == null)
                        {
                            return new ProductModel();
                        }
                        double totalCost = Convert.ToDouble((quantity * product.Cost));

                        decimal itemPrice = orderItem.Price;
                        //decimal itemPrice = product.Price;

                        double totalTax = Convert.ToDouble(quantity * (itemPrice * product.TaxRate));
                        double totalSales = Convert.ToDouble(quantity * itemPrice);
                        double taxRate = Convert.ToDouble(product.TaxRate) * 100;
                        double price = Convert.ToDouble(itemPrice);
                        double totalProfit = totalSales - totalCost;

                        return new ProductModel
                            {
                                Name = orderItem.Name,
                                Tag = orderItem.Tags,
                                Price = price,
                                TaxRate = taxRate,
                                Quantity = quantity,
                                TotalSales = totalSales,
                                TotalTax = totalTax,
                                TotalCost = totalCost,
                                TotalProfit = totalProfit
                            };
                    });

            if (orderBy == "profit")
            {
                productModels = productModels.OrderBy(x => x.TotalProfit);
            }

            return productModels.ToList();
        }

        private static void SetAsDefaultPrinter(string printerDevice)
        {
            bool installed = false;
            foreach (object printer in PrinterSettings.InstalledPrinters)
            {
                //verify that the printer exists here
                if (printer.ToString() == printerDevice)
                {
                    installed = true;
                    break;
                }
            }

            if (!installed)
            {
                throw new Exception(string.Format("{0} not installed on this computer.", printerDevice));
            }

            string path = "win32_printer.DeviceId='" + printerDevice + "'";
            using (var printer = new ManagementObject(path))
            {
                printer.InvokeMethod("SetDefaultPrinter", null, null);
            }
            return;
        }

        private DailyReportInput GetDailyReportInputWithDefaults()
        {
            var dailyReportInput = this.Bind<DailyReportInput>();

            if (dailyReportInput.Start == DateTime.MinValue)
            {
                dailyReportInput.Start = DateTime.Now.Date;
            }
            if (dailyReportInput.End == DateTime.MinValue)
            {
                dailyReportInput.End = DateTime.Now.Date;
            }
            if (string.IsNullOrEmpty(dailyReportInput.OrderBy))
            {
                dailyReportInput.OrderBy = "product";
            }
            return dailyReportInput;
        }

        private DailySheetModel GetDailySheetData(DateTime start, DateTime end, string orderBy)
        {
            if (dataImported < DateTime.Now.AddMinutes(-5))
            {
                new CafeDataBackup("http://cafeserver.aws.af.cm").Go();
                dataImported = DateTime.Now;
            }

            start = start.Date.AddMinutes(1);
            end = end.AddDays(1).Date.AddMinutes(-1);

            using (var dc = new CafeDataContext(connectionString))
            {
                IQueryable<Order> orders = dc.Orders.Where(x => x.Paid.Date >= start.Date && x.Paid <= end);
                List<Product> products = dc.Products.ToList();

                IQueryable<Debit> debits =
                    dc.Debits.Where(
                        x => x.CreatedDate.Date >= start.Date && x.CreatedDate <= end && !x.OperationalExpense);

                IEnumerable<ProductModel> productModels = Products(orders, products, orderBy);
                double totalCredit = productModels.Any()
                                         ? productModels.Sum(x => x.TotalSales) + productModels.Sum(x => x.TotalTax)
                                         : 0;

                double totalDebits = debits.Any() ? Convert.ToDouble(debits.Sum(x => x.Amout)) : 0;
                double seed = 500.00;
                double cashInRegister = (totalCredit - totalDebits) + seed;

                return new DailySheetModel
                    {
                        Products = productModels,
                        TotalCredit = totalCredit,
                        Debits = this.MapDebits(debits),
                        TotalDebits = totalDebits,
                        CashInRegister = cashInRegister,
                        Seed = seed,
                        FinalTotal = cashInRegister - seed,
                        StartDate = string.Format("{0}-{1}-{2}", start.Day, start.Month, start.Year),
                        EndDate = string.Format("{0}-{1}-{2}", end.Day, end.Month, end.Year),
                        DataImported = dataImported
                    };
            }
        }

        private IEnumerable<DebitModel> MapDebits(IQueryable<Debit> debits)
        {
            return
                debits.Select(
                    x =>
                    new DebitModel
                        {
                            Amout = x.Amout,
                            CreatedDate = x.CreatedDate,
                            Description = x.Description,
                            LocationId = x.LocationId,
                            OperationalExpense = x.OperationalExpense,
                            TaxPaid = x.TaxPaid,
                            VendorId = x.VendorId,
                            VendorName = x.VendorName,
                            _id = x._id
                        }).ToList();
        }

        private dynamic PrintDailyReport()
        {
            DailyReportInput dailyReportInput = this.GetDailyReportInputWithDefaults();

            string htmlPath = string.Format(
                "http://{0}:{1}/daily?start={2}&end={3}&orderBy={4}",
                this.Request.Url.HostName,
                this.Request.Url.Port,
                dailyReportInput.Start.ToShortDateString(),
                dailyReportInput.End.ToShortDateString(),
                dailyReportInput.OrderBy);

            this.PrintHtml(htmlPath, this.printerDeviceName);

            return View["success"];
        }

        private void PrintHtml(string htmlPath, string printerDevice)
        {
            if (!string.IsNullOrEmpty(printerDevice))
            {
                SetAsDefaultPrinter(printerDevice);
            }

            Task.Factory.StartNew(
                () => this.PrintOnStaThread(htmlPath), CancellationToken.None, TaskCreationOptions.None, this.sta).Wait(
                    );
        }

        private void PrintOnStaThread(string htmlPath)
        {
            const short PRINT_WAITFORCOMPLETION = 2;
            const int OLECMDID_PRINT = 6;
            const int OLECMDEXECOPT_DONTPROMPTUSER = 2;
            using (var browser = new WebBrowser())
            {
                browser.Navigate(htmlPath);
                while (browser.ReadyState != WebBrowserReadyState.Complete)
                {
                    Application.DoEvents();
                }

                dynamic ie = browser.ActiveXInstance;
                ie.ExecWB(OLECMDID_PRINT, OLECMDEXECOPT_DONTPROMPTUSER, PRINT_WAITFORCOMPLETION);
            }
        }

        private dynamic ViewDailyReport()
        {
            DailyReportInput dailyReportInput = this.GetDailyReportInputWithDefaults();

            DailySheetModel dailySheetModel = this.GetDailySheetData(
                dailyReportInput.Start, dailyReportInput.End, dailyReportInput.OrderBy);
            return this.View["DailySheet", dailySheetModel];
        }

        #endregion
    }
}