using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NhapHangV2.BaseAPI.Controllers;
using NhapHangV2.Entities;
using NhapHangV2.Entities.Search;
using NhapHangV2.Entities.SQLEntities;
using NhapHangV2.Extensions;
using NhapHangV2.Interface.Services;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Models;
using NhapHangV2.Request;
using NhapHangV2.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using static NhapHangV2.Utilities.CoreContants;

namespace NhapHangV2.API.Controllers
{
    [Route("api/main-order")]
    [Description("Quản lý danh sách đơn hàng")]
    [ApiController]
    [Authorize]
    public class MainOrderController : BaseController<MainOrder, MainOrderModel, MainOrderRequest, MainOrderSearch>
    {
        protected readonly IConfiguration configuration;
        protected readonly IMainOrderService mainOrderService;
        protected readonly IUserService userService;
        protected readonly IUserLevelService userLevelService;
        protected readonly IConfigurationsService configurationsService;
        protected readonly IFeeBuyProService feeBuyProService;
        protected readonly ISmallPackageService smallPackageService;
        protected readonly IMainOrderCodeService mainOrderCodeService;
        protected readonly IFeeSupportService feeSupportService;
        protected readonly IWarehouseService warehouseService;
        protected readonly IWarehouseFromService warehouseFromService;
        protected readonly IShippingTypeToWareHouseService shippingTypeToWareHouseService;
        protected readonly IStaffIncomeService staffIncomeService;
        protected readonly IFeeCheckProductService feeCheckProductService;

        public MainOrderController(IServiceProvider serviceProvider, ILogger<BaseController<MainOrder, MainOrderModel, MainOrderRequest, MainOrderSearch>> logger, IWebHostEnvironment env, IConfiguration configuration) : base(serviceProvider, logger, env)
        {
            this.domainService = this.serviceProvider.GetRequiredService<IMainOrderService>();
            this.configuration = configuration;
            mainOrderService = serviceProvider.GetRequiredService<IMainOrderService>();
            userService = serviceProvider.GetRequiredService<IUserService>();
            userLevelService = serviceProvider.GetRequiredService<IUserLevelService>();
            configurationsService = serviceProvider.GetRequiredService<IConfigurationsService>();
            feeBuyProService = serviceProvider.GetRequiredService<IFeeBuyProService>();
            smallPackageService = serviceProvider.GetRequiredService<ISmallPackageService>();
            mainOrderCodeService = serviceProvider.GetRequiredService<IMainOrderCodeService>();
            feeSupportService = serviceProvider.GetRequiredService<IFeeSupportService>();
            shippingTypeToWareHouseService = serviceProvider.GetRequiredService<IShippingTypeToWareHouseService>();
            staffIncomeService = serviceProvider.GetRequiredService<IStaffIncomeService>();
            warehouseFromService = serviceProvider.GetRequiredService<IWarehouseFromService>();
            warehouseService = serviceProvider.GetRequiredService<IWarehouseService>();
            feeCheckProductService = serviceProvider.GetRequiredService<IFeeCheckProductService>();
        }
        ///<summary>
        ///Thông tin đơn hàng trang đơn hàng của user
        /// </summary>
        [HttpGet("order-staff")]
        [AppAuthorize(new int[] { CoreContants.View })]
        public async Task<AppDomainResult> GetMainOrderOfOrderStaff([FromQuery] MainOrderToolSearch search)
        {
            var order = await mainOrderService.GetPagedListOfOrderStaff(search);
            return new AppDomainResult
            {
                Data = order,
                ResultCode = (int)HttpStatusCode.OK,
                Success = true
            };
        }

        [HttpPut("update-currency")]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> UpdateCurrency(MainOrderRequest request)
        {
            if (!ModelState.IsValid)
            {
                throw new AppException(ModelState.GetErrorMessage());
            }
            await mainOrderService.UpdateCurrency(request.Id, request.CurrentCNYVN ?? 0);
            return new AppDomainResult
            {
                ResultCode = (int)HttpStatusCode.OK,
                Success = true,
            };
        }
        ///<summary>
        ///Thông tin đơn hàng trang đơn hàng của user
        /// </summary>
        [HttpGet("get-mainorder-infor")]
        [AppAuthorize(new int[] { CoreContants.View })]
        public async Task<AppDomainResult> GetMainOrderInfor(int orderType)
        {
            int UID = LoginContext.Instance.CurrentUser.UserId;
            var users = await userService.GetSingleAsync(x => x.Id == UID);
            if (users == null)
                throw new AppException("Người dùng không tồn tại");
            var mainOrdersInfor = mainOrderService.GetMainOrdersInfor(UID, orderType);
            return new AppDomainResult
            {
                Data = mainOrdersInfor,
                ResultCode = (int)HttpStatusCode.OK,
                Success = true
            };
        }

        ///<summary>
        ///Thông tin tiền hàng trang đơn hàng của user
        /// </summary>
        [HttpGet("get-mainorders-amount")]
        [AppAuthorize(new int[] { CoreContants.View })]
        public async Task<AppDomainResult> GetMainOdersAmount(int orderType)
        {
            int UID = LoginContext.Instance.CurrentUser.UserId;
            var users = await userService.GetSingleAsync(x => x.Id == UID);
            if (users == null)
                throw new AppException("Người dùng không tồn tại");
            var mainOrderAmount = mainOrderService.GetMainOrdersAmount(UID, orderType);
            return new AppDomainResult
            {
                ResultCode = (int)HttpStatusCode.OK,
                Success = true,
                Data = mainOrderAmount
            };
        }


