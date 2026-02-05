using AssistantCore.Workers;
using AssistantCore.Workers.Dto;
using AssistantCore.Workers.Dto.Impl;

namespace CoreTests.Satellite.MockWorkers;

public class FakeTtsWorker : ITtsWorkerClient
{
    public Task<TtsResponse> InferAsync(WorkerDescriptor worker, TtsRequest input, CancellationToken ct)
    {
        return Task.FromResult(new TtsResponse("0", new WorkerUsage("fake-tts", 1), 
            new TtsOutput(new byte[SatelliteProtocol.AudioFrameSize * SatelliteProtocol.TtsFrames], 
                "pcm_s16le", 16000, 1), null));
    }
}