using MassTransit;
using NotificationService.Consumers;
using NotificationService.Hubs;
using NotificationService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<INotificationsService, NotificationsService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("nt", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<NotificationHub>("/notifications");

app.Run();
