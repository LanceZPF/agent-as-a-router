import { useState, useEffect, useRef } from 'react';
import type { Provider } from '../data/mockData';

interface GovernanceProps {
  providers: Provider[];
  flashId: string | null;
}

function ProviderCard({ provider, shouldFlash }: { provider: Provider; shouldFlash: boolean }) {
  const pct = Math.min((provider.currentSpend / provider.budgetCap) * 100, 100);
  const isBreached = pct >= 100;
  const isApproaching = pct >= 80 && pct < 100;
  const [cap, setCap] = useState(provider.budgetCap.toFixed(2));
  const cardRef = useRef<HTMLDivElement>(null);

  const borderColor = isBreached ? '#ef4444' : isApproaching ? '#f59e0b' : '#334155';
  const barColor = isBreached ? '#ef4444' : isApproaching ? '#f59e0b' : '#10b981';

  useEffect(() => {
    if (shouldFlash && cardRef.current) {
      cardRef.current.classList.remove('flash-amber', 'flash-red');
      void cardRef.current.offsetWidth;
      cardRef.current.classList.add(isBreached ? 'flash-red' : 'flash-amber');
    }
  }, [shouldFlash, isBreached]);

  return (
    <div
      ref={cardRef}
      className="rounded-lg p-3 flex flex-col gap-2.5"
      style={{
        background: '#1e293b',
        border: `1px solid ${borderColor}`,
        transition: 'border-color 0.3s',
      }}
    >
      {/* Header */}
      <div className="flex items-start justify-between gap-2">
        <div>
          <div className="text-sm font-semibold text-slate-200 leading-tight">{provider.name}</div>
          <div className="text-xs text-slate-500 mt-0.5">{provider.label}</div>
        </div>
        {isBreached && (
          <span className="shrink-0 rounded px-1.5 py-0.5 text-xs font-semibold"
            style={{ background: '#ef444422', color: '#ef4444', border: '1px solid #ef444444' }}>
            CRITICAL
          </span>
        )}
        {isApproaching && !isBreached && (
          <span className="shrink-0 rounded px-1.5 py-0.5 text-xs font-semibold"
            style={{ background: '#f59e0b22', color: '#f59e0b', border: '1px solid #f59e0b44' }}>
            WARNING
          </span>
        )}
        {!isBreached && !isApproaching && (
          <span className="shrink-0 rounded px-1.5 py-0.5 text-xs font-semibold"
            style={{ background: '#10b98122', color: '#10b981', border: '1px solid #10b98144' }}>
            OK
          </span>
        )}
      </div>

      {/* Budget Cap Input */}
      <div>
        <div className="text-xs text-slate-500 mb-1">Budget Cap</div>
        <div className="flex items-center gap-1">
          <span className="text-xs font-mono text-slate-400">$</span>
          <input
            type="text"
            value={cap}
            onChange={e => setCap(e.target.value)}
            className="rounded px-2 py-1.5 text-xs font-mono w-full"
            style={{ background: '#0f172a', border: '1px solid #334155', color: '#e2e8f0' }}
          />
        </div>
      </div>

      {/* Current Spend */}
      <div className="flex items-center justify-between">
        <span className="text-xs text-slate-500">Current Spend</span>
        <span className="font-mono text-xs" style={{ color: isBreached ? '#ef4444' : isApproaching ? '#f59e0b' : '#e2e8f0' }}>
          ${provider.currentSpend.toFixed(2)}
        </span>
      </div>

      {/* Progress Bar */}
      <div>
        <div className="flex justify-between text-xs font-mono text-slate-500 mb-1">
          <span>{pct.toFixed(1)}% utilized</span>
          <span>${provider.currentSpend.toFixed(2)} / ${provider.budgetCap.toFixed(2)}</span>
        </div>
        <div className="progress-bar-track">
          <div
            className="h-full rounded"
            style={{
              width: `${pct}%`,
              background: barColor,
              transition: 'width 0.6s ease, background-color 0.3s',
            }}
          />
        </div>
      </div>

      {/* Status Tag */}
      <div className="rounded px-2 py-1.5 text-xs"
        style={{
          background: isBreached ? '#ef444411' : isApproaching ? '#f59e0b11' : '#10b98111',
          borderTop: `1px solid ${isBreached ? '#ef444422' : isApproaching ? '#f59e0b22' : '#10b98122'}`,
        }}>
        {isBreached && (
          <span style={{ color: '#ef4444' }}>
            🚨 CRITICAL: Budget Exhausted. Fallback Engine Engaged.
          </span>
        )}
        {isApproaching && !isBreached && (
          <span style={{ color: '#f59e0b' }}>
            ⚠️ Approaching cap (Est. {provider.estimatedDaysRemaining} days remaining)
          </span>
        )}
        {!isBreached && !isApproaching && (
          <span className="text-slate-500">✅ Spend nominal</span>
        )}
      </div>
    </div>
  );
}

export default function Governance({ providers, flashId }: GovernanceProps) {
  const sorted = [...providers].sort((a, b) => {
    const pctA = a.currentSpend / a.budgetCap;
    const pctB = b.currentSpend / b.budgetCap;
    return pctB - pctA;
  });

  return (
    <div className="h-full overflow-y-auto pr-1">
      <div className="grid grid-cols-2 gap-3" style={{ gridAutoRows: 'min-content' }}>
        {sorted.map(p => (
          <ProviderCard
            key={p.id}
            provider={p}
            shouldFlash={flashId === p.id}
          />
        ))}
      </div>
    </div>
  );
}
