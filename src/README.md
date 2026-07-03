# AgenticRouter Proxy — Quick Start

`AgenticRouter` is a .NET 10 console host that runs a local Kestrel-based
HTTP proxy. It sits in front of your coding-agent client, inspects each
request's `model` field, and forwards the request to the correct upstream
provider (OpenAI, Anthropic, Alibaba, Zhipu, Moonshot, MiniMax, ...) with the
right base URL and auth header attached — similar in spirit to LiteLLM's
`model_list` proxy.

This guide covers configuring and running the proxy from `src/AgenticRouter`.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- API keys for whichever upstream providers you intend to route to

## 1. Restore and build

```bash
cd src/AgenticRouter
dotnet restore
dotnet build
```

## 2. Set provider API keys

Providers are configured in `appsettings.json` under `ModelRouting:Providers`.
Each provider declares an `ApiKeyEnvVar` — the name of the environment
variable the proxy reads the key from at request time. Set only the
variables for the providers you plan to use:

```bash
export OPENAI_API_KEY="<your-openai-key>"
export ANTHROPIC_API_KEY="<your-anthropic-key>"
export QWEN_API_KEY="<your-alibaba-key>"
export GLM_API_KEY="<your-zhipu-key>"
export KIMI_API_KEY="<your-moonshot-key>"
export MINIMAX_API_KEY="<your-minimax-key>"
```

If a provider's key is missing, requests to models routed to that provider
are forwarded without an auth header.

Alternatively, a provider can carry a literal `ApiKey` directly in
`appsettings.json` instead of (or as a fallback source ahead of) an
environment variable — see [Provider API keys](#provider-api-keys) below.

## 3. Configure routing

`src/AgenticRouter/appsettings.json` has two relevant sections:

- **`Routing`** — router policy defaults (`DefaultModel`, `MaxCandidates`,
  `MaxNeighborCount`, `EnableExploration`, `ExplorationRate`, `PolicyName`,
  `MemoryPath`).
- **`ModelRouting`** — the proxy's allowlist. Only models listed in
  `ModelList` are routable; anything else gets a `400` response.

To add a model, add an entry under `ModelList` that points at an existing
provider key:

```json
{
  "ModelName": "my-alias",
  "Provider": "anthropic",
  "ProviderModelId": "claude-sonnet-4-6"
}
```

- `ModelName` is what the client sends in the request body's `model` field.
- `Provider` must match a key under `ModelRouting:Providers`.
- `ProviderModelId` is the identifier substituted into the forwarded request.

To add a new provider, add an entry under `ModelRouting:Providers`:

```json
"my-provider": {
  "BaseUrl": "https://api.my-provider.com",
  "ApiKeyEnvVar": "MY_PROVIDER_API_KEY",
  "AuthHeaderName": "Authorization",
  "AuthHeaderScheme": "Bearer"
}
```

Set `AuthHeaderScheme` to an empty string if the provider expects the raw
key with no scheme prefix (see the `anthropic` entry, which uses
`x-api-key` with no scheme).

You can override any of these settings without editing `appsettings.json` by
using environment variables (ASP.NET Core configuration convention), e.g.:

```bash
export Routing__DefaultModel="glm-5"
```

### Provider API keys

Each provider resolves its API key in this order:

1. **`ApiKey`** — a literal key set directly on the provider entry. If
   non-empty, it is used as-is and `ApiKeyEnvVar` is not consulted.
2. **`ApiKeyEnvVar`** — the name of an environment variable read at request
   time, used only when `ApiKey` is empty or unset.
3. Neither set (or the named environment variable isn't present) — the
   request is forwarded to the provider with no auth header.

```json
"my-provider": {
  "BaseUrl": "https://api.my-provider.com",
  "ApiKey": "sk-my-literal-key",
  "ApiKeyEnvVar": "MY_PROVIDER_API_KEY",
  "AuthHeaderName": "Authorization",
  "AuthHeaderScheme": "Bearer"
}
```

Prefer `ApiKeyEnvVar` for anything checked into source control —
`appsettings.json` is typically committed to git, so a literal `ApiKey`
belongs only in an untracked/local override file or a secret store, not in
the tracked base config.

The proxy only auto-loads `appsettings.json` plus
`appsettings.{DOTNET_ENVIRONMENT}.json` — there is no ad hoc
`appsettings.Local.json` convention wired up. To use an untracked override
file, set the `DOTNET_ENVIRONMENT` variable to match its name before
running, e.g. `appsettings.Development.json` requires
`DOTNET_ENVIRONMENT=Development` (already set for you by the `AgenticRouter`
launch profile in `Properties/launchSettings.json`).

**Note:** `Program.cs` builds the outer application host as a plain generic
host (`Host.CreateDefaultBuilder`), which reads `DOTNET_ENVIRONMENT` — not
`ASPNETCORE_ENVIRONMENT` — to decide whether to load
`appsettings.Development.json`. However, `ProxyServer` separately builds a
Kestrel web host (`ConfigureWebHostDefaults`) for the proxy's HTTP listener,
and *that* host does consult `ASPNETCORE_ENVIRONMENT`. To keep both hosting
stacks consistent, set both variables to the same value (the
`AgenticRouter` launch profile in `Properties/launchSettings.json` already
does this).

## 4. Run the proxy

```bash
dotnet run
```

By default, the proxy listens on `http://localhost:5001`. Point your
coding-agent client's base URL at the proxy instead of the provider directly,
and request whichever `model` alias you configured in `ModelList`. The
proxy forwards the path and query string unchanged, rewrites the `model`
field to the provider's `ProviderModelId`, and injects the resolved auth
header before sending the request upstream.

## 5. Verify

With the proxy running, send a request using one of your configured model
aliases:

```bash
curl http://localhost:5001/v1/chat/completions \
  -H "Content-Type: application/json" \
  -d '{"model": "kimi-k2.5", "messages": [{"role": "user", "content": "hello"}]}'
```

An unconfigured model name returns a `400` with an `invalid_request_error`
body instead of being forwarded.

### Model discovery (`GET /v1/models`)

Many OpenAI-compatible clients (including VS Code's Copilot Chat BYOK
providers) call `GET /v1/models` before anything else, to discover which
models are available. Since this request has no body and your `ModelList`
can span multiple upstream providers, the proxy answers it locally from
configuration — mirroring how LiteLLM's proxy handles the same endpoint —
rather than forwarding it anywhere:

```bash
curl http://localhost:5001/v1/models
```

```json
{
  "object": "list",
  "data": [
    { "id": "gpt-5.4", "object": "model", "created": 0, "owned_by": "openai" },
    { "id": "kimi-k2.5", "object": "model", "created": 0, "owned_by": "moonshot" }
  ]
}
```

`id` is the client-facing `ModelName`, and `owned_by` is the configured
provider key. `created` is always `0` — there's no meaningful creation
timestamp for a statically configured route.

## Running with Docker

A `Dockerfile` is provided for containerized runs:

```bash
docker build -t agentic-router -f src/AgenticRouter/Dockerfile src/AgenticRouter
docker run -p 5001:5001 \
  -e ANTHROPIC_API_KEY="<your-anthropic-key>" \
  -e OPENAI_API_KEY="<your-openai-key>" \
  agentic-router
```

## Running tests

```bash
cd src/AgenticRouter.Tests
dotnet test
```
