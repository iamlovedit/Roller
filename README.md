# Roller

本项目基于.net8,基于以下第三方库，封装了常用的基础设施。

- Asp.Versioning.Mvc
- Asp.Versioning.Mvc.ApiExplorer
- AutoMapper
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.AspNetCore.Mvc.NewtonsoftJson
- Microsoft.Extensions.Configuration
- Newtonsoft.Json
- Serilog
- Serilog.AspNetCore
- Serilog.Sinks.Seq
- SqlSugarCore
- StackExchange.Redis
- Swashbuckle.AspNetCore

## 项目配置

### 跨域

```
"Cros": {
    "Enable": true,
    "PolicyName": "cros",
    "AllowAnyMethod": true,
    "AllowAnyHeader": true,
    "AllowAnyOrigin": true

},
```

### JWT

```
  "Audience": {
    "Enable": true,
    "Issuer": "test-issuer",
    "Audience": "test-audience",
    "Secret": "test-secret",
    "Duration": 3600
  },
```

### Redis

```
  "Redis": {
    "Enable": true,
    "InstanceName": "",
    "Host": "localhost",
    "Password": "password"
  },
```

### Version

```
  "Version": {
    "Enable": true,
    "HeaderName": "tutorial-api",
    "ParameterName": "tutorial-api",
    "SwaggerTitle": "tutorial-api"
  },
```

### SqlSugar

```
  "SqlSugar": {
    "Enable": true,
    "Server": "localhost",
    "Port": 3306,
    "Database": "test-db",
    "UserId": "root",
    "Password": "password",
    "SnowFlake": {
      "Enable": true,
      "WorkerId": 1
    }
  },
```

### Serilog

```
 "Serilog": {
    "Enable": true,
    "WriteFile": true,
    "SeqOptions": {
      "Enable": true,
      "Address": "localhost",
      "Secret": "test_secret"
    }
  }
```

## 环境变量

### SqlSugar

- DB_HOST
- DB_PORT
- DB_DATABASE
- DB_USER
- DB_PASSWORD
- SNOWFLAKES_WORKERID

### Redis

- REDIS_HOST
- REDIS_PASSWORD

### Seq

- SEQ_URL
- SEQ_APIKEY
- SEQ_ADMINPASSWORD

### Timezone

- TZ

### Aes

- AES_KEY

### JWT

- AUDIENCE_KEY
