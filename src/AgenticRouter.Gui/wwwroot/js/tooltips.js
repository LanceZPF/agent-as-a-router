// Floating tooltips for elements carrying a data-tip attribute. A single body-level, fixed-position
// element is repositioned on hover or keyboard focus, so tooltips are never clipped by the
// dashboard's internal scroll containers (which native title tooltips and CSS ::after tooltips both
// suffer from inside a BlazorWebView). Event delegation at document level survives Blazor re-renders.
//
// Accessibility: the tooltip element stays in the DOM at all times and is only visually hidden via
// opacity (see the .ls-tooltip/.visible rules in app.css) rather than display:none, because
// display:none removes an element from the accessibility tree and breaks aria-describedby. Every
// data-tip element is expected to carry a static aria-describedby="ls-tooltip" (set in the Razor
// markup) plus tabindex="0" if it isn't already natively focusable, so keyboard-only and
// screen-reader users can reach and hear every tooltip a mouse user can hover. Escape dismisses,
// matching the WAI-ARIA tooltip pattern.
(function () {
  const tip = document.createElement("div");
  tip.id = "ls-tooltip";
  tip.className = "ls-tooltip";
  tip.setAttribute("role", "tooltip");
  document.body.appendChild(tip);

  // Which input mode is currently showing the tooltip, so a mouseleave over unrelated content
  // doesn't dismiss a tooltip that keyboard focus opened, and vice versa.
  let activeSource = null; // "mouse" | "focus" | null

  function position(target) {
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
  }

  function show(target, source) {
    activeSource = source;
    tip.textContent = target.getAttribute("data-tip");
    tip.classList.add("visible");
    position(target);
  }

  function hide(source) {
    // Ignore a dismissal from the "other" input mode: e.g. don't let the mouse leaving the window
    // hide a tooltip that keyboard focus is currently showing.
    if (source && activeSource !== source) {
      return;
    }
    activeSource = null;
    tip.classList.remove("visible");
  }

  function findTarget(e) {
    return e.target && e.target.closest ? e.target.closest("[data-tip]") : null;
  }

  document.addEventListener("mouseover", function (e) {
    const target = findTarget(e);
    if (target) {
      show(target, "mouse");
    } else {
      hide("mouse");
    }
  });
  document.addEventListener("mouseleave", function () {
    hide("mouse");
  });

  // focusin/focusout bubble (plain focus/blur don't), so they can be handled via the same
  // document-level delegation as the mouse events above.
  document.addEventListener("focusin", function (e) {
    const target = findTarget(e);
    if (target) {
      show(target, "focus");
    }
  });
  document.addEventListener("focusout", function (e) {
    if (findTarget(e)) {
      hide("focus");
    }
  });

  document.addEventListener("keydown", function (e) {
    if (e.key === "Escape") {
      activeSource = null;
      tip.classList.remove("visible");
    }
  });

  document.addEventListener(
    "scroll",
    function () {
      activeSource = null;
      tip.classList.remove("visible");
    },
    true,
  );
  document.addEventListener(
    "click",
    function () {
      activeSource = null;
      tip.classList.remove("visible");
    },
    true,
  );
})();
