import React from "react";

// The source dashboard renders "small inline SVG glyphs" (Components/Icon.razor) but no actual
// glyph files are committed to the repo to copy in. Lucide is used here as the closest CDN match —
// same minimal 1.5-2px stroke style already implied by the dashboard's icon usage (settings gear,
// search, chevrons, alert triangle). This is a flagged substitution; see readme.md Iconography.
const SLUG = {
  settings: "settings",
  search: "search",
  close: "x",
  "chevron-down": "chevron-down",
  "chevron-right": "chevron-right",
  "alert-triangle": "triangle-alert",
  info: "info",
  "check-circle": "check-circle",
  "alert-circle": "alert-circle",
  activity: "activity",
  dot: "circle",
  "trend-up": "trending-up",
  bot: "bot",
};

export function Icon({ name, size = 16, color = "var(--text-secondary)", style, ...rest }) {
  const slug = SLUG[name] || name;
  const url = `https://unpkg.com/lucide-static@latest/icons/${slug}.svg`;
  return (
    <span
      role="img"
      aria-label={name}
      style={{
        display: "inline-block",
        width: size,
        height: size,
        backgroundColor: color,
        WebkitMaskImage: `url(${url})`,
        maskImage: `url(${url})`,
        WebkitMaskSize: "contain",
        maskSize: "contain",
        WebkitMaskRepeat: "no-repeat",
        maskRepeat: "no-repeat",
        WebkitMaskPosition: "center",
        maskPosition: "center",
        flexShrink: 0,
        ...style,
      }}
      {...rest}
    />
  );
}
