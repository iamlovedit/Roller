using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Roller.Infrastructure.Exceptions;
using Roller.Infrastructure.Filters;


namespace Infrastructure.Tests;

public class FilterTests
{
    private readonly ActionContext _actionContext = new ActionContext
    {
        HttpContext = new DefaultHttpContext(),
        RouteData = new RouteData(),
        ActionDescriptor = new ControllerActionDescriptor(),
    };

    [Fact]
    public async Task OnExceptionAsync_HandlesGenericException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GlobalExceptionsFilter>>();
        var filter = new GlobalExceptionsFilter(loggerMock.Object);

        var exceptionContext = new ExceptionContext(_actionContext, new List<IFilterMetadata>())
        {
            Exception = new Exception("Generic error")
        };
        // Act
        await filter.OnExceptionAsync(exceptionContext);
        var contentResult = Assert.IsType<ContentResult>(exceptionContext.Result);
        Assert.Equal(StatusCodes.Status200OK, contentResult.StatusCode);
        Assert.Equal("application/json;charset=utf-8", contentResult.ContentType);
        Assert.Contains("Generic error", contentResult.Content);
    }

    [Fact]
    public async Task OnExceptionAsync_HandlesFriendException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GlobalExceptionsFilter>>();
        var filter = new GlobalExceptionsFilter(loggerMock.Object);

        var exceptionContext = new ExceptionContext(_actionContext, new List<IFilterMetadata>())
        {
            Exception = new FriendlyException("Friendly error")
        };
        // Act
        await filter.OnExceptionAsync(exceptionContext);
        var contentResult = Assert.IsType<ContentResult>(exceptionContext.Result);
        Assert.Equal(StatusCodes.Status200OK, contentResult.StatusCode);
        Assert.Equal("application/json;charset=utf-8", contentResult.ContentType);
        Assert.Contains("Friendly error", contentResult.Content);
        Assert.Contains("500", contentResult.Content);
    }
}