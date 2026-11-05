// Confirmación con SweetAlert2.
// Cualquier formulario con la clase "form-eliminar" muestra un modal antes de enviarse.
// Atributos opcionales del formulario (si faltan, se usan los de "eliminar"):
//   data-mensaje -> texto del cartel
//   data-titulo  -> título del cartel
//   data-boton   -> texto del botón de confirmar
//   data-volver  -> texto del botón de cancelar
document.addEventListener('submit', function (evento) {
    const formulario = evento.target;

    if (!formulario.classList.contains('form-eliminar')) {
        return;
    }

    // Se frena el envío hasta que la persona confirme
    evento.preventDefault();

    Swal.fire({
        title: formulario.dataset.titulo || '¿Estás seguro?',
        text: formulario.dataset.mensaje || 'Esta acción no se puede deshacer.',
        icon: 'warning',
        iconColor: '#B67C2E',
        showCancelButton: true,
        reverseButtons: true,
        focusCancel: true,
        confirmButtonText: formulario.dataset.boton || 'Sí, eliminar',
        cancelButtonText: formulario.dataset.volver || 'Cancelar',
        // Se desactivan los estilos propios de SweetAlert y se usan los botones de Bootstrap con la paleta de la marca
        buttonsStyling: false,
        customClass: {
            popup: 'swal-marca',
            title: 'swal-marca-titulo',
            confirmButton: 'btn ' + (formulario.dataset.botonClase || 'btn-danger') + ' mx-1',
            cancelButton: 'btn btn-outline-secondary mx-1'
        }
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