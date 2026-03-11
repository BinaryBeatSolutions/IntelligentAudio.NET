
using IntelligentAudio.Contracts.Interfaces;
using IntelligentAudio.Contracts.Models;
using System.Diagnostics;

namespace IntelligentAudio.Dashboard.ViewModels;
public partial class NexusDashboardViewModel : ObservableObject
{
    private readonly MemoryMappedManager _nexusManager;
    private readonly NexusStorageEngine _storageEngine;
    private readonly System.Timers.Timer _refreshTimer;
    private readonly INexusProvider _nexusProvider;

    [ObservableProperty]
    private string _statusText = "NEXUS: READY";

    [ObservableProperty]
    private long _entryCount;

    // Konstruktor: Allt injiceras nu automatiskt av ServiceProvider
    public NexusDashboardViewModel(MemoryMappedManager nexusManager, NexusStorageEngine storageEngine, INexusProvider nexusProvider)
    {
        _nexusManager = nexusManager;
        _storageEngine = storageEngine;
        _nexusProvider = nexusProvider;

        _refreshTimer = new System.Timers.Timer(200);
        _refreshTimer.Elapsed += (s, e) => UpdateStats();
        _refreshTimer.Start();
    }

    [RelayCommand]
    public async Task TestSearchAsync()
    {
        // Leta efter exakt en av de nycklar vi nyss seedade (t.ex. nr 5000)
        var testKey = ParameterKey.Create(0, 1, 5000, 0);

        var sw = Stopwatch.StartNew();
        var resource = await _nexusProvider.ResolveResourceAsync(testKey);
        sw.Stop();

        if (resource.IsLocal)
            StatusText = $"FOUND! Offset: {resource.Offset} ({sw.Elapsed.TotalMilliseconds:F4}ms)";
        else
            StatusText = $"NOT FOUND (Key: {testKey.Value:X})"; // Visa hex-värdet om det skiter sig
    }

    [RelayCommand]
    public async Task SeedDataAsync()
    {
        StatusText = "SEEDING MASSIVE TEST DATA...";

        //// Vi använder den injicerade motorn direkt
        //await _storageEngine.SeedTestDataAsync();
        //EntryCount = _nexusManager.GetEntryCount();


        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _storageEngine.MassSeedTestDataAsync(10000);
        sw.Stop();


        //StatusText = "SEED COMPLETE: 1 SLOT ACTIVE";
        StatusText = $"DONE! 10k SLOTS IN {sw.ElapsedMilliseconds}ms";
    }


    private void UpdateStats()
    {
        // Vi läser direkt från MMF-vyn som managern håller i
        var index = _nexusManager.GetIndexSpan();

        // Uppdatera UI-tråden (CommunityToolkit sköter dispatching internt)
        EntryCount = _nexusManager.GetEntryCount();
    }
}