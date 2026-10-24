// Tastatur-Kuerzel der Personenseite: "/" springt in die Suche (wie bei vielen Webseiten).
(function () {
    document.addEventListener("keydown", function (e) {
        if (e.key !== "/" || e.ctrlKey || e.metaKey || e.altKey) {
            return;
        }
        var t = e.target;
        var tag = t && t.tagName ? t.tagName.toLowerCase() : "";
        if (tag === "input" || tag === "textarea" || tag === "select" || (t && t.isContentEditable)) {
            return;
        }
        var search = document.getElementById("ek-person-search");
        if (search) {
            e.preventDefault();
            search.focus();
            search.select();
        }
    });
})();
