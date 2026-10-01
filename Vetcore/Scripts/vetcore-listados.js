// Archivo: Scripts/vetcore-listados.js

function loadTableGlobal(search, page) {
    // Leemos la URL dinámica desde el input de búsqueda
    var targetUrl = $('#searchInput').data('url');

    if (!targetUrl) return; // Si no hay URL, salimos

    $('#table-container').css('opacity', '0.5');

    $.ajax({
        url: targetUrl,
        type: 'GET',
        data: { search: search, page: page },
        success: function (result) {
            $('#table-container').html(result).css('opacity', '1');
        },
        error: function () {
            $('#table-container').css('opacity', '1');
            console.error("Error al cargar la tabla.");
        }
    });
}

$(document).ready(function () {
    // 1. Buscador Dinámico con Retraso (Debounce)
    let timeout = null;
    $(document).on('keyup', '#searchInput', function () {
        clearTimeout(timeout);
        var query = $(this).val();
        timeout = setTimeout(function () {
            loadTableGlobal(query, 1);
        }, 400);
    });

    // 2. Paginación vía AJAX
    $(document).on('click', '.page-link-ajax', function (e) {
        e.preventDefault();
        var page = $(this).data('page');
        var query = $('#searchInput').val();
        loadTableGlobal(query, page);
    });
});