using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NhapHangV2.Entities;
using NhapHangV2.Entities.DomainEntities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.DbContext;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Interface.Services.Configuration;
using NhapHangV2.Interface.UnitOfWork;
using NhapHangV2.Service.Services.DomainServices;
using NhapHangV2.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users = NhapHangV2.Entities.Users;

namespace NhapHangV2.Service.Services
{
    public class OrderShopTempService : DomainService<OrderShopTemp, OrderShopTempSearch>, IOrderShopTempService
    {
        protected readonly IAppDbContext Context;

        public OrderShopTempService(IServiceProvider serviceProvider, IAppUnitOfWork unitOfWork, IMapper mapper, IAppDbContext Context) : base(unitOfWork, mapper)
        {
            this.Context = Context;
        }

        protected override string GetStoreProcName()
        {
            return "OrderShopTemp_GetPagingData";
        }

        public override async Task<OrderShopTemp> GetByIdAsync(int id)
        {
            var orderShopTemp = await Queryable.Where(e => e.Id == id && !e.Deleted).AsNoTracking().FirstOrDefaultAsync();

            if (orderShopTemp == null)
                return null;

            var user = await unitOfWork.Repository<Users>().GetQueryable().Where(e => !e.Deleted && e.Active && e.Id == orderShopTemp.UID).FirstOrDefaultAsync();
            if (user != null)
            {
                orderShopTemp.FullName = user.FullName;
                orderShopTemp.Phone = user.Phone;
                orderShopTemp.Email = user.Email;
                orderShopTemp.Address = user.Address;
            }

            var orderTemps = unitOfWork.Repository<OrderTemp>().GetQueryable().Where(e => !e.Deleted && e.Active
                && (e.OrderShopTempId == orderShopTemp.Id)).OrderByDescending(e => e.Id);

            var configurations = await unitOfWork.Repository<Entities.Configurations>()
                .GetQueryable()
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(); //Giống thằng configurationsService.GetSingleAsync()
            decimal currency = Convert.ToDecimal(configurations.Currency);
            foreach (var orderTemp in orderTemps)
            {
                if (user != null && user.Currency > 0)
                    orderTemp.Currency = user.Currency;
                else
                    orderTemp.Currency = currency;

                orderShopTemp.OrderTemps.Add(orderTemp);
            }

            return orderShopTemp;
        }

        public override async Task<bool> DeleteAsync(int id)
        {
            var exists = Queryable
                .Where(e => e.Id == id)
                .FirstOrDefault();
            if (exists != null)
            {
                var orderTemp = unitOfWork.Repository<OrderTemp>().GetQueryable()
                    .Where(e => !e.Deleted && e.Active && (e.OrderShopTempId == exists.Id))
                    .ToList();

                if (orderTemp.Any())
                    unitOfWork.Repository<OrderTemp>().Delete(orderTemp);
                unitOfWork.Repository<OrderShopTemp>().Delete(exists);

                await unitOfWork.SaveAsync();
                return true;
            }
            else
            {
                throw new Exception(id + " not exists");
            }
        }

