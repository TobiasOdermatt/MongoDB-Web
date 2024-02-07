using Microsoft.AspNetCore.Mvc;

namespace mongodbweb.Server.Tests.Controllers;
[TestFixture]
public class LogControllerTests
{
    private readonly LogController _controller = new ();
    private readonly LogManager _logger = new();
    [SetUp]
    public void Setup()
    {
        DeleteLogDirectory();
        _logger.WriteLog(LogManager.LogType.Warning, "Test message");
        _logger.WriteLog(LogManager.LogType.Error, "Test message");
        _logger.WriteLog(LogManager.LogType.Info, "Test message");
    }

    [Test]
    public void CountLog_WithInvalidDate_ReturnsBadRequest()
    {
        var result = _controller.CountLog(DateTime.MinValue) as BadRequestObjectResult;
        Assert.IsNotNull(result);
        
        if (result is not { Value: not null }) return;
        Assert.AreEqual("Invalid or missing date parameter.", result.Value);
    }
    

    private static void DeleteLogDirectory()
    {
        var logDirectory = new DirectoryInfo(LogManager.Path);
        if (logDirectory.Exists)
            logDirectory.Delete(true);
    }


}