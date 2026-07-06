// Floating tooltips for elements carrying a data-tip attribute. A single body-level, fixed-position
// element is repositioned on hover, so tooltips are never clipped by the dashboard's internal scroll
// containers (which native title tooltips and CSS ::after tooltips both suffer from inside a
// BlazorWebView). Event delegation at document level survives Blazor re-renders.
(function () {
  const tip = document.createElement("div");
  tip.className = "ls-tooltip";
  tip.style.display = "none";
  document.body.appendChild(tip);

  function hide() {
    tip.style.display = "none";
  }

  document.addEventListener("mouseover", function (e) {
    const target = e.target && e.target.closest ? e.target.closest("[data-tip]") : null;
    if (!target) {
      hide();
      return;
    }

    tip.textContent = target.getAttribute("data-tip");
    tip.style.display = "block";

    // Place below the element, clamped to the viewport; flip above if there is no room below.
    const rect = target.getBoundingClientRect();
    const width = tip.offsetWidth;
    const height = tip.offsetHeight;
    const x = Math.min(Math.max(4, rect.left), window.innerWidth - width - 4);
    let y = rect.bottom + 6;
    if (y + height > window.innerHeight - 4) {
      y = rect.top - height - 6;
    }
    tip.style.left = x + "px";
    tip.style.top = y + "px";
  });

  document.addEventListener("mouseleave", hide);
  document.addEventListener("scroll", hide, true);
  document.addEventListener("click", hide, true);
})();
