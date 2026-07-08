import React from "react";
import { costData, costLabels, agentRoi } from "./mockData.js";

export function CostAnalytics() {
  const max = Math.max(...costData);
  const w = 640, h = 160, pad = 20;
  const pts = costData.map((v, i) => {
    const x = pad + (i / (costData.length - 1)) * (w - pad * 2);
    const y = h - pad - (v / max) * (h - pad * 2);
    return [x, y];
  });
  const path = pts.map((p, i) => (i === 0 ? `M${p[0]},${p[1]}` : `L${p[0]},${p[1]}`)).join(" ");
  const area = `${path} L${pts[pts.length - 1][0]},${h - pad} L${pts[0][0]},${h - pad} Z`;

  return (
    <div style={{ padding: 16, display: "flex", flexDirection: "column", gap: 16, height: "100%", boxSizing: "border-box", overflowY: "auto" }}>
      <div style={{ background: "var(--surface-card)", border: "1px solid var(--border-default)", borderRadius: 8, padding: 14 }}>
        <div style={{ fontSize: 12, color: "var(--text-secondary)", marginBottom: 8, fontWeight: 600 }}>Cumulative Savings</div>
        <svg viewBox={`0 0 ${w} ${h}`} style={{ width: "100%", height: 180 }}>
          <path d={area} fill="#10b98122" stroke="none" />
          <path d={path} fill="none" stroke="var(--status-ok-fill)" strokeWidth="2" />
          {pts.map((p, i) => (i % 3 === 0 ? <circle key={i} cx={p[0]} cy={p[1]} r="2.5" fill="var(--status-ok-fill)" /> : null))}
        </svg>
        <div style={{ display: "flex", justifyContent: "space-between", fontSize: 10, color: "var(--text-tertiary)", fontFamily: "var(--font-mono)" }}>
          <span>{costLabels[0]}</span><span>{costLabels[costLabels.length - 1]}</span>
        </div>
      </div>

      <div style={{ background: "var(--surface-card)", border: "1px solid var(--border-default)", borderRadius: 8, padding: 14 }}>
        <div style={{ fontSize: 12, color: "var(--text-secondary)", marginBottom: 10, fontWeight: 600 }}>Cost Reduction % by Agent</div>
        <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
          {agentRoi.map((a) => {
            const color = a.reduction >= 85 ? "var(--status-ok-fill)" : a.reduction >= 70 ? "var(--sky-400)" : "var(--status-warn-fill)";
            return (
              <div key={a.agent} style={{ display: "flex", alignItems: "center", gap: 10 }}>
                <span style={{ width: 150, fontSize: 11, color: "var(--text-secondary)", flexShrink: 0 }}>{a.agent}</span>
                <div style={{ flex: 1, height: 14, background: "var(--surface-track)", borderRadius: 3, position: "relative", overflow: "hidden" }}>
                  <div style={{ height: "100%", width: `${a.reduction}%`, background: color }} />
                </div>
                <span style={{ width: 46, fontFamily: "var(--font-mono)", fontSize: 11, color, textAlign: "right", flexShrink: 0 }}>{a.reduction.toFixed(1)}%</span>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}
