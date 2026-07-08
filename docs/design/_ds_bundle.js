/* @ds-bundle: {"format":4,"namespace":"AgenticRouterDesignSystem_7f60e2","components":[{"name":"Icon","sourcePath":"components/core/Icon.jsx"},{"name":"AgentChip","sourcePath":"components/data/AgentChip.jsx"},{"name":"Card","sourcePath":"components/data/Card.jsx"},{"name":"StatItem","sourcePath":"components/data/StatItem.jsx"},{"name":"Badge","sourcePath":"components/feedback/Badge.jsx"},{"name":"ProgressBar","sourcePath":"components/feedback/ProgressBar.jsx"},{"name":"Tooltip","sourcePath":"components/feedback/Tooltip.jsx"},{"name":"Button","sourcePath":"components/forms/Button.jsx"},{"name":"Input","sourcePath":"components/forms/Input.jsx"},{"name":"Tabs","sourcePath":"components/navigation/Tabs.jsx"},{"name":"Modal","sourcePath":"components/overlay/Modal.jsx"},{"name":"CostAnalytics","sourcePath":"ui_kits/dashboard/CostAnalytics.jsx"},{"name":"Dashboard","sourcePath":"ui_kits/dashboard/Dashboard.jsx"},{"name":"Governance","sourcePath":"ui_kits/dashboard/Governance.jsx"},{"name":"Header","sourcePath":"ui_kits/dashboard/Header.jsx"},{"name":"LiveStream","sourcePath":"ui_kits/dashboard/LiveStream.jsx"},{"name":"ModelDistribution","sourcePath":"ui_kits/dashboard/ModelDistribution.jsx"},{"name":"SettingsModal","sourcePath":"ui_kits/dashboard/SettingsModal.jsx"}],"sourceHashes":{"components/core/Icon.jsx":"ad150d6ebbf3","components/data/AgentChip.jsx":"00847880bc84","components/data/Card.jsx":"d39ac895406c","components/data/StatItem.jsx":"0bc56f434afd","components/feedback/Badge.jsx":"379f57fbc545","components/feedback/ProgressBar.jsx":"b2e8c7eae630","components/feedback/Tooltip.jsx":"bbbd94e7be49","components/forms/Button.jsx":"05c28cf9602f","components/forms/Input.jsx":"8020f4cf1d2e","components/navigation/Tabs.jsx":"3f032cf7c9a5","components/overlay/Modal.jsx":"312945b0fa00","ui_kits/dashboard/CostAnalytics.jsx":"7477c26af564","ui_kits/dashboard/Dashboard.jsx":"26cc571f2e2e","ui_kits/dashboard/Governance.jsx":"8e41a77b1904","ui_kits/dashboard/Header.jsx":"7537f6ed1053","ui_kits/dashboard/LiveStream.jsx":"124aaaec1caf","ui_kits/dashboard/ModelDistribution.jsx":"041ed4ab3360","ui_kits/dashboard/SettingsModal.jsx":"6c5b1c76e6f6","ui_kits/dashboard/mockData.js":"26706b8ad508"},"inlinedExternals":[],"unexposedExports":[{"name":"agentRoi","sourcePath":"ui_kits/dashboard/mockData.js"},{"name":"colorForAgent","sourcePath":"components/data/AgentChip.jsx"},{"name":"conversations","sourcePath":"ui_kits/dashboard/mockData.js"},{"name":"costData","sourcePath":"ui_kits/dashboard/mockData.js"},{"name":"costLabels","sourcePath":"ui_kits/dashboard/mockData.js"},{"name":"modelShares","sourcePath":"ui_kits/dashboard/mockData.js"},{"name":"providers","sourcePath":"ui_kits/dashboard/mockData.js"},{"name":"tokenBuckets","sourcePath":"ui_kits/dashboard/mockData.js"}]} */

