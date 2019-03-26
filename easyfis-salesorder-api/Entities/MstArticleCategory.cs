using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace easyfis_salesorder_api.Entities
{
    public class MstArticleCategory
    {
        public Int32 Id { get; set; }
        public String Category { get; set; }
        public String CategoryImageURL { get; set; }
    }
}