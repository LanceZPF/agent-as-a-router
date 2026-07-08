// Shared mock data for the AgenticRouter Dashboard UI kit — trimmed/mirrored from
// src/AgenticRouter.Gui/Models/DashboardData.cs (MockData class) in the source repo.
export const conversations = [
  {
    id: "sess-001", title: "Code Review Analysis - PR #4521",
    first: "14:15:32", last: "14:22:18", cost: 0.04523, promptTok: 15456, complTok: 3894, fallback: false,
    turns: [
      { n: 1, agent: "Code Review Bot", model: "claude-3-haiku", roi: 85.01, cost: 0.00631, promptTok: 2104, complTok: 891, steps: 2, cache: 0, ttft: 245, ctx: 26.2, ts: "14:15:32",
        req: "Review the diff for PR #4521 (src/auth/token_service.py, 214 changed lines) and flag any security issues.",
        res: "Found 3 issues: missing null check on refresh_token (L87), unbounded retry loop (L112), and the token is logged in plaintext (L145).",
        log: [["ok","Code diff parsed (214 changed lines)"],["ok","Anthropic budget nominal; claude-3-haiku selected"],["info","Route Confirmed: claude-3-haiku"]] },
      { n: 2, agent: "Code Review Bot", model: "claude-3-haiku", roi: 84.50, cost: 0.00987, promptTok: 3240, complTok: 1205, steps: 3, cache: 72, ttft: 198, ctx: 40.5, ts: "14:17:45",
        req: "Suggest a concrete fix for the unbounded retry loop you flagged at L112.",
        res: "Replace the while-loop with a tenacity retry decorator: stop_after_attempt(3) with exponential backoff, re-raising on final failure.",
        log: [["ok","History carried forward (3,240 prompt tokens)"],["ok","Prompt cache hit: 2,333 tokens read from cache"],["info","Route Confirmed: claude-3-haiku"]] },
      { n: 3, agent: "Code Review Bot", model: "claude-3-haiku", roi: 83.20, cost: 0.01382, promptTok: 4567, complTok: 1798, steps: 4, cache: 68, ttft: 211, ctx: 57.0, ts: "14:19:22",
        req: "Apply the same retry pattern to session_service.py and show me the diff.",
        res: "Patched 2 call sites; session_refresh now shares the retry policy. Diff: +18/-9 across session_service.py and retry_util.py.",
        log: [["ok","History carried forward (4,567 prompt tokens)"],["warn","Prompt growth trending up 41% turn-over-turn"],["ok","Prompt cache hit: 3,106 tokens read from cache"],["info","Route Confirmed: claude-3-haiku"]] },
      { n: 4, agent: "Code Review Bot", model: "claude-3-haiku", roi: 88.75, cost: 0.01523, promptTok: 5545, complTok: 0, steps: 1, cache: 75, ttft: 189, ctx: 69.2, ts: "14:22:18",
        req: "Summarize all applied changes for the PR description.", res: null,
        log: [["ok","Final summary pass (no completion requested)"],["ok","Prompt cache hit: 4,159 tokens read from cache"],["info","Route Confirmed: claude-3-haiku"]] },
    ],
  },
  {
    id: "sess-002", title: "Data Pipeline Debugging - ETL Job #892",
    first: "14:08:15", last: "14:14:42", cost: 0.02545, promptTok: 8932, complTok: 2456, fallback: false,
    turns: [
      { n: 1, agent: "Data Analyst Wrapper", model: "gpt-4o-mini", roi: 87.56, cost: 0.00534, promptTok: 1890, complTok: 623, steps: 2, cache: 0, ttft: 312, ctx: 23.4, ts: "14:08:15",
        req: "ETL job #892 failed at stage 3 with ORA-01555. Here is the log excerpt - what is the root cause?",
        res: "ORA-01555 (snapshot too old): the MERGE reads a table that is mutated mid-run. Isolate the read with a staging CTAS before the merge.",
        log: [["ok","SQL error log parsed (1,234 lines)"],["ok","Short context: gpt-4o-mini sufficient"],["info","Route Confirmed: gpt-4o-mini"]] },
      { n: 2, agent: "Data Analyst Wrapper", model: "gpt-4o-mini", roi: 86.20, cost: 0.00987, promptTok: 3456, complTok: 912, steps: 3, cache: 45, ttft: 267, ctx: 42.8, ts: "14:10:33",
        req: "Here is the EXPLAIN PLAN output for the failing MERGE - where is the long read window coming from?",
        res: "The full-table scan on FACT_ORDERS forces a 40-minute read window. Add a partition-pruning predicate on LOAD_DATE to cut it.",
        log: [["ok","Query execution plan added to context"],["ok","Prompt cache hit: 1,555 tokens read from cache"],["info","Route Confirmed: gpt-4o-mini"]] },
      { n: 3, agent: "Data Analyst Wrapper", model: "gpt-4o-mini", roi: 85.90, cost: 0.01024, promptTok: 3586, complTok: 921, steps: 2, cache: 52, ttft: 278, ctx: 44.3, ts: "14:14:42",
        req: "Validate the revised MERGE statement before I schedule the rerun.",
        res: "The revised statement is safe: partition pruning cuts the read window to roughly 90 seconds, well inside undo retention.",
        log: [["ok","Fix verification pass (3,586 prompt tokens)"],["ok","Prompt cache hit: 1,865 tokens read from cache"],["info","Route Confirmed: gpt-4o-mini"]] },
    ],
  },
  {
    id: "sess-003", title: "Customer Support - Issue #78234",
    first: "13:52:10", last: "14:05:33", cost: 0.00456, promptTok: 6234, complTok: 1845, fallback: true,
    turns: [
      { n: 1, agent: "Customer Support NLP", model: "claude-3-haiku", roi: 82.30, cost: 0.00456, promptTok: 1456, complTok: 567, steps: 1, cache: 0, ttft: 189, ctx: 18.0, ts: "13:52:10",
        req: "Ticket #78234: customer reports being charged twice for the June invoice. Verify and draft a response.",
        res: "Duplicate charge confirmed against payment records. A refund-and-apology draft is ready for agent review.",
        log: [["ok","Customer inquiry classified: billing dispute"],["ok","Anthropic budget nominal; claude-3-haiku selected"],["info","Route Confirmed: claude-3-haiku"]] },
      { n: 2, agent: "Customer Support NLP", model: "fallback-cheapest-local", roi: 0, cost: 0, promptTok: 2389, complTok: 734, steps: 2, cache: 0, ttft: 445, ctx: 29.5, ts: "13:54:28", fallback: true,
        req: "The customer replied asking about the refund timeline. Draft a follow-up.",
        res: "Refunds post within 5-7 business days of approval. Suggested reply drafted with the confirmation number.",
        log: [["warn","Anthropic hourly budget breached; routing restricted"],["ok","Fallback routing activated: local model"],["info","Route Confirmed: fallback-cheapest-local"]] },
      { n: 3, agent: "Customer Support NLP", model: "fallback-cheapest-local", roi: 0, cost: 0, promptTok: 2389, complTok: 544, steps: 1, cache: 0, ttft: 512, ctx: 29.5, ts: "14:05:33", fallback: true,
        req: "Close out the ticket with a resolution summary.",
        res: "Ticket #78234 resolved: duplicate June charge refunded and a confirmation email queued to the customer.",
        log: [["warn","Anthropic budget still breached; staying on fallback"],["ok","Local model serving request"],["info","Route Confirmed: fallback-cheapest-local"]] },
    ],
  },
];

