using api_stockci.Models;
using api_stockci.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container.
builder.Services.Configure<StockManagementDatabaseSettings>(
    builder.Configuration.GetSection("StockManagementDatabase"));

builder.Services.AddSingleton<ICategoryService, CategoryService>();
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IStockMovementService, StockMovementService>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();