# Router Memory Persistence Architecture

> **Status: Partially implemented.** The JSON persistence layer described in "What's actually
> built" below is real and matches `src/AgenticRouter/Router/{IRouterMemoryStore,
> JsonRouterMemoryStore, RouterMemory}.cs`. The richer design in "Proposed future extensions"
> (schema versioning, backups, compaction, an embedding-based vector store) is **not implemented**
> — the current "vector store" is an in-memory Jaccard-similarity approximation, not a real vector
> database.

## What's actually built

The router learns from each observation and can persist that learning across process restarts
via `IRouterMemoryStore`:

```csharp
public interface IRouterMemoryStore
{
    Task<ConcurrentDictionary<string, ConcurrentDictionary<string, List<double>>>> LoadAsync();
    Task SaveAsync(ConcurrentDictionary<string, ConcurrentDictionary<string, List<double>>> memory);
}
```

The memory shape is a plain nested dictionary: `dimension -> model -> list of observed scores`.
There is no schema version field, no metadata, no variance/average precomputation — just raw
score lists; `RouterMemory` (below) computes averages on read.

### `JsonRouterMemoryStore` (the default, registered via DI)

- Reads/writes the whole dictionary as indented JSON to a single file, `RoutingOptions.MemoryPath`
  (default `"router_memory.json"`, resolved relative to `AppContext.BaseDirectory` unless the
  configured path is already rooted).
- **No backups, no atomic temp-file-then-rename, no compaction, no schema versioning.** A write is
  a direct `File.WriteAllTextAsync` overwrite of the target file.
- If the file is missing or fails to parse, `LoadAsync` logs and returns an **empty** memory — there
  is no backup file to fall back to.
- Registered as the default `IRouterMemoryStore` in `Hosting/ServiceCollectionExtensions.cs`:
  `services.AddSingleton<IRouterMemoryStore, JsonRouterMemoryStore>();`

### `VectorStoreRouterMemoryStore` (implemented, but not used by default)

Implements the same `IRouterMemoryStore` contract, but purely in-process — `LoadAsync`/`SaveAsync`
read and write an in-memory dictionary (deep-copied under a lock) with **no disk persistence at
all**. It additionally exposes:

```csharp
Task<IReadOnlyList<(string Model, double Score)>> FindSimilarAsync(
    string taskDescription, int topK = 5, CancellationToken cancellationToken = default);
```

"Similarity" here is **Jaccard token overlap** between the query text and each stored dimension
name (splitting on whitespace/punctuation, lowercasing, set intersection ÷ union) — not an
embedding model, not cosine similarity, and not backed by Milvus, Weaviate, or SQLite. It exists
and has test coverage (`src/AgenticRouter.Tests/Router/VectorStoreRouterMemoryStoreTests.cs`) but
is not wired into DI, so nothing uses it by default today.

### `RouterMemory` (the facade)

```csharp
public class RouterMemory
{
    public Task InitializeAsync();                                    // loads from the store once, at startup
    public Task AddScoreAsync(string dimension, string model, double score);
    public double? GetAverageScore(string dimension, string model);
    public IEnumerable<string> GetModelsForDimension(string dimension);
}
```

`AddScoreAsync` appends the score in-memory, then **synchronously calls `SaveAsync` with the full
dictionary on every single observation** — there is no debounced/periodic async save, no
`AutoSaveIntervalMs`. Every recorded score is an immediate full-file rewrite.

### Configuration

The only configuration surface is `RoutingOptions.MemoryPath` (default `"router_memory.json"`).
There is no `MemorySettings` section, no `EnableBackups`, `BackupRetentionDays`,
`CompactIntervalHours`, or `VectorStore*` keys anywhere in `appsettings.json` — none of that
configuration is read by any code.

## Proposed future extensions

The following was the original design for this feature and remains a reasonable roadmap, but
**none of it exists in code today**. If implementing any of these, update the corresponding
section above and remove the caveat.

- **Schema versioning + richer per-model stats**: a `version`/`lastUpdated`/`metadata` envelope
  around the score data, with precomputed `average`/`variance` per model instead of raw score
  lists recomputed on read.
- **Atomic writes + backups**: write to a temp file and rename over the target (crash-safe),
  timestamped `.bak` copies before each save with a retention window, and fallback to the most
  recent backup on load failure instead of silently starting empty.
- **Compaction**: a periodic background service that caps stored scores per model (e.g. keep the
  most recent N) once a threshold is crossed, to bound file size and load time.
- **A real vector store**: an `IVectorStoreRouterMemoryStore` abstraction with pluggable backends
  (Milvus for production-scale semantic search, Weaviate, or an embedded SQLite+vector-extension
  option), replacing the current Jaccard-overlap approximation with actual embedding similarity.
- **`MemorySettings` configuration section**: `PersistencePath`, `JsonMemoryFile`,
  `AutoSaveIntervalMs`, `CompactThresholdScoresPerModel`, `CompactIntervalHours`, `EnableBackups`,
  `BackupRetentionDays`, `VectorStoreEnabled`, `VectorStoreType`, `VectorStoreConnection`,
  `EmbeddingDimension`, `VectorStoreTopK`.
