A centered modal dialog — used for Settings, including its "Destructive Actions Zone" pattern.

```jsx
<Modal open={open} title="Settings" onClose={() => setOpen(false)}>
  <p>Body content…</p>
</Modal>
```

Click-outside (backdrop) closes it; content clicks are stopped from propagating. No entrance animation in the source — it simply appears.
