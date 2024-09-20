using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Roller.Infrastructure;

namespace Roller.Tutorial.Controller.V2;

[ApiVersion("2.0")]
[Route("tutorial/{version:apiVersion}")]
public class TutorialController(ILogger<TutorialController> logger) : RollerControllerBase
{
    [HttpGet("hello")]
    public async Task<MessageData<string>> HelloAsync()
    {
        logger.LogInformation("hello world");
        return await Task.Run(() => Success<string>("hello world"));
    }
}