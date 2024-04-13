using Roller.Infrastructure.Middlewares;
using Roller.Infrastructure.SetupExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructureSetup();

var app = builder.Build();

app.UseInfrastructure();