        public override async Task<bool> CreateAsync(OrderShopTemp item)
        {
            int UID = LoginContext.Instance.CurrentUser.UserId;
            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    //Chỉ được đặt sản phẩm theo shop trong phạm vi đã cài đặt, nếu lớn hơn thì không được đặt (mặc định là 200)
                    int? link = 200;
                    var conf = await unitOfWork.Repository<Entities.Configurations>()
                        .GetQueryable()
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefaultAsync(); //Giống thằng configurationsService.GetSingleAsync()
                    if (conf != null)
                        link = conf.NumberLinkOfOrder;

                    var orderTemps = await unitOfWork.Repository<OrderTemp>().GetQueryable().Where(x => !x.Deleted && x.UID == UID && x.ShopId == item.ShopId).CountAsync();

                    if (orderTemps >= link)
                        throw new AppException("Đã vượt quá số lượng đặt hàng");

                    var orderShopTemp = await this.GetSingleAsync(x => !x.Deleted && x.UID == UID
                        && x.ShopId.Equals(item.ShopId.Trim())
                        && x.ShopName.Equals(item.ShopName.Trim()));
                    if (orderShopTemp == null) //Chưa có shop chưa đặt
                    {
                        item.UID = UID;
                        await unitOfWork.Repository<OrderShopTemp>().CreateAsync(item);
                        await unitOfWork.SaveAsync();
                    }
                    else //Đã có shop
                    {
                        orderShopTemp.OrderTemps = item.OrderTemps;
                        item = orderShopTemp;
                    }
                    //orderShopTemp = await this.GetSingleAsync(x => !x.Deleted && x.UID == UID && x.ShopId == item.ShopId);

                    foreach (var OrderTemp in item.OrderTemps)
                    {
                        OrderTemp.UID = UID;
                        OrderTemp.OrderShopTempId = item.Id;

                        if (OrderTemp.PricePromotion == null || OrderTemp.PricePromotion == 0) OrderTemp.PricePromotion = OrderTemp.PriceOrigin;

                        //Kiểm tra xem có sản phẩm nào giống như vầy không
                        var orderTempsByUID = await unitOfWork.Repository<OrderTemp>().GetQueryable().Where(x => !x.Deleted
                        && x.UID == UID
                        && x.ItemId == OrderTemp.ItemId && x.Brand == OrderTemp.Brand && x.CategoryId == OrderTemp.CategoryId && x.Property == OrderTemp.Property).ToListAsync();

                        if (orderTempsByUID.Any())
                        {
                            foreach (var orderTempByUID in orderTempsByUID)
                            {
                                //Số lượng cũ
                                var oldQuantity = orderTempByUID.Quantity;
                                //Tăng số lượng
                                orderTempByUID.Quantity += OrderTemp.Quantity;
                                if (oldQuantity != orderTempByUID.Quantity)
                                    unitOfWork.Repository<OrderTemp>().Update(orderTempByUID);
                            }
                        }
                        else
                            await unitOfWork.Repository<OrderTemp>().CreateAsync(OrderTemp);

                        //À cái này để tính bước nhảy của order
                        //UpdatePriceInsert(UID, OrderTemp.StepPrice, OrderTemp.ItemId);
                    }

                    //Cập nhật tiền
                    if (orderShopTemp != null) //Chưa có shop chưa đặt
                    {
                        var existOrderTemp = await unitOfWork.Repository<OrderTemp>().GetQueryable().Where(x => !x.Deleted && x.UID == UID && x.OrderShopTempId == orderShopTemp.Id).ToListAsync();
                        existOrderTemp.Add(item.OrderTemps.FirstOrDefault());
                        item.OrderTemps = existOrderTemp;
                    }
                    item = await UpdatePrice(item);

                    unitOfWork.Repository<OrderShopTemp>().Update(item);

                    await unitOfWork.SaveAsync();
                    await dbContextTransaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await dbContextTransaction.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            }
            return true;
        }

