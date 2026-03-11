# 💠 ARCHITECTURE.md: IntelligentAudio.NET

**Project Vision:** A zero-latency, AI-driven Neural Audio Engine with Hybrid-Cloud Synchronization.
**Last Updated:** March 2026 (NEXUS v1.0 Validation)

---

## 1. Core Principles (The "NANO" Standard)

- **Zero-Allocation:** No Heap allocations in the audio/parameter hot paths.
- **Zero-Latency IPC:** Inter-process communication via Memory-Mapped Files (MMF).
- **Binary-First:** All parameter data is stored in raw binary structs (No JSON/XML).
- **O(log n) Scalability:** Constant-time performance whether handling 10 or 1,000,000 parameters.

---

## 2. NEXUS: The Hybrid-Cloud Parameter Engine

NEXUS is the project’s central nervous system, mapping VST/AU parameters to a global cloud registry.

### 2.1 Storage Architecture (The Slack Space)

- **File:** `NexusIndex.ian` (Pre-allocated 24.0 MB).
- **Location:** `%LocalAppData%\IntelligentAudio.NET\Cache\`.
- **Memory Layout:**
  - `[0-63]`: **Registry Header** (MagicBytes: 8, Version: 4, EntryCount: 8).
  - `[64-24,000,064]`: **Index Table** (1,000,000 slots of `RegistryEntry`).
  - `[Remainder]`: **Initial Data Buffer** (1MB) for transient parameter blobs.

### 2.2 Memory Management (Safe vs. Unsafe)

- **Infrastructure Layer (Safe):** `NexusStorageEngine` handles Disk-I/O via `FileStream` using `FileShare.ReadWrite`. This ensures OS-level integrity and atomic updates to the `.ian` file.
- **Nexus Core Layer (Unsafe):** `MemoryMappedManager` uses `unsafe` pointers to map the file directly into process RAM. This provides a "Zero-Copy" view of the registry across different processes (Server & Dashboard).

### 2.3 Search & Retrieval (O(log n))

- All lookups are performed via **Binary Search** on a `ReadOnlySpan<RegistryEntry>`.
- **Constraint:** The Index Table **MUST** remain sorted by `ParameterKey.Value` (ulong) to guarantee O(log n) performance (~20 comparisons for 1M entries).

---

## 3. Dashboard: The Monitoring Layer

- **Tech Stack:** Avalonia 11.3.8 UI + .NET 10 (Native AOT Compatible).
- **Design Pattern:** MVVM via `CommunityToolkit.Mvvm` (Source Generated properties).
- **Logic:** The Dashboard does not "own" the data. It polls the MMF Header (Offset 12) every 200ms using raw pointer reads to display the live `EntryCount`.

---

## 4. Cloud Integration (Vercel Edge)

- **Transport:** HTTP/3 (QUIC/UDP) for 0-RTT handshakes via a dedicated `VercelHttpClientFactory`.
- **Sync Model:**
  1. Local Lookup (MMF) -> If `IsLocal == true` (Return Offset).
  2. Cloud Fallback -> Fetch via Vercel Edge Config -> Patch local `.ian` file -> Notify Engine.

---

## 5. Critical Instructions for AI Agents

- **NEVER** use `LINQ` or `foreach` on the 1M entry index. Use `BinarySearch` with a `struct` comparer.
- **NEVER** lock the `.ian` file exclusively. Always use `FileShare.ReadWrite`.
- **ALWAYS** preserve the `RegistryEntry` struct alignment (24 bytes: 8 Key, 8 Offset, 8 Length).
- **ALWAYS** treat the `AudioPipeline` and `NexusProvider` as performance-critical zones where `Task` allocations should be avoided; use `ValueTask` or `ValueTask<T>`.
