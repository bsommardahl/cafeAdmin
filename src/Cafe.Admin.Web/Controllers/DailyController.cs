using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AttributeRouting.Web.Mvc;
using Cafe.Admin.Web.Models;
using Cafe.Data;

namespace Cafe.Admin.Web.Controllers
{
    public class DailyController : Controller
    {
        [GET("/daily")]
        public ActionResult Index()
        {
            return View();
        }

        [POST("/daily")]
        public ActionResult ShowDailySheet(DateTime start, DateTime end, string orderBy)
        {
            using (var dc = new CafeDataContext())
            {
                IQueryable<Order> orders = dc.Orders.Where(x => x.Paid.Date >= start.Date && x.Paid <= end);
                List<Product> products = dc.Products.ToList();

                var debits = dc.Debits.Where(x => x.CreatedDate.Date >= start.Date && x.CreatedDate <= end);

                IEnumerable<ProductModel> productModels = Products(orders, products, orderBy);
                double totalCredit = productModels.Sum(x => x.TotalSales) + productModels.Sum(x => x.TotalTax);
                double totalDebits = Convert.ToDouble(debits.Sum(x => x.Amout));
                double seed = 500.00;
                double cashInRegister = (totalCredit - totalDebits) + seed;
                return View(new DailySheetModel
                                {
                                    Products = productModels,
                                    TotalCredit = totalCredit,
                                    Debits = debits.ToList(),
                                    TotalDebits = totalDebits,
                                    CashInRegister = cashInRegister,
                                    Seed = seed,
                                    FinalTotal = cashInRegister - seed,
                                    StartDate = string.Format("{0}-{1}-{2}", start.Day, start.Month, start.Year),
                                    EndDate = string.Format("{0}-{1}-{2}", end.Day, end.Month, end.Year)

                                });
            }
        }

        static IEnumerable<ProductModel> Products(IQueryable<Order> orders, List<Product> products, string orderBy)
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

            IEnumerable<ProductModel> productModels = orderedEnumerable.Select(x =>
                                                                     {
                                                                         OrderItem orderItem = x.First();
                                                                         int quantity = x.Count();
                                                                         Product product = products.FirstOrDefault(p => p.Name == orderItem.Name);
                                                                         if (product == null)
                                                                         {
                                                                             return new ProductModel();
                                                                         }
                                                                         double totalCost = Convert.ToDouble(quantity*product.Cost);
                                                                         
                                                                         //decimal itemPrice = orderItem.Price;
                                                                         decimal itemPrice = product.Price;
                                                                         
                                                                         double totalTax = Convert.ToDouble(quantity*(itemPrice*product.TaxRate));
                                                                         double totalSales = Convert.ToDouble(quantity*itemPrice);
                                                                         double taxRate = Convert.ToDouble(product.TaxRate)*100;
                                                                         double price = Convert.ToDouble(itemPrice);
                                                                         double totalProfit = totalSales - totalCost;

                                                                         return new ProductModel
                                                                                    {
                                                                                        Name = orderItem.Name, Tag = orderItem.Tags, Price = price, TaxRate = taxRate, Quantity = quantity, TotalSales = totalSales, TotalTax = totalTax, TotalCost = totalCost, TotalProfit = totalProfit
                                                                                    };
                                                                     });

            if (orderBy == "profit")
            {
                productModels = productModels.OrderBy(x => x.TotalProfit);
            }
            
            return productModels;
        }
    }
}