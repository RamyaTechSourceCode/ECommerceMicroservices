using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

// JWT Auth setup
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = config["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"])
        )
    };
});

builder.Services.AddAuthorization();

//  Rate Limiting
builder.Services.AddMemoryCache();

builder.Services.Configure<IpRateLimitOptions>(
    builder.Configuration.GetSection("IpRateLimiting"));
/*
// moved to appsettings
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 100,   // 100 requests
            Period = "1m"  // per minute
        }
    };
});*/

builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

// Yarp reverse proxy
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(config.GetSection("ReverseProxy"));

var app = builder.Build();

// Order
app.UseAuthentication();   // 1. Authenticate
app.UseAuthorization();    // 2. Authorize
app.UseIpRateLimiting();   // 3. Rate limiting


// Request validation middleware
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("User-Agent"))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Missing User-Agent header");
        return;
    }

    // example for blocking suspicious requests
    foreach (var queryParam in context.Request.Query)
    {
        var value = queryParam.Value.ToString();

        if (value.Contains("DROP TABLE",
            StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid request");
            return;
        }
    }
    await next();
});

// Map YARP applying authorization
app.MapReverseProxy().RequireAuthorization();

app.Run();