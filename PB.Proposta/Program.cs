using Microsoft.EntityFrameworkCore;
using PB.Proposta.Application.Interfaces;
using PB.Proposta.Application.Services;
using PB.Proposta.Domain.Interfaces;
using PB.Proposta.Infrastructure.Context;
using PB.Proposta.Infrastructure.Messaging;
using PB.Proposta.Infrastructure.Repository;
using PB.Proposta.Infrastructure.Services;
using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<PropostaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMQ:Host"],
        UserName = builder.Configuration["RabbitMQ:User"],
        Password = builder.Configuration["RabbitMQ:Password"],
        DispatchConsumersAsync = true
    };
    return factory.CreateConnection();
});

builder.Services.AddScoped<IPropostaRepository, PropostaRepository>();
builder.Services.AddScoped<IPropostaService, PropostaService>();
builder.Services.AddScoped<IMessagePublisher, RabbitMQPublisher>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddHostedService<RabbitMQConsumer>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PropostaDbContext>();
    await db.Database.MigrateAsync();
}

await host.RunAsync();