using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MongoDB.Driver;
using mongodbweb.Server.Controllers;
using mongodbweb.Server.Helpers;

namespace mongodbweb.Server.Filters
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
                
                if (context.Controller is DbController controller && _mongoClient != null)
                {
                    controller.mongoDbOperations.client = _mongoClient;
                    controller.mongoDbOperations.username = _username;
                    controller.mongoDbOperations.uuid = _uuid;
                }
                await next();
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }
        }
        
        private bool ValidateConnection(HttpContext httpContext)
        {
            if(!ConfigManager.useAuthorization){
                DbConnector dBConnector = new();
                _mongoClient = DbConnector.DbConnect(DbConnector.GetConnectionString("", ""));
                return true;
            }

            var (uuid, authOtp) = ReadOtpCookie(httpContext);
            if (uuid is null || authOtp is null)
                return false;
            
            _uuid = uuid;
            
            var otpFile = OtpMemoryManagement.ReadOtpFile(uuid);

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
