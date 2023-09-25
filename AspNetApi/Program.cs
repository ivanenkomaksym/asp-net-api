using AspNetApi.Data;
using AspNetApi.Repositories;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

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

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/healthz");

app.Run();
