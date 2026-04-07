// wwwroot/authHelpers.js
function parseJwt(token) {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(
            atob(base64)
                .split('')
                .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                .join('')
        );
        return JSON.parse(jsonPayload);
    } catch (e) {
        console.error("Invalid token");
        return null;
    }
}

// Simple editor helpers used by Admin page
// Expose helpers under window.authHelpers to ensure Blazor can find them
window.authHelpers = (function () {
    function editorCommand(command) {
        try {
            document.execCommand(command, false, null);
        } catch (e) {
            console.error('editorCommand error', e);
        }
    }

    function queryCommandState(command, editorId) {
        try {
            const el = document.getElementById(editorId);
            if (el) el.focus();
            // document.queryCommandState returns boolean for many commands
            return !!document.queryCommandState(command);
        } catch (e) {
            // fallback for lists: inspect selection
            try {
                const el = document.getElementById(editorId);
                const sel = window.getSelection();
                if (sel && sel.anchorNode) {
                    let node = sel.anchorNode;
                    while (node && node !== el) {
                        if (node.nodeName === 'UL') return true;
                        node = node.parentNode;
                    }
                }
            } catch (ex) { }
            return false;
        }
    }

    function execAndGetState(command, editorId) {
        try {
            const el = document.getElementById(editorId);
            if (el) el.focus();
            document.execCommand(command, false, null);
            try { return !!document.queryCommandState(command); } catch { return queryCommandState(command, editorId); }
        } catch (e) {
            console.error('execAndGetState error', e);
            return false;
        }
    }

    function getEditorContent(id) {
        const el = document.getElementById(id);
        return el ? el.innerHTML : null;
    }

    function setEditorContent(id, html) {
        const el = document.getElementById(id);
        if (el) el.innerHTML = html;
    }
    return {
        editorCommand: editorCommand,
        getEditorContent: getEditorContent,
        setEditorContent: setEditorContent,
        queryCommandState: queryCommandState,
        execAndGetState: execAndGetState
    };
})();

