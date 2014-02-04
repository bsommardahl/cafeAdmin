using System;
using System.Collections.Generic;
using System.Linq;
using Cafe.Json;
using RestSharp;

namespace Cafe.Data
{
    public class CafeDataBackup
    {
        readonly string _apiUrl;

        public CafeDataBackup(string apiUrl)
        {
            _apiUrl = apiUrl;
        }

        CafeDataReader _dataReader;

        public void Go()
        {
            var client = new RestClient(_apiUrl);
            _dataReader = new CafeDataReader(client);

            using (var dc = new CafeDataContext())
            {
                PersistOrders(dc);
                PersistProducts(dc);
                PersistTags(dc);
                PersistDebits(dc);
                PersistEmployees(dc);
                PersistTimes(dc);
                PersistVendors(dc);
            }            
        }

        void PersistVendors(CafeDataContext dc)
        {
            Console.WriteLine("Vendors");

            var orders = _dataReader.GetData<List<VendorJson>>("vendors", null, false);
            foreach (VendorJson x in orders)
            {
                if (Queryable.Any(dc.Vendors, p => p._id == x._id)) continue;

                dc.Vendors.InsertOnSubmit(new Vendor
                                              {
                                                  _id = x._id,
                                                  LocationId = x.LocationId,
                                                  Name = x.Name,
                                                  Priority = x.Priority
                                              });
                Console.Write(".");
            }

            dc.SubmitChanges();
        }

        void PersistProducts(CafeDataContext dc)
        {
            Console.WriteLine("Products");

            var orders = _dataReader.GetData<List<ProductJson>>("products", null, false);
            foreach (ProductJson x in orders)
            {
                if (Queryable.Any(dc.Products, p => p._id == x._id)) continue;

                dc.Products.InsertOnSubmit(new Product
                                               {
                                                   _id = x._id,
                                                   LocationId = x.LocationId,
                                                   Name = x.Name,
                                                   Price = Convert.ToDecimal((double) x.Price),
                                                   Priority = x.Priority,
                                                   Tag = x.Tag
                                               });
                Console.Write(".");
            }

            dc.SubmitChanges();
        }

        void PersistDebits(CafeDataContext dc)
        {
            Console.WriteLine("Debits");

            var orders = _dataReader.GetData<List<DebitJson>>("debits");
            string locationId = "";
            foreach (DebitJson x in orders)
            {
                if (Queryable.Any(dc.Debits, p => p._id == x._id)) continue;

                try
                {
                    locationId = x.LocationId ?? locationId;
                    dc.Debits.InsertOnSubmit(new Debit
                                                 {
                                                     _id = x._id,
                                                     LocationId = locationId,
                                                     Amout = Convert.ToDecimal((string) x.Amount),
                                                     CreatedDate = x.CreatedDate,
                                                     Description = x.Description,
                                                     TaxPaid = Convert.ToDecimal((string) (x.TaxPaid ?? "0")),
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

        void PersistEmployees(CafeDataContext dc)
        {
            Console.WriteLine("Employees");

            var orders = _dataReader.GetData<List<EmployeeJson>>("employees", null, false);
            foreach (EmployeeJson x in orders)
            {
                if (Queryable.Any(dc.Employees, p => p._id == x._id)) continue;

                dc.Employees.InsertOnSubmit(new Employee
                                                {
                                                    _id = x._id,
                                                    LocationId = x.LocationId,
                                                    Name = x.Name
                                                });
                Console.Write(".");
            }

            dc.SubmitChanges();
        }

        void PersistTimes(CafeDataContext dc)
        {
            Console.WriteLine("Times");

            var orders = _dataReader.GetData<List<TimeJson>>("times");
            foreach (TimeJson x in orders)
            {
                if (Queryable.Any(dc.Times, p => p._id == x._id)) continue;

                string dateString = DateTime.Parse(x.Date).ToShortDateString();

                dc.Times.InsertOnSubmit(new Time
                                            {
                                                _id = x._id,
                                                LocationId = x.LocationId,
                                                CreatedDate = DateTime.Parse(x.CreatedDate),
                                                Date = Convert.ToDateTime(dateString),
                                                EmployeeId = x.EmployeeId,
                                                EmployeeName = x.EmployeeName,
                                                TimeIn = DateTime.Parse(string.Format("{0} {1}", dateString, x.TimeIn)),
                                                TimeOut =
                                                    DateTime.Parse(string.Format("{0} {1}", dateString, x.TimeOut)),
                                            });
                Console.Write(".");
            }

            dc.SubmitChanges();
        }

        void PersistTags(CafeDataContext dc)
        {
            Console.WriteLine("Tags");
            var orders = _dataReader.GetData<List<TagJson>>("tags", null, false);
            foreach (TagJson x in orders)
            {
                if (Queryable.Any(dc.Tags, p => p._id == x._id)) continue;

                dc.Tags.InsertOnSubmit(new Tag
                                           {
                                               _id = x._id,
                                               LocationId = x.LocationId,
                                               Name = x.Name
                                           });
                Console.Write(".");
            }

            dc.SubmitChanges();
        }

        void PersistOrders(CafeDataContext dc)
        {
            Console.WriteLine("Orders");
            var orders = _dataReader.GetData<List<OrderJson>>("orders");
            foreach (OrderJson x in orders)
            {
                if (Queryable.Any(dc.Orders, p => p._id == x._id)) continue;

                if (x.Created > DateTime.MinValue && x.Paid > DateTime.MinValue)
                {
                    var order = new Order
                                    {
                                        _id = x._id,
                                        AmountPaid = Convert.ToDecimal((double) x.AmountPaid),
                                        Created = x.Created,
                                        Paid = x.Paid,
                                        CustomerName = x.CustomerName
                                    };
                    order.OrderItems.AddRange(x.Items.Select(y => new OrderItem
                                                                      {
                                                                          Name = y.Name,
                                                                          Price =
                                                                              Convert.ToDecimal(
                                                                                  (string) y.Price),
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
    }
}