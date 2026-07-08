import React from "react";
import { Header } from "./Header.jsx";
import { LiveStream } from "./LiveStream.jsx";
import { CostAnalytics } from "./CostAnalytics.jsx";
import { ModelDistribution } from "./ModelDistribution.jsx";
import { Governance } from "./Governance.jsx";
import { SettingsModal } from "./SettingsModal.jsx";

const TABS = [
  { id: "live", label: "Live Stream" },
  { id: "cost", label: "Cost Analytics" },
  { id: "model", label: "Model Distribution" },
  { id: "gov", label: "Governance" },
];

export function Dashboard() {
  const [tab, setTab] = React.useState("live");
  const [settingsOpen, setSettingsOpen] = React.useState(false);

  return (
    <div style={{ height: "100vh", display: "flex", flexDirection: "column", overflow: "hidden", background: "var(--surface-page)", color: "var(--text-primary)", fontFamily: "var(--font-sans)" }}>
      <Header tabs={TABS} active={tab} onTab={setTab} onOpenSettings={() => setSettingsOpen(true)} onOpenGovernance={() => setTab("gov")} />
      <div style={{ flex: 1, minHeight: 0 }}>
        {tab === "live" && <LiveStream />}
        {tab === "cost" && <CostAnalytics />}
        {tab === "model" && <ModelDistribution />}
        {tab === "gov" && <Governance />}
      </div>
      <SettingsModal open={settingsOpen} onClose={() => setSettingsOpen(false)} />
    </div>
  );
}
