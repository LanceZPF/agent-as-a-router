import React from "react";

const TONE = {
  ok: { bg: "#10b9811a", text: "var(--status-ok-text)", border: "#10b98140" },
  warning: { bg: "var(--state-hover-amber)", text: "var(--status-warn-text)", border: "var(--border-warning)" },
  critical: { bg: "var(--state-hover-red)", text: "var(--status-critical-text)", border: "var(--border-critical)" },
  info: { bg: "#38bdf81a", text: "var(--status-info-text)", border: "#38bdf840" },
  neutral: { bg: "var(--slate-700)", text: "var(--text-primary)", border: "var(--border-default)" },
};

export function Badge({ tone = "neutral", children, style }) {
  const t = TONE[tone] || TONE.neutral;
  return (
    <span
      style={{
        display: "inline-flex",
        alignItems: "center",
        gap: 4,
        background: t.bg,
        color: t.text,
        border: `1px solid ${t.border}`,
        fontSize: 11,
        fontWeight: 600,
        letterSpacing: "0.03em",
        padding: "3px 9px",
        borderRadius: "var(--radius-md)",
        fontFamily: "var(--font-sans)",
        whiteSpace: "nowrap",
        ...style,
      }}
    >
      {children}
    </span>
  );
}
