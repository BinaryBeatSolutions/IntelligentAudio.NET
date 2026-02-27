Projektstruktur (Solution)

## IntelligentAudio.Contracts (Klassbibliotek)

    Detta är "Single Source of Truth". Inga beroenden utåt, bara definitioner.
    Interfaces: IPlugin, IClient, IAudioProcessor.
    Events: ChordDetectedEvent, ClientRegisteredEvent.
    Models: ChordInfo (Namn, Toner, Confidence).

## IntelligentAudio.Engine (Klassbibliotek)

    Här bor "hjärnan". Detta projekt känner till Contracts men inte din ConsoleApp.

    AudioPipeline.cs: Hanterar din Channel<float[]>.
    Processing/: NoiseGateProcessor.cs, HighPassProcessor.cs.
    Intelligence/: WhisperService.cs (Själva AI-logiken).
    Coordination/: BroadcastService.cs (Mappar events till rätt klienter).

## IntelligentAudio.Infrastructure (Klassbibliotek)

    Här bor tekniken som pratar med omvärlden.

    Audio/: MicrophoneSource.cs (NAudio/ASIO-implementation).
    Communication/: OscClient.cs (OscCore-implementation), ClientFactory.cs.
    Midi/: MidiOutputService.cs.

## IntelligentAudio.Server (Console App / Worker Service)

    Här knyter du ihop allt (Composition Root).

    Program.cs: Här konfigurerar du ServiceCollection (DI).
    appsettings.json: Portar, IP-adresser, Whisper-modellstigar.