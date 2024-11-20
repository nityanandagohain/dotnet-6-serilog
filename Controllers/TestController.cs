using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("This is a test log message.");
        _logger.LogWarning("This is a warning log.");
        _logger.LogError("This is an error log.");
        return Ok(new { message = "Check your logs!" });
    }
}