(() => {

const __ds_ns = (window.AgenticRouterDesignSystem_7f60e2 = window.AgenticRouterDesignSystem_7f60e2 || {});

const __ds_scope = {};

(__ds_ns.__errors = __ds_ns.__errors || []);

// components/core/Icon.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
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
  bot: "bot"
};
function Icon({
  name,
  size = 16,
  color = "var(--text-secondary)",
  style,
  ...rest
}) {
  const slug = SLUG[name] || name;
  const url = `https://unpkg.com/lucide-static@latest/icons/${slug}.svg`;
  return /*#__PURE__*/React.createElement("span", _extends({
    role: "img",
    "aria-label": name,
    style: {
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
      ...style
    }
  }, rest));
}
Object.assign(__ds_scope, { Icon });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/core/Icon.jsx", error: String((e && e.message) || e) }); }

// components/data/AgentChip.jsx
try { (() => {
// Deterministic FNV-1a hash → palette color, mirroring Utils/ColorUtils.cs exactly so a given
// agent name always renders the same color across sessions/launches.
const PALETTE = ["#10b981", "#38bdf8", "#818cf8", "#fb7185", "#f59e0b", "#a78bfa", "#14b8a6", "#0ea5e9", "#6366f1", "#ec4899", "#f97316", "#06b6d4"];
function colorForAgent(name) {
  if (!name) return PALETTE[0];
  let hash = 2166136261;
  for (let i = 0; i < name.length; i++) {
    hash = Math.imul(hash ^ name.charCodeAt(i), 16777619);
  }
  return PALETTE[Math.abs(hash) % PALETTE.length];
}
function AgentChip({
  name,
  size = 8
}) {
  const color = colorForAgent(name);
  return /*#__PURE__*/React.createElement("span", {
    style: {
      display: "inline-flex",
      alignItems: "center",
      gap: 6,
      fontSize: 12,
      color: "var(--text-primary)",
      fontFamily: "var(--font-sans)"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: size,
      height: size,
      borderRadius: "50%",
      background: color,
      flexShrink: 0
    }
  }), name);
}
Object.assign(__ds_scope, { colorForAgent, AgentChip });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/data/AgentChip.jsx", error: String((e && e.message) || e) }); }

// components/data/Card.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
// Standard card: 1px slate-700 border, slate-800 fill, lg radius, no shadow. Optional agentColor
// tints the left border and background the way ConversationCard/TurnCard tint by the selected
// agent's deterministic color (see components/data/AgentChip.jsx + tokens/colors.css agent-* vars).
function Card({
  agentColor,
  hoverable = true,
  children,
  style,
  ...rest
}) {
  const [hover, setHover] = React.useState(false);
  return /*#__PURE__*/React.createElement("div", _extends({
    onMouseEnter: () => hoverable && setHover(true),
    onMouseLeave: () => hoverable && setHover(false),
    style: {
      background: hover ? "var(--surface-card-hover)" : "var(--surface-card)",
      border: `1px solid ${agentColor ? agentColor + "66" : "var(--border-default)"}`,
      borderLeft: agentColor ? `3px solid ${agentColor}` : undefined,
      borderRadius: "var(--radius-lg)",
      padding: 12,
      fontFamily: "var(--font-sans)",
      transition: "background-color var(--duration-fast) ease, border-color var(--duration-fast) ease",
      ...style
    }
  }, rest), children);
}
Object.assign(__ds_scope, { Card });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/data/Card.jsx", error: String((e && e.message) || e) }); }

// components/feedback/Badge.jsx
try { (() => {
const TONE = {
  ok: {
    bg: "#10b9811a",
    text: "var(--status-ok-text)",
    border: "#10b98140"
  },
  warning: {
    bg: "var(--state-hover-amber)",
    text: "var(--status-warn-text)",
    border: "var(--border-warning)"
  },
  critical: {
    bg: "var(--state-hover-red)",
    text: "var(--status-critical-text)",
    border: "var(--border-critical)"
  },
  info: {
    bg: "#38bdf81a",
    text: "var(--status-info-text)",
    border: "#38bdf840"
  },
  neutral: {
    bg: "var(--slate-700)",
    text: "var(--text-primary)",
    border: "var(--border-default)"
  }
};
function Badge({
  tone = "neutral",
  children,
  style
}) {
  const t = TONE[tone] || TONE.neutral;
  return /*#__PURE__*/React.createElement("span", {
    style: {
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
      ...style
    }
  }, children);
}
Object.assign(__ds_scope, { Badge });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/feedback/Badge.jsx", error: String((e && e.message) || e) }); }

// components/feedback/ProgressBar.jsx
try { (() => {
const TIER = pct => pct >= 85 ? "var(--status-ok-fill)" : pct >= 70 ? "var(--sky-400)" : "var(--status-warn-fill)";
function ProgressBar({
  percent,
  color,
  height = 6
}) {
  const clamped = Math.max(0, Math.min(100, percent));
  return /*#__PURE__*/React.createElement("div", {
    style: {
      height,
      background: "var(--surface-track)",
      borderRadius: "var(--radius-sm)",
      overflow: "hidden",
      width: "100%"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      height: "100%",
      width: `${clamped}%`,
      background: color || TIER(clamped),
      transition: "width var(--duration-fast) var(--ease-standard)"
    }
  }));
}
Object.assign(__ds_scope, { ProgressBar });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/feedback/ProgressBar.jsx", error: String((e && e.message) || e) }); }

// components/feedback/Tooltip.jsx
try { (() => {
// Static positioning tooltip (hover-driven). The source uses a single body-level fixed element
// repositioned in JS (wwwroot/js/tooltips.js) so it's never clipped by scroll containers; this
// component reproduces that with a simple absolutely-positioned wrapper suitable for prototyping.
function Tooltip({
  label,
  children,
  placement = "bottom"
}) {
  const [visible, setVisible] = React.useState(false);
  return /*#__PURE__*/React.createElement("span", {
    style: {
      position: "relative",
      display: "inline-flex",
      cursor: "help"
    },
    onMouseEnter: () => setVisible(true),
    onMouseLeave: () => setVisible(false)
  }, children, visible && /*#__PURE__*/React.createElement("span", {
    style: {
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
      whiteSpace: "nowrap"
    }
  }, label));
}
Object.assign(__ds_scope, { Tooltip });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/feedback/Tooltip.jsx", error: String((e && e.message) || e) }); }

// components/data/StatItem.jsx
try { (() => {
// A single "LABEL / value" stat used in the compact stat strips (turn cards, conversation summary).
function StatItem({
  label,
  value,
  tooltip,
  valueColor
}) {
  const content = /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 1
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 10,
      letterSpacing: "0.05em",
      textTransform: "uppercase",
      color: "var(--text-tertiary)",
      whiteSpace: "nowrap"
    }
  }, label), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-mono)",
      fontSize: 12,
      color: valueColor || "var(--text-primary)"
    }
  }, value));
  return tooltip ? /*#__PURE__*/React.createElement(__ds_scope.Tooltip, {
    label: tooltip
  }, content) : content;
}
Object.assign(__ds_scope, { StatItem });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/data/StatItem.jsx", error: String((e && e.message) || e) }); }

// components/forms/Button.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Button({
  variant = "primary",
  size = "md",
  children,
  style,
  ...rest
}) {
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
    border: "1px solid transparent"
  };
  const VARIANT = {
    primary: {
      background: "var(--sky-400)",
      color: "var(--slate-900)",
      border: "1px solid var(--sky-400)"
    },
    secondary: {
      background: "var(--slate-700)",
      color: "var(--text-primary)",
      border: "1px solid var(--border-default)"
    },
    ghost: {
      background: "transparent",
      color: "var(--text-secondary)",
      border: "1px solid var(--border-default)"
    },
    destructive: {
      background: "transparent",
      color: "var(--status-critical-text)",
      border: "1px solid var(--border-critical)"
    }
  };
  return /*#__PURE__*/React.createElement("button", _extends({
    style: {
      ...base,
      ...VARIANT[variant],
      ...style
    },
    onMouseOver: e => e.currentTarget.style.opacity = "0.8",
    onMouseOut: e => e.currentTarget.style.opacity = "1"
  }, rest), children);
}
Object.assign(__ds_scope, { Button });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/forms/Button.jsx", error: String((e && e.message) || e) }); }

// components/forms/Input.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
function Input({
  type = "text",
  placeholder,
  value,
  onChange,
  style,
  ...rest
}) {
  return /*#__PURE__*/React.createElement("input", _extends({
    type: type,
    placeholder: placeholder,
    value: value,
    onChange: onChange,
    style: {
      background: "var(--surface-inset)",
      border: "1px solid var(--border-default)",
      color: "var(--text-primary)",
      outline: "none",
      borderRadius: "var(--radius-md)",
      padding: "6px 10px",
      fontFamily: "var(--font-sans)",
      fontSize: 13,
      transition: "border-color var(--duration-fast) ease",
      ...style
    },
    onFocus: e => e.currentTarget.style.borderColor = "var(--focus-ring)",
    onBlur: e => e.currentTarget.style.borderColor = "var(--border-default)"
  }, rest));
}
Object.assign(__ds_scope, { Input });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/forms/Input.jsx", error: String((e && e.message) || e) }); }

// components/navigation/Tabs.jsx
try { (() => {
function Tabs({
  tabs,
  active,
  onChange
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 4,
      borderBottom: "1px solid var(--border-default)"
    }
  }, tabs.map(t => {
    const isActive = t.id === active;
    return /*#__PURE__*/React.createElement("button", {
      key: t.id,
      onClick: () => onChange && onChange(t.id),
      className: "tab-indicator",
      style: {
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
        transition: "color var(--duration-fast) var(--ease-standard), border-color var(--duration-fast) var(--ease-standard)"
      }
    }, t.label);
  }));
}
Object.assign(__ds_scope, { Tabs });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/navigation/Tabs.jsx", error: String((e && e.message) || e) }); }

// components/overlay/Modal.jsx
try { (() => {
function Modal({
  open,
  title,
  onClose,
  children
}) {
  if (!open) return null;
  return /*#__PURE__*/React.createElement("div", {
    style: {
      position: "fixed",
      inset: 0,
      background: "rgba(15,23,42,0.7)",
      backdropFilter: "blur(2px)",
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      zIndex: 50
    },
    onClick: onClose
  }, /*#__PURE__*/React.createElement("div", {
    onClick: e => e.stopPropagation(),
    style: {
      background: "var(--surface-card)",
      border: "1px solid var(--border-default)",
      borderRadius: "var(--radius-lg)",
      padding: 20,
      width: 380,
      maxWidth: "90vw",
      fontFamily: "var(--font-sans)",
      color: "var(--text-primary)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      justifyContent: "space-between",
      alignItems: "center",
      marginBottom: 14
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 15,
      fontWeight: 600,
      color: "var(--text-heading)"
    }
  }, title), /*#__PURE__*/React.createElement("button", {
    onClick: onClose,
    style: {
      background: "transparent",
      border: "none",
      color: "var(--text-tertiary)",
      cursor: "pointer",
      fontSize: 16,
      lineHeight: 1
    }
  }, "\u2715")), children));
}
Object.assign(__ds_scope, { Modal });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/overlay/Modal.jsx", error: String((e && e.message) || e) }); }

// ui_kits/dashboard/SettingsModal.jsx
try { (() => {
function SettingsModal({
  open,
  onClose
}) {
  const [resetWord, setResetWord] = React.useState("");
  const [purgeWord, setPurgeWord] = React.useState("");
  if (!open) return null;
  return /*#__PURE__*/React.createElement("div", {
    onClick: onClose,
    style: {
      position: "fixed",
      inset: 0,
      background: "rgba(15,23,42,0.7)",
      backdropFilter: "blur(2px)",
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      zIndex: 50
    }
  }, /*#__PURE__*/React.createElement("div", {
    onClick: e => e.stopPropagation(),
    style: {
      background: "var(--surface-card)",
      border: "1px solid var(--border-default)",
      borderRadius: 8,
      padding: 20,
      width: 380,
      maxWidth: "90vw"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      justifyContent: "space-between",
      alignItems: "center",
      marginBottom: 16
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 15,
      fontWeight: 600,
      color: "var(--text-heading)"
    }
  }, "Settings"), /*#__PURE__*/React.createElement("button", {
    onClick: onClose,
    style: {
      background: "transparent",
      border: "none",
      color: "var(--text-tertiary)",
      cursor: "pointer",
      fontSize: 16
    }
  }, "\u2715")), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 11,
      textTransform: "uppercase",
      letterSpacing: "0.05em",
      color: "var(--status-critical-text)",
      marginBottom: 10
    }
  }, "Destructive Actions Zone"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 12
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      border: "1px solid var(--border-critical)",
      borderRadius: 8,
      padding: 12
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 12,
      color: "var(--text-secondary)",
      marginBottom: 8
    }
  }, "Type ", /*#__PURE__*/React.createElement("b", {
    style: {
      color: "var(--text-primary)"
    }
  }, "RESET"), " to enable"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 8
    }
  }, /*#__PURE__*/React.createElement("input", {
    value: resetWord,
    onChange: e => setResetWord(e.target.value),
    placeholder: "RESET",
    style: {
      flex: 1,
      background: "var(--surface-page)",
      border: "1px solid var(--border-default)",
      borderRadius: 6,
      color: "var(--text-primary)",
      fontSize: 12,
      padding: "6px 8px"
    }
  }), /*#__PURE__*/React.createElement("button", {
    disabled: resetWord !== "RESET",
    onClick: onClose,
    style: {
      background: "transparent",
      border: "1px solid var(--border-critical)",
      color: resetWord === "RESET" ? "var(--status-critical-text)" : "var(--text-muted)",
      borderRadius: 6,
      padding: "6px 12px",
      fontSize: 12,
      fontWeight: 600,
      cursor: resetWord === "RESET" ? "pointer" : "default"
    }
  }, "Reset Stats"))), /*#__PURE__*/React.createElement("div", {
    style: {
      border: "1px solid var(--border-critical)",
      borderRadius: 8,
      padding: 12
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 12,
      color: "var(--text-secondary)",
      marginBottom: 8
    }
  }, "Type ", /*#__PURE__*/React.createElement("b", {
    style: {
      color: "var(--text-primary)"
    }
  }, "PURGE"), " to enable"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 8
    }
  }, /*#__PURE__*/React.createElement("input", {
    value: purgeWord,
    onChange: e => setPurgeWord(e.target.value),
    placeholder: "PURGE",
    style: {
      flex: 1,
      background: "var(--surface-page)",
      border: "1px solid var(--border-default)",
      borderRadius: 6,
      color: "var(--text-primary)",
      fontSize: 12,
      padding: "6px 8px"
    }
  }), /*#__PURE__*/React.createElement("button", {
    disabled: purgeWord !== "PURGE",
    onClick: onClose,
    style: {
      background: "transparent",
      border: "1px solid var(--border-critical)",
      color: purgeWord === "PURGE" ? "var(--status-critical-text)" : "var(--text-muted)",
      borderRadius: 6,
      padding: "6px 12px",
      fontSize: 12,
      fontWeight: 600,
      cursor: purgeWord === "PURGE" ? "pointer" : "default"
    }
  }, "Clear History"))))));
}
Object.assign(__ds_scope, { SettingsModal });
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/dashboard/SettingsModal.jsx", error: String((e && e.message) || e) }); }

