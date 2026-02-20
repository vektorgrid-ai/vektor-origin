using System.Reflection;
using AssistantCore.Agents;
using AssistantCore.Tools;
using AssistantCore.Voice;
using AssistantCore.Workers;
using AssistantCore.Chat;
using AssistantCore.Companion;
using AssistantCore.Companion.Messaging;
using AssistantCore.Companion.Security;
using AssistantCore.Database;
using AssistantCore.Middleware;
using AssistantCore.Workers.LoadBalancing;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

string? seqUrl = Environment.GetEnvironmentVariable("SEQ_URL");
bool useSeq = !string.IsNullOrEmpty(seqUrl);
bool useFileLogging = Environment.GetEnvironmentVariable("USE_FILE_LOGGING") == "true";
string logLevel = Environment.GetEnvironmentVariable("MINIMUM_LOG_LEVEL") ?? "Information";
string maximumAutoApproveLevel = Environment.GetEnvironmentVariable("MAXIMUM_AUTO_APPROVE_RISK_LEVEL") ?? "Medium";

if (!Enum.TryParse<LogEventLevel>(logLevel, true, out var minimumLevel))
    minimumLevel = LogEventLevel.Information;
if (!Enum.TryParse<RiskLevel>(maximumAutoApproveLevel, true, out var autoApproveLevel))
    autoApproveLevel = RiskLevel.Medium;

ToolExecutor.MaximumAutoApproveLevel = autoApproveLevel;

var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Is(minimumLevel)
    .Enrich.FromLogContext() // captures scoped properties
    .Enrich.WithProperty("Service", "AssistantCore");

// File sink as compact JSON (easy to parse)
if (useFileLogging)
{
    loggerConfig = loggerConfig.WriteTo.File(new CompactJsonFormatter(), path: "logs/assistantcore-.json",
        rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14);
}

if (useSeq)
    loggerConfig = loggerConfig.WriteTo.Seq(seqUrl!, apiKey: null, restrictedToMinimumLevel: minimumLevel);

var recentSink = new RecentLogSink();
loggerConfig = loggerConfig
    .WriteTo.Console()
    .WriteTo.Sink(recentSink);

Log.Logger = loggerConfig.CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    // Configure DbContext to use SQLite with a file
    builder.Services.AddDbContext<AssistantDbContext>(options =>
        options.UseSqlite("Data Source=assistant.db"));

    builder.Services.AddSingleton(provider =>
    {
        var logger = provider.GetService<ILogger<ToolCollector>>();
        var tc = new ToolCollector(logger!);
        tc.SetAssemblies([Assembly.GetExecutingAssembly()]);
        return tc;
    });
    builder.Services.AddSingleton<HttpClient>();
    builder.Services.AddSingleton(recentSink);
    builder.Services.AddSingleton<CompanionManager>();
    builder.Services.AddSingleton<ICompanionMessageHandler, FirebaseMessageHandler>();
    builder.Services.AddSingleton<RequestValidator>();
    builder.Services.AddSingleton(provider => ChatManager.Create(TimeSpan.FromMinutes(30)));
    builder.Services.AddSingleton<SatelliteManager>();
    builder.Services.AddSingleton<WorkerRegistry>();
    builder.Services.AddSingleton<ILoadBalancer, RoundRobinLoadBalancer>();

    builder.Services.AddSingleton<ISttWorkerClient, HttpSttWorkerClient>();
    builder.Services.AddSingleton<IRoutingWorkerClient, HttpRoutingWorkerClient>();
    builder.Services.AddSingleton<ILlmWorkerClient, HttpLlmWorkerClient>();
    builder.Services.AddSingleton<ITtsWorkerClient, HttpTtsWorkerClient>();

    builder.Services.AddSingleton<ToolExecutor>();
    builder.Services.AddSingleton<LlmAgent>();

    var app = builder.Build();
    
    // Apply migrations on startup
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AssistantDbContext>();
        dbContext.Database.Migrate();
    }

    // Register middleware for exception logging and request logging
    app.UseMiddleware<ExceptionLoggingMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    // Register system prompts from configuration using the host logger
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var promptsSection = builder.Configuration.GetSection("LLMConfig");
    LlmInfo.ParseConfig(promptsSection, logger);

    // Configure WebSockets
    app.UseWebSockets(new WebSocketOptions
    {
        KeepAliveInterval = TimeSpan.FromSeconds(30)
    });
    app.MapControllers();

    // Ensure we cancel any active voice pipelines when host is shutting down
    app.Lifetime.ApplicationStopping.Register(() =>
    {
        var mgr = app.Services.GetRequiredService<SatelliteManager>();
        var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        startupLogger.LogInformation("Application stopping: cancelling active satellite pipelines");
        mgr.CancelAllPipelines();
    });
    
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
