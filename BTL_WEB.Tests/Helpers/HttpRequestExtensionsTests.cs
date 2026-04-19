using BTL_WEB.Helpers;
using Microsoft.AspNetCore.Http;

namespace BTL_WEB.Tests.Helpers;

public class HttpRequestExtensionsTests
{
    [Fact]
    public void IsAjaxRequest_ReturnsTrue_ForXRequestedWithHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Requested-With"] = "XMLHttpRequest";

        var isAjax = context.Request.IsAjaxRequest();

        Assert.True(isAjax);
    }

    [Fact]
    public void IsAjaxRequest_ReturnsTrue_ForJsonAcceptHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["Accept"] = "application/json";

        var isAjax = context.Request.IsAjaxRequest();

        Assert.True(isAjax);
    }

    [Fact]
    public void IsAjaxRequest_ReturnsFalse_WhenNoMatchingHeaders()
    {
        var context = new DefaultHttpContext();

        var isAjax = context.Request.IsAjaxRequest();

        Assert.False(isAjax);
    }

    [Fact]
    public void IsAjaxRequest_ReturnsFalse_WhenRequestIsNull()
    {
        HttpRequest? request = null;

        var isAjax = HttpRequestExtensions.IsAjaxRequest(request!);

        Assert.False(isAjax);
    }
}
