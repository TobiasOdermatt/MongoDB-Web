using api.Helpers;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using static api.Helpers.LogManager;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly LogManager _logger = new();
        private static object GenerateAuthResponse(string uuid, string token)
        {
            return new { uuid, token };
        }

        // If connection is successful, UUID will be returned
        [HttpPost("CreateOTP")]
        public IActionResult CreateOtp(ConnectRequestObject dataJson)
        {
            if (Request.HttpContext.Connection.RemoteIpAddress == null)
                return NoContent();

            var ipOfRequest = Request.HttpContext.Connection.RemoteIpAddress.ToString();

            DbConnector connector = new(dataJson.Username, dataJson.Password, Request.HttpContext.Connection.RemoteIpAddress.ToString());

            if (connector.client == null)
                return NoContent();

            var inputData = $"Data:{dataJson.Username}@{dataJson.Password}";
            var randData = OtpManagement.GenerateRandomBinaryData(inputData.Length * 8);
            var token = OtpManagement.EncryptUserData(inputData, randData);

            var uuid = Guid.NewGuid().ToString();

            var localDate = DateTime.Now;
            OtpFileObject newFile = new(Guid.Parse(uuid), localDate, randData, ipOfRequest, false, dataJson.Username);

            OtpMemoryManagement.WriteOtpFile(uuid, newFile);

            _logger.WriteLog(LogType.Info, "OTP created for user: " + dataJson.Username + " with UUID " + uuid + " IP: " + ipOfRequest);

            var responseAuth = GenerateAuthResponse(uuid, token);
            return new JsonResult(responseAuth);
        }

        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            var uuidString = HttpContext.Request.Cookies["UUID"];
            if (uuidString is null || !Guid.TryParse(uuidString, out var uuid))
                return Redirect("/Connect");

            OtpMemoryManagement.DeleteOtp(uuid.ToString());

            HttpContext.Response.Cookies.Delete("UUID");
            HttpContext.Response.Cookies.Delete("Token");

            return Redirect("/Connect");
        }
    }
}
