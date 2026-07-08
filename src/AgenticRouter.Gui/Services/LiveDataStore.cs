using AgenticRouter.Gui.Models;
using AgenticRouter.Gui.Telemetry;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace AgenticRouter.Gui.Services;

/// <summary>
/// Connects to the AgenticRouter proxy's <c>/telemetry/hub</c> SignalR hub and maintains a live,
/// continuously re-aggregated view of routing telemetry as <see cref="Conversation"/> records - the same
/// shape <see cref="MockData"/> exposes, so dashboard components render live and mock data identically.
/// </summary>
/// <remarks>
/// Registered as a singleton in <c>MauiProgram</c> and started once from <c>Dashboard.razor</c>'s
/// <c>OnInitializedAsync</c>. Not unit-tested: it is a thin wrapper around <see cref="HubConnection"/>
/// networking that cannot run without a live server, consistent with the rest of AgenticRouter.Gui's
/// Windows-only glue code. The logic it delegates to -
/// <see cref="ConversationAggregator.Aggregate"/> and <see cref="LiveConversationMapper.ToModel"/> - is
/// unit-tested (the former in AgenticRouter.Gui.Telemetry.Tests).
/// </remarks>
public sealed class LiveDataStore : IAsyncDisposable
{
    /// <summary>
    /// Default telemetry hub URL, matching <c>AgenticRouter.Hosting.ProxyHostedService</c>'s default
    /// proxy port (5001). Not currently configurable from the GUI - there is no existing settings
    /// storage mechanism to persist a custom URL; see docs/gui/dashboard.md for this known limitation.
    /// </summary>
    public const string DefaultHubUrl = "http://localhost:5001/telemetry/hub";

    private readonly object _lock = new();
    private readonly List<RoutingTelemetryEventDto> _events = [];
    private readonly HubConnection _hubConnection;
    private readonly ILogger<LiveDataStore>? _logger;
    private readonly string _hubUrl;

    private volatile IReadOnlyList<Conversation> _conversations = [];

    public LiveDataStore(ILogger<LiveDataStore>? logger = null, string hubUrl = DefaultHubUrl)
    {
        _logger = logger;
        _hubUrl = hubUrl;
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<RoutingTelemetryEventDto>("RoutingTelemetry", OnRoutingTelemetryReceived);
    }

    /// <summary>Conversations reconstructed from telemetry received so far. Empty until the proxy sends events.</summary>
    public IReadOnlyList<Conversation> Conversations => _conversations;

    /// <summary>
    /// Raised after a new telemetry event has been folded in, on whatever thread SignalR's client
    /// delivered the event on. Subscribers (Razor components) must marshal back to their own render
    /// context, e.g. via <c>ComponentBase.InvokeAsync(StateHasChanged)</c>.
    /// </summary>
    public event Action? Changed;

    /// <summary>
    /// Connects to the proxy's telemetry hub. Connection failures (e.g. the proxy isn't running yet)
    /// are logged and swallowed - the dashboard simply shows no live conversations until a connection
    /// succeeds; <c>WithAutomaticReconnect</c> keeps retrying in the background once first connected.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubConnection.StartAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(
                ex,
                "Could not connect to the AgenticRouter telemetry hub at {HubUrl}; live data will be unavailable until it becomes reachable.",
                _hubUrl);
        }
    }

    private void OnRoutingTelemetryReceived(RoutingTelemetryEventDto dto)
    {
        List<RoutingTelemetryEventDto> snapshot;
        lock (_lock)
        {
            _events.Add(dto);
            snapshot = [.. _events];
        }

        _conversations = ConversationAggregator.Aggregate(snapshot)
            .Select(LiveConversationMapper.ToModel)
            .ToList();

        Changed?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        await _hubConnection.DisposeAsync();
    }
}
