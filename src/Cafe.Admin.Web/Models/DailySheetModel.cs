using System.Collections;
using System.Collections.Generic;
using Cafe.Data;

namespace Cafe.Admin.Web.Models
{
    public class DailySheetModel
    {
        public IEnumerable<ProductModel> Products { get; set; }

        public double TotalCredit { get; set; }

        public IEnumerable<Debit> Debits { get; set; }

        public double TotalDebits { get; set; }

        public double CashInRegister { get; set; }

        public string StartDate { get; set; }

        public double Seed { get; set; }

        public double FinalTotal { get; set; }

        public string EndDate { get; set; }
    }
}