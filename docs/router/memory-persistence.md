# Router Memory Persistence Architecture

This document describes how the ACRouter .NET implementation persists learned routing decisions across sessions using JSON file storage and optional vector store integration.

## Overview

The router learns from each observation (feedback) and improves its routing decisions. To persist this learning across application restarts, we implement a two-tier storage strategy:

1. **Primary Storage (JSON):** Simple, portable, human-readable disk persistence
2. **Optional Storage (Vector Store):** Semantic similarity queries for advanced routing decisions

## Layer 1: JSON File Storage (Always Enabled)

### Purpose
- Store router memory in a portable, human-readable JSON format
- Enable memory recovery after application restart
- Support easy debugging and memory inspection
- Eliminate need for external database for basic routing

### Schema Design

The JSON file follows this structure:

```json
{
  "version": "1.0",
  "lastUpdated": "2025-01-15T14:32:00Z",
  "dimensions": {
	"code_gen": {
	  "gpt-4-turbo": {
		"scores": [0.95, 0.92, 0.88, 0.91],
		"count": 4,
		"average": 0.915,
		"variance": 0.000875,
		"lastUpdated": "2025-01-15T14:31:55Z"
	  },
	  "gpt-4": {
		"scores": [0.88, 0.85, 0.87],
		"count": 3,
		"average": 0.867,
		"variance": 0.000133,
		"lastUpdated": "2025-01-15T14:30:00Z"
	  },
	  "claude-3-sonnet": {
		"scores": [0.79, 0.81, 0.80],
		"count": 3,
		"average": 0.8,
		"variance": 0.000067,
		"lastUpdated": "2025-01-15T14:15:00Z"
	  }
	},
	"bug_fix": {
	  "claude-3-sonnet": {
		"scores": [0.92, 0.94, 0.93],
		"count": 3,
		"average": 0.933,
		"variance": 0.000067,
		"lastUpdated": "2025-01-15T14:25:00Z"
	  },
	  "gpt-4-turbo": {
		"scores": [0.85, 0.88, 0.86],
		"count": 3,
		"average": 0.863,
		"variance": 0.000133,
		"lastUpdated": "2025-01-15T14:20:00Z"
	  }
	},
	"refactoring": {
	  "gpt-4": {
		"scores": [0.91, 0.92, 0.90],
		"count": 3,
		"average": 0.91,
		"variance": 0.000067,
		"lastUpdated": "2025-01-15T14:10:00Z"
	  }
	}
  },
  "metadata": {
	"totalRoutingDecisions": 47,
	"uniqueDimensions": 3,
	"explorationRate": 0.15,
	"lastMemoryCompaction": "2025-01-15T10:00:00Z",
	"compactionCount": 5
  }
}
```

### Key Fields Explained

| Field | Type | Purpose |
|-------|------|---------|
| `version` | string | Schema version for backward compatibility |
| `lastUpdated` | ISO 8601 | Timestamp of last memory modification |
| `dimensions` | object | Map of task dimensions to model performance scores |
| `dimensions[dim].model_name.scores` | array | Last N scores for this model in this dimension |
| `dimensions[dim].model_name.count` | int | Total observations for this model in this dimension |
| `dimensions[dim].model_name.average` | float | Arithmetic mean of all scores (current + historical) |
| `dimensions[dim].model_name.variance` | float | Variance of scores (for confidence/stability analysis) |
| `dimensions[dim].model_name.lastUpdated` | ISO 8601 | When this model was last scored in this dimension |
| `metadata.totalRoutingDecisions` | int | Cumulative routing decisions made |
| `metadata.uniqueDimensions` | int | Count of unique task dimensions encountered |
| `metadata.explorationRate` | float | Current epsilon-greedy exploration probability |
| `metadata.lastMemoryCompaction` | ISO 8601 | When memory was last compacted |

### File Operations

