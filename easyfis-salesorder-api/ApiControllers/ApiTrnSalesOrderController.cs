using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace easyfis_salesorder_api.ApiControllers
{
    public class ApiTrnSalesOrderController : ApiController
    {
        public Data.easyfisdbDataContext db = new Data.easyfisdbDataContext();

        public String LeadingZeroes(Int32 number, Int32 length)
        {
            var result = number.ToString();

            var pad = length - result.Length;
            while (pad > 0) { result = '0' + result; pad--; }

            return result;
        }

        [HttpPost, Route("api/salesOrder/add")]
        public HttpResponseMessage AddSalesOrder(Entities.TrnSalesOrder objSalesOrder)
        {
            try
            {
                Int32 branchId = 0, customerId = 0, termId = 0, userId = 0;

                var branch = from d in db.MstBranches
                             where d.BranchCode.Equals("002")
                             select d;

                if (branch.Any())
                {
                    branchId = branch.FirstOrDefault().Id;
                }

                var customer = from d in db.MstArticles
                               where d.ArticleTypeId == 2
                               && d.ArticleCode.Equals("0000000001")
                               && d.IsLocked == true
                               select d;

                if (customer.Any())
                {
                    customerId = customer.FirstOrDefault().Id;
                    termId = customer.FirstOrDefault().TermId;
                }

                var user = from d in db.MstUsers
                           select d;

                if (user.Any())
                {
                    userId = user.FirstOrDefault().Id;
                }

                var SONumber = "0000000001";
                var lastSalesOrder = from d in db.TrnSalesOrders.OrderByDescending(d => d.Id)
                                     where d.BranchId == branchId
                                     select d;

                if (lastSalesOrder.Any())
                {
                    SONumber = LeadingZeroes((Convert.ToInt32(lastSalesOrder.FirstOrDefault().SONumber) + 0000000001), 10);
                }

                String returnMessage = "";

                if (branchId != 0 && customerId != 0 && termId != 0 && userId != 0)
                {
                    Data.TrnSalesOrder newSalesOrder = new Data.TrnSalesOrder
                    {
                        BranchId = branchId,
                        SONumber = SONumber,
                        SODate = DateTime.Today,
                        CustomerId = customerId,
                        TermId = termId,
                        DocumentReference = objSalesOrder.DocumentReference,
                        ManualSONumber = SONumber,
                        Remarks = objSalesOrder.Remarks,
                        Amount = 0,
                        SoldById = userId,
                        PreparedById = userId,
                        CheckedById = userId,
                        ApprovedById = userId,
                        Status = null,
                        IsCancelled = false,
                        IsPrinted = false,
                        IsLocked = false,
                        CreatedById = userId,
                        CreatedDateTime = DateTime.Now,
                        UpdatedById = userId,
                        UpdatedDateTime = DateTime.Now
                    };

                    db.TrnSalesOrders.InsertOnSubmit(newSalesOrder);
                    db.SubmitChanges();

                    Int32 SOId = newSalesOrder.Id;
                    returnMessage = newSalesOrder.ManualSONumber;

                    if (objSalesOrder.ListSalesOrderItems.Any())
                    {
                        List<Data.TrnSalesOrderItem> newSalesOrderItems = new List<Data.TrnSalesOrderItem>();
                        Decimal totalAmount = 0;

                        foreach (var salesOrderItem in objSalesOrder.ListSalesOrderItems)
                        {
                            Int32 itemId = 0, unitId = 0, discountId = 0, VATId = 0;
                            Decimal discountRate = 0, VATPercentage = 0;

                            var item = from d in db.MstArticles
                                       where d.ArticleTypeId == 1
                                       && d.ManualArticleCode.Equals(salesOrderItem.ItemCode)
                                       && d.IsLocked == true
                                       select d;

                            if (item.Any())
                            {
                                itemId = item.FirstOrDefault().Id;
                                unitId = item.FirstOrDefault().UnitId;
                                VATId = item.FirstOrDefault().OutputTaxId;
                                VATPercentage = item.FirstOrDefault().MstTaxType.TaxRate;
                            }

                            var discount = from d in db.MstDiscounts select d;
                            if (discount.Any())
                            {
                                discountId = discount.FirstOrDefault().Id;
                                discountRate = discount.FirstOrDefault().DiscountRate;
                            }

                            if (itemId != 0 && unitId != 0 && discountId != 0)
                            {
                                Decimal amount = salesOrderItem.Price * salesOrderItem.Quantity;
                                Decimal VATAmount = 0;

                                if (VATPercentage > 0)
                                {
                                    Decimal percentage = (VATPercentage / 100);
                                    VATAmount = (amount / (percentage + 1)) * percentage;
                                }

                                var unitConversion = from d in db.MstArticleUnits
                                                     where d.ArticleId == itemId
                                                     && d.UnitId == unitId
                                                     && d.MstArticle.IsLocked == true
                                                     select d;

                                if (unitConversion.Any())
                                {
                                    Decimal baseQuantity = salesOrderItem.Quantity * 1;
                                    if (unitConversion.FirstOrDefault().Multiplier > 0)
                                    {
                                        baseQuantity = salesOrderItem.Quantity * (1 / unitConversion.FirstOrDefault().Multiplier);
                                    }

                                    Decimal basePrice = amount;
                                    if (baseQuantity > 0)
                                    {
                                        basePrice = amount / baseQuantity;
                                    }

                                    newSalesOrderItems.Add(new Data.TrnSalesOrderItem
                                    {
                                        SOId = SOId,
                                        ItemId = itemId,
                                        ItemInventoryId = null,
                                        Particulars = salesOrderItem.Particulars,
                                        UnitId = unitId,
                                        Quantity = salesOrderItem.Quantity,
                                        Price = salesOrderItem.Price,
                                        DiscountId = discountId,
                                        DiscountRate = discountRate,
                                        DiscountAmount = 0,
                                        NetPrice = salesOrderItem.Price,
                                        Amount = amount,
                                        VATId = VATId,
                                        VATPercentage = VATPercentage,
                                        VATAmount = VATAmount,
                                        BaseUnitId = unitId,
                                        BaseQuantity = baseQuantity,
                                        BasePrice = basePrice,
                                        SalesItemTimeStamp = DateTime.Now
                                    });

                                    totalAmount += amount;
                                }
                            }
                        }

                        db.TrnSalesOrderItems.InsertAllOnSubmit(newSalesOrderItems);
                        db.SubmitChanges();

                        var salesOrder = from d in db.TrnSalesOrders
                                         where d.Id == SOId
                                         select d;

                        if (salesOrder.Any())
                        {
                            var updateSalesOrder = salesOrder.FirstOrDefault();
                            updateSalesOrder.Amount = totalAmount;

                            db.SubmitChanges();
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, returnMessage);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet, Route("api/salesOrder/list/{startDate}/{endDate}")]
        public List<Entities.TrnSalesOrder> ListSalesOrder(String startDate, String endDate)
        {
            var salesOrders = from d in db.TrnSalesOrders
                              where d.SODate >= Convert.ToDateTime(startDate)
                              && d.SODate <= Convert.ToDateTime(endDate)
                              select new Entities.TrnSalesOrder
                              {
                                  SONumber = d.SONumber,
                                  SODate = d.SODate.ToShortDateString(),
                                  DocumentReference = d.DocumentReference,
                                  Remarks = d.Remarks,
                                  ListSalesOrderItems = d.TrnSalesOrderItems.Any() ? d.TrnSalesOrderItems.Select(i => new Entities.TrnSalesOrderItem
                                  {
                                      ItemCode = i.MstArticle.ManualArticleCode,
                                      ItemDescription = i.MstArticle.Article,
                                      Price = i.Price,
                                      Quantity = i.Quantity,
                                      Amount = i.Amount
                                  }).ToList() : new List<Entities.TrnSalesOrderItem>().ToList()
                              };

            return salesOrders.ToList();
        }
    }
}
