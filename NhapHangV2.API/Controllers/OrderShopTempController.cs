using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NhapHangV2.BaseAPI.Controllers;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Interface.Services.Configuration;
using NhapHangV2.Models;
using NhapHangV2.Request;
using NhapHangV2.Request.DomainRequests;
using NhapHangV2.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.API.Controllers
{
    [Route("api/order-shop-temp")]
    [Description("Quản lý shop trong giỏ hàng")]
    [ApiController]
    [Authorize]
    public class OrderShopTempController : BaseController<OrderShopTemp, OrderShopTempModel, OrderShopTempRequest, OrderShopTempSearch>
    {
        private readonly IOrderTempService orderTempService;
        private readonly IOrderShopTempService orderShopTempService;
        private readonly IUserService userService;
        private readonly IUserLevelService userLevelService;
        private readonly IConfigurationsService configurationsService;
        private readonly IMainOrderService mainOrderService;
        protected readonly INotificationSettingService notificationSettingService;
        protected readonly INotificationTemplateService notificationTemplateService;
        protected readonly ISendNotificationService sendNotificationService;

        public OrderShopTempController(IServiceProvider serviceProvider, ILogger<BaseController<OrderShopTemp, OrderShopTempModel, OrderShopTempRequest, OrderShopTempSearch>> logger, IWebHostEnvironment env) : base(serviceProvider, logger, env)
        {
            this.domainService = this.serviceProvider.GetRequiredService<IOrderShopTempService>();
            orderTempService = serviceProvider.GetRequiredService<IOrderTempService>();
            orderShopTempService = serviceProvider.GetRequiredService<IOrderShopTempService>();
            userService = serviceProvider.GetRequiredService<IUserService>();
            userLevelService = serviceProvider.GetRequiredService<IUserLevelService>();
            configurationsService = serviceProvider.GetRequiredService<IConfigurationsService>();
            mainOrderService = serviceProvider.GetRequiredService<IMainOrderService>();
            notificationSettingService = serviceProvider.GetRequiredService<INotificationSettingService>();
            notificationTemplateService = serviceProvider.GetRequiredService<INotificationTemplateService>();
            sendNotificationService = serviceProvider.GetRequiredService<ISendNotificationService>();
        }

        /// <summary>
        /// Xóa shop + những sản phẩm của shop trong giỏ hàng đã chọn
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete("selected")]
        [AppAuthorize(new int[] { CoreContants.Delete })]
        public AppDomainResult DeleteSelectedItem(List<int> ids)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            bool success = orderShopTempService.DeleteSelectedId(ids);
            if (success)
            {
                appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                appDomainResult.Success = success;
            }
            else
                throw new AppException("Lỗi trong quá trình xử lý");

            return appDomainResult;
        }


        /// <summary>
        /// Lấy danh sách item phân trang
        /// Loại thông báo: 0-Yêu cầu nạp,1-Yêu cầu rút, 2-Đơn hàng, 3-Khiếu nại, 4-Tất cả
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpGet]
        [AppAuthorize(new int[] { CoreContants.ViewAll })]
        public override async Task<AppDomainResult> Get([FromQuery] OrderShopTempSearch baseSearch)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            if (ModelState.IsValid)
            {
                PagedList<OrderShopTemp> pagedData = await this.domainService.GetPagedListData(baseSearch);
                pagedData = await orderShopTempService.DeleteOrderShopTempAfterDays(pagedData);
                PagedList<OrderShopTempModel> pagedDataModel = mapper.Map<PagedList<OrderShopTempModel>>(pagedData);

                appDomainResult = new AppDomainResult
                {
                    Data = pagedDataModel,
                    Success = true,
                    ResultCode = (int)HttpStatusCode.OK
                };
            }
            else
                throw new AppException(ModelState.GetErrorMessage());

            return appDomainResult;
        }
        /// <summary>
        /// Cập nhật thông tin item
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public override async Task<AppDomainResult> UpdateItem([FromBody] OrderShopTempRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (ModelState.IsValid)
            {
                var item = mapper.Map<OrderShopTemp>(itemModel);
                if (item != null)
                {
                    // Kiểm tra item có tồn tại chưa?
                    var messageUserCheck = await this.domainService.GetExistItemMessage(item);
                    if (!string.IsNullOrEmpty(messageUserCheck))
                        throw new AppException(messageUserCheck);
                    success = await this.domainService.UpdateAsync(item);
                    if (success)
                    {
                        appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                        appDomainResult.Data = this.domainService.GetByIdAsync(item.Id);
                    }
                    else
                        throw new Exception("Lỗi trong quá trình xử lý");
                    appDomainResult.Success = success;
                }
                else
                    throw new KeyNotFoundException("Item không tồn tại");
            }
            else
                throw new AppException(ModelState.GetErrorMessage());

            return appDomainResult;
        }

        /// <summary>
        /// Tính tổng tiền khi chọn shop
        /// </summary>
        /// <returns></returns>
        [HttpGet("get-total-price")]
        [AppAuthorize(new int[] { CoreContants.View })]
        public async Task<AppDomainResult> GetTotalPriceVND([FromQuery] List<int> orderShopTempIds)
        {
            decimal? maxTotalPriceVND = 0;

            foreach (int id in orderShopTempIds)
            {
                var orderShopTemp = await this.domainService.GetByIdAsync(id);
                if (orderShopTemp == null) continue;

                maxTotalPriceVND += orderShopTemp.TotalPriceVND;
            }

            return new AppDomainResult()
            {
                Data = maxTotalPriceVND,
                Success = true,
                ResultCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Xóa shop + những sản phẩm của shop trong giỏ hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [AppAuthorize(new int[] { CoreContants.Delete })]
        public override async Task<AppDomainResult> DeleteItem(int id)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            if (id == 0)
                throw new KeyNotFoundException("id không tồn tại");

            bool success = await this.domainService.DeleteAsync(id);
            if (success)
            {
                appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                appDomainResult.Success = success;
            }
            else
                throw new Exception("Lỗi trong quá trình xử lý");

            return appDomainResult;
        }

        /// <summary>
        /// Cập nhật 4 trường của shop
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut("update-field")]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> UpdateField([FromBody] UpdateFieldOrderShopTempRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (ModelState.IsValid)
            {
                // Kiểm tra item có tồn tại chưa?
                var item = await this.domainService.GetByIdAsync(itemModel.Id ?? 0);
                if (item != null)
                {
                    item.IsFastDelivery = itemModel.IsFastDelivery;
                    item.IsCheckProduct = itemModel.IsCheckProduct;
                    item.IsPacked = itemModel.IsPacked;
                    item.IsInsurance = itemModel.IsInsurance;

                    //Update giá
                    item = await orderShopTempService.UpdatePrice(item);

                    //Cập nhật lại
                    success = await this.domainService.UpdateAsync(item);

                    if (success)
                        appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                    else
                        throw new Exception("Lỗi trong quá trình xử lý");
                    appDomainResult.Success = success;
                }
                else
                    throw new KeyNotFoundException("Item không tồn tại");
            }
            else
                throw new AppException(ModelState.GetErrorMessage());

            return appDomainResult;
        }

        /// <summary>
        /// Cập nhật ghi chú của shop
        /// </summary>
        /// <param name="id"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        [HttpPut("update-note")]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> UpdateNote(int id, string note)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (ModelState.IsValid)
            {
                // Kiểm tra item có tồn tại chưa?
                var item = await this.domainService.GetByIdAsync(id);
                if (item != null)
                {
                    item.Note = note;
                    success = await this.domainService.UpdateAsync(item);

                    if (success)
                        appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                    else
                        throw new Exception("Lỗi trong quá trình xử lý");
                    appDomainResult.Success = success;
                }
                else
                    throw new KeyNotFoundException("Item không tồn tại");
            }
            else
                throw new AppException(ModelState.GetErrorMessage());

            return appDomainResult;
        }

        /// <summary>
        /// Thanh toán đơn hàng
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("payment")]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> Payment([FromBody] PaymentRequest model) //Tựa với thằng MainOrder
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (ModelState.IsValid)
            {
                DateTime currentDate = DateTime.Now;
                string userName = LoginContext.Instance.CurrentUser.UserName;
                var users = await userService.GetSingleAsync(x => x.UserName == userName);

                //Lấy % đặt cọc của User              
                //var userLevel = await userLevelService.GetByIdAsync(users.LevelId ?? 0);
                //double lessDepositOfUser = 0;
                //if (userLevel != null)
                //    lessDepositOfUser = (double)userLevel.LessDeposit.Value / 100;

                int salerId = users.SaleId ?? 0;
                int datHangId = users.DatHangId ?? 0;
                int UID = users.Id;
                decimal feeBuyProUser = users.FeeBuyPro ?? 0;

                var configurations = await configurationsService.GetSingleAsync();
                decimal insurancePercent = Convert.ToDecimal(configurations.InsurancePercent);
                decimal currency = Convert.ToDecimal(configurations.Currency);

                if (users.Currency > 0)
                    currency = users.Currency ?? 0;

                //Lấy % đặt cọc của User
                var userLevel = await userLevelService.GetByIdAsync(users.LevelId ?? 0);
                decimal cKFeeBuyPro = userLevel == null ? 0 : userLevel.FeeBuyPro ?? 0;
                decimal lessDeposit = userLevel == null ? 0 : (userLevel.LessDeposit) ?? 0;

                if (users.Deposit > 0)
                    lessDeposit = (users.Deposit) ?? 0;

                //Chỗ này khác với MainOrder
                string fullName = model.FullName;
                string phone = model.Phone;
                string address = model.Address;
                string email = model.Email;

                List<MainOrder> mainOrders = new List<MainOrder>();
                
                foreach (var item in model.ShopPayments) //Thanh toán giỏ hàng
                {
                    var orderShopTemp = await orderShopTempService.GetByIdAsync(item.ShopId);

                    var orderTemps = await orderTempService.GetAsync(x => !x.Deleted && x.Active
                        && (x.OrderShopTempId == item.ShopId)
                    );

                    if (orderShopTemp == null || !orderTemps.Any())
                        throw new KeyNotFoundException("Không tìm thấy shop và sản phẩm cần thanh toán");

                    decimal pricePro = orderShopTemp.PriceVND ?? 0;
                    decimal priceProCNY = orderShopTemp.PriceCNY ?? 0;
                    decimal feebp = orderShopTemp.FeeBuyPro ?? 0;
                    //Tính phí mua hàng tối thiểu
                    feebp = feebp > (configurations.FeeBuyProMin ?? 0) ? feebp : (configurations.FeeBuyProMin ?? 0);

                    decimal feecnship = 0;

                    string shopId = orderShopTemp.ShopId;
                    string shopName = orderShopTemp.ShopName;
                    string site = orderShopTemp.Site;

                    decimal total = (orderShopTemp.IsFastPrice ?? 0) + pricePro + feebp + feecnship + (orderShopTemp.IsCheckProductPrice ?? 0) + (orderShopTemp.InsuranceMoney ?? 0);

                    //Lên giá từng sản phẩm trong shop
                    decimal priceVND = 0;
                    decimal priceCNY = 0;
                    //decimal priceCNYAllProduct = 0;

                    List<Order> orders = new List<Order>();
                    string imageOrigin = orderTemps[0].ImageOrigin.Replace("%2F", "/").Replace("%3A", ":");
                    //Chỗ này khác với MainOrder
                    foreach (var jtem in orderTemps)
                    {
                        int quantity = Convert.ToInt32(jtem.Quantity);
                        decimal originPrice = Convert.ToDecimal(jtem.PriceOrigin);
                        decimal promotionPrice = Convert.ToDecimal(jtem.PricePromotion);

                        decimal uPriceBuy = 0;
                        decimal uPriceVN = 0;
                        //decimal ePriceBuy = 0;
                        //decimal ePriceVN = 0;

                        if (promotionPrice > 0 && promotionPrice < originPrice)
                        {
                            uPriceBuy = promotionPrice;
                            uPriceVN = promotionPrice * currency;
                        }
                        else
                        {
                            uPriceBuy = originPrice;
                            uPriceVN = originPrice * currency;
                        }


                        priceCNY += (uPriceBuy * quantity);
                        priceVND += (uPriceVN * quantity);

                        //priceCNYAllProduct += ePriceBuy;

                        string image = jtem.ImageOrigin;
                        if (image.Contains("%2F"))
                        {
                            image = image.Replace("%2F", "/");
                        }
                        if (image.Contains("%3A"))
                        {
                            image = image.Replace("%3A", ":");
                        }

                        orders.Add(new Order()
                        {
                            UID = UID,
                            TitleOrigin = jtem.TitleOrigin,
                            TitleTranslated = jtem.TitleTranslated,
                            PriceOrigin = originPrice, //jtem.price_origin,
                            PricePromotion = promotionPrice, //jtem.price_promotion,
                            PropertyTranslated = jtem.PropertyTranslated,
                            Property = jtem.Property,
                            DataValue = jtem.DataValue,
                            ImageOrigin = image,
                            ShopId = jtem.ShopId,
                            ShopName = jtem.ShopName,
                            SellerId = jtem.SellerId,
                            Wangwang = jtem.Wangwang,
                            Quantity = quantity, //jtem.quantity,
                            Stock = jtem.Stock,
                            LocationSale = jtem.LocationSale,
                            Site = jtem.Site,
                            Comment = jtem.Comment,
                            ItemId = jtem.ItemId,
                            LinkOrigin = jtem.LinkOrigin,
                            OuterId = jtem.OuterId,
                            Error = jtem.Error,
                            Weight = jtem.Weight,
                            StepPrice = jtem.StepPrice,
                            Brand = jtem.Brand,
                            CategoryName = jtem.CategoryName,
                            CategoryId = jtem.CategoryId ?? 0,
                            Tool = jtem.Tool,
                            Version = jtem.Version,
                            IsTranslate = jtem.IsTranslate,
                            IsFastDelivery = orderShopTemp.IsFastDelivery,
                            IsFastDeliveryPrice = orderShopTemp.IsFastDeliveryPrice,
                            IsCheckProduct = orderShopTemp.IsCheckProduct,
                            IsCheckProductPrice = orderShopTemp.IsCheckProductPrice,
                            IsPacked = orderShopTemp.IsPacked,
                            IsPackedPrice = orderShopTemp.IsPackedPrice,
                            IsFast = orderShopTemp.IsFast,
                            IsFastPrice = orderShopTemp.IsFastPrice,
                            PriceVND = uPriceVN, //Hình như là như vầy mới đúng
                            PriceCNY = uPriceBuy, //Hình như là như vầy mới đúng
                            Note = jtem.Brand,
                            FullName = orderShopTemp.FullName,
                            Address = orderShopTemp.Address,
                            Email = orderShopTemp.Email,
                            Phone = orderShopTemp.Phone,
                            Status = 0,
                            Deposit = 0,
                            CurrentCNYVN = currency,
                            TotalPriceVND = Math.Round(total, 0),
                            RealPrice = promotionPrice > 0 ? promotionPrice : originPrice,
                            Deleted = false,
                            Active = true,
                            Created = currentDate,
                            CreatedBy = userName,
                            MainOrderId = 0, //Hiện tại chưa có
                        });
                    }

                    //string note = orderShopTemp.Note;
                    string note = item.UserNote;

                    decimal deposit = 0;
                    decimal totalPriceVND = total;
                    decimal totalProductPrice = pricePro;
                    decimal totalForDeposit = pricePro + feebp;
                    //Đặt cọc theo tiền hàng trên web
                    //decimal amountDeposit = lessDeposit > 0 ? (totalProductPrice * lessDeposit / 100) : totalProductPrice;
                    //Đặt cọc theo tiền hàng trên web + phí mua hàng
                    decimal amountDeposit = lessDeposit > 0 ? (totalForDeposit * lessDeposit / 100) : totalForDeposit;

                    //Thông báo

                    //Tính phí hoa hồng
                    List<StaffIncome> staffIncomes = new List<StaffIncome>();

                    decimal salePercent = Convert.ToDecimal(configurations.SalePercent);
                    decimal salePercentAf3M = Convert.ToDecimal(configurations.SalePercentAfter3Month);
                    decimal datHangPercent = Convert.ToDecimal(configurations.DatHangPercent);

                    if (salerId > 0)
                    {
                        StaffIncome staffIncomeSaler = new StaffIncome();
                        staffIncomeSaler.MainOrderId = 0; //Vào service thì add vào nha
                        staffIncomeSaler.OrderTotalPrice = 0;
                        staffIncomeSaler.Status = (int?)StatusStaffIncome.Unpaid;

                        var sale = await userService.GetByIdAsync(salerId);
                        if (sale != null)
                        {
                            staffIncomeSaler.UID = sale.Id;

                            int d = DateTime.Now.Subtract(users.Created.Value).Days;
                            if (d > 90)
                            {
                                staffIncomeSaler.TotalPriceReceive = feebp * salePercentAf3M / 100;
                                staffIncomeSaler.PercentReceive = salePercentAf3M;
                            }
                            else
                            {
                                staffIncomeSaler.TotalPriceReceive = feebp * salePercent / 100;
                                staffIncomeSaler.PercentReceive = salePercent;
                            }
                        }

                        staffIncomes.Add(staffIncomeSaler);
                    }
                    if (datHangId > 0)
                    {
                        StaffIncome staffIncomeOrder = new StaffIncome();
                        staffIncomeOrder.MainOrderId = 0; //Vào service thì add vào nha
                        staffIncomeOrder.OrderTotalPrice = 0;
                        staffIncomeOrder.Status = (int?)StatusStaffIncome.Unpaid;

                        var datHang = await userService.GetByIdAsync(datHangId);
                        if (datHang != null)
                        {
                            staffIncomeOrder.UID = datHang.Id;
                            staffIncomeOrder.PercentReceive = datHangPercent;
                            staffIncomeOrder.TotalPriceReceive = 0;

                            //Thông báo
                        }
                        staffIncomes.Add(staffIncomeOrder);
                    }

                    MainOrder mainOrder = new MainOrder
                    {
                        EditedFeeBuyProPercent = orderShopTemp.FeeBuyProPercent,
                        ImageOrigin = imageOrigin,
                        UID = UID,
                        ShopId = shopId,
                        ShopName = shopName,
                        Site = site,
                        FeeBuyProUser = feeBuyProUser,
                        IsFastDelivery = orderShopTemp.IsFastDelivery,
                        IsFastDeliveryPrice = orderShopTemp.IsFastDeliveryPrice,
                        IsCheckProduct = orderShopTemp.IsCheckProduct,
                        IsCheckProductPrice = orderShopTemp.IsCheckProductPrice,
                        IsCheckProductPriceCNY = orderShopTemp.IsCheckProductPrice / currency,
                        IsPacked = orderShopTemp.IsPacked,
                        IsPackedPrice = orderShopTemp.IsPackedPrice,
                        IsPackedPriceCNY = orderShopTemp.IsPackedPrice / currency,
                        PriceVND = priceVND,
                        PriceCNY = priceCNY,
                        FeeShipCN = feecnship,
                        CKFeeBuyPro = Math.Round(feebp / currency, 2),
                        FeeBuyProCK = cKFeeBuyPro,
                        FeeBuyPro = feebp,
                        FeeWeight = 0,
                        Note = note,
                        ReceiverFullName = fullName,
                        DeliveryAddress = address,
                        ReceiverEmail = email,
                        ReceiverPhone = phone,
                        Status = (int)StatusOrderContants.DonMoi,
                        Deposit = deposit,
                        CurrentCNYVN = currency,
                        TotalPriceVND = totalPriceVND,
                        SalerId = salerId,
                        DatHangId = datHangId,
                        FeeShipCNToVN = 0,
                        //IsHidden = false,
                        IsUpdatePrice = false,
                        AmountDeposit = amountDeposit,
                        OrderType = 1,
                        LessDeposit = lessDeposit,
                        FeeBuyProPT = feeBuyProUser,

                        ReceivePlace = item.WarehouseVN,
                        ShippingType = item.ShippingType,
                        FromPlace = item.WarehouseTQ,

                        IsInsurance = orderShopTemp.IsInsurance,
                        InsuranceMoney = orderShopTemp.InsuranceMoney,
                        InsurancePercent = insurancePercent,

                        Deleted = false,
                        Active = true,
                        Created = currentDate,
                        CreatedBy = userName,

                        Orders = orders,
                        StaffIncomes = staffIncomes,
                        ShopTempId = orderShopTemp.Id,

                    };
                    mainOrders.Add(mainOrder);
                }

                success = await mainOrderService.CreateOrder(mainOrders, false);

                if (success)
                    appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                else
                    throw new Exception("Lỗi trong quá trình xử lý");
                appDomainResult.Success = success;
            }
            else
            {
                throw new AppException(ModelState.GetErrorMessage());
            }
            return appDomainResult;
        }

        /// <summary>
        /// Đặt hàng qua Extension
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPost("new")]
        [AppAuthorize(new int[] { CoreContants.AddNew })]
        public async Task<AppDomainResult> AddItemNew([FromBody] OrderShopTempRequestNew itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (ModelState.IsValid)
            {
                //if(itemModel.Orders.Count <=0)
                //    throw new KeyNotFoundException("Số lượng sản phẩm nhỏ hơn 0");
                var item = mapper.Map<OrderShopTemp>(itemModel);
                item.OrderTemps = mapper.Map<List<OrderTemp>>(itemModel.Orders);
                foreach (var order in item.OrderTemps)
                {
                    order.ShopId = item.ShopId;
                    order.ShopName = item.ShopName;
                    order.Site = item.Site;
                    order.Tool = itemModel.Tool;
                    order.Version = itemModel.Version;
                    order.MinimumQuantity = itemModel.MinimumQuantity ?? 0;
                    order.StepPrice = itemModel.StepPrice;
                    order.LinkOrigin = itemModel.Link;
                    order.ItemId = itemModel.ItemID;
                    order.Wangwang = itemModel.Wangwang;
                    order.SellerId = itemModel.SalerId;
                }
                if (item != null)
                {
                    success = await orderShopTempService.CreateAsync(item);
                    if (success)
                    {
                        appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                    }
                    else
                        throw new Exception("Lỗi trong quá trình xử lý");
                    appDomainResult.Success = success;
                }
                else
                    throw new KeyNotFoundException("Item không tồn tại");
            }
            else
            {
                throw new AppException(ModelState.GetErrorMessage());
            }
            return appDomainResult;
        }

        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1); // Ví dụ, giới hạn 1 request đồng thời
        /// <summary>
        /// Đặt hàng qua Extension
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPost]
        [AppAuthorize(new int[] { CoreContants.AddNew })]
        public override async Task<AppDomainResult> AddItem([FromBody] OrderShopTempRequest itemModel)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                AppDomainResult appDomainResult = new AppDomainResult();
                bool success = false;
                if (ModelState.IsValid)
                {
                    //new
                    //var item = mapper.Map<OrderShopTemp>(itemModel);

                    //old
                    var orderTemp = mapper.Map<OrderTemp>(itemModel);
                    var item = new OrderShopTemp();
                    item.ShopId = itemModel.shop_id;
                    item.ShopName = itemModel.shop_name;
                    item.Site = itemModel.site;
                    item.OrderTemps.Add(orderTemp);

                    if (item != null)
                    {
                        // Kiểm tra item có tồn tại chưa?
                        var messageUserCheck = await this.domainService.GetExistItemMessage(item);
                        if (!string.IsNullOrEmpty(messageUserCheck))
                            throw new KeyNotFoundException(messageUserCheck);
                        success = await orderShopTempService.CreateAsync(item);
                        if (success)
                        {
                            appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                        }
                        else
                            throw new Exception("Lỗi trong quá trình xử lý");
                        appDomainResult.Success = success;
                    }
                    else
                        throw new KeyNotFoundException("Item không tồn tại");
                }
                else
                {
                    throw new AppException(ModelState.GetErrorMessage());
                }
                return appDomainResult;
            }
            finally
            {
                semaphoreSlim.Release(); // Giải phóng SemaphoreSlim để chấp nhận request mới
            }
        }
        /// <summary>
        /// Đặt đơn tương tự
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("add-same")]
        [AppAuthorize(new int[] { CoreContants.AddNew })]
        public async Task<AppDomainResult> AddSame([FromBody] AppDomainRequest request)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (ModelState.IsValid)
            {
                var item = await orderShopTempService.CreateWithMainOrderId(request.Id);
                if (item != null)
                {
                    // Kiểm tra item có tồn tại chưa?
                    var messageUserCheck = await this.domainService.GetExistItemMessage(item);
                    if (!string.IsNullOrEmpty(messageUserCheck))
                        throw new KeyNotFoundException(messageUserCheck);
                    success = await orderShopTempService.CreateAddSameAsync(item);
                    if (success)
                    {
                        appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                    }
                    else
                        throw new Exception("Lỗi trong quá trình xử lý");
                    appDomainResult.Success = success;
                }
                else
                    throw new KeyNotFoundException("Item không tồn tại");
            }
            else
            {
                throw new AppException(ModelState.GetErrorMessage());
            }
            return appDomainResult;
        }


    }
}