#### Load (Startup)
```csharp
public class JsonRouterMemoryStore : IRouterMemoryStore
{
	public async Task<RouterMemoryData> LoadAsync(CancellationToken ct = default)
	{
		// 1. Check if file exists
		if (!File.Exists(_filePath))
		{
			return new RouterMemoryData(); // return empty/default
		}

		try
		{
			// 2. Read JSON from disk
			var json = await File.ReadAllTextAsync(_filePath, ct);

			// 3. Parse and validate schema
			var data = JsonSerializer.Deserialize<RouterMemoryData>(json);

			// 4. Verify version compatibility
			if (data.Version != "1.0")
				throw new InvalidOperationException($"Unsupported memory version: {data.Version}");

			return data;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to load memory from {FilePath}", _filePath);

			// 5. Fallback: try backup file
			return await LoadFromBackupAsync(ct) ?? new RouterMemoryData();
		}
	}
}
```

#### Save (After Each Observation)
```csharp
public async Task SaveAsync(RouterMemoryData data, CancellationToken ct = default)
{
	try
	{
		// 1. Ensure directory exists
		Directory.CreateDirectory(Path.GetDirectoryName(_filePath));

		// 2. Create backup of current file (if it exists)
		if (File.Exists(_filePath) && _options.EnableBackups)
		{
			var backupPath = $"{_filePath}.{DateTime.UtcNow:yyyyMMdd_HHmmss}.bak";
			File.Copy(_filePath, backupPath, overwrite: false);

			// 3. Clean old backups (keep last N days)
			CleanOldBackups(_options.BackupRetentionDays);
		}

		// 4. Serialize and write atomically (to temp file first)
		var tempPath = _filePath + ".tmp";
		var json = JsonSerializer.Serialize(data, new JsonSerializerOptions 
		{ 
			WriteIndented = true 
		});
		await File.WriteAllTextAsync(tempPath, json, ct);

		// 5. Atomic rename (temp → main)
		File.Move(tempPath, _filePath, overwrite: true);

		_logger.LogDebug("Memory saved to {FilePath}", _filePath);
	}
	catch (Exception ex)
	{
		_logger.LogError(ex, "Failed to save memory to {FilePath}", _filePath);
		// Don't throw; log and continue (routing still works from in-memory cache)
	}
}
```

#### Compaction (Periodic)
```csharp
public async Task CompactAsync(int maxScoresPerModel, CancellationToken ct = default)
{
	try
	{
		lock (_updateLock)
		{
			// 1. Iterate all dimensions and models
			foreach (var dimension in _cache.Dimensions.Values)
			{
				foreach (var modelScores in dimension.Models.Values)
				{
					// 2. If more than maxScoresPerModel entries, summarize
					if (modelScores.Scores.Count > maxScoresPerModel)
					{
						// 3. Keep only recent N scores; compute aggregate stats
						var kept = modelScores.Scores.TakeLast(maxScoresPerModel).ToList();
						modelScores.Scores = kept;
						modelScores.Average = kept.Average();
						modelScores.Variance = CalculateVariance(kept);

						_logger.LogInformation(
							"Compacted {Model} in {Dimension}: kept {Count} scores, avg={Average:F3}",
							modelScores.Model, dimension.Name, kept.Count, modelScores.Average);
					}
				}
			}

			_cache.Metadata.LastMemoryCompaction = DateTime.UtcNow;
			_cache.Metadata.CompactionCount++;
		}

		// 4. Save compacted state to disk
		await _store.SaveAsync(_cache, ct);
	}
	catch (Exception ex)
	{
		_logger.LogError(ex, "Compaction failed");
	}
}
```

### Configuration

```json
{
  "MemorySettings": {
	"PersistencePath": "./data",
	"JsonMemoryFile": "router_memory.json",
	"AutoSaveIntervalMs": 5000,
	"CompactThresholdScoresPerModel": 100,
	"CompactIntervalHours": 24,
	"EnableBackups": true,
	"BackupRetentionDays": 7
  }
}
```

| Setting | Default | Purpose |
|---------|---------|---------|
| `PersistencePath` | `./data` | Directory where memory files are stored |
| `JsonMemoryFile` | `router_memory.json` | Filename for the main memory JSON |
| `AutoSaveIntervalMs` | `5000` | Interval for periodic async save (ms) |
| `CompactThresholdScoresPerModel` | `100` | Max scores kept per model before compaction |
| `CompactIntervalHours` | `24` | Frequency of automatic compaction |
| `EnableBackups` | `true` | Create timestamped backups before save |
| `BackupRetentionDays` | `7` | How long to keep old backup files |

