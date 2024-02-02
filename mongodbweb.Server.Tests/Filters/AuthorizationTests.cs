namespace mongodbweb.Server.Tests.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

public class AuthorizationTests
{
    private HttpContext _validHttpContext = new DefaultHttpContext();
    private readonly DbController _dbController = new ();
    
    [SetUp]
    public void Setup()
    {
        TestSetup.ConfigureDbConnector();
        _validHttpContext = TestSetup.GetValidHttpContext();
    }
            
    [Test]
    public async Task SuccessAuthorizationFilterCheck()
    {

        var authorizationFilter = new mongodbweb.Server.Filters.Authorization();
            
        var actionContext = new ActionContext(_validHttpContext, new RouteData(), new ActionDescriptor());
        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>()!, _dbController);

        async Task<ActionExecutedContext> Next()
        {
            return await Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(),
                _dbController));
        }

        await authorizationFilter.OnActionExecutionAsync(actionExecutingContext, Next);
        Assert.IsNull(actionExecutingContext.Result, "User is not authorized");
    }
    
    [Test]
    public async Task FailAuthorizationFilterCheck()
    {
        var invalidContext = new DefaultHttpContext();
        var authorizationFilter = new mongodbweb.Server.Filters.Authorization();
            
        var actionContext = new ActionContext(invalidContext, new RouteData(), new ActionDescriptor());
        var actionExecutingContext = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>()!, _dbController);

        async Task<ActionExecutedContext> Next()
        {
            return await Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(),
                _dbController));
        }

        await authorizationFilter.OnActionExecutionAsync(actionExecutingContext, Next);
        Assert.IsNotNull(actionExecutingContext.Result, "User is authorized when should not be");
    }
}