// ui_kits/dashboard/mockData.js
try { (() => {
// Shared mock data for the AgenticRouter Dashboard UI kit — trimmed/mirrored from
// src/AgenticRouter.Gui/Models/DashboardData.cs (MockData class) in the source repo.
const conversations = [{
  id: "sess-001",
  title: "Code Review Analysis - PR #4521",
  first: "14:15:32",
  last: "14:22:18",
  cost: 0.04523,
  promptTok: 15456,
  complTok: 3894,
  fallback: false,
  turns: [{
    n: 1,
    agent: "Code Review Bot",
    model: "claude-3-haiku",
    roi: 85.01,
    cost: 0.00631,
    promptTok: 2104,
    complTok: 891,
    steps: 2,
    cache: 0,
    ttft: 245,
    ctx: 26.2,
    ts: "14:15:32",
    req: "Review the diff for PR #4521 (src/auth/token_service.py, 214 changed lines) and flag any security issues.",
    res: "Found 3 issues: missing null check on refresh_token (L87), unbounded retry loop (L112), and the token is logged in plaintext (L145).",
    log: [["ok", "Code diff parsed (214 changed lines)"], ["ok", "Anthropic budget nominal; claude-3-haiku selected"], ["info", "Route Confirmed: claude-3-haiku"]]
  }, {
    n: 2,
    agent: "Code Review Bot",
    model: "claude-3-haiku",
    roi: 84.50,
    cost: 0.00987,
    promptTok: 3240,
    complTok: 1205,
    steps: 3,
    cache: 72,
    ttft: 198,
    ctx: 40.5,
    ts: "14:17:45",
    req: "Suggest a concrete fix for the unbounded retry loop you flagged at L112.",
    res: "Replace the while-loop with a tenacity retry decorator: stop_after_attempt(3) with exponential backoff, re-raising on final failure.",
    log: [["ok", "History carried forward (3,240 prompt tokens)"], ["ok", "Prompt cache hit: 2,333 tokens read from cache"], ["info", "Route Confirmed: claude-3-haiku"]]
  }, {
    n: 3,
    agent: "Code Review Bot",
    model: "claude-3-haiku",
    roi: 83.20,
    cost: 0.01382,
    promptTok: 4567,
    complTok: 1798,
    steps: 4,
    cache: 68,
    ttft: 211,
    ctx: 57.0,
    ts: "14:19:22",
    req: "Apply the same retry pattern to session_service.py and show me the diff.",
    res: "Patched 2 call sites; session_refresh now shares the retry policy. Diff: +18/-9 across session_service.py and retry_util.py.",
    log: [["ok", "History carried forward (4,567 prompt tokens)"], ["warn", "Prompt growth trending up 41% turn-over-turn"], ["ok", "Prompt cache hit: 3,106 tokens read from cache"], ["info", "Route Confirmed: claude-3-haiku"]]
  }, {
    n: 4,
    agent: "Code Review Bot",
    model: "claude-3-haiku",
    roi: 88.75,
    cost: 0.01523,
    promptTok: 5545,
    complTok: 0,
    steps: 1,
    cache: 75,
    ttft: 189,
    ctx: 69.2,
    ts: "14:22:18",
    req: "Summarize all applied changes for the PR description.",
    res: null,
    log: [["ok", "Final summary pass (no completion requested)"], ["ok", "Prompt cache hit: 4,159 tokens read from cache"], ["info", "Route Confirmed: claude-3-haiku"]]
  }]
}, {
  id: "sess-002",
  title: "Data Pipeline Debugging - ETL Job #892",
  first: "14:08:15",
  last: "14:14:42",
  cost: 0.02545,
  promptTok: 8932,
  complTok: 2456,
  fallback: false,
  turns: [{
    n: 1,
    agent: "Data Analyst Wrapper",
    model: "gpt-4o-mini",
    roi: 87.56,
    cost: 0.00534,
    promptTok: 1890,
    complTok: 623,
    steps: 2,
    cache: 0,
    ttft: 312,
    ctx: 23.4,
    ts: "14:08:15",
    req: "ETL job #892 failed at stage 3 with ORA-01555. Here is the log excerpt - what is the root cause?",
    res: "ORA-01555 (snapshot too old): the MERGE reads a table that is mutated mid-run. Isolate the read with a staging CTAS before the merge.",
    log: [["ok", "SQL error log parsed (1,234 lines)"], ["ok", "Short context: gpt-4o-mini sufficient"], ["info", "Route Confirmed: gpt-4o-mini"]]
  }, {
    n: 2,
    agent: "Data Analyst Wrapper",
    model: "gpt-4o-mini",
    roi: 86.20,
    cost: 0.00987,
    promptTok: 3456,
    complTok: 912,
    steps: 3,
    cache: 45,
    ttft: 267,
    ctx: 42.8,
    ts: "14:10:33",
    req: "Here is the EXPLAIN PLAN output for the failing MERGE - where is the long read window coming from?",
    res: "The full-table scan on FACT_ORDERS forces a 40-minute read window. Add a partition-pruning predicate on LOAD_DATE to cut it.",
    log: [["ok", "Query execution plan added to context"], ["ok", "Prompt cache hit: 1,555 tokens read from cache"], ["info", "Route Confirmed: gpt-4o-mini"]]
  }, {
    n: 3,
    agent: "Data Analyst Wrapper",
    model: "gpt-4o-mini",
    roi: 85.90,
    cost: 0.01024,
    promptTok: 3586,
    complTok: 921,
    steps: 2,
    cache: 52,
    ttft: 278,
    ctx: 44.3,
    ts: "14:14:42",
    req: "Validate the revised MERGE statement before I schedule the rerun.",
    res: "The revised statement is safe: partition pruning cuts the read window to roughly 90 seconds, well inside undo retention.",
    log: [["ok", "Fix verification pass (3,586 prompt tokens)"], ["ok", "Prompt cache hit: 1,865 tokens read from cache"], ["info", "Route Confirmed: gpt-4o-mini"]]
  }]
}, {
  id: "sess-003",
  title: "Customer Support - Issue #78234",
  first: "13:52:10",
  last: "14:05:33",
  cost: 0.00456,
  promptTok: 6234,
  complTok: 1845,
  fallback: true,
  turns: [{
    n: 1,
    agent: "Customer Support NLP",
    model: "claude-3-haiku",
    roi: 82.30,
    cost: 0.00456,
    promptTok: 1456,
    complTok: 567,
    steps: 1,
    cache: 0,
    ttft: 189,
    ctx: 18.0,
    ts: "13:52:10",
    req: "Ticket #78234: customer reports being charged twice for the June invoice. Verify and draft a response.",
    res: "Duplicate charge confirmed against payment records. A refund-and-apology draft is ready for agent review.",
    log: [["ok", "Customer inquiry classified: billing dispute"], ["ok", "Anthropic budget nominal; claude-3-haiku selected"], ["info", "Route Confirmed: claude-3-haiku"]]
  }, {
    n: 2,
    agent: "Customer Support NLP",
    model: "fallback-cheapest-local",
    roi: 0,
    cost: 0,
    promptTok: 2389,
    complTok: 734,
    steps: 2,
    cache: 0,
    ttft: 445,
    ctx: 29.5,
    ts: "13:54:28",
    fallback: true,
    req: "The customer replied asking about the refund timeline. Draft a follow-up.",
    res: "Refunds post within 5-7 business days of approval. Suggested reply drafted with the confirmation number.",
    log: [["warn", "Anthropic hourly budget breached; routing restricted"], ["ok", "Fallback routing activated: local model"], ["info", "Route Confirmed: fallback-cheapest-local"]]
  }, {
    n: 3,
    agent: "Customer Support NLP",
    model: "fallback-cheapest-local",
    roi: 0,
    cost: 0,
    promptTok: 2389,
    complTok: 544,
    steps: 1,
    cache: 0,
    ttft: 512,
    ctx: 29.5,
    ts: "14:05:33",
    fallback: true,
    req: "Close out the ticket with a resolution summary.",
    res: "Ticket #78234 resolved: duplicate June charge refunded and a confirmation email queued to the customer.",
    log: [["warn", "Anthropic budget still breached; staying on fallback"], ["ok", "Local model serving request"], ["info", "Route Confirmed: fallback-cheapest-local"]]
  }]
}];
const providers = [{
  id: "openai",
  name: "OpenAI API",
  label: "Production Pool",
  cap: 500,
  spend: 492.80,
  days: 0
}, {
  id: "anthropic",
  name: "Anthropic Claude",
  label: "Inference Pool",
  cap: 300,
  spend: 258.40,
  days: 3
}, {
  id: "gemini",
  name: "Google Gemini",
  label: "Analytics Pool",
  cap: 200,
  spend: 62.40,
  days: 21
}, {
  id: "local",
  name: "Local Inference",
  label: "Fallback Pool",
  cap: 50,
  spend: 8.20,
  days: null
}];
const costData = [0, 4.2, 9.8, 17.6, 26.1, 38.4, 51.2, 67.8, 82.5, 99.1, 112.4, 124.7, 133.2, 138.9, 141.5, 142.36];
const costLabels = ["Jun 1", "Jun 3", "Jun 5", "Jun 7", "Jun 9", "Jun 11", "Jun 13", "Jun 15", "Jun 17", "Jun 19", "Jun 21", "Jun 23", "Jun 25", "Jun 27", "Jun 29", "Jul 1"];
const agentRoi = [{
  agent: "Log Anomaly Detector",
  reduction: 91.67,
  savings: 38.20
}, {
  agent: "SQL Query Optimizer",
  reduction: 87.69,
  savings: 22.40
}, {
  agent: "Data Analyst Wrapper",
  reduction: 85.12,
  savings: 41.80
}, {
  agent: "Customer Support NLP",
  reduction: 84.30,
  savings: 18.60
}, {
  agent: "Summarization Pipeline",
  reduction: 79.50,
  savings: 12.40
}, {
  agent: "Embedding Generator",
  reduction: 78.20,
  savings: 5.80
}, {
  agent: "Code Review Bot",
  reduction: 64.10,
  savings: 2.90
}];
const tokenBuckets = [{
  slot: "Mon",
  prompt: 2840000,
  completion: 980000
}, {
  slot: "Tue",
  prompt: 3120000,
  completion: 1140000
}, {
  slot: "Wed",
  prompt: 4200000,
  completion: 1680000
}, {
  slot: "Thu",
  prompt: 3890000,
  completion: 1520000
}, {
  slot: "Fri",
  prompt: 2960000,
  completion: 1020000
}, {
  slot: "Sat",
  prompt: 1840000,
  completion: 620000
}, {
  slot: "Sun",
  prompt: 1240000,
  completion: 380000
}];
const modelShares = [{
  model: "gpt-4o-mini",
  value: 38,
  color: "#10b981"
}, {
  model: "claude-3-haiku",
  value: 22,
  color: "#38bdf8"
}, {
  model: "gemini-1.5-flash",
  value: 18,
  color: "#818cf8"
}, {
  model: "fallback-local",
  value: 10,
  color: "#f59e0b"
}, {
  model: "claude-3-5-sonnet",
  value: 7,
  color: "#fb7185"
}, {
  model: "text-embedding-3-small",
  value: 5,
  color: "#a78bfa"
}];
Object.assign(__ds_scope, { conversations, providers, costData, costLabels, agentRoi, tokenBuckets, modelShares });
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/dashboard/mockData.js", error: String((e && e.message) || e) }); }

// ui_kits/dashboard/CostAnalytics.jsx
try { (() => {
function CostAnalytics() {
  const max = Math.max(...__ds_scope.costData);
  const w = 640,
    h = 160,
    pad = 20;
  const pts = __ds_scope.costData.map((v, i) => {
    const x = pad + i / (__ds_scope.costData.length - 1) * (w - pad * 2);
    const y = h - pad - v / max * (h - pad * 2);
    return [x, y];
  });
  const path = pts.map((p, i) => i === 0 ? `M${p[0]},${p[1]}` : `L${p[0]},${p[1]}`).join(" ");
  const area = `${path} L${pts[pts.length - 1][0]},${h - pad} L${pts[0][0]},${h - pad} Z`;
  return /*#__PURE__*/React.createElement("div", {
    style: {
      padding: 16,
      display: "flex",
      flexDirection: "column",
      gap: 16,
      height: "100%",
      boxSizing: "border-box",
      overflowY: "auto"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      background: "var(--surface-card)",
      border: "1px solid var(--border-default)",
      borderRadius: 8,
      padding: 14
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 12,
      color: "var(--text-secondary)",
      marginBottom: 8,
      fontWeight: 600
    }
  }, "Cumulative Savings"), /*#__PURE__*/React.createElement("svg", {
    viewBox: `0 0 ${w} ${h}`,
    style: {
      width: "100%",
      height: 180
    }
  }, /*#__PURE__*/React.createElement("path", {
    d: area,
    fill: "#10b98122",
    stroke: "none"
  }), /*#__PURE__*/React.createElement("path", {
    d: path,
    fill: "none",
    stroke: "var(--status-ok-fill)",
    strokeWidth: "2"
  }), pts.map((p, i) => i % 3 === 0 ? /*#__PURE__*/React.createElement("circle", {
    key: i,
    cx: p[0],
    cy: p[1],
    r: "2.5",
    fill: "var(--status-ok-fill)"
  }) : null)), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      justifyContent: "space-between",
      fontSize: 10,
      color: "var(--text-tertiary)",
      fontFamily: "var(--font-mono)"
    }
  }, /*#__PURE__*/React.createElement("span", null, __ds_scope.costLabels[0]), /*#__PURE__*/React.createElement("span", null, __ds_scope.costLabels[__ds_scope.costLabels.length - 1]))), /*#__PURE__*/React.createElement("div", {
    style: {
      background: "var(--surface-card)",
      border: "1px solid var(--border-default)",
      borderRadius: 8,
      padding: 14
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 12,
      color: "var(--text-secondary)",
      marginBottom: 10,
      fontWeight: 600
    }
  }, "Cost Reduction % by Agent"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 8
    }
  }, __ds_scope.agentRoi.map(a => {
    const color = a.reduction >= 85 ? "var(--status-ok-fill)" : a.reduction >= 70 ? "var(--sky-400)" : "var(--status-warn-fill)";
    return /*#__PURE__*/React.createElement("div", {
      key: a.agent,
      style: {
        display: "flex",
        alignItems: "center",
        gap: 10
      }
    }, /*#__PURE__*/React.createElement("span", {
      style: {
        width: 150,
        fontSize: 11,
        color: "var(--text-secondary)",
        flexShrink: 0
      }
    }, a.agent), /*#__PURE__*/React.createElement("div", {
      style: {
        flex: 1,
        height: 14,
        background: "var(--surface-track)",
        borderRadius: 3,
        position: "relative",
        overflow: "hidden"
      }
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        height: "100%",
        width: `${a.reduction}%`,
        background: color
      }
    })), /*#__PURE__*/React.createElement("span", {
      style: {
        width: 46,
        fontFamily: "var(--font-mono)",
        fontSize: 11,
        color,
        textAlign: "right",
        flexShrink: 0
      }
    }, a.reduction.toFixed(1), "%"));
  }))));
}
Object.assign(__ds_scope, { CostAnalytics });
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/dashboard/CostAnalytics.jsx", error: String((e && e.message) || e) }); }

