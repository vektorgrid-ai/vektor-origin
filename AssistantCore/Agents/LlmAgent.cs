using System.Text.Json;
using AssistantCore.Chat;
using AssistantCore.Tools;
using AssistantCore.Workers;
using AssistantCore.Workers.Dto.Impl;
using AssistantCore.Workers.LoadBalancing;

namespace AssistantCore.Agents;

public class LlmAgent(
    ILlmWorkerClient llmClient,
    ToolExecutor toolExecutor,
    ToolCollector toolCollector,
    WorkerRegistry registry,
    ILoadBalancer balancer,
    ILogger<LlmAgent> logger)
{
    private const int MaxTurns = 5;

    public async Task<string> ProcessAsync(
        string userText, 
        ChatManager chat, 
        LlmSpeciality speciality, 
        string areaContext, 
        CancellationToken token)
    {
        chat.AddEvent(new UserMessage(userText));

        var tools = toolCollector.GetToolsBySpeciality(speciality)
            .Select(t => t.ToDto())
            .ToArray();

        int turn = 0;
        while (turn++ < MaxTurns)
        {
            var candidates = registry.GetAliveWorkersOfType(WorkerType.Llm);
            var worker = balancer.Select(candidates, "llm");
            
            var input = new LlmRequest("0", 
                new LlmInput(turn == 1 ? userText : null, tools, chat.GetContext()), 
                new LlmConfig(4096, 0.2f), 
                new LlmContext(areaContext));
            
            var result = await llmClient.InferAsync(worker, input, token);
            var response = result.Output;
            
            if (response.ToolCalls != null && response.ToolCalls.Count != 0)
            {
                foreach (var call in response.ToolCalls)
                {
                    logger.LogInformation("Agent requested tool: {ToolName}", call.Function.Name);
                    chat.AddEvent(new ToolCall(call.Function.Name, call.Function.Arguments.GetRawText()));
                    
                    var toolResult = await toolExecutor.ExecuteAsync(call.Function.Name, call.Function.Arguments);
                    var jsonResult = JsonSerializer.Serialize(toolResult);
                    chat.AddEvent(new ToolResult(call.Function.Name, jsonResult));
                }
                continue;
            }
            
            if (!string.IsNullOrWhiteSpace(response.Text))
            {
                chat.AddEvent(new AssistantMessage(response.Text));
                return response.Text;
            }
            
            logger.LogWarning("LLM returned null text and no tools.");
            return "I'm having trouble thinking right now.";
        }

        return "I'm sorry, I got stuck in a thought loop.";
    }
}