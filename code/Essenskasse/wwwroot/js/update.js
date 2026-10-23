// Registriert den Service Worker und zeigt den Hinweis "Neue Version verfuegbar",
// sobald eine neue Version heruntergeladen ist und auf die Aktivierung wartet.
(function () {
    if (!("serviceWorker" in navigator)) {
        return;
    }

    var texts = {
        de: ["Neue Version verfügbar.", "Jetzt aktualisieren", "Später"],
        en: ["A new version is available.", "Update now", "Later"],
        es: ["Hay una nueva versión disponible.", "Actualizar ahora", "Más tarde"],
        ru: ["Доступна новая версия.", "Обновить сейчас", "Позже"]
    };
    // gewaehlte Sprache der App (Blazored.LocalStorage speichert sie als JSON-Text), sonst die des Browsers
    var stored = "";
    try {
        stored = JSON.parse(window.localStorage.getItem("culture") || '""') || "";
    } catch (e) {
        stored = "";
    }
    var lang = (stored || navigator.language || "de").slice(0, 2).toLowerCase();
    var t = texts[lang] || texts.de;
    var reloading = false;

    function showBanner(worker) {
        if (document.getElementById("ek-update-banner")) {
            return;
        }
        var style = document.createElement("style");
        style.textContent =
            "#ek-update-banner{position:fixed;left:50%;bottom:1rem;transform:translateX(-50%);z-index:2000;" +
            "display:flex;flex-wrap:wrap;align-items:center;gap:.6rem 1rem;max-width:calc(100% - 2rem);" +
            "padding:.7rem 1rem;border-radius:12px;background:#0e3a47;color:#f2f8f7;" +
            "box-shadow:0 4px 18px rgba(0,0,0,.35);font:1rem system-ui,'Segoe UI',Roboto,sans-serif}" +
            "#ek-update-banner button{font:inherit;cursor:pointer;min-height:2.4rem;padding:.35rem 1rem;" +
            "border-radius:8px;border:1px solid #a9c4ca;background:transparent;color:inherit}" +
            "#ek-update-banner button.ek-update-go{background:#62d4a6;border-color:#62d4a6;color:#0e3a47;font-weight:650}" +
            "#ek-update-banner button:focus-visible{outline:2px solid #7db8ff;outline-offset:2px}";
        document.head.appendChild(style);

        var bar = document.createElement("div");
        bar.id = "ek-update-banner";
        bar.setAttribute("role", "status");
        var text = document.createElement("span");
        text.textContent = t[0];
        var go = document.createElement("button");
        go.type = "button";
        go.className = "ek-update-go";
        go.textContent = t[1];
        go.addEventListener("click", function () {
            go.disabled = true;
            worker.postMessage({ type: "SKIP_WAITING" });
        });
        var later = document.createElement("button");
        later.type = "button";
        later.textContent = t[2];
        later.addEventListener("click", function () { bar.remove(); });
        bar.append(text, go, later);
        document.body.appendChild(bar);
    }

    // Nach der Aktivierung der neuen Version einmal neu laden, damit alles zusammenpasst.
    navigator.serviceWorker.addEventListener("controllerchange", function () {
        if (!reloading && window.__ekHadController) {
            reloading = true;
            window.location.reload();
        }
    });
    window.__ekHadController = !!navigator.serviceWorker.controller;

    navigator.serviceWorker.register("service-worker.js").then(function (reg) {
        if (reg.waiting && navigator.serviceWorker.controller) {
            showBanner(reg.waiting);
        }
        reg.addEventListener("updatefound", function () {
            var nw = reg.installing;
            if (!nw) {
                return;
            }
            nw.addEventListener("statechange", function () {
                if (nw.state === "installed" && navigator.serviceWorker.controller) {
                    showBanner(nw);
                }
            });
        });
        // Wer die App lange offen laesst, bekommt Updates trotzdem mit (stuendlich pruefen).
        setInterval(function () { reg.update().catch(function () { }); }, 60 * 60 * 1000);
    });
})();
