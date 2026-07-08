export interface ProgressBarProps {
  /** 0–100. */
  percent: number;
  /** Explicit fill color; if omitted, auto-tiers green (≥85) / blue (≥70) / amber (below). */
  color?: string;
  /** Track height in px. Default 6. */
  height?: number;
}
