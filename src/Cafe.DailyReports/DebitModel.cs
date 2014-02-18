namespace Cafe.DailyReports
{
    using System;

    public class DebitModel
    {
        public decimal Amout { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Description { get; set; }

        public string LocationId { get; set; }

        public bool OperationalExpense { get; set; }

        public decimal TaxPaid { get; set; }

        public string VendorId { get; set; }

        public string VendorName { get; set; }

        public string _id { get; set; }
    }
}