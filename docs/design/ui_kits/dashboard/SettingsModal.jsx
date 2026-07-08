import React from "react";

export function SettingsModal({ open, onClose }) {
  const [resetWord, setResetWord] = React.useState("");
  const [purgeWord, setPurgeWord] = React.useState("");
  if (!open) return null;
  return (
    <div onClick={onClose} style={{ position: "fixed", inset: 0, background: "rgba(15,23,42,0.7)", backdropFilter: "blur(2px)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 50 }}>
      <div onClick={(e) => e.stopPropagation()} style={{ background: "var(--surface-card)", border: "1px solid var(--border-default)", borderRadius: 8, padding: 20, width: 380, maxWidth: "90vw" }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 16 }}>
          <span style={{ fontSize: 15, fontWeight: 600, color: "var(--text-heading)" }}>Settings</span>
          <button onClick={onClose} style={{ background: "transparent", border: "none", color: "var(--text-tertiary)", cursor: "pointer", fontSize: 16 }}>✕</button>
        </div>

        <div style={{ fontSize: 11, textTransform: "uppercase", letterSpacing: "0.05em", color: "var(--status-critical-text)", marginBottom: 10 }}>Destructive Actions Zone</div>

        <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
          <div style={{ border: "1px solid var(--border-critical)", borderRadius: 8, padding: 12 }}>
            <div style={{ fontSize: 12, color: "var(--text-secondary)", marginBottom: 8 }}>Type <b style={{ color: "var(--text-primary)" }}>RESET</b> to enable</div>
            <div style={{ display: "flex", gap: 8 }}>
              <input value={resetWord} onChange={(e) => setResetWord(e.target.value)} placeholder="RESET"
                style={{ flex: 1, background: "var(--surface-page)", border: "1px solid var(--border-default)", borderRadius: 6, color: "var(--text-primary)", fontSize: 12, padding: "6px 8px" }} />
              <button disabled={resetWord !== "RESET"} onClick={onClose}
                style={{ background: "transparent", border: "1px solid var(--border-critical)", color: resetWord === "RESET" ? "var(--status-critical-text)" : "var(--text-muted)", borderRadius: 6, padding: "6px 12px", fontSize: 12, fontWeight: 600, cursor: resetWord === "RESET" ? "pointer" : "default" }}>
                Reset Stats
              </button>
            </div>
          </div>

          <div style={{ border: "1px solid var(--border-critical)", borderRadius: 8, padding: 12 }}>
            <div style={{ fontSize: 12, color: "var(--text-secondary)", marginBottom: 8 }}>Type <b style={{ color: "var(--text-primary)" }}>PURGE</b> to enable</div>
            <div style={{ display: "flex", gap: 8 }}>
              <input value={purgeWord} onChange={(e) => setPurgeWord(e.target.value)} placeholder="PURGE"
                style={{ flex: 1, background: "var(--surface-page)", border: "1px solid var(--border-default)", borderRadius: 6, color: "var(--text-primary)", fontSize: 12, padding: "6px 8px" }} />
              <button disabled={purgeWord !== "PURGE"} onClick={onClose}
                style={{ background: "transparent", border: "1px solid var(--border-critical)", color: purgeWord === "PURGE" ? "var(--status-critical-text)" : "var(--text-muted)", borderRadius: 6, padding: "6px 12px", fontSize: 12, fontWeight: 600, cursor: purgeWord === "PURGE" ? "pointer" : "default" }}>
                Clear History
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
