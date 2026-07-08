export interface IconProps {
  /** Icon name — a curated key (settings, search, close, chevron-down, chevron-right,
   * alert-triangle, info, check-circle, alert-circle, activity, dot, trend-up, bot) or any
   * lucide-static slug. */
  name: string;
  /** Pixel size, square. Default 16. */
  size?: number;
  /** CSS color (fills the glyph via mask-image). Default var(--text-secondary). */
  color?: string;
  style?: React.CSSProperties;
}
