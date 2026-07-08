import React from "react";

// Standard card: 1px slate-700 border, slate-800 fill, lg radius, no shadow. Optional agentColor
// tints the left border and background the way ConversationCard/TurnCard tint by the selected
// agent's deterministic color (see components/data/AgentChip.jsx + tokens/colors.css agent-* vars).
export function Card({ agentColor, hoverable = true, children, style, ...rest }) {
  const [hover, setHover] = React.useState(false);
  return (
    <div
      onMouseEnter={() => hoverable && setHover(true)}
      onMouseLeave={() => hoverable && setHover(false)}
      style={{
        background: hover ? "var(--surface-card-hover)" : "var(--surface-card)",
        border: `1px solid ${agentColor ? agentColor + "66" : "var(--border-default)"}`,
        borderLeft: agentColor ? `3px solid ${agentColor}` : undefined,
        borderRadius: "var(--radius-lg)",
        padding: 12,
        fontFamily: "var(--font-sans)",
        transition: "background-color var(--duration-fast) ease, border-color var(--duration-fast) ease",
        ...style,
      }}
      {...rest}
    >
      {children}
    </div>
  );
}
