
using IntelligentAudio.Contracts.Interfaces;
using IntelligentAudio.Infrastructure.Services;
using IntelligentAudio.Nexus;
using IntelligentAudio.Nexus.Extensions;
using IntelligentAudio.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace IntelligentAudio.Dashboard
{
    public partial class App : Application
    {
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // 1. Skapa containern (Din manuella builder)
                var services = new ServiceCollection();

                // 2. Registrera tjänsterna (Samma som i Servern!)
                services.AddHttpClient();

                // Vi kör Null-providern här för att Dashboarden ska vara lättviktig
                services.AddSingleton<ICloudProvider, VercelCloudProviderImpl>();

                services.AddLogging();

                var nexusDllPath = Path.Combine(AppContext.BaseDirectory, "IntelligentAudio.Nexus.dll");

                if (File.Exists(nexusDllPath))
                {
                    // Init NEXUS
                    services.AddNexusEngine();
                }
                else
                {
                    services.AddSingleton<INexusProvider, MockNexusProvider>();
                }

                // Registrera din NEXUS-stack

                services.AddSingleton<NexusStorageEngine>();
                services.AddSingleton<MemoryMappedManager>();

                // Registrera din ViewModel
                services.AddSingleton<NexusDashboardViewModel>();

                // 3. Bygg providern
                var serviceProvider = services.BuildServiceProvider();

                // 4. Initialisering (Viktigt: Samma ordning som i Servern)
                var storage = serviceProvider.GetRequiredService<NexusStorageEngine>();
                var manager = serviceProvider.GetRequiredService<MemoryMappedManager>();

                if (File.Exists(NexusStorageEngine.FilePath))
                {
                    manager.Initialize(NexusStorageEngine.FilePath);
                }

                // 5. Starta fönstret med injicerad ViewModel
                var vm = serviceProvider.GetRequiredService<NexusDashboardViewModel>();
                desktop.MainWindow = new MainWindow { DataContext = vm };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}