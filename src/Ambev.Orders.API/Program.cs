using Microsoft.EntityFrameworkCore;
using Orders.API.Middlewares;
using Orders.Data.Context;
using Orders.Data.Repositories;
using Orders.Domain.Interfaces.Repositories;
using Orders.Domain.Interfaces.Services;
using Orders.Domain.Interfaces.Strategies;
using Orders.Domain.Services;
using Orders.Domain.Strategies;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/orders.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("OrdersDb"));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

var UsingTaxReform = builder.Configuration.GetValue<bool>("FeatureFlags:UsingTaxReform");

if (UsingTaxReform)
    builder.Services.AddScoped<ITaxCalculator, TaxReformaStrategy>();
else
    builder.Services.AddScoped<ITaxCalculator, TaxAtualStrategy>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
