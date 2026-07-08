export interface AgentChipProps {
  /** Agent name — hashed (FNV-1a) to a fixed palette color, matching Utils/ColorUtils.cs. */
  name: string;
  /** Dot diameter in px. Default 8. */
  size?: number;
}
