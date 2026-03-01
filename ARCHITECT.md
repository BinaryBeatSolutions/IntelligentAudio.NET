# AI Architecture Rules for IntelligentAudio.NET

## Core Tech Stack

- **Framework:** .NET 10 LTS (High-Performance profile)
- **Communication:** OSC (Open Sound Control) via UDP
- **Audio Engine:** Low-latency integration with Ableton Live / Logic
- **Key Libraries:** Whisper.net for AI Speech-to-Intent, **BuildSoft.OscCore** for OSC.

## Memory Management Strategy (STRICT)

- **Goal:** Zero-Allocation in the hot path.
- **Constraints:**
  - Use `Span<T>`, `ReadOnlySpan<T>`, and `Memory<T>` for all buffer handling.
  - NO LINQ in audio processing or OSC parsing pipelines.
  - Avoid boxing. Use generics with `where T : struct` where applicable.
  - Use `ArrayPool<T>` or `ObjectPool` for recurring objects.
  - Prohibit `foreach` on collections that cause heap allocation; use `for` or `Span` based iteration.

## Architectural Principles

1. **Decoupled Design:** DAW-specific logic must be abstracted via interfaces.

2. **Pipeline-Driven:** Use `System.IO.Pipelines` or `Channels` for data flow between modules.
3. **Event-Driven:** Use high-performance events or `ValueTask` to minimize heap pressure.
4. **Naming Convention:** Do NOT use words like "Simple" in class names. Use `Default[Name]Impl.cs` or specific descriptive names.

## Project Structure & Responsibilities

- **IntelligentAudio.Contracts**:

  - *Purpose:* Abstractions and shared types.
  - *Constraint:* **ZERO external dependencies**. Contains interfaces, `record struct`, and `const`.
- **IntelligentAudio.Infrastructure**:
  - *Purpose:* Technical implementation and I/O.
  - *Content:* Houses `BuildSoft.OscCore`. Implements non-allocating UDP listeners.
- **IntelligentAudioEngine**:
  - *Purpose:* The "Brain" and real-time pipeline.
  - *Content:* `Whisper.net`, `Span<T>` logic, chord detection, and transport control.
  - *Performance:* Critical .NET 10 optimizations occur here.
- **IntelligentAudio.Server**:
  - *Purpose:* Host, Configuration, and Composition Root.
  - *Content:* `Program.cs`, Dependency Injection setup, and service lifecycle management.

## Response Guidelines

- ALWAYS reason step-by-step (Chain-of-Thought).
- ALWAYS perform a "Latency & Allocation Analysis" before providing code.
- If a suggestion causes GC pressure, FLAG IT and suggest a non-allocating alternative.
- Prioritize C# 13 features (Ref structs, Interceptors, etc.).

## Library Specifics

- **OSC Library:** MUST use `BuildSoft.OscCore`.

- **Implementation Detail:** Focus on `OscServer` and `OscClient`. Use non-allocating methods for message parsing (e.g., `ReadValueTranscribe<T>`).
