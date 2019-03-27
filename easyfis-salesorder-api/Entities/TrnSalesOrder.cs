using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis_salesorder_api.Entities
{
    public class TrnSalesOrder
    {
        public String DocumentReference { get; set; }
        public String Remarks { get; set; }
        public List<TrnSalesOrderItem> ListSalesOrderItems { get; set; }
    }
}