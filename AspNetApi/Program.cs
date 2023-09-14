using AspNetApi.Data;
using AspNetApi.Repositories;

var builder = WebApplication.CreateBuilder(args);

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
