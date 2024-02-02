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
    public void CountLog_WithValidDate_ReturnsOkResult()
    {
        var testDate = DateTime.Now;
        var result = _controller.CountLog(testDate) as OkObjectResult;
        
        Assert.IsNotNull(result);
        if (result is not ObjectResult { Value: not null } response) return;
        var infoProperty = response.Value.GetType().GetProperty("InfoCount");
        var warningProperty = response.Value.GetType().GetProperty("WarningCount");
        var errorProperty = response.Value.GetType().GetProperty("ErrorCount");
        
        var infoCount = infoProperty?.GetValue(response.Value, null);
        var warningCount = warningProperty?.GetValue(response.Value, null);
        var errorCount = errorProperty?.GetValue(response.Value, null);
            
        Assert.AreEqual(1, infoCount);
        Assert.AreEqual(1, warningCount);
        Assert.AreEqual(1, errorCount);
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