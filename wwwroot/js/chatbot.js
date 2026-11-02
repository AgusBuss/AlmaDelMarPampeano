// Chat flotante del asistente Lumi.
// Manda la pregunta a /Chat/Preguntar y muestra la respuesta de la IA.
(function () {
    const boton = document.getElementById('lumi-boton');
    const panel = document.getElementById('lumi-panel');
    if (!boton || !panel) return;

    const cerrar = document.getElementById('lumi-cerrar');
    const zonaMensajes = document.getElementById('lumi-mensajes');
    const formulario = document.getElementById('lumi-formulario');
    const entrada = document.getElementById('lumi-entrada');
    const botonEnviar = document.getElementById('lumi-enviar');
    const url = panel.dataset.url;

    // Historial de la conversación (queda solo en la página; se pierde al recargar)
    const historial = [];
    let enviando = false;

    function abrirCerrar(abrir) {
        panel.classList.toggle('abierto', abrir);
        boton.setAttribute('aria-expanded', abrir ? 'true' : 'false');
        if (abrir) {
            if (zonaMensajes.children.length === 0) {
                agregarBurbuja('asistente', '¡Hola! Soy Lumi, el asistente de Alma del Mar Pampeano. Puedo contarte sobre nuestras colecciones, las lámparas del catálogo, los precios y cómo hacer un pedido. ¿En qué te ayudo?');
            }
            entrada.focus();
        }
    }

    boton.addEventListener('click', function () {
        abrirCerrar(!panel.classList.contains('abierto'));
    });
    cerrar.addEventListener('click', function () { abrirCerrar(false); });

    // Se escribe con textContent (nunca con innerHTML) para que ningún texto pueda inyectar HTML.
    // Solo se interpreta **negrita**, creando elementos <strong> a mano.
    function escribirTexto(elemento, texto) {
        texto.split(/(\*\*[^*]+\*\*)/g).forEach(function (trozo) {
            if (/^\*\*[^*]+\*\*$/.test(trozo)) {
                const fuerte = document.createElement('strong');
                fuerte.textContent = trozo.slice(2, -2);
                elemento.appendChild(fuerte);
            } else {
                elemento.appendChild(document.createTextNode(trozo.replace(/\*/g, '')));
            }
        });
    }

    function agregarBurbuja(tipo, texto) {
        const burbuja = document.createElement('div');
        burbuja.className = 'lumi-burbuja ' + tipo;
        escribirTexto(burbuja, texto);
        zonaMensajes.appendChild(burbuja);
        zonaMensajes.scrollTop = zonaMensajes.scrollHeight;
        return burbuja;
    }

    formulario.addEventListener('submit', async function (evento) {
        evento.preventDefault();
        const pregunta = entrada.value.trim();
        if (!pregunta || enviando) return;

        enviando = true;
        botonEnviar.disabled = true;
        entrada.value = '';
        agregarBurbuja('usuario', pregunta);
        const escribiendo = agregarBurbuja('asistente escribiendo', 'Lumi está escribiendo...');

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';

        try {
            const respuesta = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ mensaje: pregunta, historial: historial })
            });

            let texto;
            if (respuesta.status === 429) {
                texto = 'Hiciste muchas preguntas seguidas. Esperá un minuto y volvé a intentar.';
            } else if (!respuesta.ok) {
                texto = 'No pude responder en este momento. Probá de nuevo en unos minutos.';
            } else {
                const datos = await respuesta.json();
                texto = datos.respuesta;
                historial.push({ rol: 'user', texto: pregunta });
                historial.push({ rol: 'model', texto: texto });
            }

            escribiendo.remove();
            agregarBurbuja('asistente', texto);
        } catch (error) {
            escribiendo.remove();
            agregarBurbuja('asistente', 'No pude conectarme. Revisá tu conexión e intentá de nuevo.');
        } finally {
            enviando = false;
            botonEnviar.disabled = false;
            entrada.focus();
        }
    });
})();