import React from "react";
import { providers } from "./mockData.js";

export function Governance({ flashId }) {
  const sorted = [...providers].sort((a, b) => b.spend / b.cap - a.spend / a.cap);
  return (
    <div style={{ padding: 16, display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12, height: "100%", boxSizing: "border-box", overflowY: "auto", alignContent: "start" }}>
      {sorted.map((p) => {
        const pct = (p.spend / p.cap) * 100;
        const status = pct >= 100 ? "critical" : pct >= 80 ? "warning" : "ok";
        const color = status === "critical" ? "var(--status-critical-fill)" : status === "warning" ? "var(--status-warn-fill)" : "var(--status-ok-fill)";
        const textColor = status === "critical" ? "var(--status-critical-text)" : status === "warning" ? "var(--status-warn-text)" : "var(--status-ok-text)";
        return (
          <div key={p.id} className={p.id === flashId ? (status === "critical" ? "flash-red" : "flash-amber") : ""}
            style={{ background: "var(--surface-card)", border: `1px solid ${status !== "ok" ? (status === "critical" ? "var(--border-critical)" : "var(--border-warning)") : "var(--border-default)"}`, borderRadius: 8, padding: 14 }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "baseline" }}>
              <div>
                <div style={{ fontSize: 13, fontWeight: 600, color: "var(--text-primary)" }}>{p.name}</div>
                <div style={{ fontSize: 10, color: "var(--text-tertiary)" }}>{p.label}</div>
              </div>
              <span style={{
                fontSize: 10, fontWeight: 700, padding: "2px 8px", borderRadius: 4, letterSpacing: "0.03em",
                background: status === "critical" ? "var(--state-hover-red)" : status === "warning" ? "var(--state-hover-amber)" : "#10b9811a",
                color: textColor,
              }}>{status.toUpperCase()}</span>
            </div>

            <div style={{ display: "flex", alignItems: "center", gap: 8, marginTop: 10 }}>
              <span style={{ fontSize: 10, color: "var(--text-tertiary)" }}>Cap $</span>
              <input defaultValue={p.cap} style={{ width: 70, background: "var(--surface-page)", border: "1px solid var(--border-default)", borderRadius: 4, color: "var(--text-primary)", fontFamily: "var(--font-mono)", fontSize: 11, padding: "3px 6px" }} />
              <span style={{ marginLeft: "auto", fontFamily: "var(--font-mono)", fontSize: 12, color: "var(--text-primary)" }}>${p.spend.toFixed(2)}</span>
            </div>

            <div style={{ height: 6, background: "var(--surface-track)", borderRadius: 2, overflow: "hidden", marginTop: 8 }}>
              <div style={{ height: "100%", width: `${Math.min(100, pct)}%`, background: color }} />
            </div>

            <div style={{ fontSize: 10, color: "var(--text-tertiary)", marginTop: 6 }}>
              {status === "critical" ? "Fallback Engine Engaged" : p.days != null ? `~${p.days} day${p.days === 1 ? "" : "s"} remaining` : "No cap pressure"}
            </div>
          </div>
        );
      })}
    </div>
  );
}
