# Repository Restructure: Prototype File Mapping

The original React/Vite prototype arrived as a flat bolt.new export whose
**physical filenames did not match their contents** (e.g. the Analytics
component lived in a file named `download`, the real `package.json` in
`tsconfig.node.json`). To free the repository root for the .NET MAUI
solution, every prototype file was moved into `prototype/` under its
**verified real name**. Each mapping below was confirmed by inspecting the
file's contents before the move.

The prototype is kept as a **design reference only** (dark slate theme,
4 tabs: Live Stream, Cost Analytics, Model Distribution, Governance). It is
not expected to build; some original files (real `tsconfig*.json`,
`vite.config.ts`, `tailwind.config.js`, `postcss.config.js`) were lost in
the scrambled export and are absent.

## Mapping applied

| Original physical file | Actual content | New path |
|---|---|---|
| `LiveStream.tsx` | App shell (tab navigation, state) | `prototype/src/App.tsx` |
| `ModelDistribution.tsx` | Types + mock data | `prototype/src/data/mockData.ts` |
| `download` | Analytics tab (CostAnalytics component) | `prototype/src/components/CostAnalytics.tsx` |
| `package-lock.json` | ModelDistribution component | `prototype/src/components/ModelDistribution.tsx` |
| `eslint.config.js` | Governance component | `prototype/src/components/Governance.tsx` |
| `index (1).html` | LiveStream component | `prototype/src/components/LiveStream.tsx` |
| `package.json` | SettingsModal component | `prototype/src/components/SettingsModal.tsx` |
| `CostAnalytics.tsx` | Source stylesheet | `prototype/src/index.css` |
| `postcss.config.js` | Entry point (`main.tsx`) | `prototype/src/main.tsx` |
| `Governance.tsx` | Vite env type reference | `prototype/src/vite-env.d.ts` |
| `tsconfig.node.json` | Real `package.json` | `prototype/package.json` |
| `vite.config.ts` (164 KB) | Real `package-lock.json` | `prototype/package-lock.json` |
| `tsconfig.json` | Real ESLint flat config | `prototype/eslint.config.js` |
| `tsconfig.app.json` | Real `index.html` | `prototype/index.html` |
| `tailwind.config.js` | Real `.gitignore` | `prototype/.gitignore` |
| `App.tsx` | Netlify redirects | `prototype/public/_redirects` |
| `index.html` | bolt.new design prompt text | `prototype/.bolt/prompt` |
| `prompt` | bolt.new template metadata (JSON) | `prototype/.bolt/config.json` |

## Deleted (regenerable build artifacts)

| File | Reason |
|---|---|
| `index.css` | Built/minified dist stylesheet |
| `index-B9Z7yfCw.js` | Built/minified dist bundle |
| `vite-env.d.ts` | Built dist artifact (content duplicated by real source) |

## Root layout after restructure

```
AgenticRouter.Gui.sln
src/
  AgenticRouter.Gui.Core/    # plain .NET class library — domain, analytics, mock data, ViewModels
  AgenticRouter.Gui.App/     # .NET MAUI head — XAML views, chart adapter, DI
tests/
  AgenticRouter.Gui.Core.Tests/
docs/
prototype/                   # original React prototype, design reference only
```
