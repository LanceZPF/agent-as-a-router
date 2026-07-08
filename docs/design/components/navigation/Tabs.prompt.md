The dashboard's single-level primary navigation — four tabs under the header ticker, active tab underlined in sky blue.

```jsx
<Tabs tabs={[{id:"live",label:"Live Stream"},{id:"cost",label:"Cost Analytics"}]} active="live" onChange={setTab} />
```

Client-side state only — no router, no URL sync (matches the source, which has no BrowserRouter).
