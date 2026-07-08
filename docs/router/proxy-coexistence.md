# ACRouter Seamless Proxy Coexistence

> **Status: Proposed — not yet implemented.** No proxy-settings detection, backup, restore, or
> upstream-chaining code exists in `src/AgenticRouter/` today (verified: no registry/WinInet calls
> anywhere in the codebase). The implemented proxy (`Proxy/ProxyServer.cs` et al.) is a standalone
> local HTTP server; it neither reads nor modifies OS/IDE proxy settings. Everything below
> describes a **proposed** design, not current behavior — treat code-level claims as aspirational
> until this note is removed. See [system-proxy-architecture.md](./system-proxy-architecture.md)
> for the related (also proposed) system-proxy interception design.

## Overview

One of ACRouter's proposed architectural features is the ability to **coexist seamlessly with existing proxy settings** without interference, conflicts, or data loss. Whether you use a corporate proxy, VPN, or custom proxy configuration, the design calls for ACRouter to automatically detect, back up, and chain through your existing infrastructure.

---

## Why This Matters

### Problem Statement

Traditional proxy routing solutions create a "proxy takeover" problem:
- ❌ Existing proxy settings are overwritten or lost
- ❌ Restoration fails or leaves system in inconsistent state
- ❌ Manual configuration required for proxy chains
- ❌ No failover or recovery mechanism
- ❌ Complex troubleshooting when conflicts arise

### ACRouter's Solution

✅ **Automatic Detection** — Discovers existing proxy configuration at startup  
✅ **Safe Backup** — Captures exact system state before any modifications  
✅ **Intelligent Chaining** — Routes through your upstream proxy automatically  
✅ **Graceful Restoration** — Restores original settings on shutdown  
✅ **Conflict Prevention** — Detects and avoids self-referential loops  
✅ **Failover & Recovery** — Maintains health monitoring with automatic recovery  

---

## How It Works

### Phase 1: Pre-Takeover Snapshot

When ACRouter starts, it captures the complete current proxy state:

```
┌─ System Proxy Configuration ─┐
│ HTTP_PROXY: 10.0.0.1:8080   │
│ HTTPS_PROXY: 10.0.0.1:8080  │
│ NO_PROXY: *.internal.local  │
└─────────────────────────────┘
		 ↓
	[BACKUP TO DB]
		 ↓
┌─ Stored Snapshot ────────────┐
│ • Original HTTP proxy        │
│ • Original HTTPS proxy       │
│ • Bypass rules               │
│ • Platform-specific settings │
│ • Timestamp                  │
│ • Enabled/disabled state     │
└─────────────────────────────┘
```

**Database Storage Locations:**

All ACRouter data is stored in an agent-neutral directory structure:

| OS | ACRouter Data Directory |
|-----|---|
| **Windows** | `%APPDATA%\ACRouter\` |
| **macOS** | `~/.acrouter/` |
| **Linux** | `~/.acrouter/` |

**Claude Code Router (TypeScript/Electron) - Databases:**
- `config.sqlite` – Application configuration and state
- `api-keys.sqlite` – Encrypted API key storage
- `request-logs.sqlite` – Request logging and audit trail
- `usage.sqlite` – Usage statistics and tracking
- `system-proxy-snapshot.json` – Current system proxy state (backed up before takeover)
  - **Validated on startup:** ACRouter confirms the snapshot is valid before using it
  - **Validation checks:** File integrity, JSON format, schema structure, platform match, required fields
  - **Automatic recovery:** If snapshot is corrupted, it is safely ignored and removed
- `gateway.config.json` – Gateway/proxy configuration
- `certs/` – SSL/TLS certificates for proxy CA
- `provider-icons/` – Cached provider icons
- `raw-trace-spool/` – Raw trace spool data
- `data/` – Additional application data

**For cc-switch (Tauri version):**
- Config Directory: `~/.cc-switch/` (or `%APPDATA%\.cc-switch\` on Windows)
- SQLite Database: `cc-switch.db`
- Proxy Tables: `proxy_config`, `proxy_live_backup`, `proxy_request_logs`, `provider_health`

**What Gets Backed Up:**

- **Windows:** Registry values (`ProxyServer`, `ProxyOverride`, `ProxyEnable`, `AutoDetect`, WinHTTP settings)
- **macOS:** Per-network-service proxy settings (Web, Secure Web, SOCKS for Wi-Fi, Ethernet, VPN, etc.)
- **Linux:** Environment variables (`HTTP_PROXY`, `HTTPS_PROXY`, `ALL_PROXY`, `NO_PROXY`)

### Phase 2: Intelligent Conflict Detection

Before applying ACRouter's proxy configuration, three layers of conflict detection run:

#### **Loopback Detection**
```rust
// Check if system proxy already points to loopback (127.0.0.1, ::1, localhost)
fn system_proxy_points_to_loopback() -> bool {
	// If yes: skip setup (already configured)
	// If no: proceed with setup
}
```

**Scenario:** User already set `HTTP_PROXY=http://127.0.0.1:5002`
- Result: ACRouter detects this and does NOT re-setup (prevents double-proxying)

#### **Backup Corruption Detection**
```rust
// Check if stored backup contains proxy configuration (not user's original)
fn live_has_proxy_placeholder_for_app() -> bool {
	// If backup is corrupted/contains proxy config:
	//   - Don't use it (would lock user into proxy mode)
	//   - Rebuild from SSOT (Single Source of Truth)
	//   - Log warning for admin review
}
```

**Scenario:** Previous session crashed and backup stored proxy config instead of original
- Result: ACRouter rebuilds configuration from defaults instead of corrupted backup

#### **Provider Priority Recovery**
```rust
// Check if a higher-priority provider has recovered
if restored_priority < current_priority {
	// Auto-switch to recovered provider
	log::info!("Provider recovered, switching (priority {} → {})", current, restored);
}
```

**Scenario:** Primary provider becomes unavailable, system fails over to backup
- Result: When primary recovers, ACRouter automatically switches back

### Phase 3: Proxy Chaining

If you have an existing upstream proxy, ACRouter chains through it automatically:

```
User's Application (IDE, Terminal, etc.)
			↓
	 System sees: 127.0.0.1:5002
			↓
	┌─────────────────────────┐
	│   ACRouter Proxy        │
	│  (Routing decisions)    │
	└─────────────────────────┘
			↓
	 Detects upstream proxy
			↓
	┌─────────────────────────┐
	│  Your Upstream Proxy    │
	│  10.0.0.1:8080          │
	│  (Corporate/VPN/Custom) │
	└─────────────────────────┘
			↓
		Internet
```

**Example Configuration:**

```bash
# Your environment before ACRouter
export HTTP_PROXY=http://10.0.0.1:8080
export HTTPS_PROXY=http://10.0.0.1:8080
export NO_PROXY=localhost,127.0.0.1,*.internal.local
```

**After ACRouter Starts:**

```bash
# System proxy (managed by ACRouter)
HTTP_PROXY=http://127.0.0.1:5002          # ACRouter's local proxy
HTTPS_PROXY=http://127.0.0.1:5002

# Gateway process (chaining through upstream)
HTTP_PROXY=http://10.0.0.1:8080           # Original upstream
HTTPS_PROXY=http://10.0.0.1:8080
CCR_UPSTREAM_PROXY_URL=http://10.0.0.1:8080
NO_PROXY=localhost,127.0.0.1,*.internal.local,acrouter.internal
```

### Phase 4: Graceful Restoration

When ACRouter shuts down or is disabled, it restores your exact original configuration:

```
┌─ Stored Snapshot ────────────┐
│ HTTP_PROXY: 10.0.0.1:8080   │
│ HTTPS_PROXY: 10.0.0.1:8080  │
│ NO_PROXY: *.internal.local  │
└─────────────────────────────┘
		 ↓
	[RESTORE FROM DB]
		 ↓
┌─ System Proxy Configuration ─┐
│ HTTP_PROXY: 10.0.0.1:8080   │
│ HTTPS_PROXY: 10.0.0.1:8080  │
│ NO_PROXY: *.internal.local  │
└─────────────────────────────┘
```

**Platform-Specific Restoration:**

**Windows (Registry):**
```powershell
# Restore exact Registry values
Set-ItemProperty "HKCU:\Software\Microsoft\Windows\CurrentVersion\Internet Settings" `
  -Name "ProxyServer" -Value $snapshot.ProxyServer
