// delegated ui handlers, csrf wiring, and envelope-driven fetch helper
if (window.htmx) {
  window.htmx.config.allowEval = false;
  window.htmx.config.allowScriptTags = false;
}

window.Ks = (() => {
  const csrf = () => document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') || '';

  const post = async (url, body) => {
    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'X-Requested-With': 'fetch',
        'RequestVerificationToken': csrf()
      },
      body: JSON.stringify(body)
    });

    const envelope = await response.json().catch(() => ({
      success: false,
      message: 'Something went wrong. Please try again.',
      errors: [],
      toast: 'error'
    }));

    if (envelope.message) {
      window.Toast.show({ type: envelope.toast || (envelope.success ? 'success' : 'error'), message: envelope.message });
    }
    return envelope;
  };

  const navigate = (url) => {
    if (!url) return;
    if (window.htmx) {
      window.htmx.ajax('GET', url, { target: '#page', select: '#page', swap: 'outerHTML transition:true' });
      window.history.pushState({}, '', url);
      return;
    }
    window.location.assign(url);
  };

  return { csrf, post, navigate };
})();

document.addEventListener('htmx:configRequest', (e) => {
  const token = window.Ks.csrf();
  if (token) e.detail.headers['RequestVerificationToken'] = token;
});

document.addEventListener('change', (e) => {
  const select = e.target.closest('[data-auto-submit]');
  if (select && select.form) select.form.submit();
});

document.addEventListener('click', async (e) => {
  const openBtn = e.target.closest('[data-confirm-open]');
  if (openBtn) {
    const panel = document.getElementById(openBtn.dataset.confirmOpen);
    if (panel) panel.classList.remove('ks-hidden');
    return;
  }

  const closeBtn = e.target.closest('[data-confirm-close]');
  if (closeBtn) {
    const panel = document.getElementById(closeBtn.dataset.confirmClose);
    if (panel) panel.classList.add('ks-hidden');
    return;
  }

  const confirmBtn = e.target.closest('[data-confirm-post]');
  if (confirmBtn) {
    confirmBtn.disabled = true;
    const payload = JSON.parse(confirmBtn.dataset.payload || '{}');
    const env = await window.Ks.post(confirmBtn.dataset.confirmPost, payload);
    confirmBtn.disabled = false;
    if (env.success && env.data?.redirectUrl) window.Ks.navigate(env.data.redirectUrl);
    return;
  }

  const decisionBtn = e.target.closest('[data-decision-url]');
  if (decisionBtn) {
    decisionBtn.disabled = true;
    const commentEl = document.getElementById('ks-decision-comment');
    const payload = {
      requestId: decisionBtn.dataset.requestId,
      expectedVersion: Number(decisionBtn.dataset.version),
      comment: commentEl ? commentEl.value : ''
    };
    const env = await window.Ks.post(decisionBtn.dataset.decisionUrl, payload);
    decisionBtn.disabled = false;
    if (env.success && env.data?.redirectUrl) window.Ks.navigate(env.data.redirectUrl);
    return;
  }

  const userBtn = e.target.closest('[data-user-approve]');
  if (userBtn) {
    const card = userBtn.closest('[data-user-card]');
    if (!card) return;
    userBtn.disabled = true;
    const companyIds = [...card.querySelectorAll('input[name="companyIds"]:checked')].map((i) => i.value);
    const roleKeys = [...card.querySelectorAll('input[name="roleKeys"]:checked')].map((i) => i.value);
    const env = await window.Ks.post(userBtn.dataset.userApprove, {
      userId: userBtn.dataset.userId,
      expectedVersion: Number(userBtn.dataset.version),
      companyIds,
      roleKeys
    });
    userBtn.disabled = false;
    if (env.success && env.data?.redirectUrl) window.Ks.navigate(env.data.redirectUrl);
  }
});
