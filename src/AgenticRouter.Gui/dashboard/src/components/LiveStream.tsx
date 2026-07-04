import { useState } from 'react';
import { Search, AlertTriangle, CheckCircle } from 'lucide-react';
import type { RoutingEntry } from '../data/mockData';

interface LiveStreamProps {
  entries: RoutingEntry[];
  selectedId: string;
  onSelect: (id: string) => void;
}

function RoutingStepBadge({ status, message }: { status: string; message: string }) {
  if (status === 'warn') {
    return (
      <div className="flex items-start gap-2 rounded px-2 py-1.5 text-xs font-mono"
        style={{ background: 'rgba(245,158,11,0.12)', borderLeft: '2px solid #f59e0b' }}>
        <AlertTriangle size={11} className="mt-0.5 shrink-0" style={{ color: '#f59e0b' }} />
        <span style={{ color: '#fcd34d' }}>{message}</span>
      </div>
    );
  }
  if (status === 'info') {
    return (
      <div className="flex items-start gap-2 rounded px-2 py-1.5 text-xs font-mono"
        style={{ background: 'rgba(56,189,248,0.1)', borderLeft: '2px solid #38bdf8' }}>
        <span className="mt-0.5 text-sky-400">👉</span>
        <span style={{ color: '#7dd3fc' }}>{message}</span>
      </div>
    );
  }
  return (
    <div className="flex items-start gap-2 rounded px-2 py-1.5 text-xs font-mono"
      style={{ background: 'rgba(16,185,129,0.08)', borderLeft: '2px solid #10b981' }}>
      <CheckCircle size={11} className="mt-0.5 shrink-0" style={{ color: '#10b981' }} />
      <span style={{ color: '#6ee7b7' }}>{message}</span>
    </div>
  );
}

