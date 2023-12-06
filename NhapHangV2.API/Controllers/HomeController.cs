using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NhapHangV2.Interface.Services;
using NhapHangV2.Entities;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using NhapHangV2.Entities.Catalogue;
using System.Collections.Generic;
using NhapHangV2.Interface.Services.Catalogue;
using NhapHangV2.Models;
using NhapHangV2.Models.Catalogue;
using AutoMapper;
using NhapHangV2.Utilities;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;
using DocumentFormat.OpenXml.Office2010.Excel;
using NhapHangV2.Request;
using NhapHangV2.Service.Services;
using RestSharp;

namespace NhapHangV2.API.Controllers
{
    public class HomeController : Controller
    {
        protected IMapper mapper;
        protected readonly IConfigurationsService configurationsService;
        protected readonly IMenuService menuService;
        protected readonly IPageService pageService;
        protected readonly IPageTypeService pageTypeService;
        protected readonly IStepService stepService;
        protected readonly IServiceService serviceService;
        protected readonly IUserService userService;
        protected readonly ICustomerBenefitsService customerBenefitsService;
        protected readonly ICustomerTalkService customerTalkService;
        protected readonly ISearchService searchService;
        public HomeController(IServiceProvider serviceProvider, IMapper mapper)
        {
            configurationsService = serviceProvider.GetRequiredService<IConfigurationsService>();
            menuService = serviceProvider.GetRequiredService<IMenuService>();
            pageService = serviceProvider.GetRequiredService<IPageService>();
            pageTypeService = serviceProvider.GetRequiredService<IPageTypeService>();
            stepService = serviceProvider.GetRequiredService<IStepService>();
            serviceService = serviceProvider.GetRequiredService<IServiceService>();
            userService = serviceProvider.GetRequiredService<IUserService>();
            customerBenefitsService = serviceProvider.GetRequiredService<ICustomerBenefitsService>();
            customerTalkService = serviceProvider.GetRequiredService<ICustomerTalkService>();
            searchService = serviceProvider.GetRequiredService<ISearchService>();
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var homeModel = new HomeModel();
            homeModel.Type = 1;
            homeModel.Configurations = mapper.Map<ConfigurationsModel>(await configurationsService.GetByIdAsync(1));
            homeModel.MenuList = mapper.Map<IList<MenuModel>>(await menuService.GetAllAsync());
            homeModel.MenuList = await menuService.GetListMenu(homeModel.MenuList);
            homeModel.MenuList = homeModel.MenuList.OrderBy(x => x.Position).ToList();
            homeModel.StepList = mapper.Map<IList<StepModel>>((await stepService.GetAllAsync()).OrderBy(x => x.Position));
            homeModel.ServiceList = mapper.Map<IList<ServiceModel>>((await serviceService.GetAllAsync()).OrderBy(x => x.Position));
            homeModel.CustomerBenefitsList = mapper.Map<IList<CustomerBenefitsModel>>((await customerBenefitsService.GetAllAsync()).Where(x => x.ItemType == 2).OrderBy(x => x.Position).ToList());
            homeModel.CustomerTalkList = mapper.Map<IList<CustomerTalk>>((await customerTalkService.GetAllAsync()).Where(x => x.Active).ToList());
            homeModel.TopNewsPage = mapper.Map<PageTypeModel>(await pageTypeService.GetByCodeAsync("tin-tuc"));
            var token = Request?.Cookies["tokenNHTQ-demo"]?.ToString();
            if (token != null)
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(token);
                var userData = JObject.Parse(jwtSecurityToken.Claims.First().Value);
                homeModel.Username = userData["UserName"].ToString();
                homeModel.UserId = Convert.ToInt32(userData["UserId"].ToString());
                homeModel.UserGroupId = Convert.ToInt32(userData["UserGroupId"].ToString());
                homeModel.UserData = mapper.Map<UserModel>(await userService.GetByIdAsync(homeModel.UserId));
            }
            homeModel.IsRenderPopup = Convert.ToBoolean((Request?.Cookies["isRender"]?.ToString() ?? "true"));
            return View(homeModel);
        }

