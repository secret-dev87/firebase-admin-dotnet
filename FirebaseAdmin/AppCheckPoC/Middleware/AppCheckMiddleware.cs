using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using FirebaseAdmin;
using System.Net;
using System.Net.Http;
using FirebaseAdmin.AppCheck;

namespace AppCheckPoC.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class AppCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly FirebaseApp _firebaseApp;

        public AppCheckMiddleware(RequestDelegate next, FirebaseApp firebaseApp)
        {
            _next = next;
            _firebaseApp = firebaseApp;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var appCheckToken = httpContext.Request.Headers.FirstOrDefault(x => x.Key == "X-Firebase-AppCheck").Value.ToString();

            if (String.IsNullOrEmpty(appCheckToken))
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsync("Firebase AppCheck Token is missed.");
            }

            try
            {
                FirebaseAppCheck appCheck = FirebaseAppCheck.GetAppCheck(_firebaseApp);

                var appCheckClaims = await appCheck.VerifyTokenAsync(appCheckToken);

                if (appCheckClaims == null)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await httpContext.Response.WriteAsync("Firebase AppCheck Token is invalid.");
                } else
                {
                    await _next(httpContext);
                }
            }
            catch (Exception ex)
            {
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await httpContext.Response.WriteAsync($"{ex.Message}");
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class AppCheckMiddlewareExtensions
    {
        public static IApplicationBuilder UseAppCheckMiddleware(this IApplicationBuilder builder, FirebaseApp firebaseApp)
        {
            return builder.UseMiddleware<AppCheckMiddleware>(firebaseApp);
        }
    }
}
