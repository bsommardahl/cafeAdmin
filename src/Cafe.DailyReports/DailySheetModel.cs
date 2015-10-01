namespace Cafe.DailyReports
{
    using System;
    using System.Collections.Generic;

    public class DailySheetModel
    {
        #region Constructors and Destructors

        public DailySheetModel()
        {
            this.DebitsNoTax = new List<DebitModel>();
            this.Products = new List<AggregateSalesModel>();
        }

        #endregion

        #region Public Properties

        public double CashInRegister { get; set; }

        public DateTime DataImported { get; set; }

        public IEnumerable<DebitModel> DebitsNoTax { get; set; }

        public string EndDate { get; set; }

        public double FinalTotal { get; set; }

        public IEnumerable<AggregateSalesModel> Products { get; set; }

        public double Seed { get; set; }

        public string StartDate { get; set; }

        public double TotalCredit { get; set; }

        public double TotalDebits { get; set; }

        public IEnumerable<DebitModel> DebitsWithTax { get; set; }

        public IEnumerable<AggregateSalesModel> SalesNonTaxable { get; set; }

        public double TotalCreditWithoutTax { get; set; }

        public double TotalCreditWithTax { get; set; }

        public IEnumerable<DebitTypeWithDebits> DebitsByType { get; set; }

        #endregion
    }
}