        public async Task<bool> CreateAddSameAsync(OrderShopTemp item)
        {
            int UID = LoginContext.Instance.CurrentUser.UserId;
            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    //Chỉ được đặt sản phẩm theo shop trong phạm vi đã cài đặt, nếu lớn hơn thì không được đặt (mặc định là 200)
                    int? link = 200;
                    var conf = await unitOfWork.Repository<Entities.Configurations>()
                        .GetQueryable()
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefaultAsync(); //Giống thằng configurationsService.GetSingleAsync()
                    if (conf != null)
                        link = conf.NumberLinkOfOrder;
                    var orderTemps = await unitOfWork.Repository<OrderTemp>().GetQueryable().Where(x => !x.Deleted && x.UID == UID && x.ShopId == item.ShopId).CountAsync();

                    if (orderTemps >= link)
                        throw new AppException("Đã vượt quá số lượng đặt hàng");

                    var orderShopTemp = await this.GetSingleAsync(x => !x.Deleted && x.UID == UID
                        && x.ShopId.Equals(item.ShopId.Trim())
                        && x.ShopName.Equals(item.ShopName.Trim()));
                    if (orderShopTemp == null) //Chưa có shop chưa đặt
                    {
                        item.UID = UID;
                        await unitOfWork.Repository<OrderShopTemp>().CreateAsync(item);
                        await unitOfWork.SaveAsync();
                    }
                    else //Đã có shop
                    {
                        orderShopTemp.OrderTemps = item.OrderTemps;
                        item = orderShopTemp;
                    }
                    //orderShopTemp = await this.GetSingleAsync(x => !x.Deleted && x.UID == UID && x.ShopId == item.ShopId);

                    foreach (var OrderTemp in item.OrderTemps)
                    {
                        OrderTemp.UID = UID;
                        OrderTemp.OrderShopTempId = item.Id;

                        if (OrderTemp.PricePromotion == null || OrderTemp.PricePromotion == 0) OrderTemp.PricePromotion = OrderTemp.PriceOrigin;
                        //Kiểm tra xem có sản phẩm nào giống như vầy không
                        var orderTempsByUID = await unitOfWork.Repository<OrderTemp>().GetQueryable().Where(x => !x.Deleted
                        && x.UID == UID
                        && x.ItemId == OrderTemp.ItemId && x.Brand == OrderTemp.Brand && x.CategoryId == OrderTemp.CategoryId && x.Property == OrderTemp.Property).CountAsync();
                        if (orderTempsByUID <= 0)
                            await unitOfWork.Repository<OrderTemp>().CreateAsync(OrderTemp);
                        //À cái này để tính bước nhảy của order
                        //UpdatePriceInsert(UID, OrderTemp.StepPrice, OrderTemp.ItemId);
                    }

                    //Cập nhật tiền
                    if (orderShopTemp != null) //Chưa có shop chưa đặt
                    {
                        var existOrderTemp = await unitOfWork.Repository<OrderTemp>().GetQueryable().Where(x => !x.Deleted && x.UID == UID && x.OrderShopTempId == orderShopTemp.Id).ToListAsync();
                        existOrderTemp.Add(item.OrderTemps.FirstOrDefault());
                        item.OrderTemps = existOrderTemp;
                    }
                    item = await UpdatePrice(item);

                    unitOfWork.Repository<OrderShopTemp>().Update(item);

                    await unitOfWork.SaveAsync();
                    await dbContextTransaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await dbContextTransaction.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            }
            return true;
        }

        protected void UpdatePriceInsert(int UID, string StepPrice, string item_id)
        {
            decimal price_update = 0;
            bool checkpricestep = true;
            if (!string.IsNullOrEmpty(StepPrice))
            {
                string[] items = StepPrice.Split('|');
                if (items.Length - 1 > 0)
                {
                    //4-11:20|12-35:18|36-99999999:17|
                    decimal checkto = 0;
                    for (int i = 0; i < items.Length - 1; i++)
                    {
                        var item = items[i];
                        string[] elements = item.Split(':');
                        string amountft = elements[0];
                        string[] ft = amountft.Split('-');
                        //decimal from = Convert.Todecimal(ft[0]);
                        decimal to = Convert.ToDecimal(ft[1]);
                        //decimal price = Convert.Todecimal(elements[1]);
                        //fromPrice.Add(from);
                        //listPrice.Add(price);
                        //if (countproduct >= from && countproduct <= to)
                        //{
                        //    price_update = price;
                        //}
                        if (i == 0)
                        {
                            checkto = to;
                        }
                        else
                        {
                            if (to > checkto)
                            {
                                checkpricestep = false;
                            }
                        }
                    }
                }
                if (checkpricestep == false)
                {
                    var getproductbyid = unitOfWork.Repository<OrderTemp>().GetQueryable().Where(x => !x.Deleted && x.UID == UID && x.ItemId == item_id).ToList();
                    List<decimal> fromPrice = new List<decimal>();
                    List<decimal> listPrice = new List<decimal>();
                    int countproduct = 0;
                    if (getproductbyid.Count > 0)
                    {
                        foreach (var item in getproductbyid)
                        {
                            countproduct += item.Quantity ?? 0;
                        }
                        if (items.Length - 1 > 0)
                        {
                            //4-11:20|12-35:18|36-99999999:17|
                            for (int i = 0; i < items.Length - 1; i++)
                            {
                                var item = items[i];
                                string[] elements = item.Split(':');
                                string amountft = elements[0];
                                string[] ft = amountft.Split('-');

                                decimal from = Convert.ToDecimal(ft[0]);
                                decimal to = Convert.ToDecimal(ft[1]);
                                decimal price = Convert.ToDecimal(elements[1]);
                                fromPrice.Add(from);
                                listPrice.Add(price);
                                if (countproduct >= from && countproduct <= to)
                                {
                                    price_update = price;
                                }
                            }
                        }
                    }
                    decimal lowFromquantity = fromPrice.Count == 0 ? 0 : fromPrice[0];
                    if (countproduct < lowFromquantity)
                    {
                        price_update = fromPrice.Count == 0 ? 0 : listPrice[0];
                    }
                    if (price_update > 0)
                    {
                        foreach (var productU in getproductbyid)
                        {
                            productU.PriceOrigin = productU.PricePromotion = price_update;
                        }
                    }
                }
            }
        }

        public async Task<OrderShopTemp> UpdatePrice(OrderShopTemp item)
        {
            var user = await unitOfWork.Repository<Users>().GetQueryable().Where(x => x.Id == item.UID).FirstOrDefaultAsync();

            var orderTemps = item.OrderTemps;
            var conf = await unitOfWork.Repository<Entities.Configurations>()
                .GetQueryable()
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(); //Giống thằng configurationsService.GetSingleAsync()

            int counprosMore10 = 0;
            int counprosLes10 = 0;

            //item.PriceCNY = 0;

            decimal? currency = conf.Currency;

            if (user != null && user.Currency > 0)
                currency = user.Currency;

            //Tiền của shop
            item.PriceCNY = 0;
            foreach (var jtem in orderTemps)
            {
                item.PriceCNY += (jtem.Quantity * jtem.PricePromotion) ?? 0;

                //Này dành cho phí kiểm hàng
                if (item.IsCheckProduct == true)
                {
                    if (jtem.PriceOrigin >= 10)
                        counprosMore10 += jtem.Quantity ?? 0;
                    else
                        counprosLes10 += jtem.Quantity ?? 0;
                }
            }
            item.PriceVND = item.PriceCNY * currency ?? 0;
            item.PriceVND = Math.Round(item.PriceVND.Value, 0);
            //Phí mua hàng
            var userLevel = await unitOfWork.Repository<UserLevel>().GetQueryable().Where(x => x.Id == user.LevelId).FirstOrDefaultAsync();
            var cKFeeBuyPro = userLevel == null ? 0 : userLevel.FeeBuyPro ?? 0;
            decimal serviceFee = 0;
            decimal feebpnotdc = 0;
            var feeBuyPro = await unitOfWork.Repository<FeeBuyPro>().GetQueryable().Where(x => x.PriceFrom < item.PriceVND && item.PriceVND <= x.PriceTo).FirstOrDefaultAsync();
            if (feeBuyPro != null)
            {
                decimal feePercent = feeBuyPro.FeePercent > 0 ? (feeBuyPro.FeePercent ?? 0) : 0;
                serviceFee = feePercent / 100;
            }

            if (user.FeeBuyPro > 0)
                feebpnotdc = item.PriceVND * Convert.ToDecimal(user.FeeBuyPro) / 100 ?? 0;
            else
                feebpnotdc = item.PriceVND * serviceFee ?? 0;

            decimal subfeebp = feebpnotdc * (cKFeeBuyPro / 100);
            decimal feebp = feebpnotdc - subfeebp;

            //Phí mua hàng tối thiểu
            feebp = feebp > (conf.FeeBuyProMin ?? 0) ? feebp : (conf.FeeBuyProMin ?? 0);

            item.FeeBuyPro = feebp;

            //Phí giao hàng nhanh
            item.IsFastPrice = item.IsFast == true ? (item.PriceVND * 5 / 100) : 0;

            //Phí bảo hiểm của shop
            item.InsuranceMoney = item.IsInsurance == true ? (item.PriceVND * conf.InsurancePercent) / 100 : 0;

            //Phí kiểm hàng của shop

            if (item.IsCheckProduct == true)
            {
                item.IsCheckProductPrice = 0;
                var feeCheckProducts = await unitOfWork.Repository<FeeCheckProduct>().GetQueryable().ToListAsync();
                if (feeCheckProducts.Any())
                {
                    var feeCheckProduct = new List<FeeCheckProduct>();
                    if (counprosMore10 > 0)
                    {
                        feeCheckProduct = await unitOfWork.Repository<FeeCheckProduct>().GetQueryable().Where(x => !x.Deleted && x.Type == 1).ToListAsync();
                        foreach (var jtem in feeCheckProduct)
                        {
                            if (counprosMore10 >= jtem.AmountFrom && counprosMore10 <= jtem.AmountTo)
                                item.IsCheckProductPrice += jtem.Fee * counprosMore10;
                        }
                    }

                    if (counprosLes10 > 0)
                    {
                        feeCheckProduct = await unitOfWork.Repository<FeeCheckProduct>().GetQueryable().Where(x => !x.Deleted && x.Type == 2).ToListAsync();
                        foreach (var jtem in feeCheckProduct)
                        {
                            if (counprosLes10 >= jtem.AmountFrom && counprosLes10 <= jtem.AmountTo)
                                item.IsCheckProductPrice += jtem.Fee * counprosLes10;
                        }
                    }
                }
            }
            else
                item.IsCheckProductPrice = 0;

            //Phí đóng gỗ (dựa theo bảng FeePackaged)
            //if (item.IsPacked == true)
            //{

            //}
            //else
            //    item.IsPackedPrice = 0;

            return item;
        }

        public async Task<PagedList<OrderShopTemp>> DeleteOrderShopTempAfterDays(PagedList<OrderShopTemp> orderShopTemps)
        {

            using (var dbContextTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    List<OrderTemp> orderTempsMustDelete = new List<OrderTemp>();
                    foreach (var item in orderShopTemps.Items.ToList())
                    {
                        var config = await unitOfWork.Repository<Entities.Configurations>().GetQueryable().FirstOrDefaultAsync();
                        //Check item created after days
                        int dayRemoveConfig = config.RemoveCartDay;

                        TimeSpan period = (DateTime.Now).Subtract(item.Created ?? DateTime.Now);
                        if (period.Days > dayRemoveConfig)
                        {
                            //Add ordertemp to orderTempsMustDelete
                            orderTempsMustDelete.AddRange(await unitOfWork.Repository<OrderTemp>().GetQueryable().Where(x => x.OrderShopTempId == item.Id).ToListAsync());

                            //Delte ordershoptemp
                            unitOfWork.Repository<OrderShopTemp>().Delete(item);
                            orderShopTemps.Items.Remove(item);
                            orderShopTemps.TotalItem--;
                        }
                        else
                        {
                            decimal currency = config.Currency ?? 0;
                            var user = (await unitOfWork.Repository<Users>().GetQueryable().FirstOrDefaultAsync(x => x.Id == LoginContext.Instance.CurrentUser.UserId));
                            decimal userCurrency = user.Currency ?? 0;
                            if (userCurrency > 0)
                                currency = userCurrency;
                            decimal priceVNDChange = (item.PriceCNY ?? 0) * currency;
                            if (priceVNDChange != item.PriceVND)
                            {
                                item.PriceVND = priceVNDChange;
                                item.FeeBuyPro = await this.CalculateFeeBuyPro(user, item.PriceVND, config);
                                unitOfWork.Repository<OrderShopTemp>().Update(item);
                            }
                        }
                    }
                    //Delete Ordertemps
                    foreach (var orderTemp in orderTempsMustDelete)
                    {
                        unitOfWork.Repository<OrderTemp>().Delete(orderTemp);
                    }

                    await unitOfWork.SaveAsync();
                    await dbContextTransaction.CommitAsync();

                    return orderShopTemps;
                }
                catch (Exception ex)
                {
                    await dbContextTransaction.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            }
        }

        public async Task<OrderShopTemp> CreateWithMainOrderId(int mainOrderId)
        {
            var orderShopTemp = new OrderShopTemp();
            var orders = await unitOfWork.Repository<Order>().GetQueryable().Where(x => x.MainOrderId == mainOrderId).ToListAsync();
            orderShopTemp.ShopId = orders.FirstOrDefault().ShopId;
            orderShopTemp.ShopName = orders.FirstOrDefault().ShopName;
            orderShopTemp.Site = orders.FirstOrDefault().Site;
            foreach (var item in orders)
            {
                var orderTemp = mapper.Map<OrderTemp>(item);
                orderShopTemp.OrderTemps.Add(orderTemp);
            }
            return orderShopTemp;
        }

        private async Task<decimal> CalculateFeeBuyPro(Users user, decimal? priceVND, Entities.Configurations conf)
        {
            //Phí mua hàng
            var userLevel = await unitOfWork.Repository<UserLevel>().GetQueryable().Where(x => x.Id == user.LevelId).FirstOrDefaultAsync();
            var cKFeeBuyPro = userLevel == null ? 0 : userLevel.FeeBuyPro ?? 0;
            decimal serviceFee = 0;
            decimal feebpnotdc = 0;
            var feeBuyPro = await unitOfWork.Repository<FeeBuyPro>().GetQueryable().Where(x => x.PriceFrom < priceVND && priceVND <= x.PriceTo).FirstOrDefaultAsync();
            if (feeBuyPro != null)
            {
                decimal feePercent = feeBuyPro.FeePercent > 0 ? (feeBuyPro.FeePercent ?? 0) : 0;
                serviceFee = feePercent / 100;
            }

            if (user.FeeBuyPro > 0)
                feebpnotdc = priceVND * Convert.ToDecimal(user.FeeBuyPro) / 100 ?? 0;
            else
                feebpnotdc = priceVND * serviceFee ?? 0;

            decimal subfeebp = feebpnotdc * (cKFeeBuyPro / 100);
            decimal feebp = feebpnotdc - subfeebp;

            //Phí mua hàng tối thiểu
            feebp = feebp > (conf.FeeBuyProMin ?? 0) ? feebp : (conf.FeeBuyProMin ?? 0);

            return feebp;
        }
    }
}
