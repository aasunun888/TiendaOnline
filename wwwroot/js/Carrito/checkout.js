document.addEventListener('DOMContentLoaded', function () {
    // Elementos
    const tarjetaCampos = document.getElementById('tarjetaCampos');
    const radios = document.querySelectorAll('input[name="MetodoPagoSeleccionado"]');
    const numeroEl = document.getElementById('numeroTarjeta');
    const nombreEl = document.getElementById('nombre-input');
    const mesEl = document.getElementById('mes');
    const anyoEl = document.getElementById('anyo');
    const cvcEl = document.getElementById('CVC');

    const resultadoCampos = id => document.getElementById(id);

    // Toggle tarjeta UI según método
    function actualizarVisibilidad() {
        const seleccionado = document.querySelector('input[name="MetodoPagoSeleccionado"]:checked');
        if (!tarjetaCampos) return;
        tarjetaCampos.style.display = seleccionado && seleccionado.value === 'Tarjeta' ? 'block' : 'none';
    }
    radios.forEach(r => r.addEventListener('change', actualizarVisibilidad));
    actualizarVisibilidad();

    // Helpers seguros
    function setText(id, text, color) {
        const el = document.getElementById(id);
        if (!el) return;
        el.textContent = text;
        if (color) el.style.color = color; else el.style.color = '';
    }

    // Validaciones
    function validarNumeroTarjeta(numero) {
        // formato correcto: 4 grupos de 4 dígitos separados por guiones o espacios
        const clean = (numero || '').trim();
        const regex = /^(\d{4}[- ]?){3}\d{4}$/;
        if (!regex.test(clean)) {
            setText('resultado-validacion-numero', 'El número introducido no es válido', 'red');
            return false;
        }
        setText('resultado-validacion-numero', '');
        // actualizar HUD
        setText('numero-tarjeta', clean);
        return true;
    }

    function validarFecha(fechaMes, fechaAnyo) {
        const m = parseInt((fechaMes || '').trim(), 10);
        const y = parseInt((fechaAnyo || '').trim(), 10);
        if (isNaN(m) || isNaN(y) || m < 1 || m > 12) {
            setText('validacion-fecha', 'Fecha no válida', 'red');
            return false;
        }
        // convertir YY a año completo si es necesario
        const now = new Date();
        const fullYear = y < 100 ? (2000 + y) : y;
        const comp = new Date(fullYear, m - 1, 1);
        if (comp < new Date(now.getFullYear(), now.getMonth(), 1)) {
            setText('validacion-fecha', 'La tarjeta está caducada', 'red');
            return false;
        }
        setText('validacion-fecha', '');
        setText('fecha-caducidad', (m.toString().padStart(2, '0')) + '/' + (y.toString().slice(-2)));
        return true;
    }

    function validarCVC(cvc) {
        const t = (cvc || '').trim();
        if (!/^\d{3,4}$/.test(t)) {
            setText('resultado-validacion-cvc', 'CVC no válido', 'red');
            return false;
        }
        setText('resultado-validacion-cvc', '');
        setText('cvc-hud', t);
        return true;
    }

    function validarNombre(nombre) {
        const n = (nombre || '').trim();
        if (n.length < 2) {
            setText('resultado-validacion-nombre', 'Nombre demasiado corto', 'red');
            return false;
        }
        setText('resultado-validacion-nombre', '');
        setText('nombre-hud', n);
        return true;
    }

    // Exponer función para el onsubmit si existe en markup
    window.validarCampoVacio = function () {
        const nombre = nombreEl ? nombreEl.value : '';
        const numero = numeroEl ? numeroEl.value : '';
        const mes = mesEl ? mesEl.value : '';
        const anyo = anyoEl ? anyoEl.value : '';
        const cvc = cvcEl ? cvcEl.value : '';

        if (!nombre || !numero || !mes || !anyo || !cvc) {
            setText('resultado-campos', 'Uno de los campos se encuentra vacío', 'red');
            return false;
        }

        const okNum = validarNumeroTarjeta(numero);
        const okFecha = validarFecha(mes, anyo);
        const okCvc = validarCVC(cvc);
        const okNombre = validarNombre(nombre);

        const allOk = okNum && okFecha && okCvc && okNombre;
        if (!allOk) {
            setText('resultado-campos', 'Corrige los errores antes de continuar', 'red');
        } else {
            setText('resultado-campos', '', '');
        }
        return allOk;
    };

    // Actualizar HUD en tiempo real
    if (numeroEl) {
        numeroEl.addEventListener('input', () => {
            validarNumeroTarjeta(numeroEl.value);
        });
    }
    if (mesEl || anyoEl) {
        [mesEl, anyoEl].forEach(el => {
            if (!el) return;
            el.addEventListener('input', () => {
                validarFecha(mesEl ? mesEl.value : '', anyoEl ? anyoEl.value : '');
            });
        });
    }
    if (cvcEl) {
        cvcEl.addEventListener('input', () => {
            validarCVC(cvcEl.value);
        });
    }
    if (nombreEl) {
        nombreEl.addEventListener('input', () => {
            validarNombre(nombreEl.value);
        });
    }

    // También exponer cambiarHUDNumeroTarjeta si algún onChange la llama
    window.cambiarHUDNumeroTarjeta = function () {
        if (!numeroEl) return;
        setText('numero-tarjeta', numeroEl.value || '•••• •••• •••• ••••');
    };
});