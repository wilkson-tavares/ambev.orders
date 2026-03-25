using Microsoft.EntityFrameworkCore;
using Orders.Data.Context;
using Orders.Data.Repositories;
using Orders.Domain.Interfaces;
using Orders.Domain.Strategies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("OrdersDb"));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

var UsingTaxReform = builder.Configuration.GetValue<bool>("FeatureFlags:UsingTaxReform");

if (UsingTaxReform)
    builder.Services.AddScoped<ITaxCalculator, TaxReformaStrategy>();
else
    builder.Services.AddScoped<ITaxCalculator, TaxAtualStrategy>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();

app.Run();
