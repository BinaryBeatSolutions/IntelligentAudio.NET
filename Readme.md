# IntelligentAudio.NET

### .NET 10] | C# 14] | [License: MIT] | [Ableton Live]

*IntelligentAudio.NET** is a high-performance, AI-driven bridge between your voice and **Ableton Live**.
Built on the cutting-edge .NET 10 LTS stack, it transforms spoken intent and musical audio into real-time
DAW commands and MIDI data.

> "Stop clicking, start creating. Bridge the gap between musical intent and technical execution."

---

## Key Features

- **AI-Powered Inference**: Integrated with [Whisper.net](https://github.com) for state-of-the-art speech and chord recognition.
- **Zero-Allocation Pipeline**: Leverages `Span<float>` and C# 14 memory management for ultra-low latency audio processing.
- **Modular Architecture**: Decoupled Engine, Infrastructure, and Contracts projects for maximum scalability.
- **Real-time OSC Bridge**: Lightning-fast communication with Ableton Live via [OscCore](https://github.com).
- **Theory Intelligent**: Musical logic powered by [DryWetMidi](https://github.com).

---

## Architecture

The project follows a **Clean Architecture** pattern to ensure the core logic remains independent of external frameworks or hardware:

- **`IntelligentAudio.Contracts`**: Interfaces, Events, and DTOs (The "Truth").* **`IntelligentAudio.Contracts`**: Interfaces, Events, and DTOs (The "Truth").
- **`IntelligentAudio.Engine`**: The "Brain" (Processors, AI-orchestration, Audio Pipelines).* **`IntelligentAudio.Engine`**: The "Brain" (Processors, AI-orchestration, Audio Pipelines).
- **`IntelligentAudio.Infrastructure`**: The "Hands" (OSC, NAudio, MIDI implementations).* **`IntelligentAudio.Infrastructure`**: The "Hands" (OSC, NAudio, MIDI implementations).
- **`IntelligentAudio.Server`**: The Composition Root (Dependency Injection, Hosting).* **`IntelligentAudio.Server`**: The Composition Root (Dependency Injection, Hosting).

---

## Getting Started

### Prerequisites

1. **[.NET 10 SDK](https://dotnet.microsoft.com)** (LTS)
2. **Ableton Live** (11 or 12 recommended)
3. **Max for Live** (Included in Suite or as an add-on)

### Installation

1. **Clone the repository:**
    ```bash
    git clone https://github.com
    cd IntelligentAudio.NET
    ```

2. **Download Whisper Model:**
    Dev: Download a GGML model (e.g., `base.bin`) from [Hugging Face](https://huggingface.co) and place it in your model directory.
    In final production, this will be handled by Whisper it self.

3. **Configure `appsettings.json`:**
    Update the `ModelPath` and `Port` to match your local setup.

4. **Run the Server:**
    ```bash
    dotnet run --project src/IntelligentAudio.Server
    ```

5. **Setup Ableton:**
    Drag the `IntelligentAudio.amxd` (found in `/m4l`) onto a MIDI track in Ableton Live.

---

## Performance Highlights

- **`Span<T>` everywhere**: We process audio buffers without heap allocations to prevent Garbage Collector spikes during recording.
- **System.Threading.Channels**: Non-blocking audio streaming between the microphone source and the AI inference engine.
- **BackgroundService**: Fully asynchronous orchestration of I/O, AI, and Network tasks.

---

## Contributing

Contributions are welcome! Whether it's a new `IAudioProcessor` for audio cleaning or a new Ableton command, feel free to open a Pull Request.

## License

This project is licensed under the **MIT License**.

---

*Developed by **BinaryBeatSolutions** – Revolutionizing the workflow for the modern music producer.*
P-adresser, Whisper-modellstigar.