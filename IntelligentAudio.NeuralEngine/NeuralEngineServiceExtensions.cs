
using IntelligentAudio.Contracts.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace IntelligentAudio.NeuralEngine;

public static class NeuralEngineServiceExtensions
{
    public static IServiceCollection AddNeuralHarmonicEngine(this IServiceCollection services)
    {
        // 1. Definiera den fasta bas-sökvägen internt
        string commonPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "IntelligentAudio.NET");

        string modelPath = Path.Combine(commonPath, "Models", "NeuralHarmonic", "embeddings.onnx");
        string vocabPath = Path.Combine(commonPath, "Models", "NeuralHarmonic", "vocab.txt");

        // 2. Registrera Tokenizer (Infrastructure)
        services.AddSingleton<INeuralTokenizer>(sp =>
            new DefaultNeuralTokenizerImpl(vocabPath));

        // 3. Registrera Embedding Generator (Infrastructure)
        services.AddSingleton<IEmbeddingGenerator>(sp =>
            new DefaultEmbeddingGeneratorImpl(modelPath));

        // 4. Registrera Registry & Discovery (Internal Logic)
        services.AddSingleton<NeuralParameterRegistry>(sp =>
            new NeuralParameterRegistry(sp.GetRequiredService<IEmbeddingGenerator>().VectorSize));

        services.AddSingleton<DiscoveryService>();

        // 5. Starta Bakgrundsarbetaren (The Brain)
        services.AddHostedService<NeuralBackgroundWorker>();

        return services;
    }
}