        /// <summary>
        /// (Chưa có filter) Đếm số các trạng thái của tài khoản (admin thì thấy được hết) - chưa phân quyền nên chưa làm cho admin
        /// </summary>
        /// <returns></returns>
        [HttpGet("get-total-status")]
        [AppAuthorize(new int[] { CoreContants.View })]
        public async Task<AppDomainResult> GetTotalStatus()
        {
            string userName = LoginContext.Instance.CurrentUser.UserName;
            var users = await userService.GetSingleAsync(x => x.UserName == userName);
            IList<MainOrder> mainOrders = new List<MainOrder>();
            IList<TotalStatus> totalStatuses = new List<TotalStatus>();

            //Lấy ra danh sách đơn hàng của user (admin thì thấy hết, user thì chỉ thấy mỗi thằng đó thôi)
            mainOrders = await mainOrderService.GetAsync(x => !x.Deleted && x.Active
                && (x.UID == users.Id)
            );

            //Đếm trạng thái tất cả trước
            totalStatuses.Add(new TotalStatus
            {
                Status = -1,
                Total = mainOrders.Count()
            });

            //Đếm các trạng thái
            foreach (StatusOrderContants item in (StatusOrderContants[])Enum.GetValues(typeof(StatusOrderContants)))
            {
                int i = (int)item;

                //if (i == 1 || i == 3 || i == 4 || i == 8 || i == 11) //(bỏ qua 1, 3, 4, 8, 11)
                //    continue;

                totalStatuses.Add(new TotalStatus
                {
                    Status = i,
                    Total = mainOrders.Count(x => x.Status == i)
                });
            }

            return new AppDomainResult()
            {
                Data = totalStatuses,
                Success = true,
                ResultCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// (Chưa có filter) Tính tổng tiền hàng của khách hàng đó (admin thì tính tổng tất cả các đơn) - chưa phân quyền nên chưa làm cho admin
        /// </summary>
        /// <returns></returns>
        [HttpGet("get-total-amount")]
        [AppAuthorize(new int[] { CoreContants.View })]
        public async Task<AppDomainResult> GetTotalAmount()
        {
            string userName = LoginContext.Instance.CurrentUser.UserName;
            var users = await userService.GetSingleAsync(x => x.UserName == userName);
            IList<MainOrder> mainOrders = new List<MainOrder>();
            IList<TotalAmount> totalAmounts = new List<TotalAmount>();

            //Lấy ra danh sách đơn hàng của user (admin thì thấy hết, user thì chỉ thấy mỗi thằng đó thôi)
            mainOrders = await mainOrderService.GetAsync(x => !x.Deleted && x.Active
                && (x.UID == users.Id)
            );

            //Tính tổng tiền trạng thái trước
            totalAmounts.Add(new TotalAmount
            {
                Status = -1, //Tổng tiền hàng
                Amount = mainOrders.Where(x => x.Status == 2 || x.Status == 5 || x.Status == 6 || x.Status == 7).Sum(x => x.TotalPriceVND)
            });

            //Tính tổng tiền các trạng thái
            foreach (StatusOrderContants item in (StatusOrderContants[])Enum.GetValues(typeof(StatusOrderContants)))
            {
                int i = (int)item;

                //if (i == 1 || i == 3 || i == 4 || i == 8 || i == 11) //(bỏ qua 1, 3, 4, 8, 11)
                //    continue;

                if (i == 0) //Có 2 trường hợp
                {
                    totalAmounts.Add(new TotalAmount
                    {
                        Status = i,
                        IsDeposit = true,
                        Amount = mainOrders.Where(x => x.Status == i).Sum(x => x.AmountDeposit)
                    });

                    totalAmounts.Add(new TotalAmount
                    {
                        Status = i,
                        Amount = mainOrders.Where(x => x.Status == i).Sum(x => x.TotalPriceVND)
                    });

                    continue;
                }

                if (i == 7) //Có 2 trường hợp
                {
                    decimal? totalPrice7 = mainOrders.Where(x => x.Status == i).Sum(x => x.TotalPriceVND);
                    decimal? deposit7 = mainOrders.Where(x => x.Status == i).Sum(x => x.Deposit);

                    totalAmounts.Add(new TotalAmount
                    {
                        Status = i,
                        IsDeposit = true,
                        Amount = totalPrice7 - deposit7
                    });

                    totalAmounts.Add(new TotalAmount
                    {
                        Status = i,
                        Amount = totalPrice7
                    });

                    continue;
                }

                //Những trường hợp còn lại
                totalAmounts.Add(new TotalAmount
                {
                    Status = i,
                    Amount = mainOrders.Where(x => x.Status == i).Sum(x => x.TotalPriceVND)
                });
            }

            return new AppDomainResult()
            {
                Data = totalAmounts,
                Success = true,
                ResultCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng (Đặt cọc, thanh toán,...) cho User
        /// </summary>
        /// <param name="listId">Danh sách ID</param>
        /// <param name="status">1: Hủy, 2: Đặt cọc, 7: Thanh toán</param>
        /// <returns></returns>
        [HttpPut("update-order")]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> UpdateMainOrder([FromBody] List<int> listId, int status)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;

            int userId = LoginContext.Instance.CurrentUser.UserId;
            success = await mainOrderService.UpdateMainOrder(listId, status, userId);

            //Lấy danh sách các mã vận đơn của đơn hàng
            List<SmallPackage> smallPackageUpdates = new List<SmallPackage>();
            if (status == 7 && success)
            {
                foreach (var item in listId)
                {
                    var smallPackages = await smallPackageService.GetAllByMainOrderId(item);
                    smallPackageUpdates.AddRange(smallPackages);
                }
            }
            //Cập nhật trạng thái đã thanh toán các mã vận đơn của đơn hàng
            if (smallPackageUpdates.Count > 0)
            {
                foreach (var item in smallPackageUpdates)
                {
                    item.IsPayment = true;
                    success = await smallPackageService.UpdateFieldAsync(item, new Expression<Func<SmallPackage, object>>[]
                    {
                        s => s.IsPayment
                    });

                }
            }

            if (success)
            {
                appDomainResult.ResultCode = (int)HttpStatusCode.OK;
            }
            else
                throw new Exception("Lỗi trong quá trình xử lý");
            appDomainResult.Success = success;
            return appDomainResult;
        }

        /// <summary>
        /// Lấy tổng tiền đơn hàng đặt cọc
        /// </summary>
        /// <param name="listID"></param>
        /// <returns></returns>
        [HttpGet("get-total-deposit")]
        [AppAuthorize(new int[] { CoreContants.View })]
        public async Task<decimal> CheckDeposit([FromBody] List<int> listID)
        {
            if (!listID.Any())
                throw new KeyNotFoundException("List không tồn tại");

            var mainOrderList = await this.domainService.GetAsync(x => !x.Deleted && x.Active
                && (listID.Contains(x.Id))
            );

            return !mainOrderList.Any() ? 0 : (mainOrderList.Sum(x => x.AmountDeposit - x.Deposit)) ?? 0;
        }

        /// <summary>
        /// Lấy tổng tiền cần thanh toán
        /// </summary>
        /// <param name="listID"></param>
        /// <returns></returns>
        [HttpGet("get-total-payment")]
        public async Task<decimal> CheckPayment([FromQuery] List<int> listID)
        {
            if (!listID.Any())
                throw new KeyNotFoundException("List không tồn tại");

            var mainOrderList = await this.domainService.GetAsync(x => !x.Deleted && x.Active
                && (listID.Contains(x.Id))
            );

            return !mainOrderList.Any() ? 0 : (mainOrderList.Sum(x => x.TotalPriceVND - x.Deposit)) ?? 0;
        }

        /// <summary>
        /// Tạo đơn hàng khác (UID = 0 nếu là trang Users)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("add-another")]
        [AppAuthorize(new int[] { CoreContants.AddNew })]
        public async Task<AppDomainResult> AddAnotherOrder([FromBody] AnotherOrderRequest model)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (ModelState.IsValid)
            {
                DateTime currentDate = DateTime.Now;
                string userName = LoginContext.Instance.CurrentUser.UserName;
                var users = new Users();

                if (model.UID == 0)
                    users = await userService.GetSingleAsync(x => x.UserName == userName);
                else
                    users = await userService.GetByIdAsync(model.UID);

                int salerId = users.SaleId ?? 0;
                int datHangId = users.DatHangId ?? 0;
                int uID = users.Id;
                decimal feeBuyProUser = users.FeeBuyPro ?? 0;

                var configurations = await configurationsService.GetSingleAsync();
                decimal insurancePercent = Convert.ToDecimal(configurations.InsurancePercent);
                decimal currency = Convert.ToDecimal(configurations.Currency);

                if (users.Currency > 0)
                    currency = users.Currency ?? 0;

                var userLevel = await userLevelService.GetByIdAsync(users.LevelId ?? 0);
                decimal cKFeeBuyPro = userLevel == null ? 0 : userLevel.FeeBuyPro ?? 0;
                decimal lessDeposit = userLevel == null ? 0 : userLevel.LessDeposit ?? 0;

                if (users.Deposit > 0)
                    lessDeposit = users.Deposit ?? 0;

                //Chỗ này khác với OrderShopTemp
                string fullName = users.FullName;
                string phone = users.Phone;
                string address = users.Address;
                string email = users.Email;

                //Chỗ này khác với OrderShopTemp
                decimal priceCNY = model.Products.Sum(x => (x.PriceProduct * x.QuantityProduct)) ?? 0;
                decimal priceVND = priceCNY * currency;

                //Tính phí mua hàng
                decimal servicefee = 0;
                var feeBuyPro = await feeBuyProService.GetSingleAsync(x => !x.Deleted && x.Active
                    && (priceVND >= x.PriceFrom && priceVND < x.PriceTo)
                );
                if (feeBuyPro != null)
                {
                    decimal feePercent = feeBuyPro.FeePercent > 0 ? (feeBuyPro.FeePercent ?? 0) : 0;
                    servicefee = feePercent ;
                }
                decimal feebpnotdc = 0;
                decimal feebuypropt = 0;
                if (users.FeeBuyPro > 0)
                {
                    servicefee = (users.FeeBuyPro ?? 0);
                    feebuypropt = users.FeeBuyPro ?? 0;
                }

                feebpnotdc = priceVND * servicefee / 100;
                decimal subfeebp = cKFeeBuyPro > 0 ? (feebpnotdc * cKFeeBuyPro / 100) : 0;
                decimal feebp = feebpnotdc - subfeebp;
                //Phí mua hàng tối thiểu
                feebp = feebp > (configurations.FeeBuyProMin ?? 0) ? feebp : (configurations.FeeBuyProMin ?? 0);

                //Tính phí kiểm đếm
                decimal? feeCheckProductPrice = 0;
                if (model.IsCheckProduct == true)
                {
                    //Tính số lượng sản phẩm trên 10 tệ, dưới 10 tệ
                    int counprosMore10 = 0;
                    int counprosLes10 = 0;
                    foreach (var item in model.Products)
                    {
                        if (item.PriceProduct >= 10)
                            counprosMore10 += item.QuantityProduct;
                        else
                            counprosLes10 += item.QuantityProduct;
                    }
                    var feeCheckProducts = new List<FeeCheckProduct>();
                    foreach (var item in model.Products)
                    {
                        var feeCheckProduct = await feeCheckProductService.GetFeeCheckByPriceAndAmount((item.PriceProduct ?? 0), item.QuantityProduct);
                        feeCheckProductPrice += feeCheckProduct.Fee * item.QuantityProduct;
                    }
                }
                else
                    feeCheckProductPrice = 0;
                //Tính phí bảo hiểm
                decimal? feeInsurance = 0;
                feeInsurance = (model.IsInsurance == true) ? ((priceVND * configurations.InsurancePercent) / 100) : 0;
                //Tính tổng tiền đơn hàng
                decimal totalPriceVND = (priceCNY * currency) + feebp + (feeCheckProductPrice ?? 0) + (feeInsurance ?? 0);
                decimal amountDeposit = lessDeposit > 0 ? ((priceCNY * currency) * lessDeposit / 100) : (priceCNY * currency);

                //Dành cho phần Image
                List<string> filePaths = new List<string>();
                List<string> folderUploadPaths = new List<string>();
                List<string> fileUrls = new List<string>();

                List<Order> listOrder = new List<Order>();
                string imageOrigin = model.Products[0].ImageProduct;
                foreach (var item in model.Products)
                {
                    decimal productPriceCNY = item.PriceProduct ?? 0; //Cái này để tính nên phải đặt biến cho nó

                    decimal productPrice = 0;
                    decimal productPromotionPrice = 0;
                    decimal priceToPay = 0;

                    if (productPromotionPrice <= productPrice)
                        priceToPay = productPromotionPrice;
                    else
                        priceToPay = productPrice;

                    priceCNY += (priceToPay * item.QuantityProduct);

                    Order order = new Order();
                    order.MainOrderId = 0; //Vào service thêm sau
                    order.LinkOrigin = item.LinkProduct;
                    order.TitleOrigin = order.TitleTranslated = item.NameProduct;
                    order.PriceOrigin = item.PriceProduct;
                    order.PropertyTranslated = order.Property = order.DataValue = item.PropertyProduct;
                    order.Quantity = item.QuantityProduct;
                    order.Note = order.Brand = item.NoteProduct;

                    order.UID = users.Id;

                    order.Stock = 0;
                    order.ShopId = order.ShopName = order.SellerId = order.Wangwang = "";
                    order.LocationSale = order.Site = order.Comment = order.ItemId = "";
                    order.OuterId = order.Error = order.StepPrice = "";

                    order.CategoryName = "";
                    order.CategoryId = 0;

                    order.Tool = "Web";
                    order.Version = "";
                    order.IsTranslate = false;

                    order.IsFastDelivery = model.IsFastDelivery;
                    order.IsFastDeliveryPrice = 0;

                    order.IsCheckProduct = model.IsCheckProduct;
                    order.IsCheckProductPrice = feeCheckProductPrice;

                    order.IsPacked = model.IsPacked;
                    order.IsPackedPrice = 0;

                    order.IsFast = false;
                    order.IsFastPrice = 0;

                    order.PriceCNY = productPriceCNY * item.QuantityProduct;
                    order.PriceVND = order.PriceCNY * currency;

                    order.FullName = fullName;
                    order.Address = address;
                    order.Email = email;
                    order.Phone = phone;

                    order.Status = 0;
                    order.Deposit = 0;

                    order.CurrentCNYVN = currency;
                    order.TotalPriceVND = totalPriceVND;

                    order.Deleted = false;
                    order.Active = true;
                    order.Created = DateTime.Now;
                    order.CreatedBy = users.UserName;
                    order.ImageOrigin = item.ImageProduct;

                    listOrder.Add(order);
                }

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

                        int d = sale.Created.Value.Subtract(sale.Created.Value).Days;
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
                    }
                    staffIncomes.Add(staffIncomeOrder);
                }

                MainOrder mainOrder = new MainOrder
                {
                    EditedFeeBuyProPercent = servicefee,
                    ImageOrigin = imageOrigin,

                    UID = users.Id,
                    ShopId = "",
                    ShopName = "",
                    Site = "",

                    IsFastDelivery = model.IsFastDelivery,
                    IsFastDeliveryPrice = 0,

                    IsCheckProduct = model.IsCheckProduct,
                    IsCheckProductPrice = feeCheckProductPrice,
                    IsCheckProductPriceCNY = (feeCheckProductPrice ?? 0) / currency,

                    IsInsurance = model.IsInsurance,
                    InsuranceMoney = feeInsurance,
                    InsurancePercent = configurations.InsurancePercent,

                    IsPacked = model.IsPacked,
                    IsPackedPrice = 0,
                    IsPackedPriceCNY = 0,

                    PriceVND = priceVND,
                    PriceCNY = priceCNY,

                    FeeShipCN = 0,
                    CKFeeBuyPro = cKFeeBuyPro,
                    FeeBuyPro = feebp,
                    FeeBuyProUser = feeBuyProUser,
                    FeeBuyProPT = feebuypropt,
                    FeeWeight = 0,

                    ReceiverFullName = fullName,
                    DeliveryAddress = address,
                    ReceiverEmail = email,
                    ReceiverPhone = phone,
                    Note = model.UserNote,

                    Status = (int)StatusOrderContants.ChoBaoGia,
                    Deposit = 0,
                    CurrentCNYVN = currency,
                    TotalPriceVND = totalPriceVND,

                    SalerId = salerId,
                    DatHangId = datHangId,
                    AmountDeposit = amountDeposit,
                    OrderType = (int)TypeOrder.KhongXacDinh,
                    LessDeposit = lessDeposit,

                    ReceivePlace = model.WarehouseVN,
                    ShippingType = model.ShippingType,
                    FromPlace = model.WarehouseTQ,

                    Deleted = false,
                    Active = true,
                    Created = currentDate,
                    CreatedBy = users.UserName,

                    Orders = listOrder,
                    StaffIncomes = staffIncomes
                };

                List<MainOrder> listMainOrder = new List<MainOrder>();
                listMainOrder.Add(mainOrder);

                success = await mainOrderService.CreateOrder(listMainOrder, true);

                if (success)
                {
                    appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                }

                appDomainResult.Success = success;
            }
            else
                throw new AppException(ModelState.GetErrorMessage());
            return appDomainResult;
        }

