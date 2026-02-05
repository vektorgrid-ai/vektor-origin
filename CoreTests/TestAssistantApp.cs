using AssistantCore.Workers;
using AssistantCore.Workers.LoadBalancing;
using CoreTests.Satellite;
using CoreTests.Satellite.MockWorkers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public sealed class TestAssistantApp
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ISttWorkerClient>();
            services.RemoveAll<IRoutingWorkerClient>();
            services.RemoveAll<ILlmWorkerClient>();
            services.RemoveAll<ITtsWorkerClient>();
            services.RemoveAll<ILoadBalancer>();

            services.AddSingleton<ISttWorkerClient, FakeSttWorker>();
            services.AddSingleton<IRoutingWorkerClient, FakeRoutingWorker>();
            services.AddSingleton<ILlmWorkerClient, FakeGeneralLlmWorker>();
            services.AddSingleton<ITtsWorkerClient, FakeTtsWorker>();
            services.AddSingleton<ILoadBalancer, FakeLoadBalancer>();
        });
    }
}