﻿namespace Roller.Infrastructure.Security;

internal class RollerApiResponse
{
    public int Code { get; }

    public string Value { get; }

    public MessageData Message { get; }

    public RollerApiResponse(StatusCode code, string message = null)
    {
        switch (code)
        {
            case StatusCode.Code401:
                Code = 401;
                Value = "很抱歉，您无权访问该接口，请确保已经登录!";
                break;
            case StatusCode.Code403:
                Code = 403;
                Value = "很抱歉，您的访问权限等级不够，联系管理员!";
                break;
            case StatusCode.Code404:
                Code = 404;
                Value = "资源不存在!";
                break;
            case StatusCode.Code500:
                Code = 500;
                Value = message;
                break;
        }

        Message = new MessageData(code == StatusCode.Code200, Value);
    }
}

internal enum StatusCode
{
    Code200,
    Code401,
    Code403,
    Code404,
    Code500
}