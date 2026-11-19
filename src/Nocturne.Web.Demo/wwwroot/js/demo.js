// MainWindow.xaml.cs and the pages' code-behind for the web demo
(function () {
  'use strict';

  const $ = id => document.getElementById(id);
  const on = (el, name, fn) => el && el.addEventListener(name, fn);
  const base = document.querySelector('link[href*="demo.css"]').getAttribute('href').replace(/css\/demo\.css.*$/, '');

  /* ---------------------------------------------------- navigation (MainWindow) */
  const pages = [...document.querySelectorAll('.demo-page')];
  const navigation = $('Navigation');
  const history = [];
  function show(name, remember = true) {
    const page = pages.find(p => p.dataset.page === name) || pages[0];
    const current = pages.find(p => !p.hidden);
    if (current === page) return;
    if (remember && current) history.push(current.dataset.page);
    pages.forEach(p => { p.hidden = p !== page; });
    navigation?.querySelectorAll('.nc-list-box-item').forEach(item => item.setAttribute('aria-selected', String(item.dataset.value === page.dataset.page)));
    $('BackButton').disabled = history.length === 0;
    $('PageScroller').scrollTop = 0;
    window.history.replaceState(null, '', '#' + page.dataset.page);
  }
  on(navigation, 'nc-change', () => {
    const selected = navigation.querySelector('[aria-selected="true"]');
    if (selected) show(selected.dataset.value);
  });
  on(navigation, 'click', e => {
    const item = e.target.closest('.nc-list-box-item');
    if (item) show(item.dataset.value);
  });
  on($('BackButton'), 'click', () => {
    const previous = history.pop();
    if (previous) show(previous, false);
  });
  on($('NavigationToggle'), 'click', () => $('NavigationPane').classList.toggle('is-hidden'));
  if (location.hash) show(location.hash.slice(1), false);

  /* Window: the title bar style (TitleBarStyle) */
  const titleBar = $('TitleBarBox');
  if (titleBar) {
    titleBar.value = document.body.dataset.titleBarStyle || 'seamless';
    on(titleBar, 'change', () => { document.body.dataset.titleBarStyle = titleBar.value; });
  }

  /* ---------------------------------------------------------- ControlsPage */
  // A switch with its state underneath (SwitchContent with OnOffConverter)
  document.querySelectorAll('[data-demo-on-off]').forEach(state => {
    const input = state.closest('.nc-toggle-switch')?.querySelector('input');
    const sync = () => { state.textContent = input.checked ? 'On' : 'Off'; };
    on(input, 'change', sync);
    sync();
  });
  on($('BandwidthSlider'), 'nc-change', e => { $('BandwidthValue').textContent = `${Math.round(e.detail)} Mbit/s`; });
  on($('DeleteProject'), 'click', async () => {
    await Nocturne.showDialog({
      title: 'Delete project?',
      content: 'Nocturne and its 1,284 files will be deleted for everyone. This cannot be undone.',
      affirmativeText: 'Delete',
      negativeText: 'Cancel',
      affirmativeStyle: 'danger',
    });
  });
  let added = 0;
  on($('DocumentTabs'), 'nc-tab-add', () => {
    // TabHelper.AddTabCommand: the page adds the tab
    added++;
    const control = $('DocumentTabs');
    const strip = control.querySelector('.nc-tab-control__strip');
    const id = control.id + '-new-' + added;
    const tab = document.createElement('button');
    tab.type = 'button';
    tab.className = 'nc-tab-item nc-tab-item--closable';
    tab.setAttribute('role', 'tab');
    tab.setAttribute('aria-controls', id);
    tab.setAttribute('aria-selected', 'false');
    tab.innerHTML = `<span class="nc-tab-item__header">New ${added}</span>` + strip.querySelector('.nc-tab-item__close').outerHTML;
    strip.appendChild(tab);
    const panel = document.createElement('div');
    panel.className = 'nc-tab-control__content';
    panel.id = id;
    panel.setAttribute('role', 'tabpanel');
    panel.hidden = true;
    panel.innerHTML = '<p class="demo-body">An empty document.</p>';
    control.querySelector('.nc-tab-control__panel').appendChild(panel);
    control.nocturne.select(control.querySelectorAll('.nc-tab-item').length - 1);
  });

  /* ---------------------------------------------------- CustomControlsPage */
  on($('ShowDialogButton'), 'click', async () => {
    const answer = await Nocturne.showDialog({
      title: 'Discard changes?',
      content: 'You have unsaved changes in three files. Discard them and close the project?',
      affirmativeText: 'Discard',
      negativeText: 'Keep editing',
    });
    $('DialogResultText').textContent = answer === 'affirmative' ? 'Answer: Discard' : 'Answer: Keep editing';
  });
  on($('DistanceRange'), 'nc-change', e => {
    $('DistanceValue').textContent = `${Math.round(e.detail.lowerValue)} – ${Math.round(e.detail.upperValue)} km`;
  });
  on($('BusySwitch'), 'change', e => {
    const busy = $('BusyIndicator');
    busy.dataset.busy = String(e.target.checked);
    busy.setAttribute('aria-busy', String(e.target.checked));
  });

  /* -------------------------------------------------------------- GridPage */
  const cars = $('Cars') && Nocturne.nocturneGrid('Cars');
  if (cars) {
    const showStatus = () => {
      $('CarStatus').textContent = `Showing ${cars.filteredItemCount.toLocaleString('en-US')} of ${cars.totalItemCount.toLocaleString('en-US')} cars`;
    };
    const showSelection = car => {
      $('CarSelection').textContent = car ? `Selected: ${car.StockNo}, ${car.Make} ${car.Model} ${car.Year}` : 'Nothing selected';
    };
    on($('Cars'), 'nc-filtered', showStatus);
    on($('Cars'), 'nc-selected-item-changed', e => showSelection(e.detail.newValue));
    on($('Cars'), 'nc-load-failed', e => { $('CarStatus').textContent = `The server did not answer: ${e.detail.error.message}`; });
    showStatus();
    showSelection(cars.selectedItem);
    // GroupMemberPath and PageSize are bound to the combo boxes in GridPage.xaml
    on($('GroupBy'), 'change', e => { cars.groupMemberPath = e.target.value; });
    on($('PageSize'), 'change', e => { cars.pageSize = Number(e.target.value); });
    on($('ClearFilters'), 'click', () => cars.clearFilters());
    // Server mode (web only): the same rows, a page at a time from NocturneGridServer.Query
    on($('ServerMode'), 'change', e => { cars.itemsSource = e.target.checked ? base + 'api/cars' : null; });
  }

  /* -------------------------------------------------------------- TokensPage */
  function showTokens() {
    const style = getComputedStyle(document.documentElement);
    document.querySelectorAll('[data-token]').forEach(el => {
      el.textContent = style.getPropertyValue(el.dataset.token).trim().replace(/^linear-gradient\(180deg, /, '').replace(/\)$/, '').replace(/ \d+%/g, '').replace(', ', ' → ');
    });
  }
  showTokens();
  Nocturne.NocturneTheme.onAccentChanged(() => requestAnimationFrame(showTokens));

  /* -------------------------------------------------------------- IconsPage */
  const search = $('IconSearch');
  on(search, 'input', () => {
    const words = search.value.toLowerCase().split(/\s+/).filter(Boolean);
    document.querySelectorAll('#IconCards > .demo-icon-card').forEach(card => {
      const name = card.dataset.kind.toLowerCase();
      card.hidden = !words.every(w => name.includes(w));
    });
  });
  document.addEventListener('click', async e => {
    const card = e.target.closest('.demo-icon-card');
    if (!card) return;
    try {
      await navigator.clipboard.writeText(card.dataset.markup);
      $('IconCopied').textContent = 'Copied ' + card.dataset.markup;
    } catch {
      $('IconCopied').textContent = card.dataset.markup;
    }
  });
})();
