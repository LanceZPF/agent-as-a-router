import React from "react";

export function Button({ variant = "primary", size = "md", children, style, ...rest }) {
  const PAD = size === "sm" ? "5px 10px" : "8px 14px";
  const FONT = size === "sm" ? 12 : 13;
  const base = {
    fontFamily: "var(--font-sans)",
    fontWeight: 600,
    fontSize: FONT,
    padding: PAD,
    borderRadius: "var(--radius-md)",
    cursor: "pointer",
    transition: "background-color var(--duration-fast) var(--ease-standard), border-color var(--duration-fast) var(--ease-standard), opacity var(--duration-fast) var(--ease-standard)",
    border: "1px solid transparent",
  };
  const VARIANT = {
    primary: { background: "var(--sky-400)", color: "var(--slate-900)", border: "1px solid var(--sky-400)" },
    secondary: { background: "var(--slate-700)", color: "var(--text-primary)", border: "1px solid var(--border-default)" },
    ghost: { background: "transparent", color: "var(--text-secondary)", border: "1px solid var(--border-default)" },
    destructive: { background: "transparent", color: "var(--status-critical-text)", border: "1px solid var(--border-critical)" },
  };
  return (
    <button
      style={{ ...base, ...VARIANT[variant], ...style }}
      onMouseOver={(e) => (e.currentTarget.style.opacity = "0.8")}
      onMouseOut={(e) => (e.currentTarget.style.opacity = "1")}
      {...rest}
    >
      {children}
    </button>
  );
}
