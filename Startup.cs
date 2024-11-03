using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

namespace log_analytics;

public class Startup
{
    private readonly IConfiguration _configuration;
    private const string CorsPolicy = "MyCorsPolicy";

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
    {
        services.AddControllers();
        services.AddCors(options =>
        {
            options.AddPolicy(
                name: CorsPolicy,
                builder =>
                {
                    builder
                        .WithOrigins("http://localhost:1507")
                        .WithHeaders("*")
                        .WithMethods("*");

                    builder.AllowCredentials();
                });
        });
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Log Analytics API",
                Version = "v1",
                Description = "Log Analytics API"
            });
        });

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.Name = "AuthCookie";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.SlidingExpiration = true;
        })
        .AddMicrosoftIdentityWebApp(_configuration.GetSection("AzureAd"), OpenIdConnectDefaults.AuthenticationScheme, null)
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddInMemoryTokenCaches();

        services.AddApplicationInsightsTelemetry();
    }
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseCors(CorsPolicy);
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSwagger(c =>
        {
            c.RouteTemplate = "swagger/{documentname}/swagger.json";
        });
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Log Analytics API");
            c.RoutePrefix = "swagger";
        });

        app.UseEndpoints(endpoints =>
        {
            var pipeline = endpoints.CreateApplicationBuilder().Build();
            var oidcAuthAttr = new AuthorizeAttribute { AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme };
            endpoints.Map("/swagger/v1/swagger.json", pipeline).RequireAuthorization(oidcAuthAttr);
            endpoints.Map("/swagger/index.html", pipeline).RequireAuthorization(oidcAuthAttr);

            endpoints.MapControllers();
        });
    }
}
