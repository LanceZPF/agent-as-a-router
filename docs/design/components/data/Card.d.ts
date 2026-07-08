export interface CardProps {
  /** When set, tints the left border + main border with this hex — the agent-tinted-row pattern
   * used by ConversationCard/TurnCard. Omit for a plain neutral card (Governance provider cards). */
  agentColor?: string;
  /** Whether the card background lightens on hover (.card-hover). Default true. */
  hoverable?: boolean;
  children?: React.ReactNode;
  style?: React.CSSProperties;
}
