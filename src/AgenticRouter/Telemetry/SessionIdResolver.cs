using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;

namespace AgenticRouter.Telemetry;

/// <summary>
/// Resolves a conversation/session identifier for a routed request, so multiple requests from the
/// same client conversation can be grouped into turns for telemetry.
/// </summary>
public interface ISessionIdResolver
{
    /// <summary>
    /// Attempts to resolve a session id from the request's headers and (already-parsed) JSON body.
    /// </summary>
    /// <param name="headers">The inbound request headers.</param>
    /// <param name="body">The parsed request body, or <see langword="null"/> if it wasn't a JSON object.</param>
    /// <returns>The resolved session id, or <see langword="null"/> if none of the known conventions matched.</returns>
    string? Resolve(IHeaderDictionary headers, JsonObject? body);
}

/// <summary>
/// Default <see cref="ISessionIdResolver"/>. Mirrors the session-id resolution convention already
/// used by this repo's <c>claude-code-router</c> (<c>resolveSessionId</c> in
/// <c>claude-code-router/src/server/gateway/claude-code-router-plugin.ts</c>) and
/// <c>request-log-store.ts</c>'s <c>extractSessionIdFromPayload</c>, rather than inventing a new
/// convention: real IDE/CLI clients already send one of these.
/// </summary>
public sealed class SessionIdResolver : ISessionIdResolver
{
    private static readonly string[] HeaderNamesInPriorityOrder = ["x-claude-code-session-id", "x-claude-session-id"];

    private static readonly string[] BodyFieldNamesInPriorityOrder =
    [
        "session_id", "sessionId",
        "conversation_id", "conversationId",
        "chat_id", "chatId",
        "thread_id", "threadId",
    ];

    /// <inheritdoc />
    public string? Resolve(IHeaderDictionary headers, JsonObject? body)
    {
        ArgumentNullException.ThrowIfNull(headers);

        foreach (var headerName in HeaderNamesInPriorityOrder)
        {
            if (headers.TryGetValue(headerName, out var values))
            {
                var value = values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
                if (value is not null)
                {
                    return value;
                }
            }
        }

        if (body is null)
        {
            return null;
        }

        foreach (var fieldName in BodyFieldNamesInPriorityOrder)
        {
            if (TryReadNonEmptyString(body, fieldName, out var value))
            {
                return value;
            }
        }

        // Anthropic-style convention: metadata.user_id formatted as "{user}_session_{sessionId}".
        if (body["metadata"] is JsonObject metadata &&
            TryReadNonEmptyString(metadata, "user_id", out var userId))
        {
            var parts = userId.Split("_session_", StringSplitOptions.None);
            if (parts.Length > 1)
            {
                var sessionPart = parts[^1];
                if (!string.IsNullOrWhiteSpace(sessionPart))
                {
                    return sessionPart;
                }
            }
        }

        return null;
    }

    private static bool TryReadNonEmptyString(JsonObject obj, string propertyName, out string value)
    {
        value = string.Empty;

        if (obj[propertyName] is not JsonValue jsonValue || !jsonValue.TryGetValue<string>(out var stringValue))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(stringValue))
        {
            return false;
        }

        value = stringValue;
        return true;
    }
}
