export interface BadgeProps {
  /** Semantic tone. `ok`=green, `warning`=amber, `critical`=red, `info`=sky, `neutral`=slate. Default neutral. */
  tone?: "ok" | "warning" | "critical" | "info" | "neutral";
  children?: React.ReactNode;
  style?: React.CSSProperties;
}
