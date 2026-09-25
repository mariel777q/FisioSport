(function () {

    console.log("FISIOSPORT SPEECH JS CARGADO");

    const SpeechRecognition =
        window.SpeechRecognition ||
        window.webkitSpeechRecognition;

    if (!SpeechRecognition) {
        console.warn("FISIOSPORT: este navegador no soporta Web Speech API.");
        return;
    }

    const campos = [
        "MotivoConsulta",
        "TratamientosPrevios",
        "DiagnosticoMedicoRehabilitacion",
        "Reflejos",
        "Sensibilidad",
        "LenguajeOrientacion",
        "OtrosDiagnostico",
        "TrasladosValorInicial",
        "TrasladosValorFinal",
        "ObservacionesMarcha",
        "Antecedentes",
        "Alergias",
        "Observaciones",

        "observaciones"
    ];

    let reconocimientoActual = null;
    let botonActual = null;
    let campoActual = null;

    function crearBoton(campo) {

        if (!campo || campo.dataset.speechInicializado === "true") {
            return;
        }

        campo.dataset.speechInicializado = "true";

        const contenedor = document.createElement("div");

        contenedor.style.display = "flex";
        contenedor.style.alignItems = "center";
        contenedor.style.gap = "8px";
        contenedor.style.marginTop = "8px";

        const boton = document.createElement("button");

        boton.type = "button";
        boton.textContent = "🎙️ Dictar";

        boton.style.padding = "7px 12px";
        boton.style.border = "1px solid rgba(6, 182, 212, 0.35)";
        boton.style.borderRadius = "8px";
        boton.style.background = "rgba(6, 182, 212, 0.10)";
        boton.style.color = "#22d3ee";
        boton.style.fontSize = "13px";
        boton.style.fontWeight = "600";
        boton.style.cursor = "pointer";

        boton.addEventListener("mouseenter", function () {
            boton.style.background = "rgba(6, 182, 212, 0.20)";
        });

        boton.addEventListener("mouseleave", function () {
            if (botonActual !== boton) {
                boton.style.background = "rgba(6, 182, 212, 0.10)";
            }
        });

        boton.addEventListener("click", function () {
            iniciarDictado(campo, boton);
        });

        const estado = document.createElement("span");

        estado.textContent = "";

        estado.style.fontSize = "12px";
        estado.style.color = "#94a3b8";

        contenedor.appendChild(boton);
        contenedor.appendChild(estado);

        campo.parentNode.insertBefore(
            contenedor,
            campo.nextSibling
        );

        campo._speechBoton = boton;
        campo._speechEstado = estado;
    }

    function iniciarDictado(campo, boton) {

        if (reconocimientoActual) {

            if (campoActual === campo) {
                reconocimientoActual.stop();
                return;
            }

            reconocimientoActual.stop();
        }

        const reconocimiento = new SpeechRecognition();

        reconocimiento.lang = "es-ES";
        reconocimiento.continuous = true;
        reconocimiento.interimResults = true;
        reconocimiento.maxAlternatives = 1;

        reconocimientoActual = reconocimiento;
        botonActual = boton;
        campoActual = campo;

        boton.textContent = "⏹️ Detener";
        boton.style.background = "rgba(239, 68, 68, 0.15)";
        boton.style.borderColor = "rgba(239, 68, 68, 0.4)";
        boton.style.color = "#f87171";

        if (campo._speechEstado) {
            campo._speechEstado.textContent = "Escuchando...";
            campo._speechEstado.style.color = "#22d3ee";
        }

        let textoFinal = "";

        reconocimiento.onstart = function () {

            console.log(
                "FISIOSPORT: dictado iniciado en",
                campo.name || campo.id
            );
        };

        reconocimiento.onresult = function (event) {

            let textoInterino = "";

            for (
                let i = event.resultIndex;
                i < event.results.length;
                i++
            ) {

                const resultado = event.results[i];

                const texto = resultado[0].transcript;

                if (resultado.isFinal) {
                    textoFinal += texto + " ";
                } else {
                    textoInterino += texto;
                }
            }

            if (textoFinal) {

                const valorActual =
                    campo.value.trim();

                if (valorActual) {

                    campo.value =
                        valorActual +
                        " " +
                        textoFinal.trim();

                } else {

                    campo.value =
                        textoFinal.trim();
                }

                textoFinal = "";

                campo.dispatchEvent(
                    new Event("input", {
                        bubbles: true
                    })
                );

                campo.dispatchEvent(
                    new Event("change", {
                        bubbles: true
                    })
                );
            }

            if (campo._speechEstado) {

                if (textoInterino) {

                    campo._speechEstado.textContent =
                        "Escuchando: " +
                        textoInterino;

                } else {

                    campo._speechEstado.textContent =
                        "Escuchando...";
                }
            }
        };

        reconocimiento.onerror = function (event) {

            console.error(
                "FISIOSPORT SpeechRecognition error:",
                event.error
            );

            if (campo._speechEstado) {

                switch (event.error) {

                    case "not-allowed":
                        campo._speechEstado.textContent =
                            "Permiso de micrófono denegado.";
                        break;

                    case "no-speech":
                        campo._speechEstado.textContent =
                            "No se detectó voz.";
                        break;

                    case "audio-capture":
                        campo._speechEstado.textContent =
                            "No se encontró micrófono.";
                        break;

                    case "network":
                        campo._speechEstado.textContent =
                            "Error de conexión.";
                        break;

                    default:
                        campo._speechEstado.textContent =
                            "Error: " + event.error;
                        break;
                }

                campo._speechEstado.style.color = "#f87171";
            }
        };

        reconocimiento.onend = function () {

            console.log("FISIOSPORT: dictado finalizado");

            if (botonActual === boton) {

                boton.textContent = "🎙️ Dictar";

                boton.style.background =
                    "rgba(6, 182, 212, 0.10)";

                boton.style.borderColor =
                    "rgba(6, 182, 212, 0.35)";

                boton.style.color = "#22d3ee";
            }

            if (campo._speechEstado) {

                campo._speechEstado.textContent = "";

                campo._speechEstado.style.color =
                    "#94a3b8";
            }

            if (reconocimientoActual === reconocimiento) {

                reconocimientoActual = null;
                botonActual = null;
                campoActual = null;
            }
        };

        try {

            reconocimiento.start();

        } catch (error) {

            console.error(
                "FISIOSPORT: no se pudo iniciar el reconocimiento.",
                error
            );

            reconocimientoActual = null;
            botonActual = null;
            campoActual = null;
        }
    }

    function inicializar() {

        console.log("FISIOSPORT SPEECH DOM CARGADO");

        campos.forEach(function (id) {

            const campo =
                document.getElementById(id);

            if (campo) {
                crearBoton(campo);
            }
        });

        /*
         * También buscamos el textarea de observaciones
         * de la sesión aunque no tenga id.
         */

        const textareas =
            document.querySelectorAll("textarea");

        textareas.forEach(function (textarea) {

            const nombre =
                textarea.getAttribute("name");

            if (nombre === "observaciones") {
                crearBoton(textarea);
            }
        });

        console.log(
            "FISIOSPORT: inicialización completada."
        );
    }

    if (
        document.readyState === "loading"
    ) {

        document.addEventListener(
            "DOMContentLoaded",
            inicializar
        );

    } else {

        inicializar();
    }

})();