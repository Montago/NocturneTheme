// Filter forms (data-auto-submit) reload the page when a filter changes; typing waits a moment.
(function () {
  'use strict';

  let timer = 0;
  document.addEventListener('change', event => {
    const form = event.target.closest?.('form[data-auto-submit]');
    if (form && event.target.type !== 'search') {
      form.requestSubmit();
    }
  });
  document.addEventListener('input', event => {
    const form = event.target.closest?.('form[data-auto-submit]');
    if (form && event.target.type === 'search') {
      clearTimeout(timer);
      timer = setTimeout(() => form.requestSubmit(), 400);
    }
  });
  /* The accent box of the Settings flyout: NocturneTheme.SetAccent, remembered in this browser */
  const accent = document.querySelector('[data-sample-accent]');
  if (accent) {
    accent.value = Nocturne.NocturneTheme.accent;
    accent.addEventListener('change', () => Nocturne.setAccent(accent.value));
  }
})();
