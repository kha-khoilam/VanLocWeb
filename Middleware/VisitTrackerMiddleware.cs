using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using VanLocWeb.Services;

namespace VanLocWeb.Middleware
{
    public class VisitTrackerMiddleware
    {
        private readonly RequestDelegate _next;

        public VisitTrackerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, DataService dataService)
        {
            // Only track page requests, not static files or API calls if possible
            string path = context.Request.Path.Value?.ToLower() ?? "";
            bool isPageRequest = !path.Contains(".") && !path.StartsWith("/api/");

            if (isPageRequest)
            {
                // Update: Use AddSiteVisit for consistency
                dataService.AddSiteVisit();
            }

            await _next(context);
        }
    }
}
