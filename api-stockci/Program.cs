using api_stockci.Models;
using api_stockci.Services;
using System.Text.Json.Serialization;

DotNetEnv.Env.NoClobber().Load();

var builder = WebApplication.CreateBuilder(args);

//Add services to the container.
builder.Services.AddOptions<StockManagementDatabaseSettings>()
    .Bind(builder.Configuration.GetSection("StockManagementDatabase"))
    .Validate(s => !string.IsNullOrWhiteSpace(s.ConnectionString),
        "StockManagementDatabase__ConnectionString manquant (.env).")
    .Validate(s => !string.IsNullOrWhiteSpace(s.DatabaseName),
        "StockManagementDatabase__DatabaseName manquant (.env).")
    .ValidateOnStart();

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