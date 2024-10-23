using AspNetApi.Authentication;
using AspNetApi.Authorization;
using AspNetApi.Converters;
using AspNetApi.Data;
using AspNetApi.Repositories;
using AspNetApi.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestPath
                            | HttpLoggingFields.RequestMethod
                            | HttpLoggingFields.RequestQuery
                            | HttpLoggingFields.RequestHeaders
                            | HttpLoggingFields.RequestBody
                            | HttpLoggingFields.ResponseHeaders
                            | HttpLoggingFields.ResponseBody
                            | HttpLoggingFields.ResponseStatusCode;
});

builder.Services.AddSingleton<SecretHeaderAuthorizationResultTransformer>();
builder.Services.AddSingleton<MinimumAgeAuthorizationResultTransformer>();

// Register composite handler but don't make it part of the collection
builder.Services.AddCompositeAuthorizationResultTransformer(typeof(SecretHeaderAuthorizationResultTransformer), typeof(MinimumAgeAuthorizationResultTransformer));

var authenticationOptions = builder.Configuration.GetSection(AspNetApi.Configuration.AuthenticationOptions.Name).Get<AspNetApi.Configuration.AuthenticationOptions>();

// Add custom authorization policy
if (authenticationOptions != null && authenticationOptions.Enable)
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = DummySuccessAuthenticationHandler.Name;
        options.DefaultChallengeScheme = DummySuccessAuthenticationHandler.Name;
    })
    .AddScheme<AuthenticationSchemeOptions, DummySuccessAuthenticationHandler>(DummySuccessAuthenticationHandler.Name, null);
}
else
{
    builder.Services.AddAuthentication(AllowAnonymousAuthenticationHandler.Name)
        .AddScheme<AuthenticationSchemeOptions, AllowAnonymousAuthenticationHandler>(AllowAnonymousAuthenticationHandler.Name, options => { });
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HealthCheckPolicy", policy =>
    {
        if (authenticationOptions != null && authenticationOptions.Enable)
        {
            policy.RequireAuthenticatedUser();
        }
        policy.AddRequirements(new SecretHeaderRequirement("secret_header", "expected_value"));
        policy.AddRequirements(new MinimumAgeRequirement("age", 18));
    });
});

builder.Services.AddTransient<IAuthorizationHandler, SecretHeaderHandler>();
builder.Services.AddTransient<IAuthorizationHandler, MinimumAgeHandler>();

// Add services to the container.
builder.Services.AddSingleton<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<IProductContext, ProductContext>();

builder.Services.AddSingleton<IShoppingCartRepository, ShoppingCartRepository>();
builder.Services.AddSingleton<IShoppingCartContext, ShoppingCartContext>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.ConfigureOptions<ConfigureJsonOptions>();

builder.Services.AddScoped<ProductValidationFilter>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseHttpLogging();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Add(HeaderNames.ContentSecurityPolicy, new StringValues("default-src 'self'"));
    context.Response.Headers.Add(HeaderNames.XContentTypeOptions, new StringValues("nosniff"));
    context.Response.Headers.Add(HeaderNames.XFrameOptions, new StringValues("SAMEORIGIN"));
    context.Response.Headers.Add("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
    await next();
});

app.UseHsts();
app.UseHttpsRedirection();

app.UseCors(builder =>
{
    builder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/", async context =>
    {
        //Output the relevant properties as the framework sees it
        await context.Response.WriteAsync($"---As the application sees it---{Environment.NewLine}");
        await context.Response.WriteAsync($"HttpContext.Connection.RemoteIpAddress : {context.Connection.RemoteIpAddress}{Environment.NewLine}");
        await context.Response.WriteAsync($"HttpContext.Connection.RemoteIpPort : {context.Connection.RemotePort}{Environment.NewLine}");
        await context.Response.WriteAsync($"HttpContext.Request.Scheme : {context.Request.Scheme}{Environment.NewLine}");
        await context.Response.WriteAsync($"HttpContext.Request.Host : {context.Request.Host}{Environment.NewLine}");

        //Output relevant request headers
        await context.Response.WriteAsync($"{Environment.NewLine}---Request Headers---{Environment.NewLine}");
        foreach (var header in context.Request.Headers)
        {
            await context.Response.WriteAsync($"{header.Key}: {header.Value}{Environment.NewLine}");
        }

        await context.Response.WriteAsync($"{Environment.NewLine}---Response Headers---{Environment.NewLine}");
        foreach (var header in context.Response.Headers)
        {
            await context.Response.WriteAsync($"{header.Key}: {header.Value}{Environment.NewLine}");
        }
    });
});

app.MapHealthChecks("/healthz").RequireAuthorization("HealthCheckPolicy");
app.MapControllers();

app.Run();
