namespace Cafe.Data
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    using Cafe.Json;

    using RestSharp;

    public class CafeDataBackup
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["CafeReport.Properties.Settings.CafeConnectionString"].ToString();

        #region Fields

        private readonly string _apiUrl;

        private CafeDataReader _dataReader;

        #endregion

        #region Constructors and Destructors

        public CafeDataBackup(string apiUrl)
        {
            this._apiUrl = apiUrl;
        }

        #endregion

        #region Public Methods and Operators

        public void Go()
        {
            var client = new RestClient(this._apiUrl);
            this._dataReader = new CafeDataReader(client);

            using (var dc = new CafeDataContext(connectionString))
            {
                this.PersistOrders(dc);
                this.PersistProducts(dc);
                this.PersistTags(dc);
                this.PersistDebits(dc);
                this.PersistEmployees(dc);
                this.PersistTimes(dc);
                this.PersistVendors(dc);
            }
        }

        #endregion

        #region Methods

        private void PersistDebits(CafeDataContext dc)
        {
            Console.WriteLine("Debits");

            var orders = this._dataReader.GetData<List<DebitJson>>("debits");
            string locationId = "";
            foreach (DebitJson x in orders)
            {
                if (dc.Debits.Any(p => p._id == x._id))
                {
                    continue;
                }

                try
                {
                    locationId = x.LocationId ?? locationId;
                    dc.Debits.InsertOnSubmit(
                        new Debit
                            {
                                _id = x._id,
                                LocationId = locationId,
                                Amout = Convert.ToDecimal(x.Amount),
                                CreatedDate = x.CreatedDate,
                                Description = x.Description,
                                TaxPaid = Convert.ToDecimal((x.TaxPaid ?? "0")),
                                VendorId = x.VendorId,
                                VendorName = x.VendorName
                            });
                    Console.Write(".");
                }
                catch (Exception)
                {
                }
            }

            dc.SubmitChanges();
        }

        private void PersistEmployees(CafeDataContext dc)
        {
            Console.WriteLine("Employees");

            var orders = this._dataReader.GetData<List<EmployeeJson>>("employees", null, false);
            foreach (EmployeeJson x in orders)
            {
                if (dc.Employees.Any(p => p._id == x._id))
                {
                    continue;
                }

                dc.Employees.InsertOnSubmit(new Employee { _id = x._id, LocationId = x.LocationId, Name = x.Name });
                Console.Write(".");
            }

            dc.SubmitChanges();
        }

        private void PersistOrders(CafeDataContext dc)
        {
            Console.WriteLine("Orders");
            var orders = this._dataReader.GetData<List<OrderJson>>("orders");
            foreach (OrderJson x in orders)
            {
                if (dc.Orders.Any(p => p._id == x._id))
                {
                    continue;
                }

                if (x.Created > DateTime.MinValue && x.Paid > DateTime.MinValue)
                {
                    var order = new Order
                        {
                            _id = x._id,
                            AmountPaid = Convert.ToDecimal(x.AmountPaid),
                            Created = x.Created,
                            Paid = x.Paid,
                            CustomerName = x.CustomerName
                        };
                    order.OrderItems.AddRange(
                        x.Items.Select(
                            y =>
                            new OrderItem
                                {
                                    Name = y.Name,
                                    Price = Convert.ToDecimal(y.Price),
                                    Tags = y.Tag,
                                    Order = order,
                                    ProductId = y._id
                                }));
                    dc.Orders.InsertOnSubmit(order);

                    Console.Write(".");
                }
            }

            dc.SubmitChanges();
        }

        private void PersistProducts(CafeDataContext dc)
        {
            Console.WriteLine("Products");

            var orders = this._dataReader.GetData<List<ProductJson>>("products", null, false);
            foreach (ProductJson x in orders)
            {
                if (dc.Products.Any(p => p._id == x._id))
                {
                    continue;
                }

                dc.Products.InsertOnSubmit(
                    new Product
                        {
                            _id = x._id,
                            LocationId = x.LocationId,
                            Name = x.Name,
                            Price = Convert.ToDecimal(x.Price),
                            Priority = x.Priority,
                            Tag = x.Tag
                        });
                Console.Write(".");
            }

            dc.SubmitChanges();
        }

        private void PersistTags(CafeDataContext dc)
        {
            Console.WriteLine("Tags");
            var orders = this._dataReader.GetData<List<TagJson>>("tags", null, false);
            foreach (TagJson x in orders)
            {
                if (dc.Tags.Any(p => p._id == x._id))
                {
                    continue;
                }

                dc.Tags.InsertOnSubmit(new Tag { _id = x._id, LocationId = x.LocationId, Name = x.Name });
                Console.Write(".");
            }

            dc.SubmitChanges();
        }

        private void PersistTimes(CafeDataContext dc)
        {
            Console.WriteLine("Times");

            var orders = this._dataReader.GetData<List<TimeJson>>("times");
            foreach (TimeJson x in orders)
            {
                if (dc.Times.Any(p => p._id == x._id))
                {
                    continue;
                }

                string dateString = DateTime.Parse(x.Date).ToShortDateString();

                dc.Times.InsertOnSubmit(
                    new Time
                        {
                            _id = x._id,
                            LocationId = x.LocationId,
                            CreatedDate = DateTime.Parse(x.CreatedDate),
                            Date = Convert.ToDateTime(dateString),
                            EmployeeId = x.EmployeeId,
                            EmployeeName = x.EmployeeName,
                            TimeIn = DateTime.Parse(string.Format("{0} {1}", dateString, x.TimeIn)),
                            TimeOut = DateTime.Parse(string.Format("{0} {1}", dateString, x.TimeOut)),
                        });
                Console.Write(".");
            }

            dc.SubmitChanges();
        }

        private void PersistVendors(CafeDataContext dc)
        {
            Console.WriteLine("Vendors");

            var orders = this._dataReader.GetData<List<VendorJson>>("vendors", null, false);
            foreach (VendorJson x in orders)
            {
                if (dc.Vendors.Any(p => p._id == x._id))
                {
                    continue;
                }

                dc.Vendors.InsertOnSubmit(
                    new Vendor { _id = x._id, LocationId = x.LocationId, Name = x.Name, Priority = x.Priority });
                Console.Write(".");
            }

            dc.SubmitChanges();
        }

        #endregion
    }
}