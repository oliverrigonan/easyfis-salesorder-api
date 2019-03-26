using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis_salesorder_api.ApiControllers
{
    public class ApiMstArticleCategoryController : ApiController
    {
        public Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        [HttpGet, Route("api/articleCategory/list")]
        public List<Entities.MstArticleCategory> ListArticleCategory()
        {
            var articleCategories = from d in db.MstArticleCategories
                                    select new Entities.MstArticleCategory
                                    {
                                        Id = d.Id,
                                        Category = d.Category,
                                        CategoryImageURL = d.CategoryImageURL
                                    };

            return articleCategories.ToList();
        }
    }
}
