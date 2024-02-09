using System.Net;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using mongodbweb.Server.Models;

namespace mongodbweb.Server.Tests.Controllers
{
    [TestFixture]
    public class AuthControllerTests
    {
        private readonly AuthController _controller = new ();
        private string _usernameFromTestConfiguration = "";
        private string _passwordFromTestConfiguration = "";

        [SetUp]
        public void Setup()
        {
            (_usernameFromTestConfiguration, _passwordFromTestConfiguration) = TestSetup.ExtractUsernameAndPassword();
            TestSetup.ConfigureDbConnector();
        }

        [Test]
        public void CreateOTP_With_Valid_Credentials()
        {
            ConfigManager.allowedIp = "*";
            var context = new DefaultHttpContext
            {
                Connection =
                {
                    RemoteIpAddress = IPAddress.Parse("127.0.0.1")
                }
            };
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = context
            };

            var validCredentials = new ConnectRequestObject { Username = _usernameFromTestConfiguration, Password = _passwordFromTestConfiguration };
            var result = _controller.CreateOtp(validCredentials) as JsonResult;
            Assert.That(result, Is.Not.Null);
        }
        
        [Test]
        public void CreateOTP_With_Valid_Credentials_Not_Allowed_Ip()
        {
            ConfigManager.allowedIp = "127.0.0.0";
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = context
            };

            var validCredentials = new ConnectRequestObject { Username = _usernameFromTestConfiguration, Password = _passwordFromTestConfiguration };
            var result = _controller.CreateOtp(validCredentials) as JsonResult;
            Assert.That(result, Is.Null);
        }
        
        [Test]
        public void CreateOTP_With_Valid_Credentials_No_Ip_Given()
        {
            ConfigManager.allowedIp = "*";
            var context = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = context
            };

            var validCredentials = new ConnectRequestObject { Username = _usernameFromTestConfiguration, Password = _passwordFromTestConfiguration };
            var result = _controller.CreateOtp(validCredentials) as JsonResult;
            Assert.That(result, Is.Null);
        }

        
        [Test]
        public void CreateOTP_With_Invalid_Credentials()
        {
            ConfigManager.allowedIp = "*";
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = context
            };

            var validCredentials = new ConnectRequestObject { Username = "INVALID_USERNAME", Password = "INVALID_PASSWORD" };
            var result = _controller.CreateOtp(validCredentials) as JsonResult;
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Logout_Redirects_To_Connect()
        {
            var context = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = context
            };
            var result = _controller.Logout() as RedirectResult;
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.Url, Is.EqualTo("/Connect"));
        }
        
        [Test]
        public void Logout_Token_And_Invalid_UUID_Cookies_And_Redirects()
        {
            var context = new DefaultHttpContext();

            context.Request.Headers["Cookie"] = "UUID=invalid-uuid; Token=test-token";
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = context
            };

            var result = _controller.Logout() as RedirectResult;
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.Url, Is.EqualTo("/Connect"));
            
            var responseCookies = context.Response.Headers["Set-Cookie"].ToString();
            Assert.Multiple(() =>
            {
                Assert.That(responseCookies.Contains("UUID=;"), Is.False);
                Assert.That(responseCookies.Contains("Token=;"), Is.False);
            });
        }

        [Test]
        public void Logout_Removes_Token_And_Valid_UUID_Cookies_And_Redirects()
        {
            var context = new DefaultHttpContext();

            context.Request.Headers["Cookie"] = "UUID=de60ad4a-4263-4d98-b3c0-345f74231820; Token=test-token";
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = context
            };

            var result = _controller.Logout() as RedirectResult;
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result?.Url, Is.EqualTo("/Connect"));
            
            var responseCookies = context.Response.Headers["Set-Cookie"].ToString();
            Assert.Multiple(() =>
            {
                Assert.That(responseCookies.Contains("UUID=;"), Is.True);
                Assert.That(responseCookies.Contains("Token=;"), Is.True);
            });
        }
    }
}