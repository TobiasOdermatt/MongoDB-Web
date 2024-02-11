using api.Controllers;
using api.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace api.Tests.Controllers;
[TestFixture]
public class LogControllerTests
{
    private readonly LogController _controller = new();
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
        Assert.That(result, Is.Not.Null);

        if (result is not { Value: not null }) return;
        Assert.That(result.Value, Is.EqualTo("Invalid or missing date parameter."));
    }


    private static void DeleteLogDirectory()
    {
        var logDirectory = new DirectoryInfo(LogManager.Path);
        if (logDirectory.Exists)
            logDirectory.Delete(true);
    }


}