## Layer 2: Vector Store (Optional)

### Purpose
- Enable semantic similarity queries ("find models for tasks similar to this one")
- Improve routing decisions through contextual similarity, not just exact dimension matching
- Support advanced retrieval strategies (e.g., "if this code question is similar to past refactoring questions, use the refactoring-preferred model")

### Implementation Options

#### Option A: In-Memory Vector Store (Development)
- **Pros:** Zero dependencies, fast, suitable for small deployments
- **Cons:** Memory-resident, no persistence, not distributed
- **Use Case:** Local development, demos, small teams

```csharp
public class InMemoryVectorStore : IVectorStoreRouterMemoryStore
{
	private List<(string TaskId, string[] Embedding, string Model, double Score)> _vectors;

	public async Task<IEnumerable<(string Model, double Score)>> FindSimilarAsync(
		string taskDescription, int topK = 5, CancellationToken ct = default)
	{
		// 1. Embed query text using local embedder (e.g., DensePassage, ONNX model)
		var queryEmbedding = _embedder.Encode(taskDescription);

		// 2. Compute cosine similarity to all stored vectors
		var similarities = _vectors
			.Select(v => (v.Model, Similarity: CosineSimilarity(queryEmbedding, v.Embedding)))
			.OrderByDescending(x => x.Similarity)
			.Take(topK);

		return similarities;
	}
}
```

#### Option B: Milvus (Production - Distributed Vector DB)
- **Pros:** Scalable, distributed, high performance, production-grade
- **Cons:** External service, requires Milvus deployment, operational overhead
- **Use Case:** Team deployments, shared routing across multiple services

```csharp
public class MilvusVectorStore : IVectorStoreRouterMemoryStore
{
	private readonly MilvusClient _client;

	public async Task<IEnumerable<(string Model, double Score)>> FindSimilarAsync(
		string taskDescription, int topK = 5, CancellationToken ct = default)
	{
		// 1. Embed task description
		var embedding = _embedder.Encode(taskDescription);

		// 2. Query Milvus for similar vectors
		var results = await _client.SearchAsync(
			collectionName: "routing_decisions",
			vectors: new[] { embedding },
			limit: topK,
			ct
		);

		// 3. Return top-K models with scores
		return results.Select(r => (
			Model: r.Entity["model"]?.ToString(),
			Score: r.Distance
		));
	}
}
```

#### Option C: Weaviate (Alternative - GraphQL Vector DB)
- **Pros:** GraphQL interface, flexible schema, semantic search
- **Cons:** External service, less mature than Milvus
- **Use Case:** Complex semantic queries, multi-field search

#### Option D: SQLite + Vector Extension
- **Pros:** Embedded, single file, no external service
- **Cons:** Lower performance than Milvus, limited distribution
- **Use Case:** Small team, development environment

### Vector Store Configuration

```json
{
  "MemorySettings": {
	"VectorStoreEnabled": false,
	"VectorStoreType": "Milvus",
	"VectorStoreConnection": "localhost:19530",
	"EmbeddingDimension": 768,
	"VectorStoreTopK": 5,
	"EmbeddingModel": "sentence-transformers/all-MiniLM-L6-v2"
  }
}
```

| Setting | Default | Purpose |
|---------|---------|---------|
| `VectorStoreEnabled` | `false` | Enable/disable vector store integration |
| `VectorStoreType` | `Milvus` | Storage backend type |
| `VectorStoreConnection` | `localhost:19530` | Connection string |
| `EmbeddingDimension` | `768` | Dimension of embeddings (must match model) |
| `VectorStoreTopK` | `5` | Number of similar results to retrieve |
| `EmbeddingModel` | `sentence-transformers/all-MiniLM-L6-v2` | Which embedding model to use |

### Usage in Routing

```csharp
public class ACRouter
{
	private readonly IRouterMemoryStore _jsonStore;
	private readonly IVectorStoreRouterMemoryStore _vectorStore; // optional

	public async Task<RouteDecision> RouteAsync(RoutingTask task)
	{
		// 1. Primary: Exact dimension lookup from JSON
		if (_memory.TryGetBestModel(task.Dimension, out var bestModel, out var score))
		{
			return new RouteDecision(bestModel, score, reasonRating: "Exact dimension match");
		}

		// 2. Secondary (if vector store enabled): Semantic similarity
		if (_vectorStore != null)
		{
			var similar = await _vectorStore.FindSimilarAsync(task.Description, topK: 3);
			if (similar.Any())
			{
				var recommendation = similar.First();
				_logger.LogInformation("Semantic routing: using {Model} (similarity: {Score:F2})", 
					recommendation.Model, recommendation.Score);

				return new RouteDecision(recommendation.Model, recommendation.Score, 
					reasonRating: $"Semantic similarity ({recommendation.Score:F2})");
			}
		}

		// 3. Fallback: Use cheap chain or escalate
		return DefaultRoute(task);
	}
}
```

## Integration with Router

### Load Memory on Startup
```csharp
public class Program
{
	public static async Task Main(string[] args)
	{
		var host = Host.CreateDefaultBuilder(args)
			.ConfigureServices((context, services) =>
			{
				services.AddSingleton<IRouterMemoryStore>(sp =>
				{
					var options = context.Configuration.GetSection("MemorySettings")
						.Get<MemoryOptions>();

					return new JsonRouterMemoryStore(options, sp.GetRequiredService<ILogger<JsonRouterMemoryStore>>());
				});

				services.AddSingleton<RouterMemory>(async sp =>
				{
					var store = sp.GetRequiredService<IRouterMemoryStore>();
					var data = await store.LoadAsync(); // Load on startup
					return new RouterMemory(data, store);
				});
			})
			.Build();

		await host.RunAsync();
	}
}
```

### Save Memory After Each Observation
```csharp
public class ProxyMiddleware
{
	private readonly RouterMemory _memory;

	public async Task InvokeAsync(HttpContext context, ACRouter router)
	{
		// ... intercept request, route, forward ...

		// Observe outcome
		var score = CalculateScore(response);
		await _memory.ObserveAsync(
			dimension: task.Dimension,
			model: decision.ChosenModel,
			score: score
		);

		// Memory is saved asynchronously (non-blocking)
	}
}
```

### Periodic Compaction
```csharp
public class MemoryCompactionHostedService : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var interval = _options.CompactIntervalHours;
				await Task.Delay(TimeSpan.FromHours(interval), stoppingToken);

				await _store.CompactAsync(_options.CompactThresholdScoresPerModel, stoppingToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Compaction failed");
			}
		}
	}
}
```

## Testing Strategy

### Unit Tests

#### JsonRouterMemoryStoreTests
```csharp
[TestClass]
public class JsonRouterMemoryStoreTests
{
	[TestMethod]
	public async Task SaveAsync_CreatesFileWithValidJson()
	{
		var data = CreateTestMemoryData();
		await _store.SaveAsync(data);

		Assert.IsTrue(File.Exists(_tempPath));
		var json = File.ReadAllText(_tempPath);
		var restored = JsonSerializer.Deserialize<RouterMemoryData>(json);
		Assert.AreEqual(data.Version, restored.Version);
	}

	[TestMethod]
	public async Task LoadAsync_RecoveryFromCorruptedFile()
	{
		File.WriteAllText(_tempPath, "{ invalid json }");
		var data = await _store.LoadAsync();

		Assert.IsNotNull(data);
		// Should return empty or fallback, not crash
	}

	[TestMethod]
	public async Task CompactAsync_RemovesOldScores()
	{
		var data = CreateTestMemoryDataWithManyScores(200);
		await _store.CompactAsync(maxScoresPerModel: 100);

		foreach (var model in data.Dimensions.SelectMany(d => d.Value.Models.Values))
		{
			Assert.IsTrue(model.Scores.Count <= 100);
		}
	}
}
```

#### RouterMemoryTests
```csharp
[TestClass]
public class RouterMemoryTests
{
	[TestMethod]
	public async Task ObserveAsync_SavesAsynchronously()
	{
		var saveTask = Task.CompletedTask;

		// Observe should not block
		var stopwatch = Stopwatch.StartNew();
		await _memory.ObserveAsync("code_gen", "gpt-4", 0.95);
		stopwatch.Stop();

		Assert.IsTrue(stopwatch.ElapsedMilliseconds < 10, "Observe was blocking");
	}

	[TestMethod]
	public async Task ConcurrentObservations_NoDataLoss()
	{
		var tasks = Enumerable.Range(0, 100)
			.Select(i => _memory.ObserveAsync("code_gen", $"model_{i % 5}", 0.8 + i * 0.001))
			.ToList();

		await Task.WhenAll(tasks);

		var totalScores = _memory.GetTotalScoresRecorded();
		Assert.AreEqual(100, totalScores);
	}
}
```

### Integration Tests

#### PersistenceRegressionTests
```csharp
[TestClass]
public class PersistenceRegressionTests
{
	[TestMethod]
	public async Task MemorySurvivesRestart()
	{
		// 1. Create instance, make observations
		var memory1 = new RouterMemory(_store);
		await memory1.ObserveAsync("code_gen", "gpt-4-turbo", 0.95);
		await memory1.ObserveAsync("code_gen", "gpt-4-turbo", 0.92);

		// 2. "Restart" by creating new instance from disk
		var memory2 = new RouterMemory(_store);
		await _store.LoadAsync(); // Load from disk

		// 3. Verify memory was persisted
		var (model, score) = memory2.GetBestModel("code_gen");
		Assert.AreEqual("gpt-4-turbo", model);
		Assert.IsTrue(score >= 0.92);
	}
}
```

## Performance Considerations

### Save Performance
- **Async Save:** 5-50ms on SSD (doesn't block routing)
- **Backup Creation:** ~1-5ms per backup copy
- **File Atomicity:** Use temp file + atomic rename to prevent corruption

### Memory Footprint
- **In-Memory Cache:** ~1-10MB for typical router (100 dimensions, 10 models each, 100 scores)
- **JSON File Size:** ~500KB-5MB after compaction
- **Backup Storage:** Minimal (only keep last 7 days by default)

### Compaction Benefits
- **Reduces Disk I/O:** Fewer reads/writes
- **Improves Load Time:** Smaller JSON file to parse
- **Maintains Statistics:** Average, variance still accurate

## Disaster Recovery

### Backup Files
```
./data/
├── router_memory.json                    (current/active)
├── router_memory.json.20250115_143200.bak (backup from 2:32 PM)
├── router_memory.json.20250115_130000.bak (backup from 1:00 PM)
└── router_memory.json.20250114_090000.bak (backup from yesterday)
```

### Recovery Steps
1. **Corrupted Main File:** Restore from most recent backup
2. **Complete Data Loss:** Start fresh (empty memory, begin learning again)
3. **Partial Corruption:** Parse valid entries, discard corrupted rows

```csharp
public async Task<RouterMemoryData> LoadWithRecoveryAsync(CancellationToken ct = default)
{
	try
	{
		return await LoadAsync(ct);
	}
	catch (JsonException ex)
	{
		_logger.LogWarning(ex, "Main memory file corrupted, attempting recovery");

		// Try backups in reverse chronological order
		var backups = Directory.GetFiles(_directory, "*.bak")
			.OrderByDescending(f => f)
			.ToList();

		foreach (var backup in backups)
		{
			try
			{
				var json = await File.ReadAllTextAsync(backup, ct);
				var data = JsonSerializer.Deserialize<RouterMemoryData>(json);

				_logger.LogInformation("Recovered from backup: {BackupPath}", backup);
				return data;
			}
			catch { /* try next backup */ }
		}

		// No usable backup found
		_logger.LogWarning("No usable backup found, starting with empty memory");
		return new RouterMemoryData();
	}
}
```

## Next Steps

1. Implement `IRouterMemoryStore` interface
2. Implement `JsonRouterMemoryStore` with load/save/compact
3. Integrate into `RouterMemory` with async save
4. Add unit tests for all persistence operations
5. (Optional) Implement vector store integration
6. Validate end-to-end persistence in integration tests
