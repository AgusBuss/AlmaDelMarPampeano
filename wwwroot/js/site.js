// Confirmación de borrado con SweetAlert2.
// Cualquier formulario con la clase "form-eliminar" muestra un modal antes de enviarse.
// El texto del modal se toma del atributo data-mensaje del formulario.
document.addEventListener('submit', function (evento) {
    const formulario = evento.target;

    if (!formulario.classList.contains('form-eliminar')) {
        return;
    }

    // Se frena el envío hasta que la persona confirme
    evento.preventDefault();

    Swal.fire({
        title: '¿Estás seguro?',
        text: formulario.dataset.mensaje || 'Esta acción no se puede deshacer.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then(function (resultado) {
        if (resultado.isConfirmed) {
            // submit() nativo: envía el formulario sin volver a disparar este evento
            HTMLFormElement.prototype.submit.call(formulario);
        }
    });
});

// Validación de números con coma decimal (es-AR) para jQuery Validation.
// Va dentro de $(function) porque la librería de validación se carga después de este archivo.
$(function () {
    if (!$.validator) return;

    $.validator.methods.number = function (value, element) {
        return this.optional(element) ||
            /^-?(?:\d+|\d{1,3}(?:\.\d{3})+)(?:,\d+)?$/.test(value);
    };

    $.validator.methods.range = function (value, element, param) {
        var numero = parseFloat(String(value).replace(/\./g, '').replace(',', '.'));
        return this.optional(element) || (numero >= Number(param[0]) && numero <= Number(param[1]));
    };
});