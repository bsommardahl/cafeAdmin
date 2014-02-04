using System;

namespace Cafe.Json
{
    public class DebitJson
    {
        public string VendorId { get; set; }
        public string VendorName { get; set; }
        public string Amount { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string _id { get; set; }
        public string LocationId { get; set; }
        public string TaxPaid { get; set; }
    }
}