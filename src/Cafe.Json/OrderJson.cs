using System;
using System.Collections.Generic;

namespace Cafe.Json
{
    public class OrderJson
    {
        public string _id { get; set; }
        public string LocationId { get; set; }
        public string CustomerName { get; set; }
        public List<OrderItemJson> Items { get; set; }
        public double AmountPaid { get; set; }
        public DateTime Paid { get; set; }
        public DateTime Created { get; set; }
        public string AllDelivered { get; set; }
    }
}