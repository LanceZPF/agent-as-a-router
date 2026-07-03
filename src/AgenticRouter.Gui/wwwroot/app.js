// Placeholder/mock data only. The dashboard is not yet wired up to the live AgenticRouter proxy service.
const mockRecentRoutes = [
  { model: "claude-sonnet-5", provider: "anthropic", status: "ok" },
  { model: "gpt-5.4", provider: "openai", status: "ok" },
  { model: "kimi-k2.5", provider: "moonshot", status: "no credential" },
];

const tbody = document.getElementById("recent-routes");
for (const route of mockRecentRoutes) {
  const row = document.createElement("tr");
  row.innerHTML = `<td>${route.model}</td><td>${route.provider}</td><td>${route.status}</td>`;
  tbody.appendChild(row);
}
