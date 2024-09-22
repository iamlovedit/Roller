using Roller.Infrastructure.Middlewares;
using Roller.Infrastructure.SetupExtensions;

var builder = WebApplication.CreateBuilder(args);

var app = builder
    .AddInfrastructureSetup()
    .Build();

app.UseInfrastructure();