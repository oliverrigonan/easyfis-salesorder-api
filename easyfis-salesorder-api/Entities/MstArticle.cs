using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis_salesorder_api.Entities
{
    public class MstArticle
    {
        public Int32 Id { get; set; }
        public String ManualItemCode { get; set; }
        public String ItemDescription { get; set; }
        public String Category { get; set; }
        public String Particulars { get; set; }
        public Decimal Price { get; set; }
        public String Unit { get; set; }
        public String ArticleImageUrl { get; set; }
    }
}