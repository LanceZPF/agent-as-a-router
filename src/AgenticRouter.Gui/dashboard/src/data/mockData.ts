export interface RoutingEntry {
  id: string;
  sessionId: string;
  traceId: string;
  agent: string;
  model: string;
  isFallback: boolean;
  promptTokens: number;
  completionTokens: number;
  actualCost: number;
  worstCaseCost: number;
  savingsAmount: number;
  savingsPercent: number;
  timestamp: string;
  routingSteps: RoutingStep[];
}

export interface RoutingStep {
  status: 'ok' | 'warn' | 'info';
  message: string;
}

export interface Provider {
  id: string;
  name: string;
  label: string;
  budgetCap: number;
  currentSpend: number;
  estimatedDaysRemaining: number | null;
}

export interface CostDataPoint {
  time: string;
  cumulative: number;
}

export interface AgentROI {
  agent: string;
  reduction: number;
  savings: number;
}

export interface TokenBucket {
  slot: string;
  prompt: number;
  completion: number;
}

export interface ModelShare {
  model: string;
  value: number;
  color: string;
}

export const MOCK_ENTRIES: RoutingEntry[] = [
  {
    id: '1',
    sessionId: 'e89a2bc',
    traceId: 'a4f89c02',
    agent: 'Data Analyst Wrapper',
    model: 'gpt-4o-mini',
    isFallback: false,
    promptTokens: 1230,
    completionTokens: 702,
    actualCost: 0.000480,
    worstCaseCost: 0.003860,
    savingsAmount: 0.003380,
    savingsPercent: 87.56,
    timestamp: '14:32:01',
    routingSteps: [
      { status: 'ok', message: 'Input text contains source code telemetry' },
      { status: 'ok', message: 'Budget nominal: gpt-4o-mini selected' },
      { status: 'ok', message: 'Context window validated (1,932 tokens)' },
      { status: 'info', message: 'Route Confirmed: gpt-4o-mini' },
    ],
  },
  {
    id: '2',
    sessionId: 'd32a1ff',
    traceId: 'b7c21e45',
    agent: 'Code Review Bot',
    model: 'fallback-cheapest-local',
    isFallback: true,
    promptTokens: 890,
    completionTokens: 312,
    actualCost: 0.000000,
    worstCaseCost: 0.002100,
    savingsAmount: 0.000000,
    savingsPercent: 0.00,
    timestamp: '14:31:48',
    routingSteps: [
      { status: 'ok', message: 'Input text contains source code telemetry' },
      { status: 'warn', message: 'OpenAI budget breached; routing restricted' },
      { status: 'ok', message: 'Fallback routing activated for destination' },
      { status: 'info', message: 'Route Confirmed: fallback-cheapest-local' },
    ],
  },
  {
    id: '3',
    sessionId: 'f12b8de',
    traceId: 'c9d34f67',
    agent: 'Customer Support NLP',
    model: 'claude-3-haiku',
    isFallback: false,
    promptTokens: 2104,
    completionTokens: 891,
    actualCost: 0.000631,
    worstCaseCost: 0.004210,
    savingsAmount: 0.003579,
    savingsPercent: 85.01,
    timestamp: '14:31:35',
    routingSteps: [
      { status: 'ok', message: 'Conversation context parsed (3 turns)' },
      { status: 'ok', message: 'Anthropic budget nominal; claude-3-haiku selected' },
      { status: 'ok', message: 'Response latency target: <800ms' },
      { status: 'info', message: 'Route Confirmed: claude-3-haiku' },
    ],
  },
  {
    id: '4',
    sessionId: 'a45c7fe',
    traceId: 'd2e56g78',
    agent: 'SQL Query Optimizer',
    model: 'gpt-4o-mini',
    isFallback: false,
    promptTokens: 445,
    completionTokens: 203,
    actualCost: 0.000096,
    worstCaseCost: 0.000780,
    savingsAmount: 0.000684,
    savingsPercent: 87.69,
    timestamp: '14:31:22',
    routingSteps: [
      { status: 'ok', message: 'SQL schema fingerprint matched' },
      { status: 'ok', message: 'Short context: mini-model sufficient' },
      { status: 'info', message: 'Route Confirmed: gpt-4o-mini' },
    ],
  },
  {
    id: '5',
    sessionId: 'c78d2ae',
    traceId: 'e3f67h89',
    agent: 'Log Anomaly Detector',
    model: 'gemini-1.5-flash',
    isFallback: false,
    promptTokens: 3840,
    completionTokens: 128,
    actualCost: 0.000192,
    worstCaseCost: 0.002304,
    savingsAmount: 0.002112,
    savingsPercent: 91.67,
    timestamp: '14:31:09',
    routingSteps: [
      { status: 'ok', message: 'Long-context log batch detected (3,840 tokens)' },
      { status: 'ok', message: 'Gemini flash selected for cost efficiency' },
      { status: 'ok', message: 'Output: classification only (128 tokens)' },
      { status: 'info', message: 'Route Confirmed: gemini-1.5-flash' },
    ],
  },
  {
    id: '6',
    sessionId: 'b91e3cd',
    traceId: 'f4a78i90',
    agent: 'Summarization Pipeline',
    model: 'fallback-cheapest-local',
    isFallback: true,
    promptTokens: 1560,
    completionTokens: 420,
    actualCost: 0.000000,
    worstCaseCost: 0.003120,
    savingsAmount: 0.000000,
    savingsPercent: 0.00,
    timestamp: '14:30:55',
    routingSteps: [
      { status: 'ok', message: 'Document chunking completed (4 chunks)' },
      { status: 'warn', message: 'OpenAI monthly cap reached: fallback triggered' },
      { status: 'warn', message: 'Gemini quota exhausted for current period' },
      { status: 'ok', message: 'Local model activated as final fallback' },
      { status: 'info', message: 'Route Confirmed: fallback-cheapest-local' },
    ],
  },
  {
    id: '7',
    sessionId: 'g23f4bc',
    traceId: 'g5b89j01',
    agent: 'Embedding Generator',
    model: 'text-embedding-3-small',
    isFallback: false,
    promptTokens: 512,
    completionTokens: 0,
    actualCost: 0.000002,
    worstCaseCost: 0.000010,
    savingsAmount: 0.000008,
    savingsPercent: 80.00,
    timestamp: '14:30:42',
    routingSteps: [
      { status: 'ok', message: 'Embedding task detected: no completion needed' },
      { status: 'ok', message: 'text-embedding-3-small selected (optimal cost)' },
      { status: 'info', message: 'Route Confirmed: text-embedding-3-small' },
    ],
  },
  {
    id: '8',
    sessionId: 'h67g8de',
    traceId: 'h6c90k12',
    agent: 'Data Analyst Wrapper',
    model: 'claude-3-5-sonnet',
    isFallback: false,
    promptTokens: 4200,
    completionTokens: 1890,
    actualCost: 0.018270,
    worstCaseCost: 0.037800,
    savingsAmount: 0.019530,
    savingsPercent: 51.67,
    timestamp: '14:30:29',
    routingSteps: [
      { status: 'ok', message: 'Complex reasoning task detected' },
      { status: 'ok', message: 'High token count requires Sonnet-tier model' },
      { status: 'ok', message: 'Anthropic budget nominal' },
      { status: 'info', message: 'Route Confirmed: claude-3-5-sonnet' },
    ],
  },
];