        /// <summary>
        /// Cập nhật ghi chú cho đơn hàng
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

            if (id == 0)
                throw new KeyNotFoundException("id không tồn tại");

            var item = await this.domainService.GetByIdAsync(id);
            if (item != null)
            {
                item.Note = note; //Cập nhật chỗ này thôi nè

                success = await this.domainService.UpdateFieldAsync(item, new System.Linq.Expressions.Expression<Func<MainOrder, object>>[] { e => e.Note });
                if (success)
                    appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                else
                    throw new Exception("Lỗi trong quá trình xử lý");
                appDomainResult.Success = success;
            }
            else
                throw new KeyNotFoundException("Item không tồn tại");

            return appDomainResult;
        }

        /// <summary>
        /// Cập nhật nhân viên đặt hàng, nhân viên kinh doanh của đơn hàng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="staffId"></param>
        /// <param name="type">1: Nhân viên đặt hàng, 2: Nhân viên kinh doanh</param>
        /// <returns></returns>
        [HttpPut("update-staff")]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> UpdateStaff(int id, int staffId, int type)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;

            if (id == 0)
                throw new KeyNotFoundException("id không tồn tại");

            var item = await this.domainService.GetByIdAsync(id);
            if (item != null)
            {
                MainOrderRequest itemModel = new MainOrderRequest();
                itemModel.FeeBuyPro = item.FeeBuyPro;
                var user = await userService.GetByIdAsync(LoginContext.Instance.CurrentUser.UserId);
                var configurations = await configurationsService.GetSingleAsync();
                switch (type)
                {
                    case 1: //Nhân viên đặt hàng
                        itemModel.SalerId = item.SalerId;
                        itemModel.DatHangId = staffId;
                        break;
                    case 2: //Nhân viên kinh doanh
                        itemModel.SalerId = staffId;
                        itemModel.DatHangId = item.DatHangId;
                        break;
                    default:
                        break;
                }
                item.HistoryOrderChanges = new List<HistoryOrderChange>();

                List<StaffIncome> staffIncomes = new List<StaffIncome>();
                var staffIncomeSaler = new StaffIncome();
                var staffIncomeOrderer = new StaffIncome();

                decimal salePercent = Convert.ToDecimal(configurations.SalePercent);
                decimal salePercentAf3M = Convert.ToDecimal(configurations.SalePercentAfter3Month);
                decimal datHangPercent = Convert.ToDecimal(configurations.DatHangPercent);

                //Saler
                if (itemModel.SalerId != 0 && itemModel.SalerId != item.SalerId)
                {
                    var salerOld = await userService.GetByIdAsync(item.SalerId ?? 0);
                    var salerNew = await userService.GetByIdAsync(itemModel.SalerId ?? 0);
                    item.HistoryOrderChanges.Add(new HistoryOrderChange()
                    {
                        MainOrderId = item.Id,
                        UID = user.Id,
                        HistoryContent = String.Format("{0} đã đổi nhân viên saler của đơn hàng ID là: {1}, Từ: {2}, Sang: {3}.", user.UserName, item.Id, salerOld == null ? "Không xác định" : salerOld.UserName, salerNew == null ? "Không xác định" : salerNew.UserName),
                        Type = 12
                    });
                    if (salerOld != null)
                    {
                        //Xóa cái cũ
                        var staffIncomeSalerOld = await staffIncomeService.GetSingleAsync(e => e.MainOrderId == item.Id && e.UID == salerOld.Id);
                        if (staffIncomeSalerOld != null)
                        {
                            staffIncomeSalerOld.Deleted = true;
                            staffIncomes.Add(staffIncomeSalerOld);
                        }
                    }

                    //Thêm cái mới
                    staffIncomeSaler.MainOrderId = item.Id;
                    staffIncomeSaler.UID = salerNew.Id;
                }
                else
                    staffIncomeSaler = await staffIncomeService.GetSingleAsync(e => e.MainOrderId == item.Id && e.UID == itemModel.SalerId);

                if (staffIncomeSaler != null)
                {
                    //Tính
                    var saler = await userService.GetByIdAsync(itemModel.SalerId ?? 0);
                    int d = saler.Created.Value.Subtract(saler.Created.Value).Days;
                    if (d > 90)
                    {
                        staffIncomeSaler.TotalPriceReceive = (item.FeeBuyPro ?? 0) * salePercentAf3M / 100;
                        staffIncomeSaler.PercentReceive = salePercentAf3M;
                    }
                    else
                    {
                        staffIncomeSaler.TotalPriceReceive = (item.FeeBuyPro ?? 0) * salePercent / 100;
                        staffIncomeSaler.PercentReceive = salePercent;
                    }

                    staffIncomeSaler.Status = (int?)StatusStaffIncome.Unpaid;
                    staffIncomes.Add(staffIncomeSaler);
                }

                //Đặt hàng
                if (itemModel.DatHangId != 0 && itemModel.DatHangId != item.DatHangId)
                {
                    var ordererOld = await userService.GetByIdAsync(item.DatHangId ?? 0);
                    var ordererNew = await userService.GetByIdAsync(itemModel.DatHangId ?? 0);

                    item.HistoryOrderChanges.Add(new HistoryOrderChange()
                    {
                        MainOrderId = item.Id,
                        UID = user.Id,
                        HistoryContent = String.Format("{0} đã đổi nhân viên đặt hàng của đơn hàng ID là: {1}, Từ: {2}, Sang: {3}.", user.UserName, item.Id, ordererOld == null ? "Không xác định" : ordererOld.UserName, ordererNew == null ? "Không xác định" : ordererNew.UserName),
                        Type = 12
                    });


                    if (ordererOld != null)
                    {
                        //Xóa cái cũ
                        var staffIncomeOrdererOld = await staffIncomeService.GetSingleAsync(e => e.MainOrderId == item.Id && e.UID == ordererOld.Id);
                        if (staffIncomeOrdererOld != null)
                        {
                            staffIncomeOrdererOld.Deleted = true;
                            staffIncomes.Add(staffIncomeOrdererOld);
                        }
                    }

                    //Thêm cái mới
                    staffIncomeOrderer.MainOrderId = item.Id;
                    staffIncomeOrderer.UID = ordererNew.Id;
                }
                else
                    staffIncomeOrderer = await staffIncomeService.GetSingleAsync(e => e.MainOrderId == item.Id && e.UID == itemModel.DatHangId);

                if (staffIncomeOrderer != null)
                {
                    //Tính hoa hồng
                    staffIncomeOrderer.PercentReceive = datHangPercent;
                    decimal? totalPriceLoi = ((item.PriceVND ?? 0) + (item.FeeShipCN ?? 0)) - (item.TotalPriceReal ?? 0) - (item.FeeShipCNReal ?? 0);

                    staffIncomeOrderer.OrderTotalPrice = item.TotalPriceReal ?? 0;
                    staffIncomeOrderer.Status = (int?)StatusStaffIncome.Unpaid;
                    staffIncomeOrderer.TotalPriceReceive = (totalPriceLoi * (staffIncomeOrderer.PercentReceive ?? 0) / 100);

                    staffIncomes.Add(staffIncomeOrderer);
                }
                item.SalerId = itemModel.SalerId;
                item.DatHangId = itemModel.DatHangId;
                item.StaffIncomes = staffIncomes;
                success = await mainOrderService.UpdateStaff(item);

                if (success)
                    appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                else
                    throw new Exception("Lỗi trong quá trình xử lý");
                appDomainResult.Success = success;
            }
            else
                throw new KeyNotFoundException("Item không tồn tại");

