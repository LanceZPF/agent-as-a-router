import React from "react";

// Static positioning tooltip (hover-driven). The source uses a single body-level fixed element
// repositioned in JS (wwwroot/js/tooltips.js) so it's never clipped by scroll containers; this
// component reproduces that with a simple absolutely-positioned wrapper suitable for prototyping.
export function Tooltip({ label, children, placement = "bottom" }) {
  const [visible, setVisible] = React.useState(false);
  return (
    <span
      style={{ position: "relative", display: "inline-flex", cursor: "help" }}
      onMouseEnter={() => setVisible(true)}
      onMouseLeave={() => setVisible(false)}
    >
      {children}
      {visible && (
        <span
          style={{
            position: "absolute",
            zIndex: 100,
            [placement === "top" ? "bottom" : "top"]: "calc(100% + 6px)",
            left: 0,
            maxWidth: 280,
            padding: "6px 8px",
            borderRadius: 4,
            background: "var(--surface-card)",
            border: "1px solid var(--border-strong)",
            color: "var(--slate-300)",
            fontFamily: "var(--font-sans)",
            fontSize: 11,
            lineHeight: 1.4,
            boxShadow: "var(--shadow-tooltip)",
            pointerEvents: "none",
            whiteSpace: "nowrap",
          }}
        >
          {label}
        </span>
      )}
    </span>
  );
}