export const MOCK_PROVIDERS: Provider[] = [
  {
    id: 'openai',
    name: 'OpenAI API',
    label: 'Production Pool',
    budgetCap: 500,
    currentSpend: 492.80,
    estimatedDaysRemaining: 0,
  },
  {
    id: 'anthropic',
    name: 'Anthropic Claude',
    label: 'Inference Pool',
    budgetCap: 300,
    currentSpend: 258.40,
    estimatedDaysRemaining: 3,
  },
  {
    id: 'gemini',
    name: 'Google Gemini',
    label: 'Analytics Pool',
    budgetCap: 200,
    currentSpend: 62.40,
    estimatedDaysRemaining: 21,
  },
  {
    id: 'local',
    name: 'Local Inference',
    label: 'Fallback Pool',
    budgetCap: 50,
    currentSpend: 8.20,
    estimatedDaysRemaining: null,
  },
];

export const COST_DATA: CostDataPoint[] = [
  { time: 'Jun 1', cumulative: 0 },
  { time: 'Jun 3', cumulative: 4.20 },
  { time: 'Jun 5', cumulative: 9.80 },
  { time: 'Jun 7', cumulative: 17.60 },
  { time: 'Jun 9', cumulative: 26.10 },
  { time: 'Jun 11', cumulative: 38.40 },
  { time: 'Jun 13', cumulative: 51.20 },
  { time: 'Jun 15', cumulative: 67.80 },
  { time: 'Jun 17', cumulative: 82.50 },
  { time: 'Jun 19', cumulative: 99.10 },
  { time: 'Jun 21', cumulative: 112.40 },
  { time: 'Jun 23', cumulative: 124.70 },
  { time: 'Jun 25', cumulative: 133.20 },
  { time: 'Jun 27', cumulative: 138.90 },
  { time: 'Jun 29', cumulative: 141.50 },
  { time: 'Jul 1', cumulative: 142.36 },
];

export const AGENT_ROI: AgentROI[] = [
  { agent: 'Log Anomaly Detector', reduction: 91.67, savings: 38.20 },
  { agent: 'SQL Query Optimizer', reduction: 87.69, savings: 22.40 },
  { agent: 'Data Analyst Wrapper', reduction: 85.12, savings: 41.80 },
  { agent: 'Customer Support NLP', reduction: 84.30, savings: 18.60 },
  { agent: 'Summarization Pipeline', reduction: 79.50, savings: 12.40 },
  { agent: 'Embedding Generator', reduction: 78.20, savings: 5.80 },
  { agent: 'Code Review Bot', reduction: 64.10, savings: 2.90 },
];

export const TOKEN_BUCKETS: TokenBucket[] = [
  { slot: 'Mon', prompt: 2840000, completion: 980000 },
  { slot: 'Tue', prompt: 3120000, completion: 1140000 },
  { slot: 'Wed', prompt: 4200000, completion: 1680000 },
  { slot: 'Thu', prompt: 3890000, completion: 1520000 },
  { slot: 'Fri', prompt: 2960000, completion: 1020000 },
  { slot: 'Sat', prompt: 1840000, completion: 620000 },
  { slot: 'Sun', prompt: 1240000, completion: 380000 },
];

export const MODEL_SHARE: ModelShare[] = [
  { model: 'gpt-4o-mini', value: 38, color: '#10b981' },
  { model: 'claude-3-haiku', value: 22, color: '#38bdf8' },
  { model: 'gemini-1.5-flash', value: 18, color: '#818cf8' },
  { model: 'fallback-local', value: 10, color: '#f59e0b' },
  { model: 'claude-3-5-sonnet', value: 7, color: '#fb7185' },
  { model: 'text-embedding-3-small', value: 5, color: '#a78bfa' },
];