            return appDomainResult;
        }

        /// <summary>
        /// Thanh toán hoặc đặt cọc của Quản lý
        /// </summary>
        /// <param name="id"></param>
        /// <param name="paymentType">
        /// 1: Đặt cọc
        /// 2: Thanh toán
        /// </param>
        /// <param name="paymentMethod">
        /// 1: Trực tiếp
        /// 2: Ví điện tử
        /// </param>
        /// <param name="amount">Số tiền thanh toán</param>
        /// <param name="note">Nội dung thanh toán</param>
        /// <returns></returns>
        [HttpPut("payment")]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> Payment(int id, int paymentType, int paymentMethod, decimal amount, string note)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            success = await mainOrderService.Payment(id, paymentType, paymentMethod, amount, note);
            if (success)
            {
                if (paymentType == 2)
                {
                    //Cập nhật trạng thái đã thanh toán các mã vận đơn của đơn hàng
                    List<SmallPackage> smallPackageUpdates = await smallPackageService.GetInVietNamByMainOrderId(id);
                    success = false;

                    if (smallPackageUpdates.Count > 0)
                    {
                        foreach (var item in smallPackageUpdates)
                        {
                            item.IsPayment = true;
                            success = await smallPackageService.UpdateFieldAsync(item, new Expression<Func<SmallPackage, object>>[]
                            {
                                s => s.IsPayment
                            });
                        }
                    }
                }
                appDomainResult.ResultCode = (int)HttpStatusCode.OK;
            }
            else
                throw new Exception("Lỗi trong quá trình xử lý");
            appDomainResult.Success = success;
            return appDomainResult;
        }

        /// <summary>
        /// Cập nhật thông tin item
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public override async Task<AppDomainResult> UpdateItem([FromBody] MainOrderRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (ModelState.IsValid)
            {
                var user = await userService.GetByIdAsync(LoginContext.Instance.CurrentUser.UserId);
                var item = await this.domainService.GetByIdAsync(itemModel.Id);
                bool isChangeTQVNWeight = itemModel.IsChangeTQVNWeight != null ? itemModel.IsChangeTQVNWeight.Value : false;
                bool isChangeFeeWeight = itemModel.IsChangeFeeWeight != null ? itemModel.IsChangeFeeWeight.Value : false;
                var configurations = await configurationsService.GetSingleAsync();
                if (item != null)
                {
                    item.HistoryOrderChanges = new List<HistoryOrderChange>();
                    string updateSql = "";
                    foreach (var smallPackage in itemModel.SmallPackages)
                    {
                        if (smallPackage.MainOrderCodeId == null || smallPackage.MainOrderCodeId <= 0)
                            throw new AppException("Mã đơn hàng bị trống");
                    }
                    var customer = await userService.GetByIdAsync(item.UID ?? 0);
                    string statusNameOrderNew = "";
                    switch (itemModel.Status)
                    {
                        case (int)StatusOrderContants.DonHuy:
                            statusNameOrderNew = "Hủy";
                            break;
                        case (int)StatusOrderContants.ChoBaoGia:
                            statusNameOrderNew = "Chờ báo giá";
                            break;
                        case (int)StatusOrderContants.DonMoi:
                            statusNameOrderNew = "Đơn mới";
                            break;
                        case (int)StatusOrderContants.DaCoc:
                            statusNameOrderNew = "Đã đặt cọc";
                            break;
                        case (int)StatusOrderContants.DaMuaHang:
                            statusNameOrderNew = "Đã mua hàng";
                            break;
                        case (int)StatusOrderContants.ShopPhatHang:
                            statusNameOrderNew = "Shop phát hàng";
                            break;
                        case (int)StatusOrderContants.VeTQ:
                            statusNameOrderNew = "Đã về kho TQ";
                            break;
                        case (int)StatusOrderContants.DangVeVN:
                            statusNameOrderNew = "Đang về kho VN";
                            break;
                        case (int)StatusOrderContants.VeVN:
                            statusNameOrderNew = "Đã về kho VN";
                            break;
                        case (int)StatusOrderContants.DaThanhToan:
                            statusNameOrderNew = "Đã thanh toán";
                            break;
                        case (int)StatusOrderContants.HoanThanh:
                            statusNameOrderNew = "Đã hoàn thành";
                            break;
                        case (int)StatusOrderContants.KhieuNai:
                            statusNameOrderNew = "Đã khiếu nại";
                            break;
                        default:
                            break;
                    }
                    string statusNameOrderOld = "";
                    switch (item.Status)
                    {
                        case (int)StatusOrderContants.DonHuy:
                            statusNameOrderOld = "Hủy";
                            break;
                        case (int)StatusOrderContants.ChoBaoGia:
                            statusNameOrderOld = "Chờ báo giá";
                            break;
                        case (int)StatusOrderContants.DonMoi:
                            statusNameOrderOld = "Đơn mới";
                            break;
                        case (int)StatusOrderContants.DaCoc:
                            statusNameOrderOld = "Đã đặt cọc";
                            break;
                        case (int)StatusOrderContants.DaMuaHang:
                            statusNameOrderOld = "Đã mua hàng";
                            break;
                        case (int)StatusOrderContants.ShopPhatHang:
                            statusNameOrderOld = "Shop phát hàng";
                            break;
                        case (int)StatusOrderContants.VeTQ:
                            statusNameOrderOld = "Đã về kho TQ";
                            break;
                        case (int)StatusOrderContants.DangVeVN:
                            statusNameOrderOld = "Đang về kho VN";
                            break;
                        case (int)StatusOrderContants.VeVN:
                            statusNameOrderOld = "Đã về kho VN";
                            break;
                        case (int)StatusOrderContants.DaThanhToan:
                            statusNameOrderOld = "Đã thanh toán";
                            break;
                        case (int)StatusOrderContants.HoanThanh:
                            statusNameOrderOld = "Đã hoàn thành";
                            break;
                        case (int)StatusOrderContants.KhieuNai:
                            statusNameOrderOld = "Đã khiếu nại";
                            break;
                        default:
                            break;
                    }

                    //Lịch sử phần trăm phí mua hàng
                    if ((item.EditedFeeBuyProPercent ?? 0) != (itemModel.EditedFeeBuyProPercent ?? 0))
                    {
                        itemModel.IsEditFeeBuyProPercent = true;
                        string historyContent = $"{user.UserName} đã đổi phần trăm phí mua hàng từ {item.EditedFeeBuyProPercent ?? 0} sang {itemModel.EditedFeeBuyProPercent ?? 0}";
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContent}',{0},'{DateTime.Now}','{user.UserName}',0,1)";
                    }

                    //Lịch sử phí mua hàng
                    if ((item.FeeBuyPro ?? 0) != (itemModel.FeeBuyPro ?? 0))
                    {
                        string historyContent = $"{user.UserName} đã đổi phí mua hàng từ {item.FeeBuyPro ?? 0} sang {itemModel.FeeBuyPro ?? 0}";
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContent}',{0},'{DateTime.Now}','{user.UserName}',0,1)";
                    }
                    
                    //Trạng thái đơn hàng
                    if (item.Status != itemModel.Status)
                    {
                        string historyContent = String.Format("{0} đã đổi trạng thái của đơn hàng: {1} từ \"{2}\" sang \"{3}\".", user.UserName, item.Id, statusNameOrderOld, statusNameOrderNew);
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContent}',{0},'{DateTime.Now}','{user.UserName}',0,1)";
                    }
                    //Mã vận đơn
                    foreach (var smallPackage in itemModel.SmallPackages)
                    {
                        var mainOrderCode = await mainOrderCodeService.GetByIdAsync(smallPackage.MainOrderCodeId ?? 0);
                        string statusNameNew = "";
                        switch (smallPackage.Status)
                        {
                            case (int)StatusSmallPackage.DaHuy:
                                statusNameNew = "Đã hủy";
                                break;
                            case (int)StatusSmallPackage.MoiTao:
                                statusNameNew = "Mới tạo";
                                break;
                            case (int)StatusSmallPackage.VeKhoTQ:
                                statusNameNew = "Đã về kho TQ";
                                break;
                            case (int)StatusSmallPackage.XuatKhoTQ:
                                statusNameNew = "Xuất kho TQ";
                                break;
                            case (int)StatusSmallPackage.VeKhoVN:
                                statusNameNew = "Đã về kho VN";
                                break;
                            case (int)StatusSmallPackage.DaGiao:
                                statusNameNew = "Đã giao khách";
                                break;
                            default:
                                statusNameNew = "Không xác định";
                                break;
                        }

                        if (smallPackage.Id == 0)
                        {
                            smallPackage.UserNote = item.Note;
                            smallPackage.OrderTransactionCode = smallPackage.OrderTransactionCode.Replace(" ", "");
                            string historyContent = String.Format("{0} đã thêm kiện hàng của đơn hàng ID là: {1}, Mã vận đơn: {2}, Mã đơn hàng: {3}, Cân nặng {4} kg, Trạng thái kiện: \"{5}\".",
                                user.UserName, item.Id, smallPackage.OrderTransactionCode, mainOrderCode == null ? 0 : mainOrderCode.Code, smallPackage.Weight, statusNameNew);
                            updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                                $"VALUES({item.Id},{user.Id},N'{historyContent}',{0},'{DateTime.Now}','{user.UserName}',0,1)";
                            continue;
                        }
                        var existsSmallPackage = await smallPackageService.GetByIdAsync(smallPackage.Id);
                        if (existsSmallPackage == null) continue;
                        //Đơn cập nhật thì giữ lại UID, CreatedDate, CreatedBy
                        smallPackage.UID = existsSmallPackage.UID ?? 0;
                        smallPackage.Created = existsSmallPackage.Created;
                        smallPackage.CreatedBy = existsSmallPackage.CreatedBy;
                        smallPackage.UserNote = item.Note;
                        if (smallPackage.OrderTransactionCode != existsSmallPackage.OrderTransactionCode)
                        {
                            string historyContent = String.Format("{0} đã cập nhật mã vận đơn kiện hàng của đơn hàng ID là: {1}, Mã vận đơn từ: {2}, sang {3}.",
                                user.UserName, item.Id, existsSmallPackage.OrderTransactionCode, smallPackage.OrderTransactionCode);
                            updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                                $"VALUES({item.Id},{user.Id},N'{historyContent}',{0},'{DateTime.Now}','{user.UserName}',0,1)";
                        }
                        if (smallPackage.Weight != existsSmallPackage.Weight)
                        {
                            string historyContent = String.Format("{0} đã cập nhật cân nặng kiện hàng của đơn hàng ID là: {1}, Cân nặng từ: {2} kg, sang {3} kg.",
                                user.UserName, item.Id, existsSmallPackage.Weight, smallPackage.Weight);
                            updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                                $"VALUES({item.Id},{user.Id},N'{historyContent}',{0},'{DateTime.Now}','{user.UserName}',0,1)";
                        }

                        if (smallPackage.MainOrderCodeId != existsSmallPackage.MainOrderCodeId)
                        {
                            var mainOrderCodeOld = await mainOrderCodeService.GetByIdAsync(existsSmallPackage.MainOrderCodeId ?? 0);
                            string historyContent = String.Format("{0} đã cập nhật mã đơn hàng kiện hàng của đơn hàng ID là: {1}, Mã vận đơn: {2}, Mã đơn hàng từ: {3}, sang: {4}.",
                                user.UserName, item.Id, smallPackage.OrderTransactionCode, mainOrderCodeOld == null ? 0 : mainOrderCodeOld.Code, mainOrderCode == null ? 0 : mainOrderCode.Code);
                            updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                                $"VALUES({item.Id},{user.Id},N'{historyContent}',{0},'{DateTime.Now}','{user.UserName}',0,1)";
                        }

                        if (smallPackage.Status != existsSmallPackage.Status)
                        {
                            string statusNameOld = "";
                            switch (smallPackage.Status)
                            {
                                case (int)StatusSmallPackage.DaHuy:
                                    statusNameOld = "Đã hủy";
                                    break;
                                case (int)StatusSmallPackage.MoiTao:
                                    statusNameOld = "Mới tạo";
                                    break;
                                case (int)StatusSmallPackage.VeKhoTQ:
                                    statusNameOld = "Đã về kho TQ";
                                    break;
                                case (int)StatusSmallPackage.XuatKhoTQ:
                                    statusNameOld = "Xuất kho TQ";
                                    break;
                                case (int)StatusSmallPackage.VeKhoVN:
                                    statusNameOld = "Đã về kho VN";
                                    break;
                                case (int)StatusSmallPackage.DaGiao:
                                    statusNameOld = "Đã giao khách";
                                    break;
                                default:
                                    statusNameOld = "Không xác định";
                                    break;
                            }
                            string historyContent = String.Format("{0} đã cập nhật trạng thái kiện hàng của đơn hàng ID là: {1}, Mã vận đơn: {2}, Trạng thái kiện từ: \"{3}\", sang: \"{4}\".",
                                user.UserName, item.Id, smallPackage.OrderTransactionCode, statusNameOld, statusNameNew);
                            updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                                $"VALUES({item.Id},{user.Id},N'{historyContent}',{0},'{DateTime.Now}','{user.UserName}',0,1)";

                        }
                    }
                    //Danh sách phụ phí
                    foreach (var feeSupport in itemModel.FeeSupports)
                    {
                        if (feeSupport.Id == 0) //Thêm mới
                        {
                            string historyContent = String.Format("{0} đã thêm phụ phí của đơn hàng ID là: {1}, Tên phụ phí {2}, Số tiền: {3}.",
                                user.UserName, item.Id, feeSupport.SupportName, feeSupport.SupportInfoVND);
                            updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                                $"VALUES({item.Id},{user.Id},N'{historyContent}',{0},'{DateTime.Now}','{user.UserName}',0,1)";
                            continue;
                        }

                        var existsFeeSupport = await feeSupportService.GetByIdAsync(feeSupport.Id);
                        if (existsFeeSupport == null) continue;

                        if (feeSupport.SupportName != existsFeeSupport.SupportName)
                        {
                            string historyContent = String.Format("{0} đã thay đổi tên phụ phí của đơn hàng ID là: {1}, Từ: {2}, Sang {3}.",
                                user.UserName, item.Id, existsFeeSupport.SupportName, feeSupport.SupportName);
                            updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                                $"VALUES({item.Id},{user.Id},N'{historyContent}',{0},'{DateTime.Now}','{user.UserName}',0,1)";

                        }

                        if (feeSupport.SupportInfoVND != existsFeeSupport.SupportInfoVND)
                        {
                            string historyContent = String.Format("{0} đã thay đổi tên phụ phí của đơn hàng ID là: {1}, Tên phụ phí: {2}, Số tiền từ: {3}, Sang: {4}.",
                                user.UserName, item.Id, existsFeeSupport.SupportName, existsFeeSupport.SupportInfoVND, feeSupport.SupportInfoVND);
                            updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                                $"VALUES({item.Id},{user.Id},N'{historyContent}',{0},'{DateTime.Now}','{user.UserName}',0,1)";

                        }
                    }
                    //Đặt cọc
                    if (itemModel.AmountDeposit != item.AmountDeposit)
                    {
                        string historyContent = String.Format("{0} đã đổi số tiền đặt cọc của đơn hàng ID là: {1}, Từ: {2} VND, Sang: {3} VND.",
                            user.UserName, item.Id, item.AmountDeposit, itemModel.AmountDeposit);
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContent}',{0},'{DateTime.Now}','{user.UserName}',0,1)";
                    }

                    //Tổng số tiền mua thật
                    if (itemModel.TotalPriceRealCNY != item.TotalPriceRealCNY)
                    {
                        itemModel.TotalPriceReal = itemModel.TotalPriceRealCNY * itemModel.CurrentCNYVN;
                        if (itemModel.TotalPriceReal != item.TotalPriceReal)
                        {
                            string historyContentlPriceReal = String.Format("{0} đã đổi phí mua thật (VNĐ) của đơn hàng ID là: {1}, Từ: {2}, Sang: {3}.",
                                user.UserName, item.Id, item.TotalPriceRealCNY, itemModel.TotalPriceRealCNY);
                            updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                                $"VALUES({item.Id},{user.Id},N'{historyContentlPriceReal}',{0},'{DateTime.Now}','{user.UserName}',0,1)";

                        }
                    }
                    //Phí ship TQ
                    itemModel.FeeShipCN = itemModel.FeeShipCNCNY * itemModel.CurrentCNYVN;
                    if (itemModel.FeeShipCN != item.FeeShipCN)
                    {
                        string historyContentFeeShipCN = String.Format("{0} đã đổi phí ship Trung Quốc (VNĐ) của đơn hàng ID là: {1}, Từ: {2}, Sang: {3}.",
                            user.UserName, item.Id, item.FeeShipCNCNY, itemModel.FeeShipCNCNY);
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContentFeeShipCN}',{0},'{DateTime.Now}','{user.UserName}',0,1)";

                    }

                    //Phí ship TQ thật
                    itemModel.FeeShipCNReal = itemModel.FeeShipCNRealCNY * itemModel.CurrentCNYVN;
                    if (itemModel.FeeShipCNReal != item.FeeShipCNReal)
                    {
                        string historyContentFeeShipCNReal = String.Format("{0} đã đổi phí ship Trung Quốc thật (VNĐ) của đơn hàng ID là: {1}, Từ: {2}, Sang: {3}.",
                            user.UserName, item.Id, item.FeeShipCNRealCNY, itemModel.FeeShipCNRealCNY);
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContentFeeShipCNReal}',{0},'{DateTime.Now}','{user.UserName}',0,1)";

                    }

                    //Phí kiểm đếm, đóng gỗ, bảo hiểm, giao tận nhà
                    var totalPriceVNDOld = itemModel.TotalPriceVND.Value;
                    #region Phí kiểm đếm, phí bảo hiểm, phí đóng gỗ, phí giao hàng nhanh
                    //Phí kiểm đếm
                    if (item.IsCheckProduct.Value == true && (itemModel.IsCheckProduct.Value == false))
                    {
                        //Giảm tổng tiền xuống
                        itemModel.TotalPriceVND -= itemModel.IsCheckProductPrice ?? 0;
                        string historyContentIsCheckProduct = String.Format("{0} đã bỏ phí kiểm đếm của đơn hàng ID là: {1}, Tổng tiền đơn hàng từ {2} sang {3}",
                        user.UserName, item.Id, totalPriceVNDOld, totalPriceVNDOld - itemModel.IsCheckProductPrice.Value);
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContentIsCheckProduct}',{0},'{DateTime.Now}','{user.UserName}',0,1)";
                        totalPriceVNDOld += itemModel.IsCheckProductPrice ?? 0;
                        itemModel.IsCheckProductPriceCNY = itemModel.IsCheckProductPrice = 0;
                    }
                    else if (itemModel.IsCheckProduct.Value == true &&
                        (item.IsCheckProduct.Value == false || item.IsCheckProduct == null))
                    {
                        //Thêm phí kiểm đếm
                        decimal? feeCheckNewCount = 0;
                        foreach (var product in itemModel.Orders)
                        {
                            var feeCheckNew = await feeCheckProductService.GetFeeCheckByPriceAndAmount(product.PriceOrigin.Value, product.Quantity.Value);
                            feeCheckNewCount += feeCheckNew.Fee.Value * product.Quantity;
                        }
                        itemModel.IsCheckProductPrice = feeCheckNewCount ?? 0;
                        itemModel.IsCheckProductPriceCNY = Math.Round((feeCheckNewCount / itemModel.CurrentCNYVN) ?? 0, 1);
                        //Tính lại tổng tiền
                        itemModel.TotalPriceVND = itemModel.TotalOrderAmount += feeCheckNewCount ?? 0;

                        string historyContentIsCheckProduct = String.Format("{0} đã thêm phí kiểm đếm của đơn hàng ID là: {1}, Tổng tiền đơn hàng từ {2} sang {3}",
                            user.UserName, item.Id, totalPriceVNDOld, totalPriceVNDOld + feeCheckNewCount);
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContentIsCheckProduct}',{0},'{DateTime.Now}','{user.UserName}',0,1)";

                    }

                    //Phí đóng gỗ
                    if (itemModel.IsPacked.Value == false && item.IsPacked.Value == true) // đã có kiểm đếm mà bỏ ra
                    {
                        //giảm tổng tiền
                        itemModel.TotalPriceVND -= itemModel.IsPackedPrice ?? 0;

                        string historyContentIsPacked = String.Format("{0} đã bỏ tiền phí đóng gỗ (VND) của đơn hàng ID là: {1}, Tổng tiền đơn hàng từ {2} sang {3}",
                        user.UserName, item.Id, totalPriceVNDOld, totalPriceVNDOld - itemModel.IsPackedPrice.Value);
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContentIsPacked}',{(int?)TypeHistoryOrderChange.PhiDongGoi},'{DateTime.Now}','{user.UserName}',0,1)";


                        totalPriceVNDOld += itemModel.IsPackedPrice ?? 0;
                        itemModel.IsPackedPrice = itemModel.IsPackedPriceCNY = 0;
                    }
                    else if (itemModel.IsPacked.Value == true &&
                        (item.IsPacked.Value == false || item.IsPacked == null)) //chưa có kiểm đếm mà chọn vào
                    {
                        //Tính lại tổng tiền
                        itemModel.TotalPriceVND = itemModel.TotalOrderAmount += itemModel.IsPackedPrice ?? 0;
                        string historyContentIsPacked = String.Format("{0} đã thêm tiền phí đóng gỗ (VND) của đơn hàng ID là: {1}, Tổng tiền đơn hàng từ {2} sang {3}",
                            user.UserName, item.Id, totalPriceVNDOld, totalPriceVNDOld + itemModel.IsPackedPrice.Value);
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContentIsPacked}',{(int?)TypeHistoryOrderChange.PhiDongGoi},'{DateTime.Now}','{user.UserName}',0,1)";

                    }
                    else //có rồi mà đổi tiền
                    {
                        if (itemModel.IsPackedPrice != item.IsPackedPrice)
                        {
                            string historyContentIsPacked = String.Format("{0} đã đổi tiền phí đóng gỗ (VNĐ) của đơn hàng ID là: {1}, Từ: {2}, Sang: {3}.",
                                user.UserName, item.Id, item.IsPackedPrice, itemModel.IsPackedPrice);
                            updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                                $"VALUES({item.Id},{user.Id},N'{historyContentIsPacked}',{(int?)TypeHistoryOrderChange.PhiDongGoi},'{DateTime.Now}','{user.UserName}',0,1)";

                        }
                    }

                    //Phí bảo hiểm
                    if (itemModel.IsInsurance.Value == false && item.IsInsurance.Value == true) // đã có kiểm đếm mà bỏ ra
                    {
                        //giảm tổng tiền
                        itemModel.TotalPriceVND -= itemModel.InsuranceMoney ?? 0;

                        string historyContentIsInsurance = String.Format("{0} đã bỏ tiền bảo hiểm đơn hàng ID là: {1}, Tổng tiền đơn hàng từ {2} sang {3}",
                            user.UserName, item.Id, totalPriceVNDOld, totalPriceVNDOld - itemModel.InsuranceMoney.Value);
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContentIsInsurance}',{(int?)TypeHistoryOrderChange.TienDatCoc},'{DateTime.Now}','{user.UserName}',0,1)";

                        totalPriceVNDOld += itemModel.InsuranceMoney ?? 0;
                        itemModel.InsuranceMoney = 0;
                    }
                    else if (itemModel.IsInsurance.Value == true &&
                        (item.IsInsurance.Value == false || item.IsInsurance == null)) //chưa có kiểm đếm mà chọn vào
                    {
                        //Tính lại tổng tiền và tiền bảo hiểm
                        decimal? insuranceFee = Math.Round((itemModel.PriceVND * configurations.InsurancePercent / 100) ?? 0, 1); //Bảo hiểm = Tiền mua hàng * %
                        itemModel.InsuranceMoney = insuranceFee;
                        itemModel.TotalPriceVND = itemModel.TotalOrderAmount += itemModel.InsuranceMoney ?? 0;

                        string historyContentIsInsurance = String.Format("{0} đã thêm tiền bảo hiểm đơn hàng ID là: {1},  Tổng tiền đơn hàng từ {2} sang {3}",
                            user.UserName, item.Id, totalPriceVNDOld, totalPriceVNDOld + insuranceFee);
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContentIsInsurance}',{(int?)TypeHistoryOrderChange.TienDatCoc},'{DateTime.Now}','{user.UserName}',0,1)";
                    }

                    //Phí giao hàng tận nhà
                    if (itemModel.IsFastDelivery.Value == false && item.IsFastDelivery.Value == true) // đã có kiểm đếm mà bỏ ra
                    {
                        //giảm tổng tiền
                        itemModel.TotalPriceVND -= itemModel.IsFastDeliveryPrice ?? 0;

                        string historyContentIsInsurance = String.Format("{0} đã bỏ tiền phí ship giao hàng tận nhà của đơn hàng ID là: {1},  Tổng tiền đơn hàng từ {2} sang {3}",
                            user.UserName, item.Id, totalPriceVNDOld, totalPriceVNDOld - itemModel.IsFastDeliveryPrice.Value);
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContentIsInsurance}',{(int?)TypeHistoryOrderChange.TienDatCoc},'{DateTime.Now}','{user.UserName}',0,1)";

                        totalPriceVNDOld += itemModel.IsFastDeliveryPrice ?? 0;
                        itemModel.IsFastDeliveryPrice = 0;
                    }
                    else if (itemModel.IsFastDelivery.Value == true && item.IsFastDelivery == false) //chưa có kiểm đếm mà chọn vào
                    {
                        //Tính lại tổng tiền
                        itemModel.TotalPriceVND = itemModel.TotalOrderAmount += itemModel.IsFastDeliveryPrice ?? 0;
                        string historyContentIsInsurance = String.Format("{0} đã bỏ tiền phí ship giao hàng tận nhà của đơn hàng ID là: {1},  Tổng tiền đơn hàng từ {2} sang {3}",
                            user.UserName, item.Id, totalPriceVNDOld, totalPriceVNDOld - itemModel.IsFastDeliveryPrice.Value);
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContentIsInsurance}',{(int?)TypeHistoryOrderChange.TienDatCoc},'{DateTime.Now}','{user.UserName}',0,1)";
                    }
                    else
                    {
                        if (itemModel.IsFastDeliveryPrice != item.IsFastDeliveryPrice)
                        {

                            string historyContentIsInsurance = String.Format("{0} đã đổi tiền phí ship giao hàng tận nhà của đơn hàng ID là: {1}, Từ: {2}, Sang: {3}.",
                                user.UserName, item.Id, item.IsFastDeliveryPrice, itemModel.IsFastDeliveryPrice);
                            updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                                $"VALUES({item.Id},{user.Id},N'{historyContentIsInsurance}',{(int?)TypeHistoryOrderChange.TienDatCoc},'{DateTime.Now}','{user.UserName}',0,1)";

                        }
                    }
                    #endregion

                    //Kho và phương thức vận chuyển
                    //Kho TQ
                    if (itemModel.FromPlace != item.FromPlace)
                    {
                        var warehouseFromOld = await warehouseFromService.GetByIdAsync(item.FromPlace ?? 0);
                        var warehouseFromNew = await warehouseFromService.GetByIdAsync(itemModel.FromPlace ?? 0);
                        string historyContentFromPlace = String.Format("{0} đã đổi kho TQ của đơn hàng ID là: {1}, Từ: {2}, Sang: {3}.", user.UserName, item.Id, warehouseFromOld == null ? "" : warehouseFromOld.Name, warehouseFromNew == null ? "" : warehouseFromNew.Name);
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContentFromPlace}',{(int?)TypeHistoryOrderChange.TienDatCoc},'{DateTime.Now}','{user.UserName}',0,1)";

                    }

                    //Nhận hàng tại
                    if (itemModel.ReceivePlace != item.ReceivePlace)
                    {
                        var warehouseOld = await warehouseService.GetByIdAsync(item.ReceivePlace ?? 0);
                        var warehouseNew = await warehouseService.GetByIdAsync(itemModel.ReceivePlace ?? 0);
                        string historyContentReceivePlace = String.Format("{0} đã đổi nơi nhận hàng của đơn hàng ID là: {1}, Từ: {2}, Sang: {3}.", user.UserName, item.Id, warehouseOld == null ? "" : warehouseOld.Name, warehouseNew == null ? "" : warehouseNew.Name);
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContentReceivePlace}',{(int?)TypeHistoryOrderChange.TienDatCoc},'{DateTime.Now}','{user.UserName}',0,1)";

                    }

                    //Phương thức vận chuyển
                    if (itemModel.ShippingType != item.ShippingType)
                    {
                        var shippingTypeToWareHouseOld = await shippingTypeToWareHouseService.GetByIdAsync(item.ShippingType ?? 0);
                        var shippingTypeToWareHouseNew = await shippingTypeToWareHouseService.GetByIdAsync(itemModel.ShippingType ?? 0);
                        string historyContentReceivePlace = String.Format("{0} đã đổi phương thức vận chuyển của đơn hàng ID là: {1}, Từ: {2}, Sang: {3}.", user.UserName, item.Id, shippingTypeToWareHouseOld == null ? "" : shippingTypeToWareHouseOld.Name, shippingTypeToWareHouseNew == null ? "" : shippingTypeToWareHouseNew.Name);
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContentReceivePlace}',{(int?)TypeHistoryOrderChange.TienDatCoc},'{DateTime.Now}','{user.UserName}',0,1)";

                    }
                    //Trả lại tiền vào ví người dùng nếu tổng tiền của đơn hàng < tổng tiền đã trả
                    if (itemModel.Deposit > itemModel.TotalPriceVND)
                    {
                        decimal? rollBackAmount = itemModel.Deposit - itemModel.TotalPriceVND;
                        decimal oldWallet = customer.Wallet ?? 0;
                        customer.Wallet += rollBackAmount;

                        string historyContentDeposit = String.Format("{0} đã đổi tiền đã trả của đơn hàng ID là: {1}, Từ: {2}, Sang: {3}. Số tiền trong ví bạn được đổi từ {4} sang {5} ",
                            user.UserName, item.Id, item.Deposit, itemModel.TotalPriceVND, oldWallet, customer.Wallet);
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContentDeposit}',{(int?)TypeHistoryOrderChange.TienDatCoc},'{DateTime.Now}','{user.UserName}',0,1)";

                        updateSql += $" INSERT INTO [dbo].[HistoryPayWallet] ([UID] ,[MainOrderId] ,[Amount] ,[Content] ,[MoneyLeft] ,[Type] ,[TradeType] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $" VALUES ({customer.Id}, {item.Id}, {rollBackAmount}, N'Hoàn tiền đơn hàng vì số tiền đã trả lớn hơn tổng tiền đơn hàng', {customer.Wallet}, {(int)HistoryPayWalletContents.HoanTienDaTraMuaHo},2,'{DateTime.Now}','{user.UserName}',0,1) ";

                        updateSql += $"UPDATE Users SET Wallet = {customer.Wallet} WHERE Id = {customer.Id}";

                        itemModel.Deposit = itemModel.TotalPriceVND;
                    }

                    //Tính phí hoa hồng
                    //List<StaffIncome> staffIncomes = await Commission(itemModel, item, user, configurations, updateSql);

                    List<StaffIncome> staffIncomes = new List<StaffIncome>();
                    var staffIncomeSaler = new StaffIncome();
                    var staffIncomeOrderer = new StaffIncome();

                    decimal salePercent = Convert.ToDecimal(configurations.SalePercent);
                    decimal salePercentAf3M = Convert.ToDecimal(configurations.SalePercentAfter3Month);
                    decimal datHangPercent = Convert.ToDecimal(configurations.DatHangPercent);

                    //Saler
                    if (itemModel.SalerId > 0 && itemModel.SalerId != item.SalerId)
                    {
                        var salerOld = await userService.GetByIdAsync(item.SalerId ?? 0);
                        var salerNew = await userService.GetByIdAsync(itemModel.SalerId ?? 0);

                        string historyContentSalerId = String.Format("{0} đã đổi nhân viên saler của đơn hàng ID là: {1}, Từ: {2}, Sang: {3}.", user.UserName, item.Id, salerOld == null ? "Không xác định" : salerOld.UserName, salerNew == null ? "Không xác định" : salerNew.UserName);
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContentSalerId}',11,'{DateTime.Now}','{user.UserName}',0,1)";

                        if (salerOld != null)
                        {
                            //Xóa cái cũ
                            var staffIncomeSalerOld = await staffIncomeService.GetSingleAsync(e => e.MainOrderId == item.Id && e.UID == salerOld.Id);
                            if (staffIncomeSalerOld != null)
                            {
                                staffIncomeSalerOld.Deleted = true;
                                staffIncomes.Add(staffIncomeSalerOld);
                            }
                        }

                        //Thêm cái mới
                        staffIncomeSaler.MainOrderId = item.Id;
                        staffIncomeSaler.UID = salerNew.Id;
                    }
                    else
                        staffIncomeSaler = await staffIncomeService.GetSingleAsync(e => e.MainOrderId == item.Id && e.UID == itemModel.SalerId);

                    if (staffIncomeSaler != null)
                    {
                        //Tính
                        var saler = await userService.GetByIdAsync(itemModel.SalerId ?? 0);
                        int d = saler.Created.Value.Subtract(saler.Created.Value).Days;
                        if (d > 90)
                        {
                            staffIncomeSaler.TotalPriceReceive = (itemModel.FeeBuyPro ?? 0) * salePercentAf3M / 100;
                            staffIncomeSaler.PercentReceive = salePercentAf3M;
                        }
                        else
                        {
                            staffIncomeSaler.TotalPriceReceive = (itemModel.FeeBuyPro ?? 0) * salePercent / 100;
                            staffIncomeSaler.PercentReceive = salePercent;
                        }

                        staffIncomeSaler.Status = (int?)StatusStaffIncome.Unpaid;
                        staffIncomes.Add(staffIncomeSaler);
                    }

                    //Đặt hàng
                    if (itemModel.DatHangId > 0 && itemModel.DatHangId != item.DatHangId)
                    {
                        var ordererOld = await userService.GetByIdAsync(item.DatHangId ?? 0);
                        var ordererNew = await userService.GetByIdAsync(itemModel.DatHangId ?? 0);

                        string historyContentSalerId = String.Format("{0} đã đổi nhân viên đặt hàng của đơn hàng ID là: {1}, Từ: {2}, Sang: {3}.", user.UserName, item.Id, ordererOld == null ? "Không xác định" : ordererOld.UserName, ordererNew == null ? "Không xác định" : ordererNew.UserName);
                        updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                            $"VALUES({item.Id},{user.Id},N'{historyContentSalerId}',12,'{DateTime.Now}','{user.UserName}',0,1)";

                        if (ordererOld != null)
                        {
                            //Xóa cái cũ
                            var staffIncomeOrdererOld = await staffIncomeService.GetSingleAsync(e => e.MainOrderId == item.Id && e.UID == ordererOld.Id);
                            if (staffIncomeOrdererOld != null)
                            {
                                staffIncomeOrdererOld.Deleted = true;
                                staffIncomes.Add(staffIncomeOrdererOld);
                            }
                        }

                        //Thêm cái mới
                        staffIncomeOrderer.MainOrderId = item.Id;
                        staffIncomeOrderer.UID = ordererNew.Id;
                    }
                    else
                        staffIncomeOrderer = await staffIncomeService.GetSingleAsync(e => e.MainOrderId == item.Id && e.UID == itemModel.DatHangId);

                    if (staffIncomeOrderer != null)
                    {
                        //Tính hoa hồng
                        staffIncomeOrderer.PercentReceive = datHangPercent;
                        decimal? totalPriceLoi = ((itemModel.PriceVND ?? 0) + (itemModel.FeeShipCN ?? 0)) - (itemModel.TotalPriceReal ?? 0) - (itemModel.FeeShipCNReal ?? 0);

                        staffIncomeOrderer.OrderTotalPrice = item.TotalPriceReal ?? 0;
                        staffIncomeOrderer.Status = (int?)StatusStaffIncome.Unpaid;
                        staffIncomeOrderer.TotalPriceReceive = (totalPriceLoi * (staffIncomeOrderer.PercentReceive ?? 0) / 100);

                        staffIncomes.Add(staffIncomeOrderer);
                    }
                    mapper.Map(itemModel, item);

                    item.StaffIncomes = staffIncomes;
                    success = await this.domainService.UpdateAsync(item);

                    if (success)
                    {
                        mainOrderService.UpdateMainOrderFromSql(updateSql);
                        //Cập nhật lại cân nặng trên View
                        if (isChangeTQVNWeight == true)
                            await mainOrderService.UpdateMainOrderWeight(itemModel.Id, itemModel.TQVNWeight ?? 0);
                        //Cập nhật phí vận chuyển trên View
                        if (isChangeFeeWeight == true)
                            await mainOrderService.UpdateMainOrderDelivery(itemModel.Id, itemModel.FeeWeight ?? 0);
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
                throw new AppException(ModelState.GetErrorMessage());

            return appDomainResult;
        }

        /// <summary>
        /// Cập nhật cân nặng trong chi tiết đơn
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut("update-weight")]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> UpdateMainOrderWeight([FromBody] MainOrderRequest itemModel)
        {
            bool success = false;
            if (!ModelState.IsValid)
                throw new AppException(ModelState.GetErrorMessage());
            success = await mainOrderService.UpdateMainOrderWeight(itemModel.Id, itemModel.TQVNWeight ?? 0);

            return new AppDomainResult()
            {
                ResultCode = (int)HttpStatusCode.OK,
                Success = success,
                ResultMessage = "Cập nhật cân nặng thành công"
            };
        }

        /// <summary>
        /// Cập nhật phí vận chuyển
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        [HttpPut("update-delivery-price")]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> UpdateMainOrderDelivery([FromBody] MainOrderRequest itemModel)
        {
            bool success = false;
            if (!ModelState.IsValid)
                throw new AppException(ModelState.GetErrorMessage());
            success = await mainOrderService.UpdateMainOrderDelivery(itemModel.Id, itemModel.FeeWeight ?? 0);

            return new AppDomainResult()
            {
                ResultCode = (int)HttpStatusCode.OK,
                Success = success,
                ResultMessage = "Cập nhật phí vận chuyển thành công"
            };
        }
        /// <summary>
        /// Tính phí hoa hồng (cho cập nhật đơn hàng và chọn nhân viên)
        /// </summary>
        /// <param name="itemModel"></param>
        /// <param name="item"></param>
        /// <param name="user"></param>
        /// <param name="configurations"></param>
        /// <param name="updateSql"></param>
        /// <returns></returns>
        protected async Task<List<StaffIncome>> Commission(MainOrderRequest itemModel, MainOrder item, Users user, Configurations configurations, string updateSql)
        {
            List<StaffIncome> staffIncomes = new List<StaffIncome>();
            var staffIncomeSaler = new StaffIncome();
            var staffIncomeOrderer = new StaffIncome();

            decimal salePercent = Convert.ToDecimal(configurations.SalePercent);
            decimal salePercentAf3M = Convert.ToDecimal(configurations.SalePercentAfter3Month);
            decimal datHangPercent = Convert.ToDecimal(configurations.DatHangPercent);

            //Saler
            if (itemModel.SalerId != 0 && itemModel.SalerId != item.SalerId)
            {
                var salerOld = await userService.GetByIdAsync(item.SalerId ?? 0);
                var salerNew = await userService.GetByIdAsync(itemModel.SalerId ?? 0);

                string historyContentSalerId = String.Format("{0} đã đổi nhân viên saler của đơn hàng ID là: {1}, Từ: {2}, Sang: {3}.", user.UserName, item.Id, salerOld == null ? "Không xác định" : salerOld.UserName, salerNew == null ? "Không xác định" : salerNew.UserName);
                updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                    $"VALUES({item.Id},{user.Id},N'{historyContentSalerId}',11,'{DateTime.Now}','{user.UserName}',0,1)";

                if (salerOld != null)
                {
                    //Xóa cái cũ
                    var staffIncomeSalerOld = await staffIncomeService.GetSingleAsync(e => e.MainOrderId == item.Id && e.UID == salerOld.Id);
                    if (staffIncomeSalerOld != null)
                    {
                        staffIncomeSalerOld.Deleted = true;
                        staffIncomes.Add(staffIncomeSalerOld);
                    }
                }

                //Thêm cái mới
                staffIncomeSaler.MainOrderId = item.Id;
                staffIncomeSaler.UID = salerNew.Id;
            }
            else
                staffIncomeSaler = await staffIncomeService.GetSingleAsync(e => e.MainOrderId == item.Id && e.UID == itemModel.SalerId);

            if (staffIncomeSaler != null)
            {
                //Tính
                var saler = await userService.GetByIdAsync(itemModel.SalerId ?? 0);
                int d = saler.Created.Value.Subtract(saler.Created.Value).Days;
                if (d > 90)
                {
                    staffIncomeSaler.TotalPriceReceive = itemModel.FeeBuyPro * salePercentAf3M / 100;
                    staffIncomeSaler.PercentReceive = salePercentAf3M;
                }
                else
                {
                    staffIncomeSaler.TotalPriceReceive = itemModel.FeeBuyPro * salePercent / 100;
                    staffIncomeSaler.PercentReceive = salePercent;
                }

                staffIncomeSaler.Status = (int?)StatusStaffIncome.Unpaid;
                staffIncomes.Add(staffIncomeSaler);
            }

            //Đặt hàng
            if (itemModel.DatHangId != 0 && itemModel.DatHangId != item.DatHangId)
            {
                var ordererOld = await userService.GetByIdAsync(item.DatHangId ?? 0);
                var ordererNew = await userService.GetByIdAsync(itemModel.DatHangId ?? 0);

                string historyContentSalerId = String.Format("{0} đã đổi nhân viên đặt hàng của đơn hàng ID là: {1}, Từ: {2}, Sang: {3}.", user.UserName, item.Id, ordererOld == null ? "Không xác định" : ordererOld.UserName, ordererNew == null ? "Không xác định" : ordererNew.UserName);
                updateSql += " INSERT INTO [dbo].[HistoryOrderChange] ([MainOrderId] ,[UID] ,[HistoryContent] ,[Type] ,[Created] ,[CreatedBy] ,[Deleted] ,[Active]) " +
                    $"VALUES({item.Id},{user.Id},N'{historyContentSalerId}',12,'{DateTime.Now}','{user.UserName}',0,1)";

                if (ordererOld != null)
                {
                    //Xóa cái cũ
                    var staffIncomeOrdererOld = await staffIncomeService.GetSingleAsync(e => e.MainOrderId == item.Id && e.UID == ordererOld.Id);
                    if (staffIncomeOrdererOld != null)
                    {
                        staffIncomeOrdererOld.Deleted = true;
                        staffIncomes.Add(staffIncomeOrdererOld);
                    }
                }

                //Thêm cái mới
                staffIncomeOrderer.MainOrderId = item.Id;
                staffIncomeOrderer.UID = ordererNew.Id;
            }
            else
                staffIncomeOrderer = await staffIncomeService.GetSingleAsync(e => e.MainOrderId == item.Id && e.UID == itemModel.DatHangId);

            if (staffIncomeOrderer != null)
            {
                //Tính hoa hồng
                staffIncomeOrderer.PercentReceive = datHangPercent;
                //decimal? totalPriceLoi = (item.PriceVND + item.FeeShipCN) - item.TotalPriceReal;
                //decimal? totalPriceLoi = (item.PriceVND + item.FeeShipCN) - item.TotalPriceReal - item.FeeShipCNReal;
                decimal? totalPriceLoi = (itemModel.PriceVND + itemModel.FeeShipCN) - itemModel.TotalPriceReal - itemModel.FeeShipCNReal;

                staffIncomeOrderer.OrderTotalPrice = item.TotalPriceReal;
                staffIncomeOrderer.Status = (int?)StatusStaffIncome.Unpaid;
                staffIncomeOrderer.TotalPriceReceive = (totalPriceLoi * staffIncomeOrderer.PercentReceive / 100);

                staffIncomes.Add(staffIncomeOrderer);
            }

            return staffIncomes;
        }


        /// <summary>
        /// Cập nhật chờ báo giá
        /// </summary>
        /// <param name="itemModel"></param>
        /// <returns></returns>
        [HttpPut("update-notiprice")]
        [AppAuthorize(new int[] { CoreContants.Update })]
        public async Task<AppDomainResult> UpdateIsCheckNotiPrice([FromBody] MainOrderRequest itemModel)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            bool success = false;
            if (ModelState.IsValid)
            {
                // Kiểm tra item có tồn tại chưa?
                var item = await this.domainService.GetByIdAsync(itemModel.Id);
                if (item == null)
                    throw new KeyNotFoundException("Item không tồn tại");
                if (itemModel.Status == (int)StatusOrderContants.DonMoi)
                {
                    item.IsCheckNotiPrice = true;
                    item.Status = itemModel.Status;
                    success = await this.mainOrderService.UpdateIsCheckNotiPrice(item);
                }
                if (success)
                    appDomainResult.ResultCode = (int)HttpStatusCode.OK;
                else
                    throw new Exception("Lỗi trong quá trình xử lý");
                appDomainResult.Success = success;

            }
            else
                throw new AppException(ModelState.GetErrorMessage());

            return appDomainResult;
        }


        /// <summary>
        /// Tính số lượng đơn theo trạng thái
        /// </summary>
        /// <param name="mainOrderSearch"> 1:Đơn mua hộ, 3: Đơn mua hộ khác</param>
        /// <returns></returns>
        [HttpGet("number-of-orders")]
        [AppAuthorize(new int[] { CoreContants.View })]
        public AppDomainResult NumberOfOrder([FromQuery] MainOrderSearch mainOrderSearch)
        {
            var numberOfOrders = mainOrderService.GetNumberOfOrders(mainOrderSearch);
            return new AppDomainResult
            {
                Data = numberOfOrders,
                ResultCode = (int)HttpStatusCode.OK,
                ResultMessage = null,
                Success = true
            };
        }

        /// <summary>
        /// Lấy tiền trong 1 tháng
        /// </summary>
        /// <param name="mainOrderSearch"> 1:Đơn mua hộ, 3: Đơn mua hộ khác</param>
        /// <returns></returns>
        [HttpGet("price-in-month")]
        [AppAuthorize(new int[] { CoreContants.View })]
        public AppDomainResult PriceInMonth([FromQuery] MainOrderSearch mainOrderSearch)
        {
            var priceInMonth = mainOrderService.GetPriceInMonth(mainOrderSearch);
            return new AppDomainResult
            {
                Data = priceInMonth,
                ResultCode = (int)HttpStatusCode.OK,
                ResultMessage = null,
                Success = true
            };
        }

        /// <summary>
        /// Tính số lượng các đơn hàng
        /// </summary>
        /// <returns></returns>
        [HttpGet("get-count-order")]
        [AppAuthorize(new int[] { CoreContants.View })]
        public AppDomainResult GetCountAllOrder([FromQuery] MainOrderSearch mainOrderSearch)
        {
            var numberOfOrders = mainOrderService.GetCountAllOrder(mainOrderSearch);
            IDictionary<string, int> result = new Dictionary<string, int>();
            result.Add("MainOrder", numberOfOrders.MainOrder);
            result.Add("MainOrderAnother", numberOfOrders.MainOrderAnother);
            result.Add("PayHelp", numberOfOrders.PayHelp);
            result.Add("TransportationOrder", numberOfOrders.TransportationOrder);
            return new AppDomainResult
            {
                Data = result.ToArray(),
                ResultCode = (int)HttpStatusCode.OK,
                ResultMessage = null,
                Success = true
            };
        }

        #region Excel

        /// <summary>
        /// Xuất Excel
        /// </summary>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        [HttpPost("export-excel")]
        public virtual async Task<AppDomainResult> ExportExcel([FromQuery] MainOrderSearch baseSearch)
        {
            string fileResultPath = string.Empty;
            PagedList<MainOrderModel> pagedListModel = new PagedList<MainOrderModel>();
            // ------------------------------------------LẤY THÔNG TIN XUẤT EXCEL

            // 1. LẤY THÔNG TIN DATA VÀ ĐỔ DATA VÀO TEMPLATE
            PagedList<MainOrder> pagedData = await this.domainService.GetPagedListData(baseSearch);
            pagedListModel = mapper.Map<PagedList<MainOrderModel>>(pagedData);
            ExcelUtilities excelUtility = new ExcelUtilities();

            // 2. LẤY THÔNG TIN FILE TEMPLATE ĐỂ EXPORT
            string getTemplateFilePath = GetTemplateFilePath("MainOrderTemplate.xlsx"); //Danh sách đơn hàng mua hộ

            excelUtility.TemplateFileData = System.IO.File.ReadAllBytes(getTemplateFilePath);

            // 3. LẤY THÔNG TIN THAM SỐ TRUYỀN VÀO
            excelUtility.ParameterData = await GetParameterReport(pagedListModel, baseSearch);
            if (pagedListModel.Items == null || !pagedListModel.Items.Any())
                pagedListModel.Items.Add(new MainOrderModel());
            byte[] fileByteReport = excelUtility.Export(pagedListModel.Items);

            //byte[] fileByteReport = mainOrderService.GetMainOrdersExcel(baseSearch);
            // 4. LƯU THÔNG TIN FILE BÁO CÁO XUỐNG FOLDER BÁO CÁO
            string fileName = string.Format("{0}-{1}.xlsx", Guid.NewGuid().ToString(), "MainOrder");
            string filePath = Path.Combine(env.ContentRootPath, CoreContants.UPLOAD_FOLDER_NAME, CoreContants.EXCEL_FOLDER_NAME, fileName);

            string folderUploadPath = string.Empty;
            var folderUpload = configuration.GetValue<string>("MySettings:FolderUpload");
            folderUploadPath = Path.Combine(folderUpload, CoreContants.UPLOAD_FOLDER_NAME, CoreContants.EXCEL_FOLDER_NAME);
            string fileUploadPath = Path.Combine(folderUploadPath, Path.GetFileName(filePath));

            FileUtilities.CreateDirectory(folderUploadPath);
            FileUtilities.SaveToPath(fileUploadPath, fileByteReport);

            var currentLinkSite = $"{Extensions.HttpContext.Current.Request.Scheme}://{Extensions.HttpContext.Current.Request.Host}/{CoreContants.EXCEL_FOLDER_NAME}/";
            fileResultPath = Path.Combine(currentLinkSite, Path.GetFileName(filePath));

            // 5. TRẢ ĐƯỜNG DẪN FILE CHO CLIENT DOWN VỀ
            return new AppDomainResult()
            {
                Data = fileResultPath,
                ResultCode = (int)HttpStatusCode.OK,
                Success = true,
            };
        }

        //protected virtual async Task<byte[]> ExportChart(byte[] excelData, IList<MainOrderModel> listData)
        //{
        //    return excelData;
        //}

        /// <summary>
        /// Lấy đường dẫn file template
        /// </summary>
        /// <param name="fileTemplateName"></param>
        /// <returns></returns>
        protected virtual string GetTemplateFilePath(string fileTemplateName)
        {
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            string path = System.IO.Path.Combine(currentDirectory, CoreContants.TEMPLATE_FOLDER_NAME, fileTemplateName);
            if (!System.IO.File.Exists(path))
                throw new AppException("File template không tồn tại!");
            return path;
        }

        /// <summary>
        /// Lấy thông số parameter truyền vào
        /// </summary>
        /// <param name="pagedList"></param>
        /// <param name="baseSearch"></param>
        /// <returns></returns>
        protected virtual async Task<IDictionary<string, object>> GetParameterReport(PagedList<MainOrderModel> pagedList, MainOrderSearch baseSearch)
        {
            return await Task.Run(() =>
            {
                IDictionary<string, object> dictionaries = new Dictionary<string, object>();
                return dictionaries;
            });
        }

        #endregion

    }
}
