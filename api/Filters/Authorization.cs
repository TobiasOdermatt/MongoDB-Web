﻿using api.Controllers;
using api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MongoDB.Driver;

namespace api.Filters
{
    [AttributeUsage((AttributeTargets.Class | AttributeTargets.Method))]
    public class Authorization : Attribute, IAsyncActionFilter
    {
        public bool IsRequiredAdmin { get; set; } = false;

        private MongoClient? _mongoClient;
        private string _username = "";
        private string _uuid = "";

        private bool _isUserAdmin;


        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (ValidateConnection(context.HttpContext))
            {
                if (IsRequiredAdmin && !_isUserAdmin)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                if (context.Controller is DbController dbController && _mongoClient != null)
                {
                    dbController.mongoDbOperations.client = _mongoClient;
                    dbController.mongoDbOperations.username = _username;
                    dbController.mongoDbOperations.uuid = _uuid;
                }

                if (context.Controller is FileController fileController)
                    fileController.userUUID = _uuid;

                await next();
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }
        }

        private bool ValidateConnection(HttpContext httpContext)
        {
            if (!ConfigManager.useAuthorization)
            {
                DbConnector dBConnector = new();
                _mongoClient = DbConnector.DbConnect(DbConnector.GetConnectionString("", ""));
                return true;
            }

            var (uuid, authOtp) = ReadOtpCookie(httpContext);
            if (uuid is null || authOtp is null)
                return false;

            _uuid = uuid;

            var otpFile = OtpMemoryManagement.ReadOtp(uuid);

            if (otpFile is null)
                return false;

            var decryptedData = OtpManagement.DecryptUserData(authOtp, otpFile.RandomString);

            if (decryptedData is null)
                return false;

            var (username, password) = OtpManagement.GetUserData(decryptedData);
            _username = username;

            var iPAddress = httpContext.Connection.RemoteIpAddress;
            if (iPAddress == null)
                return false;

            var ipOfRequest = iPAddress.ToString();
            DbConnector connector = new(username, password, ipOfRequest);
            _mongoClient = connector.client;
            _isUserAdmin = connector.IsUserAdmin(username);

            return connector.client != null;
        }

        private static (string?, string?) ReadOtpCookie(HttpContext httpContext)
        {
            var uuid = httpContext.Request.Cookies["UUID"];
            var authOtp = Base64Decode(httpContext.Request.Cookies["Token"]);
            return (uuid, authOtp);
        }

        private static string? Base64Decode(string? base64EncodedData)
        {
            if (base64EncodedData is null)
                return null;

            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
