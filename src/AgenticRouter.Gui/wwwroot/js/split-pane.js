// Draggable divider for the Live Stream split panels. Blazor passes the container, divider, and
// left panel as ElementReferences; dragging resizes the left panel's width (the right panel is
// flex-1 and absorbs the remainder). Pointer capture keeps the drag alive outside the divider.
window.splitPane = {
  init: function (container, divider, leftPanel) {
    if (!container || !divider || !leftPanel || divider.dataset.splitInit) {
      return;
    }
    divider.dataset.splitInit = "1";

    const MIN_PCT = 20;
    const MAX_PCT = 65;
    let dragging = false;

    divider.addEventListener("pointerdown", function (e) {
      dragging = true;
      divider.setPointerCapture(e.pointerId);
      divider.classList.add("dragging");
      e.preventDefault();
    });

    divider.addEventListener("pointermove", function (e) {
      if (!dragging) {
        return;
      }
      const rect = container.getBoundingClientRect();
      if (rect.width === 0) {
        return;
      }
      const pct = ((e.clientX - rect.left) / rect.width) * 100;
      leftPanel.style.width = Math.min(MAX_PCT, Math.max(MIN_PCT, pct)) + "%";
    });

    const endDrag = function () {
      dragging = false;
      divider.classList.remove("dragging");
    };
    divider.addEventListener("pointerup", endDrag);
    divider.addEventListener("pointercancel", endDrag);
  },
};
