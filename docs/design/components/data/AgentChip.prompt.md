A color-coded dot + agent name — the routing-decision-log visual language, reused as the turn card header chip.

```jsx
<AgentChip name="Code Review Bot" />
```

Also exports `colorForAgent(name)` for tinting card borders/backgrounds to match (see `Card`'s `agentColor` prop) — the color is a deterministic hash, not a manual assignment, so it's stable across launches without a lookup table.
