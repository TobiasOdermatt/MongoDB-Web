using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MongoDB.Driver;
using MongoDB_Web.Server.Controllers;
using MongoDB_Web.Server.Helpers;
using MongoDB_Web.Server.Models;
using System.Net;

namespace MongoDB_Web.Server.Filters
{
    [AttributeUsage((AttributeTargets.Class | AttributeTargets.Method))]
    public class Authorization : Attribute, IAsyncActionFilter
    {
        private MongoClient? mongoClient;
        private bool useAuthorization;
  

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            useAuthorization = DBConnector.useAuthorization;

            if (IsCookieValid(context.HttpContext))
            {
                if (context.Controller is DbBaseController controller && mongoClient != null)
                {
                    controller.MongoClient = mongoClient;
                    await next();
                    return;
                }
            }
            context.Result = new UnauthorizedResult();
            return;
        }

        //Checks if the AuthCookie is valid
        public bool IsCookieValid(HttpContext httpContext)
        {
            if(!useAuthorization){
                DBConnector dBConnector = new();
                mongoClient = dBConnector.DbConnect(dBConnector.getConnectionString("", ""));
                return true;
            }

            (string? uuid, string? authOTP) = ReadOTPCookie(httpContext);
            if (uuid is null || authOTP is null)
                return false;

            OTPFileManagement oTPFileManagement = new();
            OTPFileObject? otpFile = oTPFileManagement.ReadOTPFile(uuid);

            if (otpFile is null)
                return false;

            OTPManagement otpManager = new();
            string? decryptedData = otpManager.DecryptUserData(authOTP, otpFile.RandomString);

            if (decryptedData is null)
                return false;

            (string username, string password) = otpManager.GetUserData(decryptedData);

            IPAddress? iPAddress = httpContext.Connection.RemoteIpAddress;
            if (iPAddress == null)
                return false;

            string ipOfRequest = iPAddress.ToString();
            DBConnector connector = new(username, password, uuid, ipOfRequest);
            mongoClient = connector.Client;
            return connector.Client != null;
        }

        public (string?, string?) ReadOTPCookie(HttpContext httpContext)
        {
            string? uuid = httpContext.Request.Cookies["UUID"];
            string? authOTP = Base64Decode(httpContext.Request.Cookies["Token"]);
            return (uuid, authOTP);
        }

        public string? Base64Decode(string? base64EncodedData)
        {
            if (base64EncodedData is null)
                return null;

            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
