import { useState } from 'react';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
  PieChart, Pie, Cell, Legend,
} from 'recharts';
import type { TooltipContentProps, DefaultLegendContentProps, LegendPayload } from 'recharts';
import type { TokenBucket, ModelShare } from '../data/mockData';

interface ModelDistributionProps {
  tokenBuckets: TokenBucket[];
  modelShare: ModelShare[];
}

const TIME_FILTERS = ['Day', 'Month', '3-Month', '6-Month', 'Year'] as const;

function fmtM(n: number) {
  return n >= 1_000_000 ? `${(n / 1_000_000).toFixed(1)}M` : `${(n / 1000).toFixed(0)}K`;
}

const TokenTooltip = ({ active, payload, label }: TooltipContentProps) => {
  if (active && payload?.length) {
    return (
      <div className="rounded px-3 py-2 text-xs font-mono" style={{ background: '#1e293b', border: '1px solid #334155' }}>
        <div className="text-slate-400 mb-1">{label}</div>
        <div style={{ color: '#38bdf8' }}>Prompt: {fmtM(Number(payload[0]?.value ?? 0))}</div>
        <div style={{ color: '#10b981' }}>Completion: {fmtM(Number(payload[1]?.value ?? 0))}</div>
      </div>
    );
  }
  return null;
};

const PieTooltip = ({ active, payload }: TooltipContentProps) => {
  if (active && payload?.length) {
    const point = payload[0].payload as ModelShare;
    return (
      <div className="rounded px-3 py-2 text-xs font-mono" style={{ background: '#1e293b', border: '1px solid #334155' }}>
        <div className="font-medium" style={{ color: point.color }}>{payload[0].name}</div>
        <div className="text-slate-300">{payload[0].value}%</div>
      </div>
    );
  }
  return null;
};

const renderLegend = ({ payload }: DefaultLegendContentProps) => {
  return (
    <div className="flex flex-wrap gap-x-4 gap-y-1 mt-2 justify-center">
      {payload?.map((entry: LegendPayload, i: number) => (
        <div key={i} className="flex items-center gap-1.5 text-xs font-mono text-slate-400">
          <span className="w-2 h-2 rounded-full shrink-0" style={{ background: entry.color }} />
          {entry.value}
        </div>
      ))}
    </div>
  );
};

export default function ModelDistribution({ tokenBuckets, modelShare }: ModelDistributionProps) {
  const [activeFilter, setActiveFilter] = useState<string>('Month');

  return (
    <div className="h-full flex flex-col gap-3 min-h-0">
      {/* Time Filter Bar */}
      <div className="flex items-center gap-2 shrink-0">
        <div className="flex rounded overflow-hidden border border-slate-700">
          {TIME_FILTERS.map(f => (
            <button
              key={f}
              onClick={() => setActiveFilter(f)}
              className="px-3 py-1.5 text-xs font-medium transition-colors"
              style={{
                background: activeFilter === f ? '#38bdf8' : '#1e293b',
                color: activeFilter === f ? '#0f172a' : '#64748b',
                borderRight: f !== 'Year' ? '1px solid #334155' : 'none',
              }}
            >
              {f}
            </button>
          ))}
        </div>
        <div className="ml-auto flex items-center gap-2">
          <input
            type="text"
            placeholder="From"
            className="rounded px-2 py-1.5 text-xs font-mono w-24"
            style={{ background: '#1e293b', border: '1px solid #334155', color: '#64748b' }}
          />
          <span className="text-slate-600 text-xs">—</span>
          <input
            type="text"
            placeholder="To"
            className="rounded px-2 py-1.5 text-xs font-mono w-24"
            style={{ background: '#1e293b', border: '1px solid #334155', color: '#64748b' }}
          />
        </div>
      </div>

      {/* Charts row */}
      <div className="flex gap-3 flex-1 min-h-0">
        {/* Token Histogram */}
        <div className="flex-1 rounded-lg p-4 flex flex-col min-h-0" style={{ background: '#1e293b', border: '1px solid #334155' }}>
          <div className="mb-3 shrink-0">
            <div className="text-xs font-semibold text-slate-400 uppercase tracking-widest">Token Volume Histogram</div>
            <div className="flex items-center gap-4 mt-1.5">
              <span className="flex items-center gap-1.5 text-xs font-mono text-slate-500">
                <span className="w-2 h-2 rounded-sm" style={{ background: '#38bdf8' }} /> Prompt
              </span>
              <span className="flex items-center gap-1.5 text-xs font-mono text-slate-500">
                <span className="w-2 h-2 rounded-sm" style={{ background: '#10b981' }} /> Completion
              </span>
            </div>
          </div>
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={tokenBuckets} margin={{ top: 4, right: 4, left: 0, bottom: 0 }} barGap={2}>
              <CartesianGrid strokeDasharray="3 3" stroke="#1e3a5f" vertical={false} />
              <XAxis
                dataKey="slot"
                tick={{ fill: '#475569', fontSize: 10, fontFamily: 'JetBrains Mono, monospace' }}
                axisLine={false}
                tickLine={false}
              />
              <YAxis
                tick={{ fill: '#475569', fontSize: 10, fontFamily: 'JetBrains Mono, monospace' }}
                axisLine={false}
                tickLine={false}
                tickFormatter={fmtM}
                width={38}
              />
              <Tooltip content={TokenTooltip} />
              <Bar dataKey="prompt" fill="#38bdf8" radius={[2, 2, 0, 0]} barSize={14} />
              <Bar dataKey="completion" fill="#10b981" radius={[2, 2, 0, 0]} barSize={14} />
            </BarChart>
          </ResponsiveContainer>
        </div>

        {/* Model Market Share */}
        <div className="rounded-lg p-4 flex flex-col min-h-0" style={{ background: '#1e293b', border: '1px solid #334155', width: '42%' }}>
          <div className="mb-2 shrink-0">
            <div className="text-xs font-semibold text-slate-400 uppercase tracking-widest">Model Market Share</div>
            <div className="text-xs text-slate-600 mt-0.5">By execution volume</div>
          </div>
          <ResponsiveContainer width="100%" height="100%">
            <PieChart>
              <Pie
                data={modelShare}
                cx="50%"
                cy="44%"
                innerRadius="42%"
                outerRadius="68%"
                paddingAngle={2}
                dataKey="value"
                nameKey="model"
              >
                {modelShare.map((entry, i) => (
                  <Cell key={i} fill={entry.color} stroke="transparent" />
                ))}
              </Pie>
              <Tooltip content={PieTooltip} />
              <Legend content={renderLegend} />
            </PieChart>
          </ResponsiveContainer>
        </div>
      </div>
    </div>
  );
}
