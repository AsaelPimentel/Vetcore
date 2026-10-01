// Archivo: Scripts/vetcore-catalogos.js

// 1. Funciones de Modales
function abrirModalCrear(entidad) {
    $.get('/Catalogos/Crear', { entidad: entidad })
        .done(function (data) {
            $('#modal-container').html(data);
            $('#modalCatalogo').modal('show');
        });
}

function abrirModalEditar(entidad, id) {
    $.get('/Catalogos/Editar', { entidad: entidad, id: id })
        .done(function (data) {
            $('#modal-container').html(data);
            $('#modalCatalogo').modal('show');
        });
}

// 2. Eventos de Guardado (Crear y Editar)
$(document).on('click', '#btnGuardarCatalogo', function () {
    var form = $('#frmCatalogo');
    $.post('/Catalogos/Crear', form.serialize())
        .done(function (response) {
            if (response.success) {
                $('#modalCatalogo').modal('hide');
                location.reload();
            } else {
                alert(response.message);
            }
        });
});

$(document).on('click', '#btnActualizarCatalogo', function () {
    var form = $('#frmEditarCatalogo');
    $.post('/Catalogos/Editar', form.serialize())
        .done(function (response) {
            if (response.success) {
                $('#modalCatalogo').modal('hide');
                location.reload();
            } else {
                alert(response.message);
            }
        });
});

// 3. Buscador en tiempo real (Client-side)
function filtrarTabla(idInput, idTabla) {
    var input, filter, table, tr, td, i, j, txtValue;
    input = document.getElementById(idInput);
    filter = input.value.toUpperCase();
    table = document.getElementById(idTabla);
    tr = table.getElementsByTagName("tr");

    for (i = 1; i < tr.length; i++) {
        tr[i].style.display = "none";
        td = tr[i].getElementsByTagName("td");
        for (j = 0; j < td.length; j++) {
            if (td[j]) {
                txtValue = td[j].textContent || td[j].innerText;
                if (txtValue.toUpperCase().indexOf(filter) > -1) {
                    tr[i].style.display = "";
                    break;
                }
            }
        }
    }
}