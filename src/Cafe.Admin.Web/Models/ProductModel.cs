namespace Cafe.Admin.Web.Models
{
    public class ProductModel
    {
        public int Quantity { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }

        public double TaxRate { get; set; }

        public double TotalSales { get; set; }

        public double TotalTax { get; set; }

        public string Tag { get; set; }

        public double TotalCost { get; set; }

        public double TotalProfit { get; set; }
    }
}