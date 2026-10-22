// Kleine Helfer fuer Blazor-Komponenten.
window.ekFocus = function (id) {
    var el = document.getElementById(id);
    if (el) {
        el.focus();
        if (el.select) {
            el.select();
        }
    }
};
