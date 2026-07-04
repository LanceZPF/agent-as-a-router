import {
  LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
  BarChart, Bar, Cell, LabelList,
} from 'recharts';
import type { TooltipContentProps } from 'recharts';
import type { CostDataPoint, AgentROI } from '../data/mockData';

interface CostAnalyticsProps {
  costData: CostDataPoint[];
  agentROI: AgentROI[];
}

const CustomTooltip = ({ active, payload, label }: TooltipContentProps) => {
  if (active && payload && payload.length) {
    return (
      <div className="rounded px-3 py-2 text-xs font-mono"
        style={{ background: '#1e293b', border: '1px solid #334155' }}>
        <div className="text-slate-400 mb-1">{label}</div>
        <div style={{ color: '#10b981' }}>Cumulative: ${Number(payload[0].value).toFixed(2)}</div>
      </div>
    );
  }
  return null;
};

const AgentTooltip = ({ active, payload, label }: TooltipContentProps) => {
  if (active && payload && payload.length) {
    return (
      <div className="rounded px-3 py-2 text-xs font-mono"
        style={{ background: '#1e293b', border: '1px solid #334155' }}>
        <div className="text-slate-400 mb-1">{label}</div>
        <div style={{ color: '#38bdf8' }}>Reduction: {Number(payload[0].value).toFixed(2)}%</div>
        {payload[1] && <div style={{ color: '#10b981' }}>Savings: ${Number(payload[1].value).toFixed(2)}</div>}
      </div>
    );
  }
  return null;
};

export default function CostAnalytics({ costData, agentROI }: CostAnalyticsProps) {
  return (
    <div className="h-full flex flex-col gap-3 min-h-0">
      {/* Cumulative Savings Chart */}
      <div className="rounded-lg p-4 shrink-0" style={{ background: '#1e293b', border: '1px solid #334155', height: '48%' }}>
        <div className="flex items-baseline justify-between mb-3">
          <div>
            <div className="text-xs font-semibold text-slate-400 uppercase tracking-widest">Cumulative Savings</div>
            <div className="font-mono text-lg font-semibold mt-0.5" style={{ color: '#10b981' }}>
              ${costData[costData.length - 1].cumulative.toFixed(2)}
            </div>
          </div>
          <div className="text-xs text-slate-600 font-mono">Jun 1 — Jul 1</div>
        </div>
        <ResponsiveContainer width="100%" height="75%">
          <LineChart data={costData} margin={{ top: 4, right: 8, left: 0, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" stroke="#1e3a5f" />
            <XAxis
              dataKey="time"
              tick={{ fill: '#475569', fontSize: 10, fontFamily: 'JetBrains Mono, monospace' }}
              axisLine={false}
              tickLine={false}
              interval={2}
            />
            <YAxis
              tick={{ fill: '#475569', fontSize: 10, fontFamily: 'JetBrains Mono, monospace' }}
              axisLine={false}
              tickLine={false}
              tickFormatter={v => `$${v}`}
              width={42}
            />
            <Tooltip content={CustomTooltip} />
            <Line
              type="monotone"
              dataKey="cumulative"
              stroke="#10b981"
              strokeWidth={2}
              dot={false}
              activeDot={{ r: 4, fill: '#10b981', strokeWidth: 0 }}
            />
          </LineChart>
        </ResponsiveContainer>
      </div>

      {/* ROI per Agent */}
      <div className="rounded-lg p-4 flex-1 min-h-0" style={{ background: '#1e293b', border: '1px solid #334155' }}>
        <div className="mb-3">
          <div className="text-xs font-semibold text-slate-400 uppercase tracking-widest">ROI by Agent</div>
          <div className="text-xs text-slate-600 mt-0.5">Cost reduction % per operational agent</div>
        </div>
        <ResponsiveContainer width="100%" height="80%">
          <BarChart
            data={agentROI}
            layout="vertical"
            margin={{ top: 0, right: 40, left: 0, bottom: 0 }}
          >
            <CartesianGrid strokeDasharray="3 3" stroke="#1e3a5f" horizontal={false} />
            <XAxis
              type="number"
              domain={[0, 100]}
              tick={{ fill: '#475569', fontSize: 9, fontFamily: 'JetBrains Mono, monospace' }}
              axisLine={false}
              tickLine={false}
              tickFormatter={v => `${v}%`}
            />
            <YAxis
              type="category"
              dataKey="agent"
              tick={{ fill: '#94a3b8', fontSize: 10, fontFamily: 'Inter, sans-serif' }}
              axisLine={false}
              tickLine={false}
              width={160}
            />
            <Tooltip content={AgentTooltip} />
            <Bar dataKey="reduction" radius={[0, 3, 3, 0]} barSize={12}>
              {agentROI.map((entry, i) => (
                <Cell
                  key={i}
                  fill={entry.reduction >= 85 ? '#10b981' : entry.reduction >= 70 ? '#38bdf8' : '#f59e0b'}
                />
              ))}
              <LabelList
                dataKey="reduction"
                position="right"
                formatter={(v: React.ReactNode) => `${Number(v).toFixed(1)}%`}
                style={{ fill: '#64748b', fontSize: 10, fontFamily: 'JetBrains Mono, monospace' }}
              />
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}
