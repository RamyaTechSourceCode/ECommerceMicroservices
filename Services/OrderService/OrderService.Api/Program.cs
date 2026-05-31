using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.CreateOrder;
using OrderService.Infrastructure.Messaging.Kafka.Consumers;
using OrderService.Infrastructure.Messaging.Kafka.Producers;
using OrderService.Infrastructure.Persistence;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration["Redis:ConnectionString"];

    return ConnectionMultiplexer.Connect(configuration);
});


//implementing producer and consumer in same service 
//builder.Services.AddHostedService<KafkaTopicInitializer>(); // setup Kafka
builder.Services.AddSingleton<OrderCreatedProducer>();
builder.Services.AddHostedService<OrderProjectionConsumer>();


var app = builder.Build();

Console.WriteLine("AFTER BUILD");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();