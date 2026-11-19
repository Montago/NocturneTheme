// GallerySnapshot.cs for the web: with ?shot=<name>, shows that sample, opens what is marked
// data-demo-open (a drop-down, menu, tool tip or validation callout) and draws the window ground behind
// the sample and its popups. Everything else on the page stays magenta, so tools/web_snapshots.py
// cuts the picture out by colour.
(function () {
  'use strict';

  const name = document.body.dataset.shot;
  if (!name) return;
  const sample = document.querySelector(`.gallery-sample[data-shot="${CSS.escape(name)}"]`);
  if (!sample) return;
  sample.classList.add('is-shot');

  const center = el => {
    const r = el.getBoundingClientRect();
    return { clientX: Math.round(r.left + Math.min(r.width / 2, 24)), clientY: Math.round(r.top + r.height / 2) };
  };
  // A mouse click as nocturne.js sees it: the state at pointerdown decides (dropDownOnClick)
  const click = el => {
    el.dispatchEvent(new PointerEvent('pointerdown', { bubbles: true, ...center(el) }));
    el.dispatchEvent(new MouseEvent('click', { bubbles: true, detail: 1, ...center(el) }));
  };
  const hover = el => {
    const at = center(el);
    document.dispatchEvent(new PointerEvent('pointermove', { bubbles: true, ...at }));
    el.dispatchEvent(new PointerEvent('pointerover', { bubbles: true, ...at }));
  };

  function open(el) {
    if (el.matches('select.nc-combo-box, .nc-color-picker')) {
      click(el);
    } else if (el.matches('.nc-date-picker, .nc-time-picker, .nc-search-combo-box')) {
      click(el.querySelector('.nc-text-box-button'));
    } else if (el.matches('.nc-auto-suggest-box')) {
      const input = el.querySelector('input');
      input.value = el.dataset.demoOpen;
      input.dispatchEvent(new Event('input', { bubbles: true }));
    } else if (el.matches('.nc-split-button, .nc-drop-down-button')) {
      el.querySelector('[popovertarget]').click();
    } else if (el.matches('.nc-menu-item[popovertarget]')) {
      el.click();
    } else if (el.matches('[data-nc-context-menu]')) {
      const r = el.getBoundingClientRect();
      el.dispatchEvent(new MouseEvent('contextmenu', { bubbles: true, cancelable: true, clientX: Math.round(r.left + 130), clientY: Math.round(r.top + 40) }));
    } else if (el.matches('.nc-nocturne-grid')) {
      // the filter of the first column with a filter
      click(el.querySelector('th.has-filter .nc-nocturne-grid__filter-button'));
    } else if (el.matches('[data-nc-error]')) {
      hover(el);
    } else if (el.matches('[data-nc-tool-tip]')) {
      hover(el);
    }
  }

  document.addEventListener('DOMContentLoaded', () => {
    // nocturne.js sets the controls up on DOMContentLoaded too, before this (it was loaded first)
    sample.querySelectorAll('[data-demo-open]').forEach(open);
    // after the tool tip's 500 ms
    setTimeout(() => {
      let { left, top, right, bottom } = sample.getBoundingClientRect();
      // room for the popups' soft shadows (28px blur, 10px down)
      const pad = 40;
      document.querySelectorAll(':popover-open, .nc-tool-tip, .nc-validation-tip').forEach(p => {
        const r = p.getBoundingClientRect();
        left = Math.min(left, r.left - pad);
        top = Math.min(top, r.top - pad);
        right = Math.max(right, r.right + pad);
        bottom = Math.max(bottom, r.bottom + pad);
      });
      const backdrop = document.getElementById('GalleryBackdrop');
      Object.assign(backdrop.style, {
        left: Math.floor(left) + 'px',
        top: Math.floor(top) + 'px',
        width: Math.ceil(right - Math.floor(left)) + 'px',
        height: Math.ceil(bottom - Math.floor(top)) + 'px',
      });
    }, 800);
  });
})();
