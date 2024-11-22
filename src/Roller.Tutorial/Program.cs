using Microsoft.Extensions.DependencyInjection.Extensions;
using Roller.Infrastructure.EventBus;
using Roller.Infrastructure.Middlewares;
using Roller.Infrastructure.SetupExtensions;
using Roller.Tutorial;
using Roller.Tutorial.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddInfrastructureSetup();
builder.Services.AddRollerRabbitMQEventBus(builder.Configuration);
builder.Services.AddTransient<MessageSentEventHandler>();
builder.Services.TryAddScoped<IPersonService, PersonService>();
var app = builder.Build();
app.UsePathBase("/test");
app.Services.SubscribeEvent<MessageSentEvent, MessageSentEventHandler>();
app.UseInfrastructure();