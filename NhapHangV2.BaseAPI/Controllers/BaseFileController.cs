using AutoMapper;
using NhapHangV2.Extensions;
using NhapHangV2.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NhapHangV2.BaseAPI.Controllers
{
    [ApiController]
    public abstract class BaseFileController : ControllerBase
    {
        protected readonly IServiceProvider serviceProvider;
        protected readonly ILogger<ControllerBase> logger;
        protected readonly IWebHostEnvironment env;
        protected readonly IMapper mapper;
        protected readonly IConfiguration configuration;
        public BaseFileController(IServiceProvider serviceProvider, ILogger<ControllerBase> logger, IWebHostEnvironment env, IMapper mapper, IConfiguration configuration)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.env = env;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        /// <summary>
        /// Upload Single File
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("upload-file")]
        [AppAuthorize(new int[] { CoreContants.Upload })]
        public virtual async Task<AppDomainResult> UploadFile(IFormFile file)
        {
            AppDomainResult appDomainResult = new AppDomainResult();
            await Task.Run(() =>
            {
                if (file != null && file.Length > 0)
                {
                    string fileName = string.Format("{0}-{1}", Guid.NewGuid().ToString(), file.FileName);

                    string fileUploadPath = Path.Combine(env.ContentRootPath, CoreContants.UPLOAD_FOLDER_NAME, CoreContants.TEMP_FOLDER_NAME);
                    string path = Path.Combine(fileUploadPath, fileName);
                    FileUtilities.CreateDirectory(fileUploadPath);
                    var fileByte = FileUtilities.StreamToByte(file.OpenReadStream());
                    FileUtilities.SaveToPath(path, fileByte);

                    string filePath = Path.Combine(env.ContentRootPath, CoreContants.UPLOAD_FOLDER_NAME, CoreContants.TEMP_FOLDER_NAME, fileName);
                    string folderUploadPath = string.Empty;
                    var folderUpload = configuration.GetValue<string>("MySettings:FolderUpload");
                    folderUploadPath = Path.Combine(folderUpload, CoreContants.UPLOAD_FOLDER_NAME, CoreContants.UPLOAD_FOLDER_NAME); //Có thể add tên thư mục vào đây để có thể đưa hình vào thư mục đó
                    string fileUploadPath2 = Path.Combine(folderUploadPath, Path.GetFileName(filePath));

                    string fileUrl = "";
                    // Kiểm tra có tồn tại file trong temp chưa?
                    if (System.IO.File.Exists(filePath) && !System.IO.File.Exists(fileUploadPath2))
                    {
                        FileUtilities.CreateDirectory(folderUploadPath);
                        FileUtilities.SaveToPath(fileUploadPath2, System.IO.File.ReadAllBytes(filePath));
                        var currentLinkSite = $"/{CoreContants.UPLOAD_FOLDER_NAME}/{CoreContants.UPLOAD_FOLDER_NAME}";
                        //var currentLinkSite = $"{Extensions.HttpContext.Current.Request.Scheme}://{Extensions.HttpContext.Current.Request.Host}/{CoreContants.UPLOAD_FOLDER_NAME}/{CoreContants.UPLOAD_FOLDER_NAME}";
                        fileUrl = Path.Combine(currentLinkSite, Path.GetFileName(filePath)); //Có thể add tên thư mục vào đây để có thể đưa hình vào thư mục đó
                                                                                             // ------- END GET URL FOR FILE
                    }
                    System.IO.File.Delete(filePath);
                    appDomainResult = new AppDomainResult()
                    {
                        Success = true,
                        Data = fileUrl.Replace('\\', '/')
                    };
                }
            });
            return appDomainResult;
        }

        /// <summary>
        /// Upload Multiple File
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        [HttpPost("upload-multiple-files")]
        [AppAuthorize(new int[] { CoreContants.Upload })]
        public virtual async Task<AppDomainResult> UploadFiles(List<IFormFile> files)
        {
            AppDomainResult appDomainResult = new AppDomainResult();

            await Task.Run(() =>
            {
                if (files != null && files.Any())
                {
                    List<string> fileUrls = new List<string>();
                    foreach (var file in files)
                    {
                        string fileName = string.Format("{0}-{1}", Guid.NewGuid().ToString(), file.FileName);
                        string fileUploadPath = Path.Combine(env.ContentRootPath, CoreContants.UPLOAD_FOLDER_NAME, CoreContants.TEMP_FOLDER_NAME);
                        string path = Path.Combine(fileUploadPath, fileName);
                        FileUtilities.CreateDirectory(fileUploadPath);
                        var fileByte = FileUtilities.StreamToByte(file.OpenReadStream());
                        FileUtilities.SaveToPath(path, fileByte);

                        string filePath = Path.Combine(env.ContentRootPath, CoreContants.UPLOAD_FOLDER_NAME, CoreContants.TEMP_FOLDER_NAME, fileName);
                        string folderUploadPath = string.Empty;
                        var folderUpload = configuration.GetValue<string>("MySettings:FolderUpload");
                        folderUploadPath = Path.Combine(folderUpload, CoreContants.UPLOAD_FOLDER_NAME); //Có thể add tên thư mục vào đây để có thể đưa hình vào thư mục đó
                        string fileUploadPath2 = Path.Combine(folderUploadPath, Path.GetFileName(filePath));

                        string fileUrl = "";
                        // Kiểm tra có tồn tại file trong temp chưa?
                        if (System.IO.File.Exists(filePath) && !System.IO.File.Exists(fileUploadPath2))
                        {
                            FileUtilities.CreateDirectory(folderUploadPath);
                            FileUtilities.SaveToPath(fileUploadPath2, System.IO.File.ReadAllBytes(filePath));
                            var currentLinkSite = $"/{CoreContants.UPLOAD_FOLDER_NAME}/{CoreContants.UPLOAD_FOLDER_NAME}";
                            fileUrl = Path.Combine(currentLinkSite, Path.GetFileName(filePath)); //Có thể add tên thư mục vào đây để có thể đưa hình vào thư mục đó
                                                                                                 // ------- END GET URL FOR FILE
                        }
                        System.IO.File.Delete(filePath);

                        fileUrls.Add(fileUrl);
                    }
                    appDomainResult = new AppDomainResult()
                    {
                        Success = true,
                        Data = fileUrls
                    };
                }
            });
            return appDomainResult;
        }
    }

    public class DataUploadImageResponse
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileUploadPath { get; set; }
    }
}
