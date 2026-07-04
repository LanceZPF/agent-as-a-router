import { useState } from 'react';
import { Settings, Activity, BarChart2, TrendingUp, ShieldCheck } from 'lucide-react';
import LiveStream from './components/LiveStream';
import CostAnalytics from './components/CostAnalytics';
import ModelDistribution from './components/ModelDistribution';
import Governance from './components/Governance';
import SettingsModal from './components/SettingsModal';
import {
  MOCK_ENTRIES,
  MOCK_PROVIDERS,
  COST_DATA,
  AGENT_ROI,
  TOKEN_BUCKETS,
  MODEL_SHARE,
} from './data/mockData';

type TabId = 'live' | 'analytics' | 'distribution' | 'governance';

const TABS: { id: TabId; icon: React.ReactNode; label: string }[] = [
  { id: 'live', icon: <Activity size={13} />, label: 'Live Stream' },
  { id: 'analytics', icon: <BarChart2 size={13} />, label: 'Cost Analytics' },
  { id: 'distribution', icon: <TrendingUp size={13} />, label: 'Model Distribution' },
  { id: 'governance', icon: <ShieldCheck size={13} />, label: 'Governance' },
];

function getSystemStatus(providers: typeof MOCK_PROVIDERS) {
  const breached = providers.filter(p => p.currentSpend / p.budgetCap >= 1);
  const approaching = providers.filter(p => {
    const pct = p.currentSpend / p.budgetCap;
    return pct >= 0.8 && pct < 1;
  });
  return { breached, approaching };
}

