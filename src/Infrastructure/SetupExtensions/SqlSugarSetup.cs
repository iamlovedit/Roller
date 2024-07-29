using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Roller.Infrastructure.Options;
using Serilog;
using SqlSugar;
using SqlSugar.Extensions;

namespace Roller.Infrastructure.SetupExtensions;

public static class SqlSugarSetup
{
    public static void AddSqlSugarSetup(this IServiceCollection services, IConfiguration configuration,
        IWebHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(services);

        ArgumentNullException.ThrowIfNull(configuration);

        ArgumentNullException.ThrowIfNull(hostEnvironment);

        var sqlSugarOptions = configuration.GetSection(SqlSugarOptions.Name).Get<SqlSugarOptions>();

        if (!(sqlSugarOptions?.Enable ?? false))
        {
            return;
        }

        if (sqlSugarOptions.SnowFlake?.Enable ?? false)
        {
            var workerId = configuration["SNOWFLAKES_WORKERID"]?.ObjToInt() ?? sqlSugarOptions.SnowFlake?.WorkerId;
            ArgumentNullException.ThrowIfNull(workerId);
            SnowFlakeSingle.WorkId = (int)workerId;
        }

        var server = configuration["DB_HOST"] ?? sqlSugarOptions.Server;
        ArgumentException.ThrowIfNullOrEmpty(server);

        var port = configuration["DB_PORT"] ?? sqlSugarOptions.Port?.ToString();
        ArgumentException.ThrowIfNullOrEmpty(port);

        var database = configuration["DB_DATABASE"] ?? sqlSugarOptions.Database;
        ArgumentException.ThrowIfNullOrEmpty(database);

        var userId = configuration["DB_USER"] ?? sqlSugarOptions.UserId;
        ArgumentException.ThrowIfNullOrEmpty(userId);

        var password = configuration["DB_PASSWORD"] ?? sqlSugarOptions.Password;
        ArgumentException.ThrowIfNullOrEmpty(password);

        var connectionString = $"server={server};port={port};database={database};userid={userId};password={password};";

        var connectionConfig = new ConnectionConfig()
        {
            DbType = DbType.PostgreSQL,
            ConnectionString = connectionString,
            InitKeyType = InitKeyType.Attribute,
            IsAutoCloseConnection = true,
            MoreSettings = new ConnMoreSettings()
            {
                PgSqlIsAutoToLower = false,
                PgSqlIsAutoToLowerCodeFirst = false,
            }
        };

        var sugarScope = new SqlSugarScope(connectionConfig, config =>
        {
            config.QueryFilter.AddTableFilter<IDeleteable>(d => !d.IsDeleted);
            if (hostEnvironment.IsDevelopment() || hostEnvironment.IsStaging())
            {
                config.Aop.OnLogExecuting = (sql, parameters) => { Log.Logger.Information(sql); };
            }
        });
        services.AddSingleton<ISqlSugarClient>(sugarScope);
    }
}