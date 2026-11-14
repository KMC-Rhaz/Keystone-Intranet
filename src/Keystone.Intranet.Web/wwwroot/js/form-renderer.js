// renders workflow forms from the server view model; server re-validates every field on submit
(() => {
  const allowedAccept = '.jpg,.jpeg,.png,.webp,.heic,.pdf,.doc,.docx,.xls,.xlsx,.pptx';

  const initForm = () => {
    const root = document.getElementById('ks-form-root');
    if (!root || root.dataset.mounted === '1') return;
    root.dataset.mounted = '1';

    const model = JSON.parse(root.dataset.model);
    const uploadUrl = root.dataset.uploadUrl;
    const values = { ...(model.initialValues || {}) };
    const fileMap = new Map((model.initialFiles || []).map((f) => [f.id, f]));
    const form = document.createElement('form');
    form.noValidate = true;
    form.className = 'ks-stack';

    const clearErrors = () => {
      form.querySelectorAll('.ks-field-error').forEach((el) => { el.textContent = ''; });
      form.querySelectorAll('.is-invalid').forEach((el) => el.classList.remove('is-invalid'));
    };

    const showErrors = (errors) => {
      (errors || []).forEach((err) => {
        const slot = form.querySelector(`[data-error-for="${CSS.escape(err.field)}"]`);
        const input = form.querySelector(`[name="${CSS.escape(err.field)}"]`);
        if (slot) slot.textContent = err.message;
        if (input) input.classList.add('is-invalid');
      });
    };

    const renderFiles = (fieldKey, listEl) => {
      listEl.innerHTML = '';
      const ids = (values[fieldKey] || '').split(',').filter(Boolean);
      ids.forEach((id) => {
        const meta = fileMap.get(id);
        const chip = document.createElement('div');
        chip.className = 'ks-file-chip';

        const name = document.createElement('span');
        name.textContent = meta ? meta.name : id;

        const remove = document.createElement('button');
        remove.type = 'button';
        remove.className = 'ks-btn ks-btn-ghost ks-btn-sm';
        remove.textContent = 'Remove';
        remove.addEventListener('click', () => {
          values[fieldKey] = ids.filter((x) => x !== id).join(',');
          renderFiles(fieldKey, listEl);
        });

        chip.append(name, remove);
        listEl.appendChild(chip);
      });
    };

    const buildInput = (field) => {
      if (field.type === 'textarea') {
        const el = document.createElement('textarea');
        el.className = 'ks-textarea';
        el.name = field.key;
        el.value = values[field.key] || '';
        if (field.maxLength) el.maxLength = field.maxLength;
        el.addEventListener('input', () => { values[field.key] = el.value; });
        return el;
      }

      if (field.type === 'select') {
        const el = document.createElement('select');
        el.className = 'ks-select';
        el.name = field.key;
        const blank = document.createElement('option');
        blank.value = '';
        blank.textContent = 'Choose...';
        el.appendChild(blank);
        (field.options || []).forEach((opt) => {
          const o = document.createElement('option');
          o.value = opt.value;
          o.textContent = opt.label;
          if (values[field.key] === opt.value) o.selected = true;
          el.appendChild(o);
        });
        el.addEventListener('change', () => { values[field.key] = el.value; });
        return el;
      }

      if (field.type === 'checkbox') {
        const wrap = document.createElement('label');
        wrap.className = 'ks-check-row';
        const el = document.createElement('input');
        el.type = 'checkbox';
        el.name = field.key;
        el.checked = values[field.key] === 'true';
        el.addEventListener('change', () => { values[field.key] = el.checked ? 'true' : 'false'; });
        const span = document.createElement('span');
        span.textContent = field.label;
        wrap.append(el, span);
        return wrap;
      }

      if (field.type === 'file') {
        const wrap = document.createElement('div');
        const input = document.createElement('input');
        input.type = 'file';
        input.className = 'ks-input';
        input.name = field.key;
        input.accept = allowedAccept;
        input.multiple = true;
        const listEl = document.createElement('div');
        listEl.className = 'ks-file-list';
        renderFiles(field.key, listEl);

        input.addEventListener('change', async () => {
          for (const file of input.files) {
            const fd = new FormData();
            fd.append('file', file);
            const res = await fetch(uploadUrl, {
              method: 'POST',
              headers: { 'Accept': 'application/json', 'X-Requested-With': 'fetch', 'RequestVerificationToken': window.Ks.csrf() },
              body: fd
            });
            const env = await res.json();
            if (!env.success) {
              window.Toast.show({ type: 'error', message: env.message || 'Upload failed.' });
              continue;
            }
            fileMap.set(env.data.id, env.data);
            const current = (values[field.key] || '').split(',').filter(Boolean);
            current.push(env.data.id);
            values[field.key] = current.join(',');
            renderFiles(field.key, listEl);
          }
          input.value = '';
        });

        wrap.append(input, listEl);
        return wrap;
      }

      const el = document.createElement('input');
      el.className = 'ks-input';
      el.name = field.key;
      el.value = values[field.key] || '';
      if (field.type === 'date') el.type = 'date';
      else if (field.type === 'number' || field.type === 'currency') {
        el.type = 'number';
        el.step = field.type === 'currency' ? '0.01' : 'any';
        el.inputMode = 'decimal';
      } else {
        el.type = 'text';
      }
      el.addEventListener('input', () => { values[field.key] = el.value; });
      return el;
    };

    (model.sections || []).forEach((section) => {
      const card = document.createElement('section');
      card.className = 'ks-card';

      const heading = document.createElement('h2');
      heading.className = 'ks-card-title';
      heading.textContent = section.title;
      card.appendChild(heading);

      if (section.intro) {
        const intro = document.createElement('p');
        intro.className = 'ks-card-desc';
        intro.textContent = section.intro;
        card.appendChild(intro);
      }

      const body = document.createElement('div');
      body.className = 'ks-section-body';
      (section.fields || []).forEach((field) => {
        const wrap = document.createElement('div');
        wrap.className = 'ks-field';

        if (field.type !== 'checkbox') {
          const label = document.createElement('label');
          label.className = 'ks-label';
          label.textContent = field.required ? `${field.label} *` : field.label;
          wrap.appendChild(label);
        }

        wrap.appendChild(buildInput(field));

        if (field.helpText) {
          const help = document.createElement('p');
          help.className = 'ks-help';
          help.textContent = field.helpText;
          wrap.appendChild(help);
        }

        const err = document.createElement('p');
        err.className = 'ks-field-error';
        err.dataset.errorFor = field.key;
        wrap.appendChild(err);
        body.appendChild(wrap);
      });

      card.appendChild(body);
      form.appendChild(card);
    });

    const submit = document.createElement('button');
    submit.type = 'submit';
    submit.className = 'ks-btn ks-btn-gold';
    submit.textContent = model.submitLabel || 'Send request';
    form.appendChild(submit);

    form.addEventListener('submit', async (e) => {
      e.preventDefault();
      clearErrors();
      submit.disabled = true;
      const payload = model.resubmitRequestId
        ? { requestId: model.resubmitRequestId, expectedVersion: model.expectedVersion, values }
        : { definitionKey: model.definitionKey, linkedRequestId: model.linkedRequestId, values };

      const env = await window.Ks.post(model.submitUrl, payload);
      submit.disabled = false;
      if (!env.success) {
        showErrors(env.errors);
        return;
      }
      if (env.data?.redirectUrl) window.Ks.navigate(env.data.redirectUrl);
    });

    root.appendChild(form);
  };

  document.addEventListener('DOMContentLoaded', initForm);
  document.addEventListener('htmx:afterSwap', initForm);
})();
