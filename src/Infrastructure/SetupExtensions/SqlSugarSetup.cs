using Microsoft.AspNetCore.Hosting;
using Roller.Infrastructure.Options;
using Roller.Infrastructure.Repository;
using Serilog;
using SqlSugar;
using SqlSugar.Extensions;

namespace Roller.Infrastructure.SetupExtensions;

public static class SqlSugarSetup
{
    public static IServiceCollection AddSqlSugarSetup(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment hostEnvironment,
        List<ConnectionConfig>? connectionConfigs = null,
        Action<object, DataFilterModel>? onDataChanging = null,
        Action<DiffLogModel>? onDiffLogEvent = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        ArgumentNullException.ThrowIfNull(configuration);

        ArgumentNullException.ThrowIfNull(hostEnvironment);

        var sqlSugarOptions = configuration.GetSection(SqlSugarOptions.Name).Get<SqlSugarOptions>();
        if (sqlSugarOptions is null || !sqlSugarOptions.Enable)
        {
            return services;
        }

        services.TryAddScoped(typeof(IRepositoryBase<,>), typeof(RepositoryBase<,>));
        services.TryAddScoped<IUnitOfWork, UnitOfWork>();
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

        connectionConfigs ??= [];
        connectionConfigs.Add(new ConnectionConfig()
        {
            DbType = DbType.PostgreSQL,
            ConnectionString = connectionString,
            InitKeyType = InitKeyType.Attribute,
            IsAutoCloseConnection = true,
            MoreSettings = new ConnMoreSettings()
            {
                PgSqlIsAutoToLower = false,
                PgSqlIsAutoToLowerCodeFirst = false,
            },
            ConfigureExternalServices = new ConfigureExternalServices()
            {
                EntityService = (p, c) =>
                {
                    if (c.IsPrimarykey == false &&
                        new NullabilityInfoContext().Create(p).WriteState is NullabilityState.Nullable)
                    {
                        c.IsNullable = true;
                    }

                    c.DbColumnName = UtilMethods.ToUnderLine(c.DbColumnName);
                },
                EntityNameService = (t, e) => { e.DbTableName = UtilMethods.ToUnderLine(e.DbTableName); }
            }
        });
        var sugarScope = new SqlSugarScope(connectionConfigs, client =>
        {
            client.QueryFilter.AddTableFilter<IDeletable>(d => !d.IsDeleted);
            if (hostEnvironment.IsDevelopment() || hostEnvironment.IsStaging())
            {
                client.Aop.OnLogExecuted = (sql, parameters) =>
                {
                    var elapsed = client.Ado.SqlExecutionTime.TotalSeconds;
                    Log.Logger.Information("sql: {sql}  elapsed: {time} seconds", sql, elapsed);
                };
            }

            client.Aop.DataExecuting = onDataChanging;
            client.Aop.OnDiffLogEvent = onDiffLogEvent;
        });
        services.AddSingleton<ISqlSugarClient>(sugarScope);
        return services;
    }
}