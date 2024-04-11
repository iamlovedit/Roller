using Infrastructure.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SqlSugar;
using SqlSugar.Extensions;

namespace Infrastructure.SetupExtensions;

public static class SqlSugarSetup
{
    public static void AddSqlSugarSetup(this IServiceCollection services, IConfiguration configuration,
        IWebHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(services);

        ArgumentNullException.ThrowIfNull(configuration);

        ArgumentNullException.ThrowIfNull(hostEnvironment);

        var sqlSugarOptions = configuration.GetSection(SqlSugarOptions.Name).Get<SqlSugarOptions>();


        SnowFlakeSingle.WorkId = configuration["SNOWFLAKES_WORKERID"]?.ObjToInt() ??
                                 throw new ArgumentNullException("Snowflakes workerid is null");
        var connectionString =
            $"server={configuration["POSTGRESQL_HOST"]};" +
            $"port={configuration["POSTGRESQL_PORT"]};" +
            $"database={configuration["POSTGRESQL_DATABASE"]};" +
            $"userid={configuration["POSTGRESQL_USER"]};" +
            $"password={configuration["POSTGRESQL_PASSWORD"]};";

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
            config.QueryFilter.AddTableFilter<IDeletable>(d => !d.IsDeleted);
            if (hostEnvironment.IsDevelopment() || hostEnvironment.IsStaging())
            {
                config.Aop.OnLogExecuting = (sql, parameters) => { Log.Logger.Information(sql); };
            }
        });

        services.AddSingleton<ISqlSugarClient>(sugarScope);
    }
}