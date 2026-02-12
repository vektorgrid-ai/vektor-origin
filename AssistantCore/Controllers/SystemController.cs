using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace AssistantCore.Controllers;

[ApiController]
[Route("")]
public class SystemController(
    RecentLogSink recentLogs,
    ILogger<SystemController> logger) : ControllerBase
{
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        var now = DateTime.UtcNow;
        var process = Process.GetCurrentProcess();
        var processStart = process.StartTime.ToUniversalTime();
        var uptime = DateTime.UtcNow - processStart;

        // CPU: approximate process CPU usage since process start
        var totalCpuMs = process.TotalProcessorTime.TotalMilliseconds;
        var cpuCount = Environment.ProcessorCount;
        double cpuPercentSinceStart = 0;
        if (uptime.TotalMilliseconds > 0)
            cpuPercentSinceStart = (totalCpuMs / uptime.TotalMilliseconds) * 100.0 / cpuCount;

        // Memory: process working set and attempt to read system total memory (platform-specific)
        var workingSet = process.WorkingSet64;
        var (systemTotalMemory, memoryPercentOfSystem) = GetSystemMemoryInfo(workingSet, logger);

        var health = new
        {
            status = "ok",
            timestamp = now,
            uptime = new
            {
                process_start_utc = processStart,
                uptime_seconds = (long)uptime.TotalSeconds,
                uptime = uptime.ToString()
            },
            server = new
            {
                pid = process.Id,
                cpu = new
                {
                    cpu_count = cpuCount,
                    total_processor_time_ms = (long)totalCpuMs,
                    cpu_percent_since_start = Math.Round(cpuPercentSinceStart, 2)
                },
                memory = new
                {
                    working_set_bytes = workingSet,
                    system_total_bytes = systemTotalMemory,
                    percent_of_system = memoryPercentOfSystem is double v ? Math.Round(v, 2) : (double?)null
                }
            },
            application = new
            {
                assembly = Assembly.GetEntryAssembly()?.GetName().Name,
                version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
            }
        };

        return Ok(health);
    }

    [HttpGet("logs")]
    public IActionResult GetLogs([FromQuery] int count = 100, [FromQuery] int offset = 0)
    {
        var snapshot = recentLogs.GetSnapshot()
            .Skip(offset)
            .Take(count)
            .Select(l => new
            {
                timestamp = l.Timestamp,
                level = l.Level.ToString(),
                message = l.RenderMessage(),
                exception = l.Exception?.ToString(),
                properties = l.Properties.ToDictionary(p => p.Key, p => p.Value.ToString())
            });
        return Ok(snapshot);
    }

    // Helper: determine total system memory and percent used by workingSet, using OS-specific checks
    private static (long? systemTotalMemory, double? memoryPercentOfSystem) GetSystemMemoryInfo(long workingSet, ILogger? logger)
    {
        long? systemTotalMemory = null;
        double? memoryPercentOfSystem = null;
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Read from /proc/meminfo on Linux
                if (System.IO.File.Exists("/proc/meminfo"))
                {
                    var meminfo = System.IO.File.ReadAllLines("/proc/meminfo");
                    foreach (var line in meminfo)
                    {
                        if (line.StartsWith("MemTotal:"))
                        {
                            // format: MemTotal:       16367456 kB
                            var parts = line.Split(':', StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length >= 2)
                            {
                                var tokens = parts[1].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                if (long.TryParse(tokens[0], out var kb))
                                {
                                    systemTotalMemory = kb * 1024;
                                }
                            }
                            break;
                        }
                    }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows/dev machines: use GC fallback as a conservative estimate
                logger?.LogDebug("HealthController: running on Windows - using GC.GetGCMemoryInfo() fallback for total memory");
                var gcInfo = GC.GetGCMemoryInfo();
                if (gcInfo.TotalAvailableMemoryBytes > 0)
                    systemTotalMemory = (long)gcInfo.TotalAvailableMemoryBytes;
            }
            else
            {
                // Other OS (macOS, etc.) - use GC fallback
                logger?.LogDebug("HealthController: running on unknown OS - using GC.GetGCMemoryInfo() fallback for total memory");
                var gcInfo = GC.GetGCMemoryInfo();
                if (gcInfo.TotalAvailableMemoryBytes > 0)
                    systemTotalMemory = (long)gcInfo.TotalAvailableMemoryBytes;
            }

            if (systemTotalMemory is > 0)
                memoryPercentOfSystem = (workingSet / (double)systemTotalMemory.Value) * 100.0;
        }
        catch (Exception ex)
        {
            logger?.LogDebug(ex, "Determining system memory failed");
        }

        return (systemTotalMemory, memoryPercentOfSystem);
    }
}