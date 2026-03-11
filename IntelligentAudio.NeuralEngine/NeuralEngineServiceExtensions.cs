
using IntelligentAudio.Contracts;
using IntelligentAudio.Contracts.Interfaces;
using IntelligentAudio.NeuralEngine.Handlers;

namespace IntelligentAudio.NeuralEngine;

public static class NeuralEngineServiceExtensions
{
    public static IServiceCollection AddNeuralHarmonicEngine(this IServiceCollection services)
    {

        services.AddSingleton<IIntentHandler, NeuralIntentHandlerImpl>();

        // 1. Registrera Modell-tjänsten som äger sökvägarna
        services.AddSingleton<INeuralModelService, DefaultNeuralModelServiceImpl>();

        // 2. Tokenizer hämtar VocabPath från Modell-tjänsten
        services.AddSingleton<INeuralTokenizer>(sp => {
            var modelService = sp.GetRequiredService<INeuralModelService>();
            return new DefaultNeuralTokenizerImpl(modelService.VocabPath);
        });

        // 3. Embedding Generator hämtar ModelPath från Modell-tjänsten
        services.AddSingleton<IEmbeddingGenerator>(sp => {
            var modelService = sp.GetRequiredService<INeuralModelService>();
            return new DefaultEmbeddingGeneratorImpl(modelService.ModelPath);
        });

        // 4. Resten av motorn...
        services.AddSingleton<NeuralParameterRegistry>(sp =>
            new NeuralParameterRegistry(sp.GetRequiredService<IEmbeddingGenerator>().VectorSize));

        services.AddSingleton<NeuralOscActionHandler>();
        services.AddSingleton<DiscoveryService>();
        services.AddHostedService<NeuralBackgroundWorker>();

        return services;
    }
}