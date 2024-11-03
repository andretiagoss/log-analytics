using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace log_analytics.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class LogControllerController(ILogger<LogControllerController> logger) : ControllerBase
{
    [HttpGet]
    public IActionResult Get(LogLevel logLevel)
    {
        var user = User;

        var description = $"Test Log - {(int)logLevel} - {logLevel.ToString()}";
        logger.Log(logLevel, "Test Log - {LogLevel} - {LogDescription}", (int)logLevel, logLevel.ToString());

        return Ok(description);
    }

    [HttpGet("exception")]
    public IActionResult GenerateException()
    {
        throw new Exception("Test Log - Exception");
    }
}
