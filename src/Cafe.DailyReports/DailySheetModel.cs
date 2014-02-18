namespace Cafe.DailyReports
{
    using System;
    using System.Collections.Generic;

    public class DailySheetModel
    {
        #region Constructors and Destructors

        public DailySheetModel()
        {
            this.Debits = new List<DebitModel>();
            this.Products = new List<ProductModel>();
        }

        #endregion

        #region Public Properties

        public double CashInRegister { get; set; }

        public DateTime DataImported { get; set; }

        public IEnumerable<DebitModel> Debits { get; set; }

        public string EndDate { get; set; }

        public double FinalTotal { get; set; }

        public IEnumerable<ProductModel> Products { get; set; }

        public double Seed { get; set; }

        public string StartDate { get; set; }

        public double TotalCredit { get; set; }

        public double TotalDebits { get; set; }

        #endregion
    }
}