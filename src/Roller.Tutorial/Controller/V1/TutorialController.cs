using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Roller.Infrastructure;

namespace Roller.Tutorial.Controller.V1;

[ApiVersion("1.0")]
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