export default function App() {
  const [activeTab, setActiveTab] = useState<TabId>('live');
  const [selectedEntryId, setSelectedEntryId] = useState(MOCK_ENTRIES[0].id);
  const [showSettings, setShowSettings] = useState(false);
  const [flashId, setFlashId] = useState<string | null>(null);

  const { breached, approaching } = getSystemStatus(MOCK_PROVIDERS);

  function handleAlertClick() {
    setActiveTab('governance');
    const targetId = breached[0]?.id ?? approaching[0]?.id;
    if (targetId) {
      setFlashId(null);
      setTimeout(() => setFlashId(targetId), 50);
      setTimeout(() => setFlashId(null), 4000);
    }
  }

  function StatusBanner() {
    if (breached.length > 0 && approaching.length > 0) {
      return (
        <button onClick={handleAlertClick} className="flex items-center gap-2 hover:opacity-80 transition-opacity">
          <span className="font-semibold text-xs" style={{ color: '#ef4444' }}>
            🚨 {breached.length} BREACHED
          </span>
          <span className="text-slate-600">|</span>
          <span className="font-semibold text-xs" style={{ color: '#f59e0b' }}>
            ⚠️ {approaching.length} APPROACHING LIMIT
          </span>
        </button>
      );
    }
    if (breached.length > 0) {
      return (
        <button onClick={handleAlertClick} className="flex items-center gap-2 hover:opacity-80 transition-opacity">
          <span className="font-semibold text-xs" style={{ color: '#ef4444' }}>
            🚨 {breached.length} PROVIDER BREACHED
          </span>
        </button>
      );
    }
    if (approaching.length > 0) {
      return (
        <button onClick={handleAlertClick} className="flex items-center gap-2 hover:opacity-80 transition-opacity">
          <span className="font-semibold text-xs" style={{ color: '#f59e0b' }}>
            ⚠️ {approaching.length} PROVIDER APPROACHING LIMIT
          </span>
        </button>
      );
    }
    return (
      <div className="flex items-center gap-2">
        <span className="pulse-dot w-1.5 h-1.5 rounded-full" style={{ background: '#10b981' }} />
        <span className="text-xs font-medium" style={{ color: '#10b981' }}>System Status: OK</span>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-screen overflow-hidden" style={{ background: '#0f172a' }}>
      {/* ── HEADER ── */}
      <header style={{ background: '#1e293b', borderBottom: '1px solid #334155' }}>
        <div className="flex items-center justify-between px-4 py-2.5">
          {/* Brand */}
          <div className="flex items-center gap-2">
            <span className="text-sm font-bold tracking-tight text-slate-100">
              🤖 Router Optimization Engine
            </span>
          </div>

          {/* Status Banner */}
          <div className="flex items-center">
            <StatusBanner />
          </div>

          {/* Actions */}
          <div className="flex items-center gap-2">
            <button
              onClick={() => setShowSettings(true)}
              className="flex items-center gap-1.5 rounded px-2.5 py-1.5 text-xs font-medium border border-slate-700 text-slate-400 hover:border-slate-500 hover:text-slate-200 transition-colors"
            >
              <Settings size={12} />
              Settings
            </button>
          </div>
        </div>

        {/* Ticker row */}
        <div className="flex items-center gap-6 px-4 py-1.5" style={{ borderTop: '1px solid #0f172a', background: '#172033' }}>
          <div className="flex items-center gap-2">
            <span className="text-xs text-slate-500">Total Saved</span>
            <span className="font-mono text-xs font-semibold" style={{ color: '#10b981' }}>$142.36</span>
          </div>
          <div className="w-px h-3" style={{ background: '#334155' }} />
          <div className="flex items-center gap-2">
            <span className="text-xs text-slate-500">System Tokens</span>
            <span className="font-mono text-xs font-semibold text-slate-200">12.4M</span>
          </div>
          <div className="w-px h-3" style={{ background: '#334155' }} />
          <div className="flex items-center gap-2">
            <span className="text-xs text-slate-500">Avg. Cost Reduction</span>
            <span className="font-mono text-xs font-semibold" style={{ color: '#10b981' }}>74.20% ↓</span>
          </div>
          <div className="ml-auto flex items-center gap-1.5">
            <span className="pulse-dot w-1.5 h-1.5 rounded-full" style={{ background: '#10b981' }} />
            <span className="text-xs font-mono text-slate-600">LIVE</span>
          </div>
        </div>
      </header>

      {/* ── TAB BAR ── */}
      <nav
        className="flex items-end px-4 gap-0 shrink-0"
        style={{ background: '#1e293b', borderBottom: '1px solid #334155' }}
      >
        {TABS.map(tab => {
          const isActive = activeTab === tab.id;
          return (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className="relative flex items-center gap-1.5 px-4 py-2.5 text-xs font-medium transition-colors"
              style={{
                color: isActive ? '#38bdf8' : '#64748b',
                background: isActive ? '#0f172a' : 'transparent',
                borderTop: isActive ? '1px solid #334155' : '1px solid transparent',
                borderLeft: isActive ? '1px solid #334155' : '1px solid transparent',
                borderRight: isActive ? '1px solid #334155' : '1px solid transparent',
                borderBottom: isActive ? '1px solid #0f172a' : '1px solid transparent',
                borderRadius: '6px 6px 0 0',
                marginBottom: '-1px',
              }}
            >
              {tab.icon}
              {tab.label}
            </button>
          );
        })}
      </nav>

      {/* ── WORKSPACE ── */}
      <main className="flex-1 overflow-hidden p-3 min-h-0">
        {activeTab === 'live' && (
          <LiveStream
            entries={MOCK_ENTRIES}
            selectedId={selectedEntryId}
            onSelect={setSelectedEntryId}
          />
        )}
        {activeTab === 'analytics' && (
          <CostAnalytics costData={COST_DATA} agentROI={AGENT_ROI} />
        )}
        {activeTab === 'distribution' && (
          <ModelDistribution tokenBuckets={TOKEN_BUCKETS} modelShare={MODEL_SHARE} />
        )}
        {activeTab === 'governance' && (
          <Governance providers={MOCK_PROVIDERS} flashId={flashId} />
        )}
      </main>

      {/* ── SETTINGS MODAL ── */}
      {showSettings && <SettingsModal onClose={() => setShowSettings(false)} />}
    </div>
  );
}
