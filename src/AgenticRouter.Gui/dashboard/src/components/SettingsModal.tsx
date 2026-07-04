import { useState, useRef } from 'react';
import { X, AlertTriangle, Trash2, RotateCcw } from 'lucide-react';

interface SettingsModalProps {
  onClose: () => void;
}

type Action = 'reset' | 'purge' | null;

export default function SettingsModal({ onClose }: SettingsModalProps) {
  const [activeAction, setActiveAction] = useState<Action>(null);
  const [confirmText, setConfirmText] = useState('');
  const inputRef = useRef<HTMLInputElement>(null);

  const required = activeAction === 'reset' ? 'RESET' : 'PURGE';
  const isConfirmed = confirmText === required;

  function startAction(a: Action) {
    setActiveAction(a);
    setConfirmText('');
    setTimeout(() => inputRef.current?.focus(), 50);
  }

  function executeAction() {
    if (!isConfirmed) return;
    setActiveAction(null);
    setConfirmText('');
    onClose();
  }

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center"
      style={{ backgroundColor: 'rgba(0,0,0,0.7)', backdropFilter: 'blur(4px)' }}
      onClick={(e) => e.target === e.currentTarget && onClose()}
    >
      <div className="w-full max-w-md rounded-lg border border-slate-700" style={{ background: '#1e293b' }}>
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 border-b border-slate-700">
          <span className="text-sm font-semibold text-slate-200 tracking-wide uppercase">System Settings</span>
          <button onClick={onClose} className="text-slate-400 hover:text-slate-200 transition-colors">
            <X size={16} />
          </button>
        </div>

        <div className="p-5 space-y-4">
          {/* Destructive zone label */}
          <div className="flex items-center gap-2 text-xs text-amber-400 font-medium uppercase tracking-widest">
            <AlertTriangle size={12} />
            <span>Destructive Actions Zone</span>
          </div>

          {/* Action Buttons */}
          {!activeAction && (
            <div className="grid grid-cols-2 gap-3">
              <button
                onClick={() => startAction('reset')}
                className="flex items-center justify-center gap-2 rounded px-4 py-3 text-sm font-medium border border-amber-500/40 text-amber-400 hover:bg-amber-500/10 transition-colors"
              >
                <RotateCcw size={14} />
                Reset Stats
              </button>
              <button
                onClick={() => startAction('purge')}
                className="flex items-center justify-center gap-2 rounded px-4 py-3 text-sm font-medium border border-red-500/40 text-red-400 hover:bg-red-500/10 transition-colors"
              >
                <Trash2 size={14} />
                Clear History
              </button>
            </div>
          )}

          {/* Confirmation step */}
          {activeAction && (
            <div className="space-y-3 rounded border p-4"
              style={{ borderColor: activeAction === 'reset' ? '#f59e0b44' : '#ef444444', background: '#0f172a' }}>
              <p className="text-xs text-slate-400 leading-5">
                This action is <span className="text-red-400 font-semibold">irreversible</span>. Type{' '}
                <span className="font-mono text-slate-200 bg-slate-700 px-1.5 py-0.5 rounded text-xs">{required}</span>{' '}
                to confirm.
              </p>
              <input
                ref={inputRef}
                type="text"
                value={confirmText}
                onChange={(e) => setConfirmText(e.target.value.toUpperCase())}
                placeholder={`Type ${required}...`}
                className="w-full rounded px-3 py-2 text-sm font-mono"
                style={{ background: '#1e293b', border: '1px solid #334155', color: '#e2e8f0' }}
              />
              <div className="flex gap-2">
                <button
                  onClick={() => { setActiveAction(null); setConfirmText(''); }}
                  className="flex-1 rounded px-3 py-2 text-xs text-slate-400 border border-slate-600 hover:bg-slate-700 transition-colors"
                >
                  Cancel
                </button>
                <button
                  onClick={executeAction}
                  disabled={!isConfirmed}
                  className="flex-1 rounded px-3 py-2 text-xs font-semibold transition-colors"
                  style={{
                    background: isConfirmed ? (activeAction === 'reset' ? '#f59e0b' : '#ef4444') : '#1e293b',
                    color: isConfirmed ? '#0f172a' : '#475569',
                    border: `1px solid ${isConfirmed ? 'transparent' : '#334155'}`,
                    cursor: isConfirmed ? 'pointer' : 'not-allowed',
                  }}
                >
                  Confirm {activeAction === 'reset' ? 'Reset' : 'Purge'}
                </button>
              </div>
            </div>
          )}

          <p className="text-xs text-slate-600 pt-1">
            All system configurations, routing rules, and budget caps are preserved.
          </p>
        </div>
      </div>
    </div>
  );
}
