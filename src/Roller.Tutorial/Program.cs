using Roller.Infrastructure.EventBus;
using Roller.Infrastructure.EventBus.RabbitMQ;
using Roller.Infrastructure.Middlewares;
using Roller.Infrastructure.SetupExtensions;
using Roller.Tutorial;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRollerRabbitMQEventBus(builder.Configuration);
builder.Services.AddTransient<MessageSentEventHandler>();
var app = builder
    .AddInfrastructureSetup()
    .Build();
app.Services.SubscribeEvent<MessageSentEvent, MessageSentEventHandler>();
app.UseInfrastructure();