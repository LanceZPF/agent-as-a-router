A thin track-and-fill bar for budget utilization (Governance) and cost-reduction bars (Cost Analytics).

```jsx
<ProgressBar percent={86} />
```

The default auto-tier coloring (green ≥85%, blue ≥70%, amber below) matches the Cost Analytics agent-ROI bars; pass an explicit `color` for Governance's spend-vs-cap bars where color signals proximity to the cap instead.