// ui_kits/dashboard/Governance.jsx
try { (() => {
function Governance({
  flashId
}) {
  const sorted = [...__ds_scope.providers].sort((a, b) => b.spend / b.cap - a.spend / a.cap);
  return /*#__PURE__*/React.createElement("div", {
    style: {
      padding: 16,
      display: "grid",
      gridTemplateColumns: "1fr 1fr",
      gap: 12,
      height: "100%",
      boxSizing: "border-box",
      overflowY: "auto",
      alignContent: "start"
    }
  }, sorted.map(p => {
    const pct = p.spend / p.cap * 100;
    const status = pct >= 100 ? "critical" : pct >= 80 ? "warning" : "ok";
    const color = status === "critical" ? "var(--status-critical-fill)" : status === "warning" ? "var(--status-warn-fill)" : "var(--status-ok-fill)";
    const textColor = status === "critical" ? "var(--status-critical-text)" : status === "warning" ? "var(--status-warn-text)" : "var(--status-ok-text)";
    return /*#__PURE__*/React.createElement("div", {
      key: p.id,
      className: p.id === flashId ? status === "critical" ? "flash-red" : "flash-amber" : "",
      style: {
        background: "var(--surface-card)",
        border: `1px solid ${status !== "ok" ? status === "critical" ? "var(--border-critical)" : "var(--border-warning)" : "var(--border-default)"}`,
        borderRadius: 8,
        padding: 14
      }
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        display: "flex",
        justifyContent: "space-between",
        alignItems: "baseline"
      }
    }, /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
      style: {
        fontSize: 13,
        fontWeight: 600,
        color: "var(--text-primary)"
      }
    }, p.name), /*#__PURE__*/React.createElement("div", {
      style: {
        fontSize: 10,
        color: "var(--text-tertiary)"
      }
    }, p.label)), /*#__PURE__*/React.createElement("span", {
      style: {
        fontSize: 10,
        fontWeight: 700,
        padding: "2px 8px",
        borderRadius: 4,
        letterSpacing: "0.03em",
        background: status === "critical" ? "var(--state-hover-red)" : status === "warning" ? "var(--state-hover-amber)" : "#10b9811a",
        color: textColor
      }
    }, status.toUpperCase())), /*#__PURE__*/React.createElement("div", {
      style: {
        display: "flex",
        alignItems: "center",
        gap: 8,
        marginTop: 10
      }
    }, /*#__PURE__*/React.createElement("span", {
      style: {
        fontSize: 10,
        color: "var(--text-tertiary)"
      }
    }, "Cap $"), /*#__PURE__*/React.createElement("input", {
      defaultValue: p.cap,
      style: {
        width: 70,
        background: "var(--surface-page)",
        border: "1px solid var(--border-default)",
        borderRadius: 4,
        color: "var(--text-primary)",
        fontFamily: "var(--font-mono)",
        fontSize: 11,
        padding: "3px 6px"
      }
    }), /*#__PURE__*/React.createElement("span", {
      style: {
        marginLeft: "auto",
        fontFamily: "var(--font-mono)",
        fontSize: 12,
        color: "var(--text-primary)"
      }
    }, "$", p.spend.toFixed(2))), /*#__PURE__*/React.createElement("div", {
      style: {
        height: 6,
        background: "var(--surface-track)",
        borderRadius: 2,
        overflow: "hidden",
        marginTop: 8
      }
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        height: "100%",
        width: `${Math.min(100, pct)}%`,
        background: color
      }
    })), /*#__PURE__*/React.createElement("div", {
      style: {
        fontSize: 10,
        color: "var(--text-tertiary)",
        marginTop: 6
      }
    }, status === "critical" ? "Fallback Engine Engaged" : p.days != null ? `~${p.days} day${p.days === 1 ? "" : "s"} remaining` : "No cap pressure"));
  }));
}
Object.assign(__ds_scope, { Governance });
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/dashboard/Governance.jsx", error: String((e && e.message) || e) }); }

