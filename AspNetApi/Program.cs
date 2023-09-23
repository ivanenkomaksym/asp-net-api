using AspNetApi.Data;
using AspNetApi.Repositories;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddHealthChecks();

// Add services to the container.
builder.Services.AddSingleton<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<IProductContext, ProductContext>();

builder.Services.AddSingleton<IShoppingCartRepository, ShoppingCartRepository>();
builder.Services.AddSingleton<IShoppingCartContext, ShoppingCartContext>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpLogging();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Add(HeaderNames.ContentSecurityPolicy, new StringValues("default-src 'self'"));
    context.Response.Headers.Add(HeaderNames.XContentTypeOptions, new StringValues("nosniff"));
    context.Response.Headers.Add(HeaderNames.XFrameOptions, new StringValues("SAMEORIGIN"));
    context.Response.Headers.Add(HeaderNames.XXSSProtection, new StringValues("1; mode=block"));
    await next();
});

app.UseHsts();
app.UseHttpsRedirection();

app.UseCors(builder =>
{
    builder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
});

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/", async context =>
    {
        await context.Response.WriteAsync("Hello World!");

        //Output the relevant properties as the framework sees it
        await context.Response.WriteAsync($"---As the application sees it{Environment.NewLine}");
        await context.Response.WriteAsync($"HttpContext.Connection.RemoteIpAddress : {context.Connection.RemoteIpAddress}{Environment.NewLine}");
        await context.Response.WriteAsync($"HttpContext.Connection.RemoteIpPort : {context.Connection.RemotePort}{Environment.NewLine}");
        await context.Response.WriteAsync($"HttpContext.Request.Scheme : {context.Request.Scheme}{Environment.NewLine}");
        await context.Response.WriteAsync($"HttpContext.Request.Host : {context.Request.Host}{Environment.NewLine}");

        //Output relevant request headers (starting with an X)
        await context.Response.WriteAsync($"{Environment.NewLine}---Request Headers starting with X{Environment.NewLine}");
        foreach (var header in context.Request.Headers.Where(h => h.Key.StartsWith("X", StringComparison.OrdinalIgnoreCase)))
        {
            await context.Response.WriteAsync($"Request-Header {header.Key}: {header.Value}{Environment.NewLine}");
        }
    });
});

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/healthz");

app.Run();
