import React from "react";

export function Tabs({ tabs, active, onChange }) {
  return (
    <div style={{ display: "flex", gap: 4, borderBottom: "1px solid var(--border-default)" }}>
      {tabs.map((t) => {
        const isActive = t.id === active;
        return (
          <button
            key={t.id}
            onClick={() => onChange && onChange(t.id)}
            className="tab-indicator"
            style={{
              appearance: "none",
              background: "transparent",
              border: "none",
              borderBottom: `2px solid ${isActive ? "var(--sky-400)" : "transparent"}`,
              color: isActive ? "var(--text-heading)" : "var(--text-secondary)",
              fontFamily: "var(--font-sans)",
              fontSize: 13,
              fontWeight: isActive ? 600 : 500,
              padding: "10px 14px",
              cursor: "pointer",
              transition: "color var(--duration-fast) var(--ease-standard), border-color var(--duration-fast) var(--ease-standard)",
            }}
          >
            {t.label}
          </button>
        );
      })}
    </div>
  );
}
