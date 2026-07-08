import React from "react";
import { tokenBuckets, modelShares } from "./mockData.js";

export function ModelDistribution() {
  const [range, setRange] = React.useState("Month");
  const max = Math.max(...tokenBuckets.map((b) => b.prompt + b.completion));
  const total = modelShares.reduce((s, m) => s + m.value, 0);
  let acc = 0;
  const segs = modelShares.map((m) => {
    const start = (acc / total) * 360; acc += m.value;
    const end = (acc / total) * 360;
    return { ...m, start, end };
  });
  function seg(cx, cy, r, r2, start, end) {
    const toXY = (deg, rad) => [cx + rad * Math.sin((deg * Math.PI) / 180), cy - rad * Math.cos((deg * Math.PI) / 180)];
    const [x1, y1] = toXY(start, r); const [x2, y2] = toXY(end, r);
    const [x3, y3] = toXY(end, r2); const [x4, y4] = toXY(start, r2);
    const large = end - start > 180 ? 1 : 0;
    return `M${x1},${y1} A${r},${r} 0 ${large} 1 ${x2},${y2} L${x3},${y3} A${r2},${r2} 0 ${large} 0 ${x4},${y4} Z`;
  }

  return (
    <div style={{ padding: 16, display: "flex", flexDirection: "column", gap: 16, height: "100%", boxSizing: "border-box", overflowY: "auto" }}>
      <div style={{ display: "flex", gap: 6, alignItems: "center" }}>
        {["Day", "Month", "3-Month", "6-Month", "Year"].map((r) => (
          <button key={r} onClick={() => setRange(r)} style={{
            background: r === range ? "var(--sky-400)" : "var(--slate-700)", color: r === range ? "var(--slate-900)" : "var(--text-secondary)",
            border: "none", borderRadius: 6, padding: "5px 10px", fontSize: 11, fontWeight: 600, cursor: "pointer",
          }}>{r}</button>
        ))}
        <div style={{ marginLeft: "auto", display: "flex", gap: 6, alignItems: "center", fontSize: 11, color: "var(--text-tertiary)" }}>
          From <input defaultValue="2026-06-01" style={{ background: "var(--surface-page)", border: "1px solid var(--border-default)", borderRadius: 4, color: "var(--text-primary)", fontSize: 11, padding: "3px 6px" }} />
          To <input defaultValue="2026-07-01" style={{ background: "var(--surface-page)", border: "1px solid var(--border-default)", borderRadius: 4, color: "var(--text-primary)", fontSize: 11, padding: "3px 6px" }} />
        </div>
      </div>

      <div style={{ background: "var(--surface-card)", border: "1px solid var(--border-default)", borderRadius: 8, padding: 14 }}>
        <div style={{ fontSize: 12, color: "var(--text-secondary)", marginBottom: 10, fontWeight: 600 }}>Token Volume — Prompt vs Completion</div>
        <div style={{ display: "flex", alignItems: "flex-end", gap: 10, height: 120 }}>
          {tokenBuckets.map((b) => (
            <div key={b.slot} style={{ flex: 1, display: "flex", flexDirection: "column", alignItems: "center", gap: 4 }}>
              <div style={{ display: "flex", gap: 3, alignItems: "flex-end", height: 100 }}>
                <div style={{ width: 10, height: `${(b.prompt / max) * 100}%`, background: "var(--sky-400)", borderRadius: "2px 2px 0 0" }} />
                <div style={{ width: 10, height: `${(b.completion / max) * 100}%`, background: "var(--emerald-500)", borderRadius: "2px 2px 0 0" }} />
              </div>
              <span style={{ fontSize: 10, color: "var(--text-tertiary)" }}>{b.slot}</span>
            </div>
          ))}
        </div>
        <div style={{ display: "flex", gap: 14, marginTop: 8, fontSize: 11, color: "var(--text-secondary)" }}>
          <span><span style={{ display: "inline-block", width: 8, height: 8, background: "var(--sky-400)", borderRadius: 2, marginRight: 5 }} />Prompt</span>
          <span><span style={{ display: "inline-block", width: 8, height: 8, background: "var(--emerald-500)", borderRadius: 2, marginRight: 5 }} />Completion</span>
        </div>
      </div>

      <div style={{ background: "var(--surface-card)", border: "1px solid var(--border-default)", borderRadius: 8, padding: 14, display: "flex", gap: 20, alignItems: "center" }}>
        <svg width="140" height="140" viewBox="-70 -70 140 140">
          {segs.map((s) => <path key={s.model} d={seg(0, 0, 68, 42, s.start, s.end)} fill={s.color} />)}
        </svg>
        <div style={{ display: "flex", flexDirection: "column", gap: 6 }}>
          {modelShares.map((m) => (
            <div key={m.model} style={{ display: "flex", alignItems: "center", gap: 6, fontSize: 11, color: "var(--text-secondary)" }}>
              <span style={{ width: 8, height: 8, borderRadius: 2, background: m.color }} />
              <span style={{ fontFamily: "var(--font-mono)" }}>{m.model}</span>
              <span style={{ marginLeft: "auto", color: "var(--text-primary)" }}>{m.value}%</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
