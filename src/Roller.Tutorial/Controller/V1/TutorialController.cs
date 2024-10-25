using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Roller.Infrastructure;
using Roller.Infrastructure.Attributes;
using Roller.Infrastructure.EventBus;

namespace Roller.Tutorial.Controller.V1;

[ApiVersion("1.0")]
[Route("tutorial/{version:apiVersion}")]
public class TutorialController(ILogger<TutorialController> logger, IEventBus eventBus) : RollerControllerBase
{
    [HttpGet("hello")]
    public async Task<MessageData<string>> HelloAsync()
    {
        logger.LogInformation("hello world");
        return await Task.Run(() => Success<string>("hello world"));
    }

    [HttpPost]
    [Idempotency(nameof(account))]
    public async Task<MessageData> TestAsync([FromBody] Account account)
    {
        return Success();
    }
    
    [HttpPost("send")]
    public IActionResult SendMessage([FromBody] string message)
    {
        eventBus.Publish(new MessageSentEvent { Message = message });
        return Ok("Message sent.");
    }
}

public class Account
{
    public string Username { get; set; }

    public string Password { get; set; }
}