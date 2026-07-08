using System.Collections.Concurrent;

namespace AgenticRouter.Telemetry;

/// <summary>
/// Tracks the 1-based turn number of each request within a conversation (session), for grouping
/// routing telemetry into multi-turn conversations.
/// </summary>
public interface IConversationTurnTracker
{
    /// <summary>
    /// Advances and returns the next turn number for the given session id. The first call for a
    /// given <paramref name="sessionId"/> returns 1.
    /// </summary>
    /// <param name="sessionId">The session id to advance. Must not be null or whitespace.</param>
    /// <returns>The turn number for this call, starting at 1.</returns>
    int NextTurn(string sessionId);
}

/// <summary>
/// In-memory, thread-safe <see cref="IConversationTurnTracker"/>. Memory grows with the number of
/// distinct sessions seen since process start; there is no eviction, matching the proxy's existing
/// "no persistence beyond the process" telemetry model. A future iteration could add TTL-based
/// eviction if long-running processes see unbounded session churn.
/// </summary>
public sealed class ConversationTurnTracker : IConversationTurnTracker
{
    private readonly ConcurrentDictionary<string, int> _turnsBySession = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public int NextTurn(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("Session id must not be null or whitespace.", nameof(sessionId));
        }

        return _turnsBySession.AddOrUpdate(sessionId, addValueFactory: _ => 1, updateValueFactory: (_, existing) => existing + 1);
    }
}