// ui_kits/dashboard/Header.jsx
try { (() => {
function Header({
  onOpenSettings,
  onOpenGovernance,
  tabs,
  active,
  onTab
}) {
  const breached = __ds_scope.providers.filter(p => p.spend / p.cap >= 1);
  const approaching = __ds_scope.providers.filter(p => p.spend / p.cap >= 0.8 && p.spend / p.cap < 1);
  let status = {
    tone: "ok",
    text: "System Status: OK"
  };
  if (breached.length) status = {
    tone: "critical",
    text: `🚨 ${breached.length} PROVIDER BREACHED${approaching.length ? ` · ${approaching.length} APPROACHING` : ""}`
  };else if (approaching.length) status = {
    tone: "warning",
    text: `⚠️ ${approaching.length} PROVIDER APPROACHING LIMIT`
  };
  return /*#__PURE__*/React.createElement("div", {
    style: {
      background: "var(--surface-card)",
      borderBottom: "1px solid var(--border-default)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between",
      padding: "12px 20px"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 10
    }
  }, /*#__PURE__*/React.createElement("img", {
    src: "../../assets/icon-app.svg",
    width: "22",
    height: "22",
    alt: ""
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: "var(--text-lg)",
      fontWeight: 700,
      letterSpacing: "var(--tracking-tight)",
      color: "var(--text-heading)"
    }
  }, "Router Optimization Engine")), /*#__PURE__*/React.createElement("button", {
    onClick: () => status.tone !== "ok" && onOpenGovernance && onOpenGovernance(),
    style: {
      background: "transparent",
      border: "none",
      cursor: status.tone !== "ok" ? "pointer" : "default",
      color: status.tone === "critical" ? "var(--status-critical-text)" : status.tone === "warning" ? "var(--status-warn-text)" : "var(--status-ok-text)",
      fontSize: 12,
      fontWeight: 600,
      display: "flex",
      alignItems: "center",
      gap: 6
    }
  }, status.tone === "ok" && /*#__PURE__*/React.createElement("span", {
    className: "pulse-dot",
    style: {
      width: 7,
      height: 7,
      borderRadius: "50%",
      background: "var(--status-ok-fill)"
    }
  }), status.text), /*#__PURE__*/React.createElement("button", {
    onClick: onOpenSettings,
    style: {
      background: "transparent",
      border: "1px solid var(--border-default)",
      color: "var(--text-secondary)",
      borderRadius: 6,
      padding: "6px 12px",
      fontSize: 12,
      fontWeight: 600,
      cursor: "pointer"
    }
  }, "\u2699 Settings")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 24,
      padding: "0 20px 12px"
    }
  }, /*#__PURE__*/React.createElement(Ticker, {
    label: "Total Saved",
    value: "$142.36",
    color: "var(--status-ok-text)"
  }), /*#__PURE__*/React.createElement(Ticker, {
    label: "System Tokens",
    value: "4.2M"
  }), /*#__PURE__*/React.createElement(Ticker, {
    label: "Avg. Cost Reduction",
    value: "82.1%",
    color: "var(--status-ok-text)"
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      marginLeft: "auto",
      display: "flex",
      alignItems: "center",
      gap: 6,
      fontSize: 11,
      color: "var(--status-critical-text)",
      fontWeight: 700,
      letterSpacing: "0.05em"
    }
  }, /*#__PURE__*/React.createElement("span", {
    className: "pulse-dot",
    style: {
      width: 6,
      height: 6,
      borderRadius: "50%",
      background: "var(--status-critical-fill)"
    }
  }), "LIVE")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 4,
      padding: "0 16px"
    }
  }, tabs.map(t => {
    const isActive = t.id === active;
    return /*#__PURE__*/React.createElement("button", {
      key: t.id,
      onClick: () => onTab(t.id),
      style: {
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
        transition: "color .15s ease, border-color .15s ease"
      }
    }, t.label);
  })));
}
function Ticker({
  label,
  value,
  color
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "baseline",
      gap: 6
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-mono)",
      fontSize: 15,
      fontWeight: 700,
      color: color || "var(--text-primary)"
    }
  }, value), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 10,
      textTransform: "uppercase",
      letterSpacing: "0.05em",
      color: "var(--text-tertiary)"
    }
  }, label));
}
Object.assign(__ds_scope, { Header });
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/dashboard/Header.jsx", error: String((e && e.message) || e) }); }

