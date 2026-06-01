using Confluent.Kafka;
using ECommerce.Contracts.Events;
using InventoryService.Application;
using InventoryService.Application.Abstractions;
using InventoryService.Infrastructure.Messaging.Consumers;
using InventoryService.Infrastructure.Messaging.Redis;
using InventoryService.Infrastructure.Persistence;
using MassTransit;

using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateInventoryCommand).Assembly));

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IInventoryDbContext>(
    provider => provider.GetRequiredService<InventoryDbContext>());
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration =
        builder.Configuration.GetConnectionString("Redis");

    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddMassTransit(x =>
{
    // REQUIRED: main bus (fixes IBus error)
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });

    
    // Kafka Rider
    x.AddRider(rider =>
    {
        rider.AddProducer<InventoryReservedEvent>("inventory-reserved-event");
        rider.AddProducer<InventoryRejectedEvent>("inventory-rejected-event");

        rider.AddConsumer<ProductCreatedConsumer>();
        rider.AddConsumer<OrderCreatedConsumer>();

        rider.UsingKafka((context, k) =>
        {
            k.Host("localhost:9092");

            k.TopicEndpoint<ProductCreatedEvent>(
                "product.created",
                "inventory.service",
                e =>
                {
                    e.ConfigureConsumer<ProductCreatedConsumer>(context);
                    e.AutoOffsetReset = AutoOffsetReset.Earliest;
                });

            k.TopicEndpoint<OrderCreatedEvent>(
              "order.created",
              "inventory.service",
              e =>
              {
                  e.ConfigureConsumer<OrderCreatedConsumer>(context);
                  e.AutoOffsetReset = AutoOffsetReset.Earliest;
              });

        });
    });
});

builder.Services.AddScoped<IRedisService, RedisService>();

var app = builder.Build();



app.Run();