export default function LiveStream({ entries, selectedId, onSelect }: LiveStreamProps) {
  const [search, setSearch] = useState('');
  const [collapsed, setCollapsed] = useState(false);
  const selected = entries.find(e => e.id === selectedId) ?? entries[0];

  const filtered = entries.filter(e =>
    !search || e.sessionId.includes(search) || e.agent.toLowerCase().includes(search.toLowerCase())
  );

  const promptPct = selected.promptTokens / (selected.promptTokens + selected.completionTokens) * 100;
  const completionPct = 100 - promptPct;
  const totalTokens = selected.promptTokens + selected.completionTokens;

  return (
    <div className="flex h-full gap-2 min-h-0">
      {/* Left column: stream */}
      <div className="flex flex-col min-h-0" style={{ width: '40%' }}>
        {/* Search */}
        <div className="relative shrink-0 mb-2">
          <Search size={13} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
          <input
            type="text"
            placeholder="Search by session or agent..."
            value={search}
            onChange={e => setSearch(e.target.value)}
            className="w-full rounded pl-8 pr-3 py-2 text-xs font-mono"
            style={{ background: '#1e293b', border: '1px solid #334155', color: '#94a3b8' }}
          />
        </div>
        {/* Cards */}
        <div className="flex-1 overflow-y-auto space-y-1.5 pr-1">
          {filtered.map(entry => (
            <button
              key={entry.id}
              onClick={() => onSelect(entry.id)}
              className="w-full text-left rounded card-hover"
              style={{
                background: selectedId === entry.id ? '#263548' : '#1e293b',
                border: `1px solid ${selectedId === entry.id ? '#38bdf8' : entry.isFallback ? '#f59e0b33' : '#334155'}`,
                padding: '8px 10px',
              }}
            >
              <div className="flex items-center gap-2 mb-1">
                {entry.isFallback && (
                  <span className="shrink-0 rounded px-1.5 py-0.5 text-xs font-semibold"
                    style={{ background: '#f59e0b22', color: '#f59e0b', border: '1px solid #f59e0b44' }}>
                    ⚠
                  </span>
                )}
                <span className="text-xs font-mono text-slate-400">
                  Session: <span className="text-slate-200">{entry.sessionId}...</span>
                </span>
                <span className="ml-auto text-xs text-slate-600 font-mono">{entry.timestamp}</span>
              </div>
              <div className="text-xs font-mono mb-1">
                <span className="text-slate-500">Model: </span>
                <span style={{ color: entry.isFallback ? '#f59e0b' : '#38bdf8' }}>{entry.model}</span>
              </div>
              <div className="text-xs font-mono">
                {entry.isFallback ? (
                  <span style={{ color: '#64748b' }}>Saved: $0.000000 (0.00% <span>➔</span>)</span>
                ) : (
                  <span style={{ color: '#10b981' }}>
                    Saved: ${entry.savingsAmount.toFixed(6)}{' '}
                    <span className="text-emerald-400">({entry.savingsPercent.toFixed(2)}% ↓)</span>
                  </span>
                )}
              </div>
              <div className="text-xs text-slate-600 mt-0.5">{entry.agent}</div>
            </button>
          ))}
        </div>
      </div>

      {/* Right column: drilldown */}
      <div className="flex flex-col min-h-0 overflow-y-auto flex-1 space-y-2 pr-1">
        {/* Header */}
        <div className="rounded p-3 shrink-0" style={{ background: '#1e293b', border: '1px solid #334155' }}>
          <div className="flex items-baseline gap-3 mb-0.5">
            <span className="text-xs text-slate-400">Trace</span>
            <span className="font-mono text-xs text-sky-400">#{selected.traceId}...</span>
          </div>
          <div className="text-xs text-slate-400">
            Assigned Agent: <span className="text-slate-200 font-medium">{selected.agent}</span>
          </div>
        </div>

        {/* Token Volume */}
        <div className="rounded p-3 shrink-0" style={{ background: '#1e293b', border: '1px solid #334155' }}>
          <div className="text-xs font-semibold text-slate-400 uppercase tracking-widest mb-3">Token Volume</div>
          <div className="grid grid-cols-3 gap-2 mb-3">
            {[
              { label: 'Prompt', val: selected.promptTokens },
              { label: 'Completion', val: selected.completionTokens },
              { label: 'Total', val: totalTokens },
            ].map(({ label, val }) => (
              <div key={label} className="rounded p-2 text-center" style={{ background: '#0f172a' }}>
                <div className="font-mono text-sm text-slate-200">{val.toLocaleString()}</div>
                <div className="text-xs text-slate-500 mt-0.5">{label}</div>
              </div>
            ))}
          </div>
          {/* Progress bar */}
          <div className="space-y-1">
            <div className="flex h-2 rounded overflow-hidden">
              <div style={{ width: `${promptPct}%`, background: '#38bdf8' }} />
              <div style={{ width: `${completionPct}%`, background: '#10b981' }} />
            </div>
            <div className="flex justify-between text-xs font-mono text-slate-500">
              <span><span style={{ color: '#38bdf8' }}>■</span> {promptPct.toFixed(2)}% Prompt</span>
              <span><span style={{ color: '#10b981' }}>■</span> {completionPct.toFixed(2)}% Completion</span>
            </div>
          </div>
        </div>

        {/* Cost Performance */}
        <div className="rounded p-3 shrink-0" style={{ background: '#1e293b', border: '1px solid #334155' }}>
          <div className="text-xs font-semibold text-slate-400 uppercase tracking-widest mb-3">Cost Performance</div>
          <div className="space-y-2">
            {[
              { label: 'Actual Allocated Cost', val: `$${selected.actualCost.toFixed(6)}`, color: '#e2e8f0' },
              { label: 'Worst-Case Pool Cost', val: `$${selected.worstCaseCost.toFixed(6)}`, color: '#94a3b8' },
            ].map(({ label, val, color }) => (
              <div key={label} className="flex items-center justify-between">
                <span className="text-xs text-slate-500">{label}</span>
                <span className="font-mono text-xs" style={{ color }}>{val}</span>
              </div>
            ))}
            <div className="flex items-center justify-between pt-1 border-t border-slate-700/50">
              <span className="text-xs text-slate-400 font-medium">Net Transaction Savings</span>
              {selected.isFallback ? (
                <span className="font-mono text-xs text-slate-500">$0.000000 (0.00% ➔)</span>
              ) : (
                <span className="font-mono text-xs" style={{ color: '#10b981' }}>
                  ${selected.savingsAmount.toFixed(6)} ({selected.savingsPercent.toFixed(2)}% ↓)
                </span>
              )}
            </div>
          </div>
        </div>

        {/* Routing Decision Inspector */}
        <div className="rounded shrink-0" style={{ background: '#1e293b', border: '1px solid #334155' }}>
          <button
            onClick={() => setCollapsed(c => !c)}
            className="w-full flex items-center justify-between px-3 py-2.5 text-left"
          >
            <span className="text-xs font-semibold text-slate-400 uppercase tracking-widest">
              Routing Decision Inspector
            </span>
            <span className="text-slate-500 text-xs">{collapsed ? '▶' : '▼'}</span>
          </button>
          {!collapsed && (
            <div className="px-3 pb-3 space-y-1.5">
              {selected.routingSteps.map((step, i) => (
                <RoutingStepBadge key={i} status={step.status} message={step.message} />
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