// ui_kits/dashboard/LiveStream.jsx
try { (() => {
function colorForAgent(name) {
  const PALETTE = ["#10b981", "#38bdf8", "#818cf8", "#fb7185", "#f59e0b", "#a78bfa", "#14b8a6", "#0ea5e9", "#6366f1", "#ec4899", "#f97316", "#06b6d4"];
  if (!name) return PALETTE[0];
  let hash = 2166136261;
  for (let i = 0; i < name.length; i++) hash = Math.imul(hash ^ name.charCodeAt(i), 16777619);
  return PALETTE[Math.abs(hash) % PALETTE.length];
}
function fmtTok(n) {
  return n >= 1000 ? (n / 1000).toFixed(1) + "K" : String(n);
}
function LiveStream() {
  const [selectedId, setSelected] = React.useState(__ds_scope.conversations[0].id);
  const [query, setQuery] = React.useState("");
  const [leftPct, setLeftPct] = React.useState(35);
  const [expanded, setExpanded] = React.useState(null);
  const dragRef = React.useRef(false);
  const containerRef = React.useRef(null);
  const filtered = __ds_scope.conversations.filter(c => {
    const q = query.toLowerCase();
    if (!q) return true;
    return c.title.toLowerCase().includes(q) || c.turns.some(t => t.agent.toLowerCase().includes(q) || t.model.toLowerCase().includes(q));
  });
  const selected = __ds_scope.conversations.find(c => c.id === selectedId) || __ds_scope.conversations[0];
  function onPointerMove(e) {
    if (!dragRef.current || !containerRef.current) return;
    const rect = containerRef.current.getBoundingClientRect();
    const pct = (e.clientX - rect.left) / rect.width * 100;
    setLeftPct(Math.min(65, Math.max(20, pct)));
  }
  return /*#__PURE__*/React.createElement("div", {
    ref: containerRef,
    onPointerMove: onPointerMove,
    onPointerUp: () => dragRef.current = false,
    style: {
      display: "flex",
      height: "100%",
      padding: 12,
      gap: 0,
      boxSizing: "border-box"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      width: `${leftPct}%`,
      minWidth: 220,
      display: "flex",
      flexDirection: "column",
      gap: 8
    }
  }, /*#__PURE__*/React.createElement("input", {
    value: query,
    onChange: e => setQuery(e.target.value),
    placeholder: "Search conversations\u2026",
    style: {
      background: "var(--surface-page)",
      border: "1px solid var(--border-default)",
      borderRadius: 6,
      padding: "6px 10px",
      color: "var(--text-primary)",
      fontSize: 12,
      outline: "none"
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 6,
      overflowY: "auto"
    }
  }, filtered.map(c => {
    const isSel = c.id === selectedId;
    const agents = [...new Set(c.turns.map(t => t.agent))].slice(0, 2);
    return /*#__PURE__*/React.createElement("div", {
      key: c.id,
      onClick: () => setSelected(c.id),
      style: {
        background: isSel ? "var(--surface-card-hover)" : "var(--surface-card)",
        border: `1px solid ${c.fallback ? "var(--border-warning)" : "var(--border-default)"}`,
        borderLeft: c.fallback ? "3px solid var(--status-warn-fill)" : "1px solid var(--border-default)",
        borderRadius: 8,
        padding: 10,
        cursor: "pointer"
      }
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        display: "flex",
        justifyContent: "space-between",
        gap: 6
      }
    }, /*#__PURE__*/React.createElement("span", {
      style: {
        fontSize: 12,
        fontWeight: 600,
        color: "var(--text-primary)",
        overflow: "hidden",
        textOverflow: "ellipsis",
        whiteSpace: "nowrap"
      }
    }, c.title), c.fallback && /*#__PURE__*/React.createElement("span", {
      style: {
        color: "var(--status-warn-text)",
        fontSize: 12
      }
    }, "\u26A0")), /*#__PURE__*/React.createElement("div", {
      style: {
        fontSize: 10,
        color: "var(--text-tertiary)",
        fontFamily: "var(--font-mono)",
        marginTop: 3
      }
    }, c.first, " \u2192 ", c.last), /*#__PURE__*/React.createElement("div", {
      style: {
        display: "flex",
        justifyContent: "space-between",
        marginTop: 6,
        alignItems: "center"
      }
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        display: "flex",
        gap: 8
      }
    }, agents.map(a => /*#__PURE__*/React.createElement("span", {
      key: a,
      style: {
        display: "flex",
        alignItems: "center",
        gap: 4,
        fontSize: 10,
        color: "var(--text-secondary)"
      }
    }, /*#__PURE__*/React.createElement("span", {
      style: {
        width: 6,
        height: 6,
        borderRadius: "50%",
        background: colorForAgent(a)
      }
    }), a))), /*#__PURE__*/React.createElement("span", {
      style: {
        fontFamily: "var(--font-mono)",
        fontSize: 10,
        color: "var(--text-tertiary)"
      }
    }, "$", c.cost.toFixed(4), " \xB7 ", c.turns.length, " turns")));
  }))), /*#__PURE__*/React.createElement("div", {
    onPointerDown: e => {
      dragRef.current = true;
      e.currentTarget.setPointerCapture(e.pointerId);
    },
    style: {
      flex: "0 0 8px",
      margin: "0 2px",
      cursor: "col-resize",
      borderRadius: 4,
      position: "relative"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      position: "absolute",
      top: "50%",
      left: "50%",
      width: 2,
      height: 28,
      transform: "translate(-50%,-50%)",
      borderRadius: 1,
      background: "var(--border-strong)"
    }
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      minWidth: 0,
      display: "flex",
      flexDirection: "column",
      gap: 8
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      background: "var(--surface-card)",
      border: "1px solid var(--border-default)",
      borderRadius: 8,
      padding: 10
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      justifyContent: "space-between",
      alignItems: "baseline"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 13,
      fontWeight: 600,
      color: "var(--text-primary)"
    }
  }, selected.title), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 10,
      color: "var(--text-tertiary)",
      fontFamily: "var(--font-mono)"
    }
  }, selected.id, " \xB7 ", selected.first, "\u2013", selected.last)), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 20,
      marginTop: 8,
      flexWrap: "wrap"
    }
  }, /*#__PURE__*/React.createElement(Stat, {
    label: "Total Cost",
    value: `$${selected.cost.toFixed(4)}`
  }), /*#__PURE__*/React.createElement(Stat, {
    label: "Total Tokens",
    value: fmtTok(selected.promptTok + selected.complTok)
  }), /*#__PURE__*/React.createElement(Stat, {
    label: "Avg ROI",
    value: `${(selected.turns.reduce((s, t) => s + t.roi, 0) / selected.turns.length).toFixed(1)}%`,
    valueColor: "var(--status-ok-text)"
  }), /*#__PURE__*/React.createElement(Stat, {
    label: "Turns",
    value: selected.turns.length
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 6,
      overflowY: "auto"
    }
  }, selected.turns.map(t => {
    const isOpen = expanded === t.n;
    const color = colorForAgent(t.agent);
    return /*#__PURE__*/React.createElement("div", {
      key: t.n,
      style: {
        background: "var(--surface-card)",
        border: `1px solid ${color}66`,
        borderLeft: `3px solid ${color}`,
        borderRadius: 8,
        padding: 10
      }
    }, /*#__PURE__*/React.createElement("div", {
      onClick: () => setExpanded(isOpen ? null : t.n),
      style: {
        cursor: "pointer"
      }
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        display: "flex",
        gap: 8,
        alignItems: "center"
      }
    }, /*#__PURE__*/React.createElement("span", {
      style: {
        fontSize: 10,
        color: "var(--text-tertiary)",
        fontFamily: "var(--font-mono)"
      }
    }, t.n, "/", selected.turns.length), /*#__PURE__*/React.createElement("span", {
      style: {
        fontSize: 12,
        fontWeight: 600,
        color: "var(--text-primary)",
        overflow: "hidden",
        textOverflow: "ellipsis",
        whiteSpace: "nowrap",
        flex: 1
      }
    }, t.req), /*#__PURE__*/React.createElement("span", {
      style: {
        display: "flex",
        alignItems: "center",
        gap: 4,
        fontSize: 10,
        color,
        flexShrink: 0
      }
    }, /*#__PURE__*/React.createElement("span", {
      style: {
        width: 6,
        height: 6,
        borderRadius: "50%",
        background: color
      }
    }), t.agent), t.fallback && /*#__PURE__*/React.createElement("span", {
      style: {
        color: "var(--status-warn-text)",
        fontSize: 10,
        fontWeight: 700
      }
    }, "\u26A0 FALLBACK"), /*#__PURE__*/React.createElement("span", {
      style: {
        fontSize: 10,
        color: "var(--text-tertiary)",
        fontFamily: "var(--font-mono)",
        flexShrink: 0
      }
    }, t.ts)), /*#__PURE__*/React.createElement("div", {
      style: {
        display: "flex",
        gap: 14,
        flexWrap: "wrap",
        marginTop: 6
      }
    }, /*#__PURE__*/React.createElement(Stat, {
      label: "ROI",
      value: `${t.roi.toFixed(1)}%`,
      valueColor: t.roi > 0 ? "var(--status-ok-text)" : "var(--text-tertiary)",
      small: true
    }), /*#__PURE__*/React.createElement(Stat, {
      label: "Cost",
      value: `$${t.cost.toFixed(4)}`,
      small: true
    }), /*#__PURE__*/React.createElement(Stat, {
      label: "Tok P/C",
      value: `${t.promptTok}/${t.complTok}`,
      small: true
    }), /*#__PURE__*/React.createElement(Stat, {
      label: "Steps",
      value: t.steps,
      small: true
    }), /*#__PURE__*/React.createElement(Stat, {
      label: "Cache",
      value: `${t.cache}%`,
      small: true
    }), /*#__PURE__*/React.createElement(Stat, {
      label: "TTFT",
      value: `${t.ttft}ms`,
      small: true
    }), /*#__PURE__*/React.createElement(Stat, {
      label: "Ctx",
      value: `${t.ctx}%`,
      small: true
    }), /*#__PURE__*/React.createElement(Stat, {
      label: "Model",
      value: t.model,
      small: true
    }))), isOpen && /*#__PURE__*/React.createElement("div", {
      style: {
        marginTop: 10,
        borderTop: "1px solid var(--border-subtle)",
        paddingTop: 8
      }
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        fontSize: 10,
        textTransform: "uppercase",
        letterSpacing: "0.05em",
        color: "var(--text-tertiary)",
        marginBottom: 6
      }
    }, "Routing Decision"), /*#__PURE__*/React.createElement("div", {
      style: {
        display: "flex",
        flexDirection: "column",
        gap: 2,
        marginBottom: 8
      }
    }, t.log.map(([status, msg], i) => /*#__PURE__*/React.createElement("div", {
      key: i,
      style: {
        fontSize: 11,
        fontFamily: "var(--font-mono)",
        padding: "3px 6px",
        borderRadius: 4,
        background: status === "ok" ? "#10b9811a" : status === "warn" ? "var(--state-hover-amber)" : "#38bdf81a",
        color: status === "ok" ? "var(--status-ok-text)" : status === "warn" ? "var(--status-warn-text)" : "var(--status-info-text)"
      }
    }, msg))), /*#__PURE__*/React.createElement("div", {
      style: {
        maxHeight: "8rem",
        overflowY: "auto",
        fontSize: 11,
        color: "var(--text-secondary)",
        background: "var(--surface-page)",
        borderRadius: 6,
        padding: 8
      }
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        marginBottom: 6
      }
    }, /*#__PURE__*/React.createElement("b", {
      style: {
        color: "var(--text-tertiary)"
      }
    }, "Request:"), " ", t.req), t.res && /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("b", {
      style: {
        color: "var(--text-tertiary)"
      }
    }, "Response:"), " ", t.res))));
  }))));
}
function Stat({
  label,
  value,
  valueColor,
  small
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 1
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: small ? 9 : 10,
      letterSpacing: "0.05em",
      textTransform: "uppercase",
      color: "var(--text-tertiary)",
      whiteSpace: "nowrap"
    }
  }, label), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-mono)",
      fontSize: small ? 11 : 12,
      color: valueColor || "var(--text-primary)"
    }
  }, value));
}
Object.assign(__ds_scope, { LiveStream });
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/dashboard/LiveStream.jsx", error: String((e && e.message) || e) }); }

