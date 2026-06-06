// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', () => {
  const tableScroll = document.querySelector('[data-monthly-table-scroll]');
  const topScroll = document.querySelector('[data-monthly-scrollbar]');

  if (!tableScroll || !topScroll) {
    return;
  }

  const topScrollInner = topScroll.querySelector('.controle-mensal-scrollbar-inner');
  if (!topScrollInner) {
    return;
  }

  let syncing = false;
  const syncScrollbarWidth = () => {
    topScrollInner.style.width = `${tableScroll.scrollWidth}px`;
  };

  const syncScrollLeft = (source, target) => {
    if (syncing) {
      return;
    }

    syncing = true;
    target.scrollLeft = source.scrollLeft;
    syncing = false;
  };

  const setScrollLeft = (value) => {
    const maxScrollLeft = tableScroll.scrollWidth - tableScroll.clientWidth;
    const nextScrollLeft = Math.max(0, Math.min(value, maxScrollLeft));
    syncing = true;
    tableScroll.scrollLeft = nextScrollLeft;
    topScroll.scrollLeft = nextScrollLeft;
    syncing = false;
  };

  syncScrollbarWidth();
  window.addEventListener('resize', syncScrollbarWidth);
  tableScroll.addEventListener('scroll', () => syncScrollLeft(tableScroll, topScroll));
  topScroll.addEventListener('scroll', () => syncScrollLeft(topScroll, tableScroll));

  document.querySelectorAll('[data-monthly-scroll]').forEach((button) => {
    button.addEventListener('click', () => {
      const direction = button.getAttribute('data-monthly-scroll');
      const step = 480;

      if (direction === 'start') {
        setScrollLeft(0);
        return;
      }

      if (direction === 'end') {
        setScrollLeft(tableScroll.scrollWidth);
        return;
      }

      setScrollLeft(tableScroll.scrollLeft + (direction === 'left' ? -step : step));
    });
  });
});
