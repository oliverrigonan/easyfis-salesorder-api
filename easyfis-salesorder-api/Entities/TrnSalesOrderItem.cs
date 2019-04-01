using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis_salesorder_api.Entities
{
    public class TrnSalesOrderItem
    {
        public String ItemCode { get; set; }
        public String ItemDescription { get; set; }
        public String Particulars { get; set; }
        public Decimal Quantity { get; set; }
        public Decimal Price { get; set; }
        public Decimal Amount { get; set; }
    }
}