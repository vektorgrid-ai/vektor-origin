using System.Text.Json;
using AssistantCore.Workers;
using AssistantCore.Workers.Dto;
using Microsoft.AspNetCore.Mvc;

namespace AssistantCore.Controllers;

[ApiController]
[Route("worker")]
public class WorkerController(WorkerRegistry registry) : ControllerBase
{
    [HttpPost("register")]
    public IActionResult RegisterWorker([FromBody] WorkerRegisterRequest request)
    {
        if (!Enum.TryParse<WorkerType>(request.WorkerType, ignoreCase: true, out var type))
            return BadRequest("Invalid worker type");
        
        var workerId = Guid.NewGuid().ToString();
        var descriptor = new WorkerDescriptor
        {
            Type = type,
            Endpoint = new Uri(request.Endpoint),
            WorkerId = workerId,
            Capabilities = new WorkerCapabilities
            {
                // TODO
            }
        };
        
        registry.Register(descriptor);
        
        var result = new WorkerRegisterResult
        {
            Accepted = true,
            WorkerId = workerId
        };
        return Ok(result);
    }

    [HttpPost("heartbeat")]
    public IActionResult Heartbeat([FromBody] WorkerHeartbeatRequest request)
    {
        registry.Heartbeat(request.WorkerId);
        return Ok();
    }

    [HttpGet("")]
    public IActionResult GetAllWorkers()
    {
        var workers = registry.GetAllWorkers().Select(WorkerToDto);
        return Ok(workers);
    }
    
    [HttpGet("alive")]
    public IActionResult GetAliveWorkers()
    {
        var workers = registry.GetAliveWorkers().Select(WorkerToDto);
        return Ok(workers);
    }
    
    [HttpGet("{workerId}")]
    public IActionResult GetWorker(string workerId)
    {
        var w = registry.GetWorker(workerId);
        if (w == null)
            return NotFound("Worker not found");
        
        var worker = WorkerToDto(w);
        return Ok(worker);
    }

    [HttpGet("tasks")]
    public IActionResult GetActiveTasks()
    {
        // TODO: keep track of tasks
        return NoContent();
    }

    private static object WorkerToDto(WorkerDescriptor w)
    {
        return new
        {
            worker_id = w.WorkerId,
            type = w.Type.ToString(),
            endpoint = w.Endpoint,
            last_seen = w.LastSeenUtc.ToString("O"),
            capabilities = new
            {
                supports_streaming = w.Capabilities.SupportsStreaming,
                supports_tools = w.Capabilities.SupportsTools,
                models = w.Capabilities.Models
            },
        };
    }
}