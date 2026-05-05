let fullscreenElement = null;
let dotNetObjectReference = null;

export function init(reference) {
    dotNetObjectReference = reference;
    
    if (!document.fullscreenEnabled && 
        !document.webkitFullscreenEnabled && 
        !document.mozFullScreenEnabled && 
        !document.msFullscreenEnabled) {
        console.warn('Fullscreen API is not supported in this browser');
        return false;
    }
    
    return true;
}

export function subscribeToEvents(reference) {
    document.addEventListener('fullscreenchange', handleFullscreenChange);
    document.addEventListener('webkitfullscreenchange', handleFullscreenChange);
    document.addEventListener('mozfullscreenchange', handleFullscreenChange);
    document.addEventListener('MSFullscreenChange', handleFullscreenChange);
}

export function unsubscribeFromEvents() {
    document.removeEventListener('fullscreenchange', handleFullscreenChange);
    document.removeEventListener('webkitfullscreenchange', handleFullscreenChange);
    document.removeEventListener('mozfullscreenchange', handleFullscreenChange);
    document.removeEventListener('MSFullscreenChange', handleFullscreenChange);
}

export function enterFullscreen() {
    const element = document.documentElement;
    
    if (element.requestFullscreen) {
        return element.requestFullscreen().catch(handleError);
    } else if (element.webkitRequestFullscreen) {
        return element.webkitRequestFullscreen().catch(handleError);
    } else if (element.mozRequestFullScreen) {
        return element.mozRequestFullScreen().catch(handleError);
    } else if (element.msRequestFullscreen) {
        return element.msRequestFullscreen().catch(handleError);
    } else {
        throw new Error('Fullscreen API not supported');
    }
}

export function exitFullscreen() {
    if (document.exitFullscreen) {
        return document.exitFullscreen().catch(handleError);
    } else if (document.webkitExitFullscreen) {
        return document.webkitExitFullscreen().catch(handleError);
    } else if (document.mozCancelFullScreen) {
        return document.mozCancelFullScreen().catch(handleError);
    } else if (document.msExitFullscreen) {
        return document.msExitFullscreen().catch(handleError);
    } else {
        throw new Error('Fullscreen API not supported');
    }
}

function handleFullscreenChange() {
    const isFullscreen = Boolean(
        document.fullscreenElement ||
        document.webkitFullscreenElement ||
        document.mozFullScreenElement ||
        document.msFullscreenElement
    );
    
    if (dotNetObjectReference) {
        dotNetObjectReference.invokeMethodAsync('OnFullscreenChanged', isFullscreen);
    }
}

function handleError(error) {
    console.error('Fullscreen error:', error);
    throw error;
}