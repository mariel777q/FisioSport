(function () {

    "use strict";

    console.log("FISIOSPORT PWA JS CARGADO");

    let deferredInstallPrompt = null;
    let installButton = null;

    function isStandalone() {
        return (
            window.matchMedia("(display-mode: standalone)").matches ||
            window.navigator.standalone === true
        );
    }

    function crearBotonInstalar() {

        if (installButton) {
            return;
        }

        installButton = document.createElement("button");

        installButton.id = "fisiosport-install-button";
        installButton.type = "button";
        installButton.setAttribute(
            "aria-label",
            "Instalar FisioSport"
        );

        installButton.innerHTML = `
            <span style="
                font-size: 20px;
                line-height: 1;
            ">📱</span>

            <span style="
                display: flex;
                flex-direction: column;
                align-items: flex-start;
                line-height: 1.15;
            ">
                <strong style="
                    font-size: 14px;
                    font-weight: 700;
                ">
                    Instalar
                </strong>

                <span style="
                    font-size: 11px;
                    opacity: 0.8;
                ">
                    FisioSport
                </span>
            </span>
        `;

        Object.assign(installButton.style, {
            position: "fixed",
            right: "20px",
            bottom: "20px",
            zIndex: "99999",
            display: "none",
            alignItems: "center",
            gap: "10px",
            padding: "12px 16px",
            border: "1px solid rgba(255,255,255,0.15)",
            borderRadius: "14px",
            background: "#111827",
            color: "#ffffff",
            boxShadow: "0 10px 30px rgba(0,0,0,0.35)",
            cursor: "pointer",
            fontFamily: "inherit",
            transition: "transform 0.2s ease, opacity 0.2s ease"
        });

        installButton.addEventListener("mouseenter", function () {
            installButton.style.transform = "translateY(-2px)";
        });

        installButton.addEventListener("mouseleave", function () {
            installButton.style.transform = "translateY(0)";
        });

        installButton.addEventListener("click", instalarPWA);

        document.body.appendChild(installButton);
    }

    function mostrarBotonInstalar() {

        if (!installButton) {
            return;
        }

        if (isStandalone()) {
            installButton.style.display = "none";
            return;
        }

        installButton.style.display = "flex";
    }

    function ocultarBotonInstalar() {

        if (!installButton) {
            return;
        }

        installButton.style.display = "none";
    }

    async function instalarPWA() {

        if (!deferredInstallPrompt) {
            return;
        }

        deferredInstallPrompt.prompt();

        const resultado = await deferredInstallPrompt.userChoice;

        console.log(
            "FISIOSPORT PWA: resultado instalación:",
            resultado.outcome
        );

        deferredInstallPrompt = null;

        ocultarBotonInstalar();
    }

    window.addEventListener(
        "beforeinstallprompt",
        function (event) {

            console.log(
                "FISIOSPORT PWA: aplicación instalable"
            );

            event.preventDefault();

            deferredInstallPrompt = event;

            mostrarBotonInstalar();
        }
    );

    window.addEventListener(
        "appinstalled",
        function () {

            console.log(
                "FISIOSPORT PWA: aplicación instalada"
            );

            deferredInstallPrompt = null;

            ocultarBotonInstalar();
        }
    );

    async function registrarServiceWorker() {

        if (!("serviceWorker" in navigator)) {

            console.log(
                "FISIOSPORT PWA: Service Worker no soportado"
            );

            return;
        }

        try {

            const registration =
                await navigator.serviceWorker.register(
                    "/service-worker.js",
                    {
                        scope: "/"
                    }
                );

            console.log(
                "FISIOSPORT PWA: Service Worker registrado",
                registration.scope
            );

        } catch (error) {

            console.error(
                "FISIOSPORT PWA: error registrando Service Worker",
                error
            );
        }
    }

    function iniciarPWA() {

        if (!document.body) {
            return;
        }

        crearBotonInstalar();

        if (isStandalone()) {
            ocultarBotonInstalar();
        }

        registrarServiceWorker();
    }

    if (document.readyState === "loading") {

        document.addEventListener(
            "DOMContentLoaded",
            iniciarPWA
        );

    } else {

        iniciarPWA();
    }

})();