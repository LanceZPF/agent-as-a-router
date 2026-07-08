import React from "react";
import { Tooltip } from "../feedback/Tooltip.jsx";

// A single "LABEL / value" stat used in the compact stat strips (turn cards, conversation summary).
export function StatItem({ label, value, tooltip, valueColor }) {
  const content = (
    <div style={{ display: "flex", flexDirection: "column", gap: 1 }}>
      <span
        style={{
          fontSize: 10,
          letterSpacing: "0.05em",
          textTransform: "uppercase",
          color: "var(--text-tertiary)",
          whiteSpace: "nowrap",
        }}
      >
        {label}
      </span>
      <span style={{ fontFamily: "var(--font-mono)", fontSize: 12, color: valueColor || "var(--text-primary)" }}>
        {value}
      </span>
    </div>
  );
  return tooltip ? <Tooltip label={tooltip}>{content}</Tooltip> : content;
}
