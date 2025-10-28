using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Minio;
using MockProjectService.Contract.Message;
using MockProjectService.Core.Interfaces;
using MockProjectService.Domain.Entities;
using MockProjectService.Infrastructure.DataContext;
using MockProjectService.Infrastructure.Implementations;
using MockProjectService.Infrastructure.Kafka;
using System;
using System.Linq;
using MediatR;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;
using MockProjectService.Core.Extensions;
using MockProjectService.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
    EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
});

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MockProjectService API",
        Version = "v1",
        Description = "API utility operations"
    });
    c.AddServer(new OpenApiServer { Url = "/" });
});

// Services Registration
builder.Services.AddScoped<IAppConfiguration, AppConfiguration>();
builder.Services.AddScoped<ICombinedTransaction, CombinedTransaction>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IKafkaProducer, KafkaProducer>();
builder.Services.AddScoped(typeof(IKafkaProducerRepository<>), typeof(KafkaProducerRepository<>));
builder.Services.AddScoped(typeof(IKafkaConsumerRepository<>), typeof(KafkaConsumerRepository<>));
builder.Services.AddScoped(typeof(IKafkaConsumerFactory<>), typeof(KafkaConsumerFactory<>));
builder.Services.AddScoped(typeof(IKafkaResponseHandler<>), typeof(KafkaResponseHandler<>));
builder.Services.AddScoped<IKafkaTransaction, KafkaTransaction>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
builder.Services.AddScoped(typeof(IQueryExtensions<>), typeof(QueryExtensions<>));

// Database Context
builder.Services.AddDbContext<MockProjectServiceDataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(MockProjectService.Core.AssemblyReference).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(MockProjectService.Contract.AssemblyReference).Assembly);
});

// Kafka Consumers
var sourceServices = builder.Configuration.GetSection("Kafka:SourceServices").Get<string[]>() ?? [];

Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Registering {sourceServices.Length} Kafka consumers for sources: [{string.Join(", ", sourceServices)}]");

foreach (var sourceService in sourceServices)
{
    var currentSource = sourceService;

    builder.Services.AddSingleton<IHostedService>(sp =>
    {
        var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

        return ActivatorUtilities.CreateInstance<KafkaConsumerHostedService<MockProject>>(
            sp,
            scopeFactory,
            currentSource
        );
    });

    builder.Services.AddSingleton<IHostedService>(sp =>
    {
        var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

        return ActivatorUtilities.CreateInstance<KafkaConsumerHostedService<Submission>>(
            sp,
            scopeFactory,
            currentSource
        );
    });

    builder.Services.AddSingleton<IHostedService>(sp =>
    {
        var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

        return ActivatorUtilities.CreateInstance<KafkaConsumerHostedService<Process>>(
            sp,
            scopeFactory,
            currentSource
        );
    });
}


var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(() =>
{
    using var scope = app.Services.CreateScope();
    var appConfiguration = scope.ServiceProvider.GetRequiredService<IAppConfiguration>();
    KafkaResponseConsumer.Initialize(appConfiguration);
    Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] KafkaResponseConsumer initialized");
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MockProjectService API v1");
    c.DocumentTitle = "MockProjectService API Documentation";
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();
app.MapControllers();

var currentService = builder.Configuration["KafkaCommunication:CurrentService"];
Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] {currentService} Service Started!");
Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Kafka Consumers registered for: [{string.Join(", ", sourceServices)}]");

app.Run();