Set-ItemProperty ... -Name "ProxyOverride" -Value $snapshot.ProxyOverride
# etc.
```

**macOS (per-service):**
```bash
# Restore for each network service
networksetup -setwebproxy "Wi-Fi" $snapshot.web_proxy_host $snapshot.web_proxy_port
networksetup -setwebproxystate "Wi-Fi" $snapshot.web_proxy_enabled
# etc.
```

**Linux (environment variables):**
```bash
# Restore exact environment variables
export HTTP_PROXY=$snapshot.http_proxy
export HTTPS_PROXY=$snapshot.https_proxy
# etc.
```

---

## Platform-Specific Implementations

### Windows

**Backup:**
- HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings
  - `ProxyServer`
  - `ProxyOverride`
  - `ProxyEnable`
  - `AutoDetect`
- WinHTTP proxy (via `netsh.exe`)

**Setup:**
- Sets local proxy: `127.0.0.1:5002`
- Bypass rules: `<local>` (localhost, 127.0.0.1)
- Configures WinHTTP for curl/wget compatibility

**Restoration:**
- Restores all Registry values from snapshot
- Clears WinHTTP proxy if it was originally disabled
- No hardcoded defaults—uses exact snapshot

### macOS

**Backup:**
- Per network service (Wi-Fi, Ethernet, VPN, Thunderbolt Bridge, etc.):
  - Web proxy settings
  - Secure proxy settings
  - SOCKS proxy settings
  - Authentication status

**Setup:**
- For each service: `networksetup -setwebproxy <service> 127.0.0.1 5002`
- Sets bypass list: `<local>, localhost, 127.0.0.1`
- Disables SOCKS if originally disabled

**Restoration:**
- Restores per-service settings exactly as they were
- Respects which services had proxy enabled/disabled
- Handles multiple network services correctly

### Linux

**Backup:**
- Environment variables: `HTTP_PROXY`, `HTTPS_PROXY`, `ALL_PROXY`, `NO_PROXY`
- Shell profile settings (if configured)

**Setup:**
- Sets environment: `HTTP_PROXY=http://127.0.0.1:5002`
- Detects and chains upstream proxy automatically
- Falls back to direct connection if no proxy needed

**Restoration:**
- Restores exact environment variables
- Handles both uppercase and lowercase variants
- Preserves shell profile state

---

## Failover and Provider Health Monitoring

ACRouter maintains intelligent failover with automatic recovery:

```
Initial State:
┌──────────────────────────────────────┐
├── 1. Provider A [Healthy]     ✓ ACTIVE
├── 2. Provider B [Degraded]    ⚠ STANDBY
└── 3. Provider C [Unhealthy]   ✗ BACKUP
└──────────────────────────────────────┘

Provider A fails:
		 ↓
├── 1. Provider A [Unhealthy]   ✗ FAILED
├── 2. Provider B [Degraded]    ⚠ ACTIVE (promoted)
└── 3. Provider C [Unhealthy]   ✗ BACKUP
└──────────────────────────────────────┘

Provider A recovers:
		 ↓
├── 1. Provider A [Healthy]     ✓ ACTIVE (recovered, higher priority)
├── 2. Provider B [Healthy]     ✓ STANDBY (recovered, auto-demoted)
└── 3. Provider C [Unhealthy]   ✗ BACKUP
└──────────────────────────────────────┘
```

**Health Thresholds:**

| Consecutive Failures | Status | Display | Action |
|---------------------|--------|---------|--------|
| 0 | Healthy | 🟢 Green | Use actively |
| 1-2 | Degraded | 🟡 Yellow | Monitor closely |
| 3+ | Unhealthy | 🔴 Red | Failover immediately |

**Auto-Recovery:**
- When a failed provider recovers, ACRouter automatically switches back if it has higher priority
- Seamless without user intervention
- Logged for audit trail

---

## Use Cases

### Use Case 1: Corporate Environment with Proxy

**Scenario:** Your organization requires all traffic through a corporate proxy.

```
Setup:
  export HTTP_PROXY=http://proxy.corp.internal:3128
  export HTTPS_PROXY=http://proxy.corp.internal:3128
  dotnet run ACRouter

Result:
  ✓ ACRouter backs up corporate proxy settings
  ✓ ACRouter chains requests through corporate proxy
  ✓ IDE/tools see both routing and corporate controls
  ✓ On shutdown, corporate proxy automatically restored
```

