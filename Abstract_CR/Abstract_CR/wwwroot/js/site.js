// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

/**
 * Aplica un rango de días a los campos de fecha del formulario de reportes
 * @param {number} dias - Número de días hacia atrás desde hoy
 */
function aplicarRangoDias(dias) {
    var fechaFin = new Date();
    var fechaInicio = new Date();
    fechaInicio.setDate(fechaInicio.getDate() - dias);

    // Formatear fechas como YYYY-MM-DD para inputs type="date"
    var formatoFecha = function(fecha) {
        var año = fecha.getFullYear();
        var mes = String(fecha.getMonth() + 1).padStart(2, '0');
        var dia = String(fecha.getDate()).padStart(2, '0');
        return año + '-' + mes + '-' + dia;
    };

    var inputDesde = document.getElementById('desde');
    var inputHasta = document.getElementById('hasta');

    if (inputDesde && inputHasta) {
        inputDesde.value = formatoFecha(fechaInicio);
        inputHasta.value = formatoFecha(fechaFin);

        // Enviar el formulario automáticamente
        var formulario = document.getElementById('filtroFechas');
        if (formulario) {
            formulario.submit();
        }
    }
}