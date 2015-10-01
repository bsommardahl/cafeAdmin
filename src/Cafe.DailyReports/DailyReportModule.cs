using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Cafe.DailyReports
{
    public class DailyReportModule : NancyModule
    {
        #region Static Fields

        static DateTime dataImported;

        #endregion

        #region Fields

        readonly string connectionString =
            ConfigurationManager.ConnectionStrings["CafeReport.Properties.Settings.CafeConnectionString"].ToString();

        readonly string printerDeviceName = ConfigurationManager.AppSettings["printerName"];

        readonly TaskScheduler sta = new StaTaskScheduler(1);

        #endregion

        #region Constructors and Destructors

        public DailyReportModule()
        {
            Get["/daily/start"] = o => View["dailyStart"];

            Get["/daily"] = o =>
                                {
                                    DailyReportInput input = GetDailyReportInputWithDefaults();

                                    if (input.Action == "Modificar Gastos")
                                        return ViewModifyDebits(input);

                                    return ViewDailyReport(input);                                    
                                };



            Get["/print/daily"] = o => PrintDailyReport();
        }

        dynamic ViewModifyDebits(DailyReportInput input)
        {
            using (var dc = new CafeDataContext(connectionString))
            {
                var debits = GetDebitsInDateRange(input.Start, input.End.Value, dc);

                var types = new List<DebitTypeModel>
                                {
                                    new DebitTypeModel("Administracion"),
                                    new DebitTypeModel("Venta"),
                                    new DebitTypeModel("Compra"),
                                    new DebitTypeModel("Operacion"),
                                };

                return View["modifyDebits", new DebitModificationModel(input.Start, input.End.Value, MapDebits(debits), types)];
            }
        }

        #endregion

        #region Methods

        static IEnumerable<AggregateSalesModel> Sales(
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
                orderedEnumerable = list.OrderBy(x => x.Count()*x.First().Price);
            }
            else
            {
                orderedEnumerable = list.OrderBy(x => x.Count());
            }

            IEnumerable<AggregateSalesModel> productModels = orderedEnumerable.Select(
                x =>
                    {
                        OrderItem orderItem = x.First();
                        int quantity = x.Count();
                        Product product = products.FirstOrDefault(p => p.Name == orderItem.Name);
                        if (product == null)
                        {
                            return new AggregateSalesModel();
                        }
                        double totalCost = Convert.ToDouble((quantity*product.Cost));

                        decimal itemPrice = orderItem.Price;
                        //decimal itemPrice = product.Price;

                        double totalTax = Convert.ToDouble(quantity*(itemPrice*product.TaxRate));
                        double totalSales = Convert.ToDouble(quantity*itemPrice);
                        double taxRate = Convert.ToDouble(product.TaxRate)*100;
                        double price = Convert.ToDouble(itemPrice);
                        double totalProfit = totalSales - totalCost;

                        return new AggregateSalesModel
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

        static void SetAsDefaultPrinter(string printerDevice)
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

        DailyReportInput GetDailyReportInputWithDefaults()
        {
            var dailyReportInput = this.Bind<DailyReportInput>();

            if (dailyReportInput.Start == DateTime.MinValue)
            {
                dailyReportInput.Start = DateTime.Now.Date;
            }
            if (!dailyReportInput.End.HasValue)
            {
                dailyReportInput.End = dailyReportInput.Start;
            }
            if (string.IsNullOrEmpty(dailyReportInput.OrderBy))
            {
                dailyReportInput.OrderBy = "product";
            }
            return dailyReportInput;
        }

        DailySheetModel GetDailySheetData(DateTime start, DateTime end, string orderBy)
        {
            //if (dataImported < DateTime.Now.AddMinutes(-5))
            //{
            //    new CafeDataBackup("http://cafeserver.aws.af.cm").Go();
            //    dataImported = DateTime.Now;
            //}

            start = start.Date.AddMinutes(1);
            end = end.AddDays(1).Date.AddMinutes(-1);

            using (var dc = new CafeDataContext(connectionString))
            {
                IQueryable<Order> orders = dc.Orders.Where(x => x.Paid.Date >= start.Date && x.Paid <= end);
                List<Product> products = dc.Products.ToList();

                var allDebits = GetDebitsInDateRange(start, end, dc);
                var debitsForSales = allDebits.Where(x => x.Type == "Venta");
                
                var debitsNoTax = debitsForSales.Where(x => x.TaxPaid == 0).OrderBy(x => x.CreatedDate);

                var debitsWithTax = debitsForSales.Where(x => x.TaxPaid > 0).OrderBy(x => x.CreatedDate);

                IEnumerable<AggregateSalesModel> allSales = Sales(orders, products, orderBy);

                double totalDebits = debitsNoTax.Any() ? Convert.ToDouble(debitsForSales.Sum(x => x.Amout)) : 0;

                double seed = 500.00;
                double cashInRegister = ((allSales.Any()
                                              ? allSales.Sum(x => x.TotalSales) + allSales.Sum(x => x.TotalTax)
                                              : 0) - totalDebits) + seed;

                IEnumerable<AggregateSalesModel> taxableSales = allSales.Where(x => x.TaxRate > 0);
                IEnumerable<AggregateSalesModel> nonTaxableSales = allSales.Where(x => x.TaxRate.Equals(0));

                return new DailySheetModel
                           {
                               Products = taxableSales.Where(x => x.Quantity > 0),
                               SalesNonTaxable = nonTaxableSales.Where(x => x.Quantity > 0),
                               TotalCredit = allSales.Any()
                                                 ? allSales.Sum(x => x.TotalSales) + allSales.Sum(x => x.TotalTax)
                                                 : 0,
                               TotalCreditWithTax = taxableSales.Any()
                                                        ? taxableSales.Sum(x => x.TotalSales) +
                                                          taxableSales.Sum(x => x.TotalTax)
                                                        : 0,
                               TotalCreditWithoutTax = nonTaxableSales.Any()
                                                           ? nonTaxableSales.Sum(x => x.TotalSales)
                                                           : 0,
                               DebitsNoTax = MapDebits(debitsNoTax),
                               DebitsWithTax = MapDebits(debitsWithTax),
                               DebitsByType = allDebits.Where(x=> x.Type!="Venta").GroupBy(x=> x.Type).Select(x => 
                                   new DebitTypeWithDebits(x.Key, MapDebits(x))),
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

        static IEnumerable<Debit> GetDebitsInDateRange(DateTime start, DateTime end, CafeDataContext dc, params string[] types)
        {
            var debits =
                dc.Debits.Where(
                    x => x.CreatedDate.Date >= start.Date && x.CreatedDate < end.AddDays(1).Date).ToList();

            if (types.Any())
            {
                debits = debits.Where(x => types.Select(y=> y.ToLower()).Contains(x.Type.ToLower())).ToList();
            }
            return debits;
        }

        IEnumerable<DebitModel> MapDebits(IEnumerable<Debit> debits)
        {
            return
                debits.Select(
                    x =>
                    new DebitModel
                        {
                            Amout = x.Amout,
                            CreatedDate = x.CreatedDate,
                            DescriptionTruncated = Truncate(x.Description, 40),
                            Description = x.Description,
                            LocationId = x.LocationId,
                            Type = x.Type,
                            TaxPaid = x.TaxPaid,
                            VendorId = x.VendorId,
                            VendorName = x.VendorName,
                            _id = x._id
                        }).ToList();
        }

        static string Truncate(string str, int descriptionMaxLength)
        {
            if (str.Length <= descriptionMaxLength) return str;
            string truncated = str.Substring(1, descriptionMaxLength);
            return truncated + "...";
        }

        dynamic PrintDailyReport()
        {
            DailyReportInput dailyReportInput = GetDailyReportInputWithDefaults();

            string htmlPath = string.Format(
                "http://{0}:{1}/daily?start={2}&end={3}&orderBy={4}",
                Request.Url.HostName,
                Request.Url.Port,
                dailyReportInput.Start.ToShortDateString(),
                dailyReportInput.End.Value.ToShortDateString(),
                dailyReportInput.OrderBy);

            PrintHtml(htmlPath, printerDeviceName);

            return View["success"];
        }

        void PrintHtml(string htmlPath, string printerDevice)
        {
            if (!string.IsNullOrEmpty(printerDevice))
            {
                SetAsDefaultPrinter(printerDevice);
            }

            Task.Factory.StartNew(
                () => PrintOnStaThread(htmlPath), CancellationToken.None, TaskCreationOptions.None, sta).Wait(
                );
        }

        void PrintOnStaThread(string htmlPath)
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

        dynamic ViewDailyReport(DailyReportInput dailyReportInput)
        {
            DailySheetModel dailySheetModel = GetDailySheetData(
                dailyReportInput.Start, dailyReportInput.End.Value, dailyReportInput.OrderBy);
            return View["DailySheet", dailySheetModel];
        }

        #endregion
    }

    public class DebitTypeWithDebits
    {
        public string Type { get; set; }
        public IEnumerable<DebitModel> Debits { get; set; }

        public DebitTypeWithDebits(string type, IEnumerable<DebitModel> debits)
        {
            Type = type;
            Debits = debits;
        }
    }
}