// wwwroot/js/Usuarios/ComprobarRegistro.js
(function () {
    const form = document.getElementById('registroForm');
    if (!form) return;

    //Inputs
    const nombreInput = form.querySelector('input[name="Nombre"]');
    const apellidoInput = form.querySelector('input[name="Apellido"]');
    const emailInput = form.querySelector('input[name="Email"]');
    const passInput = form.querySelector('input[name="Contraseña"]');

    //msg de errores
    const nombreError = document.getElementById('nombreError');
    const apellidoError = document.getElementById('apellidoError');
    const emailError = document.getElementById('emailError');
    const passError = document.getElementById('passError');
    const parametrosVacios = document.getElementById('parametrosVacios');
    const usuarioExistente = document.getElementById('usuarioExistente');

    function limpiarErrores() {
        [nombreError, apellidoError, emailError, passError, parametrosVacios, usuarioExistente].forEach(el => {
            if (el) el.textContent = '';
        });
        [nombreInput, apellidoInput, emailInput, passInput].forEach(i => {
            if (i) i.classList.remove('is-invalid');
        });
    }

    function validarEmail(value) {
        if (!value)
            return 'El email es obligatorio.';
        if (!value.includes('@') || value.split('@').length !== 2)
            return 'El formato de email no es válido.';
        return '';
    }

    function validarPassword(value) {
        switch (value) {
            case !value: return 'La contraseña es obligatoria.';
            case value.length < 8: return 'La contraseña debe tener al menos 8 caracteres.';
            case !/[A-Z]/.test(value): return 'La contraseña debe contener al menos una letra mayúscula.';
            case !/\d/.test(value): return 'La contraseña debe contener al menos un número.';
            default: return '';
        }
    }

    function validarNombre(value, campoNombre) {
        if (!value)
            return `${campoNombre} es obligatorio.`;
        return '';
    }

    // Validación en tiempo real (añadir listener)
    if (nombreInput) nombreInput.addEventListener('input', () => {
        const msg = validarNombre(nombreInput.value.trim(), 'El nombre');
        nombreError.textContent = msg;
        nombreInput.classList.toggle('is-invalid', !!msg);
    });

    if (apellidoInput) apellidoInput.addEventListener('input', () => {
        const msg = validarNombre(apellidoInput.value.trim(), 'El apellido');
        apellidoError.textContent = msg;
        apellidoInput.classList.toggle('is-invalid', !!msg);
    });

    if (emailInput) emailInput.addEventListener('input', () => {
        const msg = validarEmail(emailInput.value.trim());
        emailError.textContent = msg;
        emailInput.classList.toggle('is-invalid', !!msg);
    });

    if (passInput) passInput.addEventListener('input', () => {
        const msg = validarPassword(passInput.value);
        passError.textContent = msg;
        passInput.classList.toggle('is-invalid', !!msg);
    });

    form.addEventListener('submit', function (e) {
        limpiarErrores();

        const nombreVal = nombreInput ? nombreInput.value.trim() : '';
        const apellidoVal = apellidoInput ? apellidoInput.value.trim() : '';
        const emailVal = emailInput ? emailInput.value.trim() : '';
        const passVal = passInput ? passInput.value : '';

        // Comprobar campos vacíos
        if (!nombreVal || !apellidoVal || !emailVal || !passVal) {
            parametrosVacios.textContent = 'Uno de los campos obligatorios está vacío';
            if (!nombreVal && nombreInput) nombreInput.classList.add('is-invalid');
            if (!apellidoVal && apellidoInput) apellidoInput.classList.add('is-invalid');
            if (!emailVal && emailInput) emailInput.classList.add('is-invalid');
            if (!passVal && passInput) passInput.classList.add('is-invalid');
            e.preventDefault();
            return;
        }

        // Validaciones detalladas
        let blocked = false;

        const nombreMsg = validarNombre(nombreVal, 'El nombre');
        if (nombreMsg) {
            nombreError.textContent = nombreMsg; nombreInput.classList.add('is-invalid'); blocked = true;
        }

        const apellidoMsg = validarNombre(apellidoVal, 'El apellido');
        if (apellidoMsg) {
            apellidoError.textContent = apellidoMsg; apellidoInput.classList.add('is-invalid'); blocked = true;
        }

        const emailMsg = validarEmail(emailVal);
        if (emailMsg) {
            emailError.textContent = emailMsg; emailInput.classList.add('is-invalid'); blocked = true;
        }

        const passMsg = validarPassword(passVal);
        if (passMsg) {
            passError.textContent = passMsg; passInput.classList.add('is-invalid'); blocked = true;
        }

        if (blocked) {
            e.preventDefault();
            return;
        }

        // Si se desea, aquí se podría hacer una comprobación AJAX para "usuario existente"
        // pero mantener validación servidor como fallback por seguridad.
    });
})();
