import React from "react";
import { providers } from "./mockData.js";

export function Header({ onOpenSettings, onOpenGovernance, tabs, active, onTab }) {
  const breached = providers.filter((p) => p.spend / p.cap >= 1);
  const approaching = providers.filter((p) => p.spend / p.cap >= 0.8 && p.spend / p.cap < 1);
  let status = { tone: "ok", text: "System Status: OK" };
  if (breached.length) status = { tone: "critical", text: `🚨 ${breached.length} PROVIDER BREACHED${approaching.length ? ` · ${approaching.length} APPROACHING` : ""}` };
  else if (approaching.length) status = { tone: "warning", text: `⚠️ ${approaching.length} PROVIDER APPROACHING LIMIT` };

  return (
    <div style={{ background: "var(--surface-card)", borderBottom: "1px solid var(--border-default)" }}>
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", padding: "12px 20px" }}>
        <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
          <img src="../../assets/icon-app.svg" width="22" height="22" alt="" />
          <span style={{ fontSize: "var(--text-lg)", fontWeight: 700, letterSpacing: "var(--tracking-tight)", color: "var(--text-heading)" }}>
            Router Optimization Engine
          </span>
        </div>
        <button
          onClick={() => status.tone !== "ok" && onOpenGovernance && onOpenGovernance()}
          style={{
            background: "transparent", border: "none",
            cursor: status.tone !== "ok" ? "pointer" : "default",
            color: status.tone === "critical" ? "var(--status-critical-text)" : status.tone === "warning" ? "var(--status-warn-text)" : "var(--status-ok-text)",
            fontSize: 12, fontWeight: 600, display: "flex", alignItems: "center", gap: 6,
          }}
        >
          {status.tone === "ok" && <span className="pulse-dot" style={{ width: 7, height: 7, borderRadius: "50%", background: "var(--status-ok-fill)" }} />}
          {status.text}
        </button>
        <button onClick={onOpenSettings} style={{ background: "transparent", border: "1px solid var(--border-default)", color: "var(--text-secondary)", borderRadius: 6, padding: "6px 12px", fontSize: 12, fontWeight: 600, cursor: "pointer" }}>
          ⚙ Settings
        </button>
      </div>

      <div style={{ display: "flex", alignItems: "center", gap: 24, padding: "0 20px 12px" }}>
        <Ticker label="Total Saved" value="$142.36" color="var(--status-ok-text)" />
        <Ticker label="System Tokens" value="4.2M" />
        <Ticker label="Avg. Cost Reduction" value="82.1%" color="var(--status-ok-text)" />
        <span style={{ marginLeft: "auto", display: "flex", alignItems: "center", gap: 6, fontSize: 11, color: "var(--status-critical-text)", fontWeight: 700, letterSpacing: "0.05em" }}>
          <span className="pulse-dot" style={{ width: 6, height: 6, borderRadius: "50%", background: "var(--status-critical-fill)" }} />
          LIVE
        </span>
      </div>

      <div style={{ display: "flex", gap: 4, padding: "0 16px" }}>
        {tabs.map((t) => {
          const isActive = t.id === active;
          return (
            <button key={t.id} onClick={() => onTab(t.id)} style={{
              appearance: "none", background: "transparent", border: "none",
              borderBottom: `2px solid ${isActive ? "var(--sky-400)" : "transparent"}`,
              color: isActive ? "var(--text-heading)" : "var(--text-secondary)",
              fontFamily: "var(--font-sans)", fontSize: 13, fontWeight: isActive ? 600 : 500,
              padding: "10px 14px", cursor: "pointer", transition: "color .15s ease, border-color .15s ease",
            }}>{t.label}</button>
          );
        })}
      </div>
    </div>
  );
}

function Ticker({ label, value, color }) {
  return (
    <div style={{ display: "flex", alignItems: "baseline", gap: 6 }}>
      <span style={{ fontFamily: "var(--font-mono)", fontSize: 15, fontWeight: 700, color: color || "var(--text-primary)" }}>{value}</span>
      <span style={{ fontSize: 10, textTransform: "uppercase", letterSpacing: "0.05em", color: "var(--text-tertiary)" }}>{label}</span>
    </div>
  );
}
