namespace Cafe.DailyReports
{
    using System;

    public class DailyReportInput
    {
        #region Public Properties

        public DateTime End { get; set; }

        public string OrderBy { get; set; }

        public DateTime Start { get; set; }

        #endregion
    }
}