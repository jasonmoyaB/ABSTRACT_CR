
//Script para la funcionalidad de la pagina de Reportes


// Aplica un rango de días a los campos de fecha del formulario de reportes

/**
 * 
 * @param {number} dias 
 */
function aplicarRangoDias(dias) {
    var fechaFin = new Date();
    var fechaInicio = new Date();
    fechaInicio.setDate(fechaInicio.getDate() - dias);

   
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

        
        var formulario = document.getElementById('filtroFechas');
        if (formulario) {
            formulario.submit();
        }
    }
}

/**
 
 * @param {Array} nuevosUsuariosData 
 * @param {Array} suscripcionesData 
 */
function inicializarGraficosReportes(nuevosUsuariosData, suscripcionesData) {
   
    if (typeof Chart === 'undefined') {
        console.error('Chart.js no está cargado. Asegúrate de incluir la librería antes de llamar a esta función.');
        return;
    }

    // Graafico de lineas: Altas de usuarios por diaa
    const ctxNuevosUsuarios = document.getElementById('chartNuevosUsuarios');
    if (ctxNuevosUsuarios && nuevosUsuariosData && nuevosUsuariosData.length > 0) {
        new Chart(ctxNuevosUsuarios, {
            type: 'line',
            data: {
                labels: nuevosUsuariosData.map(d => {
                    var date = new Date(d.label);
                    return date.toLocaleDateString('es-ES', { day: '2-digit', month: '2-digit' });
                }),
                datasets: [{
                    label: 'Nuevas altas',
                    data: nuevosUsuariosData.map(d => d.valor),
                    tension: 0.3,
                    fill: true,
                    borderWidth: 2,
                    backgroundColor: 'rgba(54, 162, 235, 0.08)',
                    borderColor: 'rgb(54, 162, 235)',
                    pointRadius: 3,
                    pointHoverRadius: 5
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    x: {
                        ticks: {
                            maxTicksLimit: 15
                        }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1
                        }
                    }
                }
            }
        });
    }

    // Grafico de dona: Suscripciones por estado
    const ctxSuscripciones = document.getElementById('chartSuscripciones');
    if (ctxSuscripciones && suscripcionesData && suscripcionesData.length > 0) {
        var colores = {
            'Activa': '#28a745',
            'Pausada': '#ffc107',
            'Cancelada': '#dc3545'
        };

        new Chart(ctxSuscripciones, {
            type: 'doughnut',
            data: {
                labels: suscripcionesData.map(d => d.categoria),
                datasets: [{
                    data: suscripcionesData.map(d => d.valor),
                    backgroundColor: suscripcionesData.map(d => colores[d.categoria] || '#6c757d')
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        });
    }
}

