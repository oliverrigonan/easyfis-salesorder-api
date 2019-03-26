using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis_salesorder_api.ApiControllers
{
    [RoutePrefix("api/article")]
    public class ApiMstArticleController : ApiController
    {
        public Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        [HttpGet, Route("item/listByCategory/{category}")]
        public List<Entities.MstArticle> ListArticleItemByCategory(String category)
        {
            var items = from d in db.MstArticles
                        where d.ArticleTypeId == 1
                        && d.IsLocked == true
                        && d.Category.Equals(category)
                        select new Entities.MstArticle
                        {
                            Id = d.Id,
                            ManualItemCode = d.ManualArticleCode,
                            ItemDescription = d.Article,
                            Category = d.Category,
                            Particulars = d.Particulars,
                            Price = d.Price,
                            Unit = d.MstUnit.Unit,
                            ArticleImageUrl = d.ArticleImageURL
                        };

            return items.ToList();
        }
    }
}