### Use Case 2: VPN + Local Development

**Scenario:** You're on a corporate VPN and developing locally.

```
Setup:
  # VPN configured system-wide
  # (system sees VPN proxy)

  dotnet run ACRouter

Result:
  ✓ ACRouter detects VPN proxy
  ✓ Backs up VPN proxy configuration
  ✓ Chains ACRouter through VPN
  ✓ All routing decisions respect VPN constraints
  ✓ On disconnection from VPN, ACRouter adapts gracefully
```

### Use Case 3: Upstream Gateway

**Scenario:** You have a gateway/proxy server in your infrastructure.

```
Setup:
  export HTTP_PROXY=http://gateway.internal:8080
  dotnet run ACRouter

Result:
  ✓ ACRouter becomes routing layer on top of gateway
  ✓ Gateway continues managing security policies
  ✓ ACRouter provides intelligent model routing
  ✓ Requests: Gateway → ACRouter → Model Selection → API
```

### Use Case 4: Multiple Network Services (macOS)

**Scenario:** Different network interfaces have different proxy settings.

```
macOS Network Setup:
  Wi-Fi: proxy.corp.internal:3128
  Ethernet: direct connection
  VPN: vpn-proxy.corp.internal:3128

Result:
  ✓ ACRouter backs up all three per-service settings
  ✓ Each service maintains its own proxy configuration
  ✓ Switching networks works correctly
  ✓ Restoration happens per-service
```

---

## Troubleshooting

### Issue: Proxy Not Restored After ACRouter Shutdown

**Diagnosis:**
```bash
# Check if backup was created
# Windows: Query database for stored snapshot
# macOS: Check networksetup history
# Linux: Check environment variables in profile
```

**Solution:**

1. **Automatic Restoration:**
   ```bash
   # Disable and re-enable ACRouter
   dotnet run ACRouter --disable-proxy
   dotnet run ACRouter --enable-proxy
   ```

2. **Manual Restoration (Windows):**
   ```powershell
   # Reset to default (direct connection)
   netsh winhttp reset proxy

   # Or restore known proxy
   netsh winhttp set proxy "<proxy>:<port>"
   ```

3. **Manual Restoration (macOS):**
   ```bash
   # Disable proxy on all services
   networksetup -setwebproxystate Wi-Fi off
   networksetup -setsecurewebproxystate Wi-Fi off
   ```

### Issue: Self-Referential Loop Detected

**Symptom:** ACRouter skips proxy setup with message "Already configured"

**Cause:** Environment already has `HTTP_PROXY=http://127.0.0.1:5002`

**Solution:**
```bash
# Clear the environment variable
unset HTTP_PROXY HTTPS_PROXY

# Restart ACRouter
dotnet run ACRouter
```

### Issue: Backup Corruption Detected

**Symptom:** ACRouter rebuilds configuration from defaults

**Cause:** Previous session stored proxy config in backup instead of original

**Solution:**
```bash
# Clear corrupted backup
# (ACRouter will auto-rebuild from SSOT)

# Verify system proxy is correct
# Windows: Check Registry
# macOS: networksetup -getwebproxy <service>
# Linux: echo $HTTP_PROXY
```

---

## Configuration

### Environment Variables

**ACRouter Proxy Configuration:**
```bash
# Port for ACRouter proxy
export PROXY_LISTEN_PORT=5002

# Upstream proxy (detected automatically if not set)
export CCR_UPSTREAM_PROXY_URL=http://10.0.0.1:8080

# No-proxy list (in addition to defaults)
export NO_PROXY=localhost,127.0.0.1,*.internal.local
```

### appsettings.json (.NET 10)

```json
{
  "ACRouter": {
	"Proxy": {
	  "Enabled": true,
	  "ListenPort": 5002,
	  "DetectUpstream": true,
	  "BackupOnStartup": true,
	  "RestoreOnShutdown": true,
	  "HealthCheckIntervalMs": 30000,
	  "FailoverThreshold": 3
	}
  }
}
```

---

## Best Practices

### ✅ Recommended

1. **Let ACRouter auto-detect your proxy:**
   ```bash
   # Set standard proxy env vars before running
   export HTTP_PROXY=http://proxy.company.com:3128
   dotnet run ACRouter
   ```

2. **Monitor health status:**
   ```bash
   # Check provider health
   curl http://127.0.0.1:5002/api/health
   ```

3. **Enable backup/restore (default):**
   - ACRouter automatically backs up on startup
   - Automatically restores on shutdown

4. **Use upstream proxy for corporate constraints:**
   - Your corporate proxy handles security policies
   - ACRouter handles intelligent routing
   - Both work together

### ❌ Avoid

1. **Don't manually modify system proxy while ACRouter is running:**
   - Snapshot becomes stale
   - Restoration may fail
   - Use ACRouter UI or API instead

2. **Don't set HTTP_PROXY to loopback manually:**
   - Creates confusion for ACRouter
   - ACRouter detects and skips setup
   - Use PROXY_LISTEN_PORT if you need custom port

3. **Don't delete ACRouter database while proxies are active:**
   - Loses backup information
   - Cannot restore original settings
   - Always stop ACRouter before clearing DB

4. **Don't assume hardcoded defaults on restoration:**
   - ACRouter restores YOUR exact original settings
   - Not predefined proxy settings
   - Exact values are preserved in snapshot

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    User's System                            │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐ │
│  │           Application / IDE / Tool                   │ │
│  │  (Uses system proxy: http://127.0.0.1:5002)         │ │
│  └──────────────┬───────────────────────────────────────┘ │
│                 │                                         │
│                 ↓                                         │
│  ┌──────────────────────────────────────────────────────┐ │
│  │    ACRouter Proxy (Routing Decision Layer)           │ │
│  │    • Detects model for request                       │ │
│  │    • Routes to optimal provider                      │ │
│  │    • Chains through upstream proxy                   │ │
│  └──────────────┬───────────────────────────────────────┘ │
│                 │                                         │
│    ┌────────────┴────────────┐                           │
│    │                         │                           │
│    ↓                         ↓                           │
│ ┌──────────────┐     ┌─────────────────────┐            │
│ │Direct Path   │     │Upstream Proxy Path  │            │
│ │(if any)      │     │10.0.0.1:8080       │            │
│ └──────────────┘     │(Corporate/VPN)     │            │
│                      └────────┬───────────┘            │
└─────────────────────────────────┼──────────────────────┘
								  │
					┌─────────────┴─────────────┐
					│                           │
					↓                           ↓
			┌──────────────┐        ┌──────────────────┐
			│  API Server  │        │  API Server      │
			│  (gpt-4)     │        │  (claude)        │
			└──────────────┘        └──────────────────┘
```

**Request Flow with Existing Proxy:**

```
Request comes in
	↓
[ACRouter Routing Logic]
  • Parse request
  • Determine model
  • Check history
	↓
[Detect Upstream Proxy]
  • Is HTTP_PROXY set?
  • Chain through it
	↓
[Forward Request]
  • To selected API
  • Through upstream if needed
	↓
Response returns
  • Through same chain
  • Logged in routing memory
	↓
[Return to User]
```

---

## Summary

ACRouter's seamless proxy coexistence is achieved through:

| Component | Capability | Benefit |
|-----------|-----------|---------|
| **Snapshot System** | Captures exact proxy state | Safe backup, no data loss |
| **Conflict Detection** | 3-layer detection (loopback, corruption, priority) | Prevents self-conflicts |
| **Proxy Chaining** | Automatically chains through upstream | Works with corporate proxies |
| **Graceful Restoration** | Restores exact original settings | Perfect restoration |
| **Health Monitoring** | Tracks provider health, auto-recovers | Resilient and self-healing |
| **Platform Support** | Windows, macOS, Linux specific code | Reliable across OSes |

**Result:** You can deploy ACRouter in any networking environment—corporate, VPN, gateway, or local—and it will coexist peacefully with your existing infrastructure, require minimal configuration, and restore perfectly on shutdown.

---

## See Also

- [system-proxy-architecture.md](./system-proxy-architecture.md) — Low-level proxy implementation details
- [AGENTS.md](../../AGENTS.md) — Repository policies including Serilog logging
- [serilog-logging-guide.md](./serilog-logging-guide.md) — Logging all proxy operations
