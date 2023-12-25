using Microsoft.AspNetCore.Mvc;

namespace MongoDB_Web.Tests
{
    [TestFixture]
    public class AuthControllerTests
    {
        private AuthController _controller;

        [SetUp]
        public void Setup()
        {
            _controller = new AuthController();
        }

        [Test]
        public void Login_WhenCalled_ReturnsOkResult()
        {
            var result = _controller.Login("testUser", "testPassword");
            Assert.That(result, Is.TypeOf<OkResult>());
        }

    }
}