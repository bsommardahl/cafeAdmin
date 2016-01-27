using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using AutoMapper;
using Cafe.Json;
using RestSharp;

namespace Cafe.Data
{
    public class CafeDataBackup
    {
        readonly string connectionString =
            ConfigurationManager.ConnectionStrings["CafeReport.Properties.Settings.CafeConnectionString"].ToString();

        #region Fields

        readonly ICafeDataReader _dataReader;

        #endregion

        #region Constructors and Destructors

        public CafeDataBackup(ICafeDataReader cafeDataReader)
        {            
            _dataReader = cafeDataReader;
        }

        #endregion

        #region Public Methods and Operators

        public void Go()
        {
            Mapper.CreateMap<ProductJson, Product>();

            using (var dc = new CafeDataContext(connectionString))
            {
                PersistOrders(dc, _dataReader.GetData<OrderJson>("orders"));
                PersistProducts(dc, _dataReader.GetData<ProductJson>("products", null, false));
                PersistTags(dc, _dataReader.GetData<TagJson>("tags", null, false));
                PersistDebits(dc, _dataReader.GetData<DebitJson>("debits"));
                PersistEmployees(dc, _dataReader.GetData<EmployeeJson>("employees", null, false));
                PersistTimes(dc, _dataReader.GetData<TimeJson>("times"));
                PersistVendors(dc, _dataReader.GetData<VendorJson>("vendors", null, false));
            }
        }

        #endregion

        #region Methods

        void PersistDebits(CafeDataContext dc, IEnumerable<DebitJson> items)
        {
            Console.WriteLine("Debits");
            string locationId = "";
            foreach (DebitJson x in items)
            {
                try
                {
                    DateTime createdDate = x.CreatedDate;
                    if (createdDate == DateTime.MinValue) throw new Exception("Created date was minvalue.");

                    Debit existingDebit = dc.Debits.FirstOrDefault(p => p._id == x._id);
                    if (existingDebit != null)
                    {
                        existingDebit.Amout = Convert.ToDecimal(x.Amount);
                        existingDebit.CreatedDate = x.CreatedDate;
                        existingDebit.Description = x.Description;
                        existingDebit.TaxPaid = Convert.ToDecimal((x.TaxPaid ?? "0"));
                        existingDebit.VendorId = x.VendorId;
                        existingDebit.VendorName = x.VendorName;
                        continue;
                    }

                    locationId = x.LocationId ?? locationId;
                    dc.Debits.InsertOnSubmit(
                        new Debit
                            {
                                _id = x._id,
                                LocationId = locationId,
                                Amout = Convert.ToDecimal(x.Amount),
                                CreatedDate = createdDate,
                                Description = x.Description,
                                TaxPaid = Convert.ToDecimal((x.TaxPaid ?? "0")),
                                VendorId = x.VendorId,
                                VendorName = x.VendorName,
                                Type = x.Type ?? "Venta"
                            });
                    Console.Write(".");
                }
                catch (Exception)
                {
                }
            }

            dc.SubmitChanges();
        }

        void PersistEmployees(CafeDataContext dc, IEnumerable<EmployeeJson> items)
        {
            Console.WriteLine("Employees");

            foreach (EmployeeJson x in items)
            {
                if (dc.Employees.Any(p => p._id == x._id))
                {
                    continue;
                }

                dc.Employees.InsertOnSubmit(new Employee {_id = x._id, LocationId = x.LocationId, Name = x.Name});
                Console.Write(".");
            }

            dc.SubmitChanges();
        }

        void PersistOrders(CafeDataContext dc, IEnumerable<OrderJson> items)
        {
            Console.WriteLine("Orders");
            foreach (OrderJson x in items)
            {
                if (x.Created < new DateTime(2015, 11, 30)) continue;

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

        void PersistProducts(CafeDataContext dc, IEnumerable<ProductJson> items)
        {
            Console.WriteLine("Products");

            foreach (ProductJson x in items)
            {
                Product existing = dc.Products.FirstOrDefault(p => p._id == x._id);

                if (existing != null)
                {
                    Product productInDb = dc.Products.FirstOrDefault(p => p._id == x._id);
                    Mapper.Map(x, productInDb);
                }
                else
                {
                    dc.Products.InsertOnSubmit(Mapper.Map<ProductJson, Product>(x));
                }
                Console.Write(".");
            }

            dc.SubmitChanges();
        }

        void PersistTags(CafeDataContext dc, IEnumerable<TagJson> items)
        {
            Console.WriteLine("Tags");
            foreach (TagJson x in items)
            {
                if (dc.Tags.Any(p => p._id == x._id))
                {
                    continue;
                }

                dc.Tags.InsertOnSubmit(new Tag {_id = x._id, LocationId = x.LocationId, Name = x.Name});
                Console.Write(".");
            }

            dc.SubmitChanges();
        }

        void PersistTimes(CafeDataContext dc, IEnumerable<TimeJson> items)
        {
            Console.WriteLine("Times");

            foreach (TimeJson x in items)
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

        void PersistVendors(CafeDataContext dc, IEnumerable<VendorJson> items)
        {
            Console.WriteLine("Vendors");

            foreach (VendorJson x in items)
            {
                if (dc.Vendors.Any(p => p._id == x._id))
                {
                    continue;
                }

                dc.Vendors.InsertOnSubmit(
                    new Vendor {_id = x._id, LocationId = x.LocationId, Name = x.Name, Priority = x.Priority});
                Console.Write(".");
            }

            dc.SubmitChanges();
        }

        #endregion
    }
}