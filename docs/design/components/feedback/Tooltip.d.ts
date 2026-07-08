export interface TooltipProps {
  /** Tooltip text (usually a metric definition, e.g. "Return on investment vs. worst-case model"). */
  label: string;
  /** The element the tooltip is anchored to — typically a stat label with `cursor: help`. */
  children?: React.ReactNode;
  placement?: "top" | "bottom";
}
