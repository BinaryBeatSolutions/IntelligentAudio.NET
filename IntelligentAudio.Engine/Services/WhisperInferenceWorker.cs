using Whisper.net;

namespace IntelligentAudio.Engine.Services;

public sealed class WhisperInferenceWorker(
    IEnumerable<IIntentHandler> intentHandlers, // Hämtar alla (t.ex. MusicTheoryHandler)
    DefaultWhisperModelService aiService,
    AudioPipeline pipeline,
    IAudioProcessorFactory filterFactory,
    IAudioInput audioInput,
    ILogger<WhisperInferenceWorker> logger) : BackgroundService
{
    private readonly ArrayPool<float> _pool = ArrayPool<float>.Shared;
    private IAudioProcessor? _highPass;

    public void OnRecordingStarted(FilterType selectedType, float cutoff)
    {
        int rate = audioInput.SampleRate;
        _highPass = filterFactory.CreateHighPassFilter(FilterType.Butterworth24dB, 80f, rate);
    }

    public void OnAudioBufferReceived(Span<float> buffer)
    {
        _highPass?.Process(buffer);
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await aiService.EnsureModelReadyAsync(WhisperModelType.Tiny, ct);
        using var processor = aiService.CreateProcessor();

        const int sessionSize = 48000;
        float[] sessionBuffer = _pool.Rent(sessionSize);
        int currentPos = 0;

        try
        {
            await foreach (var audioBuffer in pipeline.Reader.ReadAllAsync(ct))
            {
                int toCopy = Math.Min(audioBuffer.Length, sessionSize - currentPos);
                Array.Copy(audioBuffer, 0, sessionBuffer, currentPos, toCopy);
                currentPos += toCopy;
                _pool.Return(audioBuffer);

                if (currentPos >= sessionSize)
                {
                    // Whisper processar 3-sekundersfönstret
                    await foreach (var result in processor.ProcessAsync(sessionBuffer.AsMemory(0, sessionSize), ct))
                    {
                        var text = result.Text.Trim();
                        if (string.IsNullOrWhiteSpace(text)) continue;

                        logger.LogInformation("[WHISPER RAW]: {text}", text);

                        // Vi frågar alla handlare (t.ex. MusicTheoryHandler) 
                        foreach (var handler in intentHandlers)
                        {
                            if (handler.CanHandle(text))
                            {
                                // Körs asynkront så vi inte blockerar nästa ljudsegment
                                await handler.HandleAsync(text, ct);
                            }
                        }
                        // ----------------------------
                    }

                    currentPos = 0;
                    Array.Clear(sessionBuffer, 0, sessionSize);
                }
            }
        }
        finally
        {
            _pool.Return(sessionBuffer);
        }
    }
}