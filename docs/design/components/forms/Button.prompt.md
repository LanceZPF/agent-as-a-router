A button, intentional additions to cover the source's implied action set (Settings "Reset Stats"/"Clear History" are destructive; "Show Dashboard" tray action and modal confirms are primary/secondary).

```jsx
<Button variant="primary">Show Dashboard</Button>
<Button variant="destructive" size="sm">Reset Stats</Button>
```

Hover state is `opacity: 0.8` across all variants — the dashboard has no dedicated hover-color set for buttons, only the `.hover:opacity-80` utility.
