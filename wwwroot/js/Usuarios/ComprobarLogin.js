
//Comprobar que el usuario ha introducido un email y una contraseña
(function () {
    const form = document.getElementById('loginForm');
    if (!form) return;

    const emailInput = form.querySelector('input[name="email"]');
    const passInput = form.querySelector('input[name="contraseña"]');

    const emailError = document.getElementById('emailError');
    const passError = document.getElementById('passError');
    const parametrosVacios = document.getElementById('parametrosVacios');

    function limpiarErrores() {
        [emailError, passError, parametrosVacios].forEach(el => {
            if (el) el.textContent = '';
        });
        [emailInput, passInput].forEach(i => {
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

    // Validación en tiempo real
    if (emailInput)
        //Añadir listener para validar el email en tiempo real, mostrando el mensaje de error debajo del campo y añadiendo la clase 'is-invalid' si hay un error
        emailInput.addEventListener('input', () => {
        let msg = validarEmail(emailInput.value.trim());
        emailError.textContent = msg;
        emailInput.classList.toggle('is-invalid', msg);
    });

    if (passInput)
        //Añadir listener para validar el email en tiempo real, mostrando el mensaje de error debajo del campo y añadiendo la clase 'is-invalid' si hay un error
        passInput.addEventListener('input', () => {
        let msg = validarPassword(passInput.value);
        passError.textContent = msg;
        passInput.classList.toggle('is-invalid', msg);
    });

    form.addEventListener('submit', function (e) {
        limpiarErrores();

        const emailVal = emailInput ? emailInput.value.trim() : '';
        const passVal = passInput ? passInput.value : '';

        // Comprobar campos vacíos (coincide con la lógica del LoginViewModel)
        if (!emailVal || !passVal) {
            parametrosVacios.textContent = 'Uno de los campos está vacío';
            if (!emailVal && emailInput) emailInput.classList.add('is-invalid');
            if (!passVal && passInput) passInput.classList.add('is-invalid');
            e.preventDefault();
            return;
        }

        // Validaciones detalladas
        const emailMsg = validarEmail(emailVal);
        const passMsg = validarPassword(passVal);

        let blocked = false;
        if (emailMsg) {
            emailError.textContent = emailMsg;
            emailInput.classList.add('is-invalid');
            blocked = true;
        }
        if (passMsg) {
            passError.textContent = passMsg;
            passInput.classList.add('is-invalid');
            blocked = true;
        }

        if (blocked) {
            e.preventDefault();
            return;
        }

        // Si todo OK, se envía el formulario al servidor (se mantiene validación servidor como fallback)
    });
})();