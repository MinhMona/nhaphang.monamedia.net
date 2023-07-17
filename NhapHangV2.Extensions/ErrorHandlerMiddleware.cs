using NhapHangV2.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NhapHangV2.Extensions
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private ILogger<ControllerBase> _logger;


        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ControllerBase> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(Microsoft.AspNetCore.Http.HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                //var response = context.Response;
                context.Response.ContentType = "application/json";

                switch (error)
                {
                    case AggregateException e: //423
                        context.Response.StatusCode = (int)HttpStatusCode.Locked;
                        break;
                    case AppException e: //400
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;
                    case UnauthorizedAccessException e: //401
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        break;
                    case InvalidCastException e: //403
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        break;
                    case EntryPointNotFoundException e: //404
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    case KeyNotFoundException e: //404
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    case SecurityTokenExpiredException e:
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        break;
                    case TimeoutException e: //408
                        context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                        break;
                    default:
                        {
                            var RouteData = context.Request.Path.Value.Split("/");
                            string apiName = string.Empty;
                            string actionName = string.Empty;

                            if (RouteData.Count() >= 2)
                                apiName = RouteData[1];
                            if (RouteData.Count() >= 3)
                                actionName = RouteData[2];

                            _logger.LogError(string.Format("{0} {1}: {2}", apiName
                                , actionName, error.Message));
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        }

                        break;
                }


                var result = new AppDomainResult()
                {
                    ResultCode = context.Response.StatusCode,
                    ResultMessage = error?.Message,
                    Success = false
                };

                if (result.ResultMessage.Contains("IDX10223: Lifetime validation failed. The token is expired. ValidTo: 'System.DateTime', Current time: 'System.DateTime'."))
                {
                    result.ResultCode = (int)HttpStatusCode.Unauthorized;
                    result.ResultMessage = "Unauthorized";
                }
                await context.Response.WriteAsync(result.ToString());
            }
        }
    }
}
