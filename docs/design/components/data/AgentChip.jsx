import React from "react";

// Deterministic FNV-1a hash → palette color, mirroring Utils/ColorUtils.cs exactly so a given
// agent name always renders the same color across sessions/launches.
const PALETTE = ["#10b981","#38bdf8","#818cf8","#fb7185","#f59e0b","#a78bfa","#14b8a6","#0ea5e9","#6366f1","#ec4899","#f97316","#06b6d4"];

export function colorForAgent(name) {
  if (!name) return PALETTE[0];
  let hash = 2166136261;
  for (let i = 0; i < name.length; i++) {
    hash = Math.imul(hash ^ name.charCodeAt(i), 16777619);
  }
  return PALETTE[Math.abs(hash) % PALETTE.length];
}

export function AgentChip({ name, size = 8 }) {
  const color = colorForAgent(name);
  return (
    <span style={{ display: "inline-flex", alignItems: "center", gap: 6, fontSize: 12, color: "var(--text-primary)", fontFamily: "var(--font-sans)" }}>
      <span style={{ width: size, height: size, borderRadius: "50%", background: color, flexShrink: 0 }} />
      {name}
    </span>
  );
}