// ui_kits/dashboard/ModelDistribution.jsx
try { (() => {
function ModelDistribution() {
  const [range, setRange] = React.useState("Month");
  const max = Math.max(...__ds_scope.tokenBuckets.map(b => b.prompt + b.completion));
  const total = __ds_scope.modelShares.reduce((s, m) => s + m.value, 0);
  let acc = 0;
  const segs = __ds_scope.modelShares.map(m => {
    const start = acc / total * 360;
    acc += m.value;
    const end = acc / total * 360;
    return {
      ...m,
      start,
      end
    };
  });
  function seg(cx, cy, r, r2, start, end) {
    const toXY = (deg, rad) => [cx + rad * Math.sin(deg * Math.PI / 180), cy - rad * Math.cos(deg * Math.PI / 180)];
    const [x1, y1] = toXY(start, r);
    const [x2, y2] = toXY(end, r);
    const [x3, y3] = toXY(end, r2);
    const [x4, y4] = toXY(start, r2);
    const large = end - start > 180 ? 1 : 0;
    return `M${x1},${y1} A${r},${r} 0 ${large} 1 ${x2},${y2} L${x3},${y3} A${r2},${r2} 0 ${large} 0 ${x4},${y4} Z`;
  }
  return /*#__PURE__*/React.createElement("div", {
    style: {
      padding: 16,
      display: "flex",
      flexDirection: "column",
      gap: 16,
      height: "100%",
      boxSizing: "border-box",
      overflowY: "auto"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 6,
      alignItems: "center"
    }
  }, ["Day", "Month", "3-Month", "6-Month", "Year"].map(r => /*#__PURE__*/React.createElement("button", {
    key: r,
    onClick: () => setRange(r),
    style: {
      background: r === range ? "var(--sky-400)" : "var(--slate-700)",
      color: r === range ? "var(--slate-900)" : "var(--text-secondary)",
      border: "none",
      borderRadius: 6,
      padding: "5px 10px",
      fontSize: 11,
      fontWeight: 600,
      cursor: "pointer"
    }
  }, r)), /*#__PURE__*/React.createElement("div", {
    style: {
      marginLeft: "auto",
      display: "flex",
      gap: 6,
      alignItems: "center",
      fontSize: 11,
      color: "var(--text-tertiary)"
    }
  }, "From ", /*#__PURE__*/React.createElement("input", {
    defaultValue: "2026-06-01",
    style: {
      background: "var(--surface-page)",
      border: "1px solid var(--border-default)",
      borderRadius: 4,
      color: "var(--text-primary)",
      fontSize: 11,
      padding: "3px 6px"
    }
  }), "To ", /*#__PURE__*/React.createElement("input", {
    defaultValue: "2026-07-01",
    style: {
      background: "var(--surface-page)",
      border: "1px solid var(--border-default)",
      borderRadius: 4,
      color: "var(--text-primary)",
      fontSize: 11,
      padding: "3px 6px"
    }
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      background: "var(--surface-card)",
      border: "1px solid var(--border-default)",
      borderRadius: 8,
      padding: 14
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 12,
      color: "var(--text-secondary)",
      marginBottom: 10,
      fontWeight: 600
    }
  }, "Token Volume \u2014 Prompt vs Completion"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "flex-end",
      gap: 10,
      height: 120
    }
  }, __ds_scope.tokenBuckets.map(b => /*#__PURE__*/React.createElement("div", {
    key: b.slot,
    style: {
      flex: 1,
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      gap: 4
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 3,
      alignItems: "flex-end",
      height: 100
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      width: 10,
      height: `${b.prompt / max * 100}%`,
      background: "var(--sky-400)",
      borderRadius: "2px 2px 0 0"
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      width: 10,
      height: `${b.completion / max * 100}%`,
      background: "var(--emerald-500)",
      borderRadius: "2px 2px 0 0"
    }
  })), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 10,
      color: "var(--text-tertiary)"
    }
  }, b.slot)))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 14,
      marginTop: 8,
      fontSize: 11,
      color: "var(--text-secondary)"
    }
  }, /*#__PURE__*/React.createElement("span", null, /*#__PURE__*/React.createElement("span", {
    style: {
      display: "inline-block",
      width: 8,
      height: 8,
      background: "var(--sky-400)",
      borderRadius: 2,
      marginRight: 5
    }
  }), "Prompt"), /*#__PURE__*/React.createElement("span", null, /*#__PURE__*/React.createElement("span", {
    style: {
      display: "inline-block",
      width: 8,
      height: 8,
      background: "var(--emerald-500)",
      borderRadius: 2,
      marginRight: 5
    }
  }), "Completion"))), /*#__PURE__*/React.createElement("div", {
    style: {
      background: "var(--surface-card)",
      border: "1px solid var(--border-default)",
      borderRadius: 8,
      padding: 14,
      display: "flex",
      gap: 20,
      alignItems: "center"
    }
  }, /*#__PURE__*/React.createElement("svg", {
    width: "140",
    height: "140",
    viewBox: "-70 -70 140 140"
  }, segs.map(s => /*#__PURE__*/React.createElement("path", {
    key: s.model,
    d: seg(0, 0, 68, 42, s.start, s.end),
    fill: s.color
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 6
    }
  }, __ds_scope.modelShares.map(m => /*#__PURE__*/React.createElement("div", {
    key: m.model,
    style: {
      display: "flex",
      alignItems: "center",
      gap: 6,
      fontSize: 11,
      color: "var(--text-secondary)"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 8,
      height: 8,
      borderRadius: 2,
      background: m.color
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-mono)"
    }
  }, m.model), /*#__PURE__*/React.createElement("span", {
    style: {
      marginLeft: "auto",
      color: "var(--text-primary)"
    }
  }, m.value, "%"))))));
}
Object.assign(__ds_scope, { ModelDistribution });
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/dashboard/ModelDistribution.jsx", error: String((e && e.message) || e) }); }

// ui_kits/dashboard/Dashboard.jsx
try { (() => {
const TABS = [{
  id: "live",
  label: "Live Stream"
}, {
  id: "cost",
  label: "Cost Analytics"
}, {
  id: "model",
  label: "Model Distribution"
}, {
  id: "gov",
  label: "Governance"
}];
function Dashboard() {
  const [tab, setTab] = React.useState("live");
  const [settingsOpen, setSettingsOpen] = React.useState(false);
  return /*#__PURE__*/React.createElement("div", {
    style: {
      height: "100vh",
      display: "flex",
      flexDirection: "column",
      overflow: "hidden",
      background: "var(--surface-page)",
      color: "var(--text-primary)",
      fontFamily: "var(--font-sans)"
    }
  }, /*#__PURE__*/React.createElement(__ds_scope.Header, {
    tabs: TABS,
    active: tab,
    onTab: setTab,
    onOpenSettings: () => setSettingsOpen(true),
    onOpenGovernance: () => setTab("gov")
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      minHeight: 0
    }
  }, tab === "live" && /*#__PURE__*/React.createElement(__ds_scope.LiveStream, null), tab === "cost" && /*#__PURE__*/React.createElement(__ds_scope.CostAnalytics, null), tab === "model" && /*#__PURE__*/React.createElement(__ds_scope.ModelDistribution, null), tab === "gov" && /*#__PURE__*/React.createElement(__ds_scope.Governance, null)), /*#__PURE__*/React.createElement(__ds_scope.SettingsModal, {
    open: settingsOpen,
    onClose: () => setSettingsOpen(false)
  }));
}
Object.assign(__ds_scope, { Dashboard });
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/dashboard/Dashboard.jsx", error: String((e && e.message) || e) }); }

__ds_ns.Icon = __ds_scope.Icon;

__ds_ns.AgentChip = __ds_scope.AgentChip;

__ds_ns.Card = __ds_scope.Card;

__ds_ns.StatItem = __ds_scope.StatItem;

__ds_ns.Badge = __ds_scope.Badge;

__ds_ns.ProgressBar = __ds_scope.ProgressBar;

__ds_ns.Tooltip = __ds_scope.Tooltip;

__ds_ns.Button = __ds_scope.Button;

__ds_ns.Input = __ds_scope.Input;

__ds_ns.Tabs = __ds_scope.Tabs;

__ds_ns.Modal = __ds_scope.Modal;

__ds_ns.CostAnalytics = __ds_scope.CostAnalytics;

__ds_ns.Dashboard = __ds_scope.Dashboard;

__ds_ns.Governance = __ds_scope.Governance;

__ds_ns.Header = __ds_scope.Header;

__ds_ns.LiveStream = __ds_scope.LiveStream;

__ds_ns.ModelDistribution = __ds_scope.ModelDistribution;

__ds_ns.SettingsModal = __ds_scope.SettingsModal;

})();
