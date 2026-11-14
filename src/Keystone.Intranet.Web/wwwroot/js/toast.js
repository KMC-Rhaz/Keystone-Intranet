// global non-intrusive toast host; replaces alert, confirm and prompt
window.Toast = (() => {
  const maxVisible = 3;
  const host = () => document.getElementById('ks-toast-host');

  const show = ({ type = 'info', message = '', duration } = {}) => {
    const container = host();
    if (!container || !message) return;

    while (container.children.length >= maxVisible) {
      container.firstElementChild.remove();
    }

    const el = document.createElement('div');
    el.className = `ks-toast ks-toast-${type}`;
    el.setAttribute('role', 'status');

    const text = document.createElement('span');
    text.textContent = message;

    const close = document.createElement('button');
    close.type = 'button';
    close.className = 'ks-toast-close';
    close.setAttribute('aria-label', 'Close');
    close.textContent = 'x';

    el.append(text, close);
    container.appendChild(el);

    const ttl = duration ?? (type === 'error' ? 6000 : 4000);
    let timer = null;
    const remove = () => {
      if (timer) clearTimeout(timer);
      el.remove();
    };
    const start = () => { timer = setTimeout(remove, ttl); };

    close.addEventListener('click', remove);
    el.addEventListener('mouseenter', () => { if (timer) clearTimeout(timer); });
    el.addEventListener('mouseleave', start);
    start();
  };

  return { show };
})();
