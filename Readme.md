# IntelligentAudio.NET

[.NET 10 LTS] | [C# 14] | [License: MIT] | [Ableton] | [FL-Studio] | [LOGIC PRO]

**IntelligentAudio.NET** is a high-performance, AI-driven engine designed to bridge the gap between
musical intent and DAW execution. Built on the **.NET 10 LTS** stack, it utilizes state-of-the-art
speech-to-chord inference to provide hands-free control for music producers.

---

## Architectural Excellence

    This version represents a complete architectural rebuild, moving away from legacy Reflection-based systems to a modern,
    decoupled **Event-Driven Architecture**:

    - Zero-Allocation Pipeline: Leveraging `Span<float>` and `Memory<T>` for real-time audio cleaning without Garbage Collector spikes.
    - Asynchronous Orchestration: Powered by `System.Threading.Channels` for non-blocking communication between the Microphone, AI Engine, and Network layers.
    - DAW-Agnostic Design: A Factory-based driver system (`IDawClient`) that currently supports Ableton Live via OSC, with a roadmap for FL Studio (MIDI) and Logic Pro (VST Bridge).
    - In-Memory Event Bus: A `DefaultEventAggregator` that decouples AI analysis from hardware delivery, ensuring a "No-Bug" environment.

    IntelligentAudio.NET uses a Dynamic Discovery Architecture. 
    Instead of hardcoding ports, clients announce themselves via a handshake protocol. 
    The server dynamically spins up dedicated UDP transmitters for each instance, ensuring perfect isolation and zero configuration for the end user."

---

## Technical Stack

AI Inference: [Whisper.net](https://github.com) (Optimized for .NET 10).

Network Layer: [BuildSoft.OscCore](https://github.com) (High-performance, low-latency OSC).

Audio I/O: [NAudio](https://github.com) (Windows ASIO/Wasapi) with an abstracted `IAudioSource` for future macOS support.

Music Theory: [DryWetMidi](https://github.com).

---

## Project Structure

- **`IntelligentAudio.Contracts`**: The "Truth" (Interfaces, Events, Models). No external dependencies.
- **`IntelligentAudio.Engine`**: The "Brain" (Audio Processing, AI Orchestration, Event Aggregation).
- **`IntelligentAudio.Infrastructure`**: The "Hands" (OSC Clients, MIDI, Audio Hardware Drivers).
- **`IntelligentAudio.Server`**: The Composition Root (Dependency Injection, Hosting).

---

## Roadmap

    .NET 10 Core Engine & Pipeline
    Ableton Live OSC Driver
    FL Studio MIDI Scripting Driver
    VST3 Bridge for Logic Pro/Cubase
    macOS CoreAudio Implementation

---

## Getting Started

### Requirements

- **.NET 10 SDK** (Long Term Support).
- A Whisper GGML model (e.g., `base.bin`).
