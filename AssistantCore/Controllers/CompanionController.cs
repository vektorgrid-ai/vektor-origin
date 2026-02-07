using AssistantCore.Companion.Dto;
using Microsoft.AspNetCore.Mvc;

namespace AssistantCore.Controllers;

[ApiController]
[Route("companion")]
public class CompanionController(ILogger<CompanionController> logger) : ControllerBase
{
    [HttpPost("register_token")]
    public async Task<IActionResult> RegisterNewToken([FromBody] NewTokenRequest request)
    {
        // TODO: Implement token registration logic
        return Ok("Token updated successfully");
    }

    [HttpPost("answer_request")]
    public async Task<IActionResult> AnswerRequest([FromBody] ToolAnswer request)
    {
        // TODO: Forward answer to companion handler
        return Ok("Answer received");
    }
}