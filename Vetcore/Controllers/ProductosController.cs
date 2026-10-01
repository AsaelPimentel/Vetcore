using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Vetcore.Filters;
using Vetcore.Helpers;
using Vetcore.Models;
using Vetcore.Services;

namespace Vetcore.Controllers
{
    [Authorize]
    public class ProductosController : Controller
    {
        private readonly ProductoService _productoService = new ProductoService();

<<<<<<< HEAD
        // ELIMINADO: Se borró ObtenerIdEmpleadoActual() dependiente de Session
=======
        private int ObtenerIdEmpleadoActual()
        {
            return Session["IdEmpleado"] != null ? (int)Session["IdEmpleado"] : 1;
        }
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba

        public async Task<ActionResult> Index(string search = "", int page = 1)
        {
            var productosList = await _productoService.ObtenerProductosAsync(search);

            ViewBag.SearchTerm = search;

            int pageSize = 10;
            int totalItems = productosList.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages == 0 ? 1 : totalPages;

            var lista = productosList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (Request.IsAjaxRequest())
            {
                return PartialView("_TablaProductos", lista);
            }

            return View(lista);
        }

        [AuthorizeRol(RolesSistema.Admin)]
        public async Task<ActionResult> Create()
        {
            var categorias = await _productoService.ObtenerCategoriasAsync();
            ViewBag.IdCategoria = new SelectList(categorias, "IdCategoria", "Nombre");

            var categoriasServicio = categorias.Where(c => c.EsServicio == true).Select(c => c.IdCategoria).ToList();
            ViewBag.CategoriasServicio = string.Join(",", categoriasServicio);

            return PartialView("_CreateModal", new Productos { StockActual = 0, StockMinimo = 5, PrecioCosto = 0, PrecioVenta = 0, Activo = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRol(RolesSistema.Admin)]
        public async Task<ActionResult> Create(Productos producto)
        {
            if (ModelState.IsValid)
            {
                await _productoService.CrearProductoAsync(producto);

<<<<<<< HEAD
                // MIGRACIÓN A CLAIMS
                int idEmpleado = HttpContext.ObtenerIdEmpleado();
=======
                int idEmpleado = ObtenerIdEmpleadoActual();
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba
                await AuditoriaService.RegistrarAccionAsync(
                    idEmpleado,
                    "CREAR_PRODUCTO",
                    $"Se registró el nuevo producto/servicio: {producto.Nombre} (ID: {producto.IdProducto}, Costo: ${producto.PrecioCosto}, Venta: ${producto.PrecioVenta})"
                );

                return Json(new { success = true });
            }

            var categorias = await _productoService.ObtenerCategoriasAsync();
            ViewBag.IdCategoria = new SelectList(categorias, "IdCategoria", "Nombre", producto.IdCategoria);

            var categoriasServicio = categorias.Where(c => c.EsServicio == true).Select(c => c.IdCategoria).ToList();
            ViewBag.CategoriasServicio = string.Join(",", categoriasServicio);

            return PartialView("_CreateModal", producto);
        }

<<<<<<< HEAD
        // CORRECCIÓN: Uso de constante en lugar de string quemado
        [AuthorizeRol(RolesSistema.Admin)]
=======
        [AuthorizeRol("Administrador")]
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var producto = await _productoService.ObtenerPorIdAsync(id.Value);
            if (producto == null) return HttpNotFound();

            var categorias = await _productoService.ObtenerCategoriasAsync();
            ViewBag.IdCategoria = new SelectList(categorias, "IdCategoria", "Nombre", producto.IdCategoria);
            return PartialView("_EditModal", producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRol(RolesSistema.Admin)]
        public async Task<ActionResult> Edit(Productos producto)
        {
            if (ModelState.IsValid)
            {
                await _productoService.ActualizarProductoAsync(producto);

<<<<<<< HEAD
                // MIGRACIÓN A CLAIMS
                int idEmpleado = HttpContext.ObtenerIdEmpleado();
=======
                int idEmpleado = ObtenerIdEmpleadoActual();
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba
                await AuditoriaService.RegistrarAccionAsync(
                    idEmpleado,
                    "MODIFICAR_PRODUCTO",
                    $"Se actualizaron los datos del producto: {producto.Nombre} (ID: {producto.IdProducto})"
                );

                return Json(new { success = true });
            }

            var categorias = await _productoService.ObtenerCategoriasAsync();
            ViewBag.IdCategoria = new SelectList(categorias, "IdCategoria", "Nombre", producto.IdCategoria);
            return PartialView("_EditModal", producto);
        }

<<<<<<< HEAD
        // CORRECCIÓN: Protección de ruta financiera confidencial
        [AuthorizeRol(RolesSistema.Admin)]
=======
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba
        public async Task<ActionResult> ReporteInventario()
        {
            var productos = await _productoService.ObtenerInventarioCompletoAsync();

            ViewBag.TotalProductos = productos.Count;
            ViewBag.InversionTotal = productos.Sum(p => (p.StockActual ?? 0) * p.PrecioCosto);
            ViewBag.ValorVentaTotal = productos.Sum(p => (p.StockActual ?? 0) * p.PrecioVenta);
            ViewBag.ProductosStockBajo = productos.Count(p => (p.StockActual ?? 0) <= p.StockMinimo);

            return View(productos);
        }

<<<<<<< HEAD
        // CORRECCIÓN: Protección de ruta financiera confidencial
        [AuthorizeRol(RolesSistema.Admin)]
=======
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba
        public async Task<ActionResult> ReporteInventarioImprimir()
        {
            var productos = await _productoService.ObtenerInventarioCompletoAsync();

            ViewBag.TotalProductos = productos.Count;
            ViewBag.InversionTotal = productos.Sum(p => (p.StockActual ?? 0) * p.PrecioCosto);
            ViewBag.ValorVentaTotal = productos.Sum(p => (p.StockActual ?? 0) * p.PrecioVenta);
            ViewBag.ProductosStockBajo = productos.Count(p => (p.StockActual ?? 0) <= p.StockMinimo);

            return PartialView("_ReporteInventarioPrint", productos);
        }

<<<<<<< HEAD
        // CORRECCIÓN: Condición OR en roles (Admin O Medico)
        [AuthorizeRol(RolesSistema.Admin, RolesSistema.Medico)]
=======
        [AuthorizeRol(RolesSistema.Admin), AuthorizeRol(RolesSistema.Medico)]
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba
        public async Task<ActionResult> AjusteInventario(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var producto = await _productoService.ObtenerPorIdAsync(id.Value);
            if (producto == null) return HttpNotFound();

            ViewBag.NombreProducto = producto.Nombre;
            ViewBag.StockActual = producto.StockActual;

            ViewBag.TiposMovimiento = new SelectList(new[]
            {
                new { Id = "CompraProveedor", Nombre = "Entrada por Compra a Proveedor (+)" },
                new { Id = "DevolucionCliente", Nombre = "Devolución de Cliente (+)" },
                new { Id = "AjusteEntrada", Nombre = "Ajuste Administrativo / Inventario Inicial (+)" },
                new { Id = "MermaCaducidad", Nombre = "Baja por Producto Caducado (-)" },
                new { Id = "MermaDano", Nombre = "Baja por Producto Dañado / Roto (-)" },
                new { Id = "MermaRobo", Nombre = "Baja por Robo o Faltante (-)" },
                new { Id = "UsoClinico", Nombre = "Uso Clínico / Hospitalización (-)" },
                new { Id = "RecetaMedica", Nombre = "Dispensación por Receta Médica (-)" },
                new { Id = "AjusteSalida", Nombre = "Ajuste Administrativo Negativo (-)" }
            }, "Id", "Nombre");

            var kardexDto = new KardexInventario
            {
                IdProducto = id.Value,
                Cantidad = 1
            };

            return PartialView("_AjusteInventarioModal", kardexDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
<<<<<<< HEAD
        // CORRECCIÓN: Condición OR en roles (Admin O Medico)
        [AuthorizeRol(RolesSistema.Admin, RolesSistema.Medico)]
        public async Task<ActionResult> AjusteInventario(KardexInventario modelo)
        {
            // MIGRACIÓN A CLAIMS
            int idEmpleadoSesion = HttpContext.ObtenerIdEmpleado();
=======
        [AuthorizeRol(RolesSistema.Admin), AuthorizeRol(RolesSistema.Medico)]
        public async Task<ActionResult> AjusteInventario(KardexInventario modelo)
        {
            int idEmpleadoSesion = ObtenerIdEmpleadoActual();
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba

            if (ModelState.IsValid)
            {
                bool esSalida = modelo.TipoMovimiento.Contains("Merma") ||
                                modelo.TipoMovimiento == "AjusteSalida" ||
                                modelo.TipoMovimiento == "UsoClinico" ||
                                modelo.TipoMovimiento == "RecetaMedica" ||
                                modelo.TipoMovimiento == "VentaPOS";

                if (esSalida)
                {
                    var productoActual = await _productoService.ObtenerPorIdAsync(modelo.IdProducto);

                    if (productoActual != null && modelo.Cantidad > productoActual.StockActual)
                    {
                        ModelState.AddModelError("Cantidad", "La cantidad a retirar supera el stock disponible en farmacia.");
                    }
                    else
                    {
                        bool resultado = await _productoService.RegistrarMovimientoKardexAsync(modelo.IdProducto, idEmpleadoSesion, modelo.TipoMovimiento, modelo.Cantidad, modelo.Motivo);
                        if (resultado) return Json(new { success = true });
                    }
                }
                else
                {
                    bool resultado = await _productoService.RegistrarMovimientoKardexAsync(modelo.IdProducto, idEmpleadoSesion, modelo.TipoMovimiento, modelo.Cantidad, modelo.Motivo);
                    if (resultado) return Json(new { success = true });
                }
            }

            var prod = await _productoService.ObtenerPorIdAsync(modelo.IdProducto);

            ViewBag.NombreProducto = prod != null ? prod.Nombre : "";
            ViewBag.StockActual = prod != null ? prod.StockActual : 0;
            ViewBag.TiposMovimiento = new SelectList(new[]
            {
                new { Id = "CompraProveedor", Nombre = "Entrada por Compra a Proveedor (+)" },
                new { Id = "DevolucionCliente", Nombre = "Devolución de Cliente (+)" },
                new { Id = "AjusteEntrada", Nombre = "Ajuste Administrativo / Inventario Inicial (+)" },
                new { Id = "MermaCaducidad", Nombre = "Baja por Producto Caducado (-)" },
                new { Id = "MermaDano", Nombre = "Baja por Producto Dañado / Roto (-)" },
                new { Id = "MermaRobo", Nombre = "Baja por Robo o Faltante (-)" },
                new { Id = "UsoClinico", Nombre = "Uso Clínico / Hospitalización (-)" },
                new { Id = "RecetaMedica", Nombre = "Dispensación por Receta Médica (-)" },
                new { Id = "AjusteSalida", Nombre = "Ajuste Administrativo Negativo (-)" }
            }, "Id", "Nombre", modelo.TipoMovimiento);

            return PartialView("_AjusteInventarioModal", modelo);
        }

        [AuthorizeRol(RolesSistema.Admin)]
        public async Task<ActionResult> KardexHistorial(int? idCategoria, string tipoMovimiento, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var historial = await _productoService.ObtenerKardexHistorialAsync(idCategoria, tipoMovimiento, fechaInicio, fechaFin);

            ViewBag.ListaCategorias = await _productoService.ObtenerCategoriasAsync();
            ViewBag.CategoriaSeleccionada = idCategoria;
            ViewBag.TipoMovimientoSeleccionado = tipoMovimiento;

            return View(historial);
        }

        [AuthorizeRol(RolesSistema.Admin)]
        public async Task<ActionResult> KardexHistorialImprimir(int? idCategoria, string tipoMovimiento, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var historial = await _productoService.ObtenerKardexHistorialAsync(idCategoria, tipoMovimiento, fechaInicio, fechaFin);

            ViewBag.FiltroTipo = tipoMovimiento;
            ViewBag.FiltroInicio = fechaInicio?.ToString("dd/MM/yyyy");
            ViewBag.FiltroFin = fechaFin?.ToString("dd/MM/yyyy");

            return PartialView("_KardexHistorialPrint", historial);
        }

        [AuthorizeRol(RolesSistema.Admin)]
        public async Task<ActionResult> ExportarExcel()
        {
            var productos = await _productoService.ObtenerInventarioCompletoAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Inventario y Farmacia");

                worksheet.Cell("A1").Value = "VetCore - Reporte General de Inventario";
                worksheet.Cell("A1").Style.Font.Bold = true;
                worksheet.Cell("A1").Style.Font.FontSize = 14;
                worksheet.Cell("A1").Style.Font.FontColor = XLColor.FromHtml("#0284c7");

                worksheet.Cell("A2").Value = $"Fecha de emisión: {DateTime.Now:dd/MM/yyyy HH:mm}";
                worksheet.Cell("A2").Style.Font.Italic = true;

                int filaCabecera = 4;
                worksheet.Cell(filaCabecera, 1).Value = "Código de Barras";
                worksheet.Cell(filaCabecera, 2).Value = "Producto / Servicio";
                worksheet.Cell(filaCabecera, 3).Value = "Categoría";
                worksheet.Cell(filaCabecera, 4).Value = "Stock Actual";
                worksheet.Cell(filaCabecera, 5).Value = "Costo Unitario";
                worksheet.Cell(filaCabecera, 6).Value = "Precio Venta";
                worksheet.Cell(filaCabecera, 7).Value = "Inversión Total";
                worksheet.Cell(filaCabecera, 8).Value = "Valor Venta Potencial";

                var rangoCabecera = worksheet.Range($"A{filaCabecera}:H{filaCabecera}");
                rangoCabecera.Style.Fill.BackgroundColor = XLColor.FromHtml("#0284c7");
                rangoCabecera.Style.Font.FontColor = XLColor.White;
                rangoCabecera.Style.Font.Bold = true;

                int fila = filaCabecera + 1;
                foreach (var item in productos)
                {
                    bool esServicio = item.StockActual == null;
                    int stock = esServicio ? 0 : item.StockActual.Value;

                    worksheet.Cell(fila, 1).Value = string.IsNullOrEmpty(item.CodigoBarras) ? "N/D" : item.CodigoBarras;
                    worksheet.Cell(fila, 2).Value = item.Nombre;
                    worksheet.Cell(fila, 3).Value = item.Categorias != null ? item.Categorias.Nombre : "Sin Categoría";

                    if (esServicio)
                    {
                        worksheet.Cell(fila, 4).Value = "Ilimitado (Servicio)";
                    }
                    else
                    {
                        worksheet.Cell(fila, 4).Value = stock;
                    }

                    worksheet.Cell(fila, 5).Value = item.PrecioCosto;
                    worksheet.Cell(fila, 6).Value = item.PrecioVenta;

                    if (esServicio)
                    {
                        worksheet.Cell(fila, 7).Value = "-";
                        worksheet.Cell(fila, 8).Value = "-";
                    }
                    else
                    {
                        worksheet.Cell(fila, 7).Value = stock * item.PrecioCosto;
                        worksheet.Cell(fila, 8).Value = stock * item.PrecioVenta;
                    }

                    worksheet.Cell(fila, 5).Style.NumberFormat.Format = "$#,##0.00";
                    worksheet.Cell(fila, 6).Style.NumberFormat.Format = "$#,##0.00";
                    if (!esServicio)
                    {
                        worksheet.Cell(fila, 7).Style.NumberFormat.Format = "$#,##0.00";
                        worksheet.Cell(fila, 8).Style.NumberFormat.Format = "$#,##0.00";
                    }

                    fila++;
                }

                worksheet.Columns().AdjustToContents();

<<<<<<< HEAD
                // MIGRACIÓN A CLAIMS
                int idAdmin = HttpContext.ObtenerIdEmpleado();
=======
                int idAdmin = ObtenerIdEmpleadoActual();
>>>>>>> 7af84dbc4060a8bbaa8bab50c489e93f45a0ceba
                await AuditoriaService.RegistrarAccionAsync(
                    idAdmin,
                    "EXPORTAR_INVENTARIO_EXCEL",
                    "Se descargó el reporte financiero y de valorización de inventario en Excel."
                );

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Reporte_Inventario_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
                }
            }
        }
    }
}