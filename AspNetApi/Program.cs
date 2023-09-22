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
    });
});

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/healthz");

app.Run();
