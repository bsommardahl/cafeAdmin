using System;
using System.Collections.Generic;

namespace Cafe.DailyReports
{
    public class DebitModificationModel
    {
        public DebitModificationModel(DateTime startDate, DateTime endDate, IEnumerable<DebitModel> debits,
                                      IEnumerable<DebitTypeModel> types)
        {
            Debits = debits;
            StartDate = startDate;
            EndDate = endDate;
            Types = types;
        }

        public IEnumerable<DebitModel> Debits { get; internal set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public IEnumerable<DebitTypeModel> Types { get; set; }
    }
}