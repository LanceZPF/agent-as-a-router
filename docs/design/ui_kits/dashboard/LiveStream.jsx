import React from "react";
import { conversations } from "./mockData.js";

function colorForAgent(name) {
  const PALETTE = ["#10b981","#38bdf8","#818cf8","#fb7185","#f59e0b","#a78bfa","#14b8a6","#0ea5e9","#6366f1","#ec4899","#f97316","#06b6d4"];
  if (!name) return PALETTE[0];
  let hash = 2166136261;
  for (let i = 0; i < name.length; i++) hash = Math.imul(hash ^ name.charCodeAt(i), 16777619);
  return PALETTE[Math.abs(hash) % PALETTE.length];
}

function fmtTok(n) { return n >= 1000 ? (n / 1000).toFixed(1) + "K" : String(n); }

export function LiveStream() {
  const [selectedId, setSelected] = React.useState(conversations[0].id);
  const [query, setQuery] = React.useState("");
  const [leftPct, setLeftPct] = React.useState(35);
  const [expanded, setExpanded] = React.useState(null);
  const dragRef = React.useRef(false);
  const containerRef = React.useRef(null);

  const filtered = conversations.filter((c) => {
    const q = query.toLowerCase();
    if (!q) return true;
    return c.title.toLowerCase().includes(q) || c.turns.some((t) => t.agent.toLowerCase().includes(q) || t.model.toLowerCase().includes(q));
  });
  const selected = conversations.find((c) => c.id === selectedId) || conversations[0];

  function onPointerMove(e) {
    if (!dragRef.current || !containerRef.current) return;
    const rect = containerRef.current.getBoundingClientRect();
    const pct = ((e.clientX - rect.left) / rect.width) * 100;
    setLeftPct(Math.min(65, Math.max(20, pct)));
  }

  return (
    <div
      ref={containerRef}
      onPointerMove={onPointerMove}
      onPointerUp={() => (dragRef.current = false)}
      style={{ display: "flex", height: "100%", padding: 12, gap: 0, boxSizing: "border-box" }}
    >
      {/* Left: conversation list */}
      <div style={{ width: `${leftPct}%`, minWidth: 220, display: "flex", flexDirection: "column", gap: 8 }}>
        <input
          value={query} onChange={(e) => setQuery(e.target.value)} placeholder="Search conversations…"
          style={{ background: "var(--surface-page)", border: "1px solid var(--border-default)", borderRadius: 6, padding: "6px 10px", color: "var(--text-primary)", fontSize: 12, outline: "none" }}
        />
        <div style={{ display: "flex", flexDirection: "column", gap: 6, overflowY: "auto" }}>
          {filtered.map((c) => {
            const isSel = c.id === selectedId;
            const agents = [...new Set(c.turns.map((t) => t.agent))].slice(0, 2);
            return (
              <div key={c.id} onClick={() => setSelected(c.id)}
                style={{
                  background: isSel ? "var(--surface-card-hover)" : "var(--surface-card)",
                  border: `1px solid ${c.fallback ? "var(--border-warning)" : "var(--border-default)"}`,
                  borderLeft: c.fallback ? "3px solid var(--status-warn-fill)" : "1px solid var(--border-default)",
                  borderRadius: 8, padding: 10, cursor: "pointer",
                }}>
                <div style={{ display: "flex", justifyContent: "space-between", gap: 6 }}>
                  <span style={{ fontSize: 12, fontWeight: 600, color: "var(--text-primary)", overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>{c.title}</span>
                  {c.fallback && <span style={{ color: "var(--status-warn-text)", fontSize: 12 }}>⚠</span>}
                </div>
                <div style={{ fontSize: 10, color: "var(--text-tertiary)", fontFamily: "var(--font-mono)", marginTop: 3 }}>{c.first} → {c.last}</div>
                <div style={{ display: "flex", justifyContent: "space-between", marginTop: 6, alignItems: "center" }}>
                  <div style={{ display: "flex", gap: 8 }}>
                    {agents.map((a) => (
                      <span key={a} style={{ display: "flex", alignItems: "center", gap: 4, fontSize: 10, color: "var(--text-secondary)" }}>
                        <span style={{ width: 6, height: 6, borderRadius: "50%", background: colorForAgent(a) }} />{a}
                      </span>
                    ))}
                  </div>
                  <span style={{ fontFamily: "var(--font-mono)", fontSize: 10, color: "var(--text-tertiary)" }}>${c.cost.toFixed(4)} · {c.turns.length} turns</span>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Divider */}
      <div
        onPointerDown={(e) => { dragRef.current = true; e.currentTarget.setPointerCapture(e.pointerId); }}
        style={{ flex: "0 0 8px", margin: "0 2px", cursor: "col-resize", borderRadius: 4, position: "relative" }}
      >
        <div style={{ position: "absolute", top: "50%", left: "50%", width: 2, height: 28, transform: "translate(-50%,-50%)", borderRadius: 1, background: "var(--border-strong)" }} />
      </div>

      {/* Right: summary + turns */}
      <div style={{ flex: 1, minWidth: 0, display: "flex", flexDirection: "column", gap: 8 }}>
        <div style={{ background: "var(--surface-card)", border: "1px solid var(--border-default)", borderRadius: 8, padding: 10 }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "baseline" }}>
            <span style={{ fontSize: 13, fontWeight: 600, color: "var(--text-primary)" }}>{selected.title}</span>
            <span style={{ fontSize: 10, color: "var(--text-tertiary)", fontFamily: "var(--font-mono)" }}>{selected.id} · {selected.first}–{selected.last}</span>
          </div>
          <div style={{ display: "flex", gap: 20, marginTop: 8, flexWrap: "wrap" }}>
            <Stat label="Total Cost" value={`$${selected.cost.toFixed(4)}`} />
            <Stat label="Total Tokens" value={fmtTok(selected.promptTok + selected.complTok)} />
            <Stat label="Avg ROI" value={`${(selected.turns.reduce((s, t) => s + t.roi, 0) / selected.turns.length).toFixed(1)}%`} valueColor="var(--status-ok-text)" />
            <Stat label="Turns" value={selected.turns.length} />
          </div>
        </div>

        <div style={{ display: "flex", flexDirection: "column", gap: 6, overflowY: "auto" }}>
          {selected.turns.map((t) => {
            const isOpen = expanded === t.n;
            const color = colorForAgent(t.agent);
            return (
              <div key={t.n} style={{ background: "var(--surface-card)", border: `1px solid ${color}66`, borderLeft: `3px solid ${color}`, borderRadius: 8, padding: 10 }}>
                <div onClick={() => setExpanded(isOpen ? null : t.n)} style={{ cursor: "pointer" }}>
                  <div style={{ display: "flex", gap: 8, alignItems: "center" }}>
                    <span style={{ fontSize: 10, color: "var(--text-tertiary)", fontFamily: "var(--font-mono)" }}>{t.n}/{selected.turns.length}</span>
                    <span style={{ fontSize: 12, fontWeight: 600, color: "var(--text-primary)", overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap", flex: 1 }}>{t.req}</span>
                    <span style={{ display: "flex", alignItems: "center", gap: 4, fontSize: 10, color, flexShrink: 0 }}>
                      <span style={{ width: 6, height: 6, borderRadius: "50%", background: color }} />{t.agent}
                    </span>
                    {t.fallback && <span style={{ color: "var(--status-warn-text)", fontSize: 10, fontWeight: 700 }}>⚠ FALLBACK</span>}
                    <span style={{ fontSize: 10, color: "var(--text-tertiary)", fontFamily: "var(--font-mono)", flexShrink: 0 }}>{t.ts}</span>
                  </div>
                  <div style={{ display: "flex", gap: 14, flexWrap: "wrap", marginTop: 6 }}>
                    <Stat label="ROI" value={`${t.roi.toFixed(1)}%`} valueColor={t.roi > 0 ? "var(--status-ok-text)" : "var(--text-tertiary)"} small />
                    <Stat label="Cost" value={`$${t.cost.toFixed(4)}`} small />
                    <Stat label="Tok P/C" value={`${t.promptTok}/${t.complTok}`} small />
                    <Stat label="Steps" value={t.steps} small />
                    <Stat label="Cache" value={`${t.cache}%`} small />
                    <Stat label="TTFT" value={`${t.ttft}ms`} small />
                    <Stat label="Ctx" value={`${t.ctx}%`} small />
                    <Stat label="Model" value={t.model} small />
                  </div>
                </div>
                {isOpen && (
                  <div style={{ marginTop: 10, borderTop: "1px solid var(--border-subtle)", paddingTop: 8 }}>
                    <div style={{ fontSize: 10, textTransform: "uppercase", letterSpacing: "0.05em", color: "var(--text-tertiary)", marginBottom: 6 }}>Routing Decision</div>
                    <div style={{ display: "flex", flexDirection: "column", gap: 2, marginBottom: 8 }}>
                      {t.log.map(([status, msg], i) => (
                        <div key={i} style={{
                          fontSize: 11, fontFamily: "var(--font-mono)", padding: "3px 6px", borderRadius: 4,
                          background: status === "ok" ? "#10b9811a" : status === "warn" ? "var(--state-hover-amber)" : "#38bdf81a",
                          color: status === "ok" ? "var(--status-ok-text)" : status === "warn" ? "var(--status-warn-text)" : "var(--status-info-text)",
                        }}>{msg}</div>
                      ))}
                    </div>
                    <div style={{ maxHeight: "8rem", overflowY: "auto", fontSize: 11, color: "var(--text-secondary)", background: "var(--surface-page)", borderRadius: 6, padding: 8 }}>
                      <div style={{ marginBottom: 6 }}><b style={{ color: "var(--text-tertiary)" }}>Request:</b> {t.req}</div>
                      {t.res && <div><b style={{ color: "var(--text-tertiary)" }}>Response:</b> {t.res}</div>}
                    </div>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}

function Stat({ label, value, valueColor, small }) {
  return (
    <div style={{ display: "flex", flexDirection: "column", gap: 1 }}>
      <span style={{ fontSize: small ? 9 : 10, letterSpacing: "0.05em", textTransform: "uppercase", color: "var(--text-tertiary)", whiteSpace: "nowrap" }}>{label}</span>
      <span style={{ fontFamily: "var(--font-mono)", fontSize: small ? 11 : 12, color: valueColor || "var(--text-primary)" }}>{value}</span>
    </div>
  );
}
