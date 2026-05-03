// ShareCar web client scripts

function showNotifyDialog(title, message, isError) {
  const dialog = document.getElementById('notifyDialog');
  if (!dialog) return;
  document.getElementById('notifyDialogTitle').textContent = title;
  document.getElementById('notifyDialogMessage').textContent = message;
  const icon = document.getElementById('notifyDialogIcon');
  if (isError) {
    icon.textContent = 'error';
    icon.className = 'material-symbols-outlined text-2xl mt-0.5 text-red-500';
  } else {
    icon.textContent = 'check_circle';
    icon.className = 'material-symbols-outlined text-2xl mt-0.5 text-green-500';
  }
  dialog.showModal();
}

document.addEventListener('submit', async function (e) {
  const form = e.target.closest('form[data-ajax]');
  if (!form) return;

  e.preventDefault();

  const submitBtn = form.querySelector('[type="submit"]');
  if (submitBtn) submitBtn.disabled = true;

  try {
    const response = await fetch(form.action, {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body: new URLSearchParams(new FormData(form))
    });

    const result = await response.json();

    if (result.success) {
      showNotifyDialog('Success', result.message, false);
      document.getElementById('notifyDialog').addEventListener('close', () => window.location.reload(), { once: true });
    } else {
      showNotifyDialog('Error', result.message, true);
    }
  } catch {
    showNotifyDialog('Error', 'An unexpected error occurred. Please try again.', true);
  } finally {
    if (submitBtn) submitBtn.disabled = false;
  }
});