        public IActionResult Logout() 
        {
            if (Request.Cookies["tokenNHTQ-demo"] != null)
            {
                Response.Cookies.Delete("tokenNHTQ-demo");
            }
            return RedirectToAction("Index");
        } 

        [HttpGet("{id}")]
        public async Task<IActionResult> Index(string id)
        {
            var homeModel = new HomeModel();
            homeModel.Configurations= mapper.Map<ConfigurationsModel>(await configurationsService.GetByIdAsync(1));
            homeModel.MenuList = mapper.Map<IList<MenuModel>>(await menuService.GetAllAsync());
            homeModel.MenuList = await menuService.GetListMenu(homeModel.MenuList);
            homeModel.MenuList = homeModel.MenuList.OrderBy(x => x.Position).ToList();

            homeModel.Type = 2;
            homeModel.PageTypeContent = mapper.Map<PageTypeModel>(await pageTypeService.GetByCodeAsync(id));
            homeModel.TopNewsPage = mapper.Map<PageTypeModel>(await pageTypeService.GetByCodeAsync("tin-tuc"));

            homeModel.PageTypeList = mapper.Map<IList<PageTypeModel>>(await pageTypeService.GetAllAsync());
            homeModel.PageTypeList = homeModel.PageTypeList.Where(x=> !x.Deleted && x.Active).ToList();
            if (homeModel.PageTypeContent == null)
            {
                homeModel.Type = 3;
                homeModel.PageContent = mapper.Map<Models.PageModel>(await pageService.GetByCodeAsync(id));
                if(homeModel.PageContent == null)
                {
                   homeModel.Type = 4;
                }
                else
                {
                    homeModel.PageTypeContent = mapper.Map<PageTypeModel>(await pageTypeService.GetByIdAsync(homeModel.PageContent.PageTypeId??0));
                }
            }

            var token = Request?.Cookies["tokenNHTQ-demo"]?.ToString();
            if (token != null)
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(token);
                var userData = JObject.Parse(jwtSecurityToken.Claims.First().Value);
                homeModel.Username = userData["UserName"].ToString();
                homeModel.UserId = Convert.ToInt32(userData["UserId"].ToString());
                homeModel.UserGroupId = Convert.ToInt32(userData["UserGroupId"].ToString());
                homeModel.UserData  = mapper.Map<UserModel>( await userService.GetByIdAsync(homeModel.UserId));
            }

            return View(homeModel);
        }

        [HttpPost]
        public IActionResult SearchProduct([FromForm]string keyWord, [FromForm] int site)
        {
            string url = searchService.SearchContent(site, keyWord).Data.ToString();
            return Redirect(url);
        }

        public class HomeModel: Microsoft.AspNetCore.Mvc.RazorPages.PageModel
        { 
            /// <summary>
            /// 1: home page, 2: page type, 3: page, 4: Error
            /// </summary>
            public int Type { get; set; }
            public ConfigurationsModel Configurations { get; set; }
            public IList<MenuModel> MenuList { get; set; }
            public Models.PageModel PageContent { get; set; }
            public PageTypeModel PageTypeContent { get; set; }
            public IList<StepModel> StepList { get; set; }
            public string Username { get; set; }
            public int UserId { get; set; }
            public int UserGroupId { get; set; }
            public UserModel UserData { get; set; }
            public IList<ServiceModel> ServiceList { get; set; }
            public IList<CustomerBenefitsModel> CustomerBenefitsList { get; set; }
            public IList<CustomerTalk> CustomerTalkList { get; set; }
            public PageTypeModel TopNewsPage { get; set; }
            public bool IsRenderPopup { get; set; } 
            public IList<PageTypeModel> PageTypeList { get; set; }
        }
    }
}
