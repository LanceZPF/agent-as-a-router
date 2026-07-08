import React from "react";

const TIER = (pct) => (pct >= 85 ? "var(--status-ok-fill)" : pct >= 70 ? "var(--sky-400)" : "var(--status-warn-fill)");

export function ProgressBar({ percent, color, height = 6 }) {
  const clamped = Math.max(0, Math.min(100, percent));
  return (
    <div
      style={{
        height,
        background: "var(--surface-track)",
        borderRadius: "var(--radius-sm)",
        overflow: "hidden",
        width: "100%",
      }}
    >
      <div
        style={{
          height: "100%",
          width: `${clamped}%`,
          background: color || TIER(clamped),
          transition: "width var(--duration-fast) var(--ease-standard)",
        }}
      />
    </div>
  );
}
