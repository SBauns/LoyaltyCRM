let updatePendingReload = false;

async function registerServiceWorker() {
    const registration = await navigator.serviceWorker.register('service-worker.js');

    registration.addEventListener('updatefound', () => {
        const installingWorker = registration.installing;
        if (installingWorker === null) {
            return;
        }

        installingWorker.addEventListener('statechange', () => {
            if (installingWorker.state === 'installed'
                && navigator.serviceWorker.controller !== null) {
                // A new version is installed and waiting. Let the user decide.
                showUpdateAvailable(() => {
                    updatePendingReload = true;
                    installingWorker.postMessage({ type: 'SKIP_WAITING' });
                });
            }
        });
    });
}

navigator.serviceWorker.addEventListener('controllerchange', () => {
    // Guard against a reload loop during activation.
    if (updatePendingReload) {
        location.reload();
    }
});

function showUpdateAvailable(onAccept) {
    const banner = document.createElement('div');
    banner.textContent = 'A new version is available.';
    const button = document.createElement('button');
    button.textContent = 'Reload';
    button.addEventListener('click', onAccept);
    banner.appendChild(button);
    document.body.appendChild(banner);
}

registerServiceWorker();