export const providers = [
  { id: "openai", name: "OpenAI API", label: "Production Pool", cap: 500, spend: 492.80, days: 0 },
  { id: "anthropic", name: "Anthropic Claude", label: "Inference Pool", cap: 300, spend: 258.40, days: 3 },
  { id: "gemini", name: "Google Gemini", label: "Analytics Pool", cap: 200, spend: 62.40, days: 21 },
  { id: "local", name: "Local Inference", label: "Fallback Pool", cap: 50, spend: 8.20, days: null },
];

export const costData = [0,4.2,9.8,17.6,26.1,38.4,51.2,67.8,82.5,99.1,112.4,124.7,133.2,138.9,141.5,142.36];
export const costLabels = ["Jun 1","Jun 3","Jun 5","Jun 7","Jun 9","Jun 11","Jun 13","Jun 15","Jun 17","Jun 19","Jun 21","Jun 23","Jun 25","Jun 27","Jun 29","Jul 1"];

export const agentRoi = [
  { agent: "Log Anomaly Detector", reduction: 91.67, savings: 38.20 },
  { agent: "SQL Query Optimizer", reduction: 87.69, savings: 22.40 },
  { agent: "Data Analyst Wrapper", reduction: 85.12, savings: 41.80 },
  { agent: "Customer Support NLP", reduction: 84.30, savings: 18.60 },
  { agent: "Summarization Pipeline", reduction: 79.50, savings: 12.40 },
  { agent: "Embedding Generator", reduction: 78.20, savings: 5.80 },
  { agent: "Code Review Bot", reduction: 64.10, savings: 2.90 },
];

export const tokenBuckets = [
  { slot: "Mon", prompt: 2840000, completion: 980000 },
  { slot: "Tue", prompt: 3120000, completion: 1140000 },
  { slot: "Wed", prompt: 4200000, completion: 1680000 },
  { slot: "Thu", prompt: 3890000, completion: 1520000 },
  { slot: "Fri", prompt: 2960000, completion: 1020000 },
  { slot: "Sat", prompt: 1840000, completion: 620000 },
  { slot: "Sun", prompt: 1240000, completion: 380000 },
];

export const modelShares = [
  { model: "gpt-4o-mini", value: 38, color: "#10b981" },
  { model: "claude-3-haiku", value: 22, color: "#38bdf8" },
  { model: "gemini-1.5-flash", value: 18, color: "#818cf8" },
  { model: "fallback-local", value: 10, color: "#f59e0b" },
  { model: "claude-3-5-sonnet", value: 7, color: "#fb7185" },
  { model: "text-embedding-3-small", value: 5, color: "#a78bfa" },
];
