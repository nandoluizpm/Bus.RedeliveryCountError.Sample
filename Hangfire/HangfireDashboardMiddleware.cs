using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace Bus.RedeliveryCountError.Sample.Hangfire;

public class HangfireDashboardMiddleware
{
    private readonly RequestDelegate _nextRequestDelegate;
    private readonly JobStorage _jobStorage;
    private readonly DashboardOptions _dashboardOptions;
    private readonly RouteCollection _routeCollection;

    public HangfireDashboardMiddleware(RequestDelegate nextRequestDelegate, JobStorage storage, DashboardOptions options, RouteCollection routes)
    {
        _nextRequestDelegate = nextRequestDelegate;
        _jobStorage = storage;
        _dashboardOptions = options;
        _routeCollection = routes;
    }

    public Task Invoke(HttpContext httpContext)
    {
        var findResult = _routeCollection.FindDispatcher(httpContext.Request?.Path.Value ?? String.Empty);
        return findResult == null ? WhenNullResult(httpContext) : WhenResult(httpContext, findResult);
    }

    private Task WhenResult(HttpContext httpContext, Tuple<IDashboardDispatcher, Match> findResult)
    {
        var aspNetCoreDashboardContext = new AspNetCoreDashboardContext(_jobStorage, _dashboardOptions, httpContext);
        aspNetCoreDashboardContext.UriMatch = findResult.Item2;
        return findResult.Item1.Dispatch(aspNetCoreDashboardContext);
    }

    private Task WhenNullResult(HttpContext httpContext) => _nextRequestDelegate?.Invoke(httpContext);
}