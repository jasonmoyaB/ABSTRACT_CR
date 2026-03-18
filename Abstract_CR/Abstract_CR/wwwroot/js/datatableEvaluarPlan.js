<script>
    $(document).ready(function () {

        function getVal(row, keys, def = null) {
            if (!row) return def;
            for (var i = 0; i < keys.length; i++) {
                var k = keys[i];
                if (row.hasOwnProperty(k) && row[k] != null && row[k] != undefined) return row[k];
            }
            var lower = {};
            Object.keys(row).forEach(rk => lower[rk.toLowerCase()] = row[rk]);
            for (var j = 0; j < keys.length; j++) {
                var kk = keys[j].toLowerCase();
                if (lower.hasOwnProperty(kk) && lower[kk] != null && lower[kk] != undefined) return lower[kk];
            }
            return def;
        }

            var table = $('#tblEvaluaciones').DataTable({
        ajax: {
        url: '@Url.Action("GetEvaluaciones", "PlanNutricional")',
    dataSrc: ''
                },
    columns: [
    {data: row => getVal(row, ['NombrePlan','nombrePlan'], '(Sin plan)') },
    {data: row => getVal(row, ['NombreUsuario','nombreUsuario','UsuarioNombre'], '(Usuario desconocido)') },
    {data: row => getVal(row, ['Calificacion','calificacion'], '') },
    {
        data: row => {
                            var c = getVal(row, ['Comentario','comentario','Observacion'], '');
                            return (c && c.length > 80) ? c.substr(0,80) + '...' : c;
                        }
                    },
    {
        data: row => {
                            var f = getVal(row, ['FechaRegistro','fechaRegistro','createdAt'], '');
    var d = new Date(f);
    return isNaN(d.getTime()) ? f : d.toLocaleString();
                        }
                    },
    {
        data: null,
    orderable: false,
    searchable: false,
    className: 'text-center',
    render: function (data, type, row) {
                            var id = getVal(row, ['EvaluacionID','evaluacionId'], '');
    return `<button class="btn btn-sm btn-primary btn-ver" data-id="${id}" title="Ver"><i class="fa-solid fa-eye"></i></button>`;
                        }
                    }
    ],
    order: [[4, 'desc']],
    dom: 'Bfrtip',
    buttons: [
    {extend: 'excelHtml5', text: '<i class="fa-solid fa-file-excel"></i> Excel' },
    {extend: 'pdfHtml5', text: '<i class="fa-solid fa-file-pdf"></i> PDF' },
    {extend: 'print', text: '<i class="fa-solid fa-print"></i> Imprimir' }
    ],
    language: {
        url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json"
                }
            });

    // Modal: ver detalle
    $('#tblEvaluaciones tbody').on('click', '.btn-ver', function () {
                var row = table.row($(this).closest('tr')).data();
    if (!row) return;

    $('#det-plan').text(getVal(row, ['NombrePlan','nombrePlan'], '(Sin plan)'));
    $('#det-usuario').text(getVal(row, ['NombreUsuario','nombreUsuario','UsuarioNombre'], '(Usuario desconocido)'));
    $('#det-calificacion').text(getVal(row, ['Calificacion','calificacion'], ''));
    $('#det-comentario').text(getVal(row, ['Comentario','comentario'], ''));
    var f = getVal(row, ['FechaRegistro','fechaRegistro','createdAt'], '');
    var d = new Date(f);
    $('#det-fecha').text(isNaN(d.getTime()) ? f : d.toLocaleString());

    new bootstrap.Modal(document.getElementById('modalDetalle')).show();
            });
        });
</script>