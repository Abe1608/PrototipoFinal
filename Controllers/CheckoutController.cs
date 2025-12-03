using iTextSharp.text;
using iTextSharp.text.pdf;
using PrototipoFinal.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PrototipoFinal.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly FitStyleDBEntities db = new FitStyleDBEntities();

        
        private int? GetUsuarioId()
        {
            if (!User.Identity.IsAuthenticated)
            {
                Session["UserId"] = null;
                return null;
            }

            if (Session["UserId"] is int idSesion)
                return idSesion;

            var email = User.Identity.Name;
            var usuario = db.Usuarios.SingleOrDefault(u => u.email == email);
            if (usuario != null)
            {
                Session["UserId"] = usuario.id_usuario;
                return usuario.id_usuario;
            }

            return null;
        }

        // =========================
        //   SELECCIONAR MÉTODO
        // =========================
        [HttpGet]
        public ActionResult MetodoPago()
        {
            var idUsuario = GetUsuarioId();
            if (idUsuario == null)
                return RedirectToAction("Login", "Account");

            // Verificar que haya carrito con productos
            var carrito = db.Carritos
                            .SingleOrDefault(c => c.id_usuario == idUsuario && c.estado == "Abierto");

            if (carrito == null || !carrito.Carrito_Detalle.Any())
            {
                TempData["Error"] = "Tu carrito está vacío.";
                return RedirectToAction("Index", "Productos");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MetodoPago(string metodo)
        {
            if (metodo != "Tarjeta" && metodo != "PayPal")
            {
                ModelState.AddModelError("", "Selecciona un método de pago.");
                return View();
            }

            // Guardamos método para la siguiente pantalla
            TempData["MetodoPago"] = metodo;
            return RedirectToAction("Pagar", new { metodo = metodo });
        }

        // =========================
        //   PANTALLA DE PAGO
        // =========================
        [HttpGet]
        public ActionResult Pagar(string metodo)
        {
            var idUsuario = GetUsuarioId();
            if (idUsuario == null)
                return RedirectToAction("Login", "Account");

            // Recuperar método si viene de TempData
            if (string.IsNullOrEmpty(metodo))
                metodo = TempData["MetodoPago"] as string;

            if (string.IsNullOrEmpty(metodo))
                return RedirectToAction("MetodoPago");

            var carrito = db.Carritos
                            .SingleOrDefault(c => c.id_usuario == idUsuario && c.estado == "Abierto");

            if (carrito == null || !carrito.Carrito_Detalle.Any())
            {
                TempData["Error"] = "Tu carrito está vacío.";
                return RedirectToAction("Index", "Productos");
            }

            // Calcular total desde BD
            var total = (from d in db.Carrito_Detalle
                         join inv in db.Inventario_Detalle on d.id_inventario equals inv.id_inventario
                         join p in db.Productos on inv.id_producto equals p.id_producto
                         where d.id_carrito == carrito.id_carrito
                         select (decimal?)d.cantidad * p.precio)
                        .Sum() ?? 0m;

            ViewBag.MetodoPago = metodo;
            ViewBag.Total = total;

            return View();
        }

        // =========================
        //   CONFIRMAR PAGO
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pagar(
            string nombre,
            string tarjeta,
            string vencimiento,
            string cvv,
            decimal total,
            string metodoPago)
        {
            var idUsuario = GetUsuarioId();
            if (idUsuario == null)
                return RedirectToAction("Login", "Account");

            var carrito = db.Carritos
                            .SingleOrDefault(c => c.id_usuario == idUsuario && c.estado == "Abierto");

            if (carrito == null || !carrito.Carrito_Detalle.Any())
            {
                TempData["Error"] = "Tu carrito está vacío.";
                return RedirectToAction("Index", "Productos");
            }

            // Recalcular total REAL desde BD (no confiar en el hidden)
            var totalReal = (from d in db.Carrito_Detalle
                             join inv in db.Inventario_Detalle on d.id_inventario equals inv.id_inventario
                             join p in db.Productos on inv.id_producto equals p.id_producto
                             where d.id_carrito == carrito.id_carrito
                             select (decimal?)d.cantidad * p.precio)
                            .Sum() ?? 0m;

            // Aquí iría la integración real con pasarela (Stripe/PayPal).
            // De momento asumimos que el pago fue "aceptado".

            // 1) Crear registro en Pedidos
            var pedido = new Pedidos
            {
                id_usuario = idUsuario,
                total = totalReal,
                estado = "Pagado",
                fecha_creacion = DateTime.Now
            };
            db.Pedidos.Add(pedido);
            db.SaveChanges(); // para tener id_pedido

            // 2) Crear detalles de pedido a partir del carrito
            var detallesCarrito = db.Carrito_Detalle
                                    .Where(d => d.id_carrito == carrito.id_carrito)
                                    .ToList();

            foreach (var det in detallesCarrito)
            {
                var inv = db.Inventario_Detalle.SingleOrDefault(i => i.id_inventario == det.id_inventario);
                var producto = inv?.Productos;
                decimal precioUnitario = producto != null ? producto.precio : 0m;

                db.Pedido_Detalle.Add(new Pedido_Detalle
                {
                    id_pedido = pedido.id_pedido,
                    id_inventario = det.id_inventario,
                    cantidad = det.cantidad,
                    precio_unitario = precioUnitario
                });

                // Opcional: actualizar stock
                if (inv != null)
                {
                    inv.stock_actual -= det.cantidad;
                }
            }

            // 3) Crear factura
            string numeroFactura = "FS-" + DateTime.Now.ToString("yyyyMMddHHmmss");

            var factura = new Facturas
            {
                id_pedido = pedido.id_pedido,
                id_usuario = idUsuario.Value,
                numero_factura = numeroFactura,
                monto_total = totalReal,
                metodo_pago = metodoPago,
                fecha_emision = DateTime.Now,
                estado = "Emitida"
            };
            db.Facturas.Add(factura);

            // 4) Cerrar carrito
            carrito.estado = "Cerrado";
            carrito.ultima_actualizacion = DateTime.Now;

            db.SaveChanges();

            // 5) Pasar datos a la pantalla de éxito
            TempData["Total"] = totalReal;
            TempData["Nombre"] = nombre;
            TempData["IdFactura"] = factura.id_factura;

            return RedirectToAction("Exito");
        }

        public ActionResult Exito()
        {
            return View();
        }

        // Aquí luego agregamos FacturaPdf(int id) para el PDF.

        public FileResult FacturaPdf(int id)
        {
            var factura = db.Facturas
                .Include("Usuarios")
                .Include("Pedidos")
                .Include("Pedidos.Pedido_Detalle")
                .Include("Pedidos.Pedido_Detalle.Inventario_Detalle")
                .Include("Pedidos.Pedido_Detalle.Inventario_Detalle.Productos")
                .SingleOrDefault(f => f.id_factura == id);

            if (factura == null)
                throw new HttpException(404, "Factura no encontrada");

            using (var ms = new MemoryStream())
            {
                // Documento tamaño carta con márgenes
                var doc = new Document(PageSize.LETTER, 40f, 40f, 50f, 60f);
                var writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // ================================
                //  FUENTES PERSONALIZADAS
                // ================================
                BaseFont bfRegular = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
                BaseFont bfBold = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);

                var fontTitle = new Font(bfBold, 18, Font.BOLD, new BaseColor(33, 33, 33));
                var fontSub = new Font(bfRegular, 10, Font.NORMAL, new BaseColor(120, 120, 120));
                var fontLabel = new Font(bfBold, 10, Font.BOLD, new BaseColor(70, 70, 70));
                var fontNormal = new Font(bfRegular, 10, Font.NORMAL, BaseColor.BLACK);
                var fontTableH = new Font(bfBold, 10, Font.BOLD, BaseColor.WHITE);
                var fontTotal = new Font(bfBold, 12, Font.BOLD, new BaseColor(0, 0, 0));
                var fontFooter = new Font(bfRegular, 8, Font.NORMAL, new BaseColor(130, 130, 130));

                // ================================
                //  COLORES
                // ================================
                BaseColor colorPrimario = new BaseColor(255, 140, 0);
                BaseColor colorGrisClaro = new BaseColor(245, 245, 245);
                BaseColor colorTablaHead = new BaseColor(33, 33, 33);

                // ================================
                //  HEADER CON LOGO + TITULO
                // ================================
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 30, 70 });

                try
                {
                    string logoPath = Server.MapPath("~/images/logo/Logo.png");
                    var logo = Image.GetInstance(logoPath);
                    logo.ScaleAbsoluteHeight(40);
                    logo.ScaleAbsoluteWidth(40);

                    var cellLogo = new PdfPCell(logo);
                    cellLogo.Border = Rectangle.NO_BORDER;
                    cellLogo.VerticalAlignment = Element.ALIGN_MIDDLE;
                    headerTable.AddCell(cellLogo);
                }
                catch
                {
                    var cellEmpty = new PdfPCell(new Phrase(" "));
                    cellEmpty.Border = Rectangle.NO_BORDER;
                    headerTable.AddCell(cellEmpty);
                }

                var headerText = new PdfPTable(1);
                headerText.WidthPercentage = 100;

                var cellTitle = new PdfPCell(new Phrase("FACTURA", fontTitle));
                cellTitle.Border = Rectangle.NO_BORDER;
                headerText.AddCell(cellTitle);

                var cellSub = new PdfPCell(new Phrase("FitStyle - Ropa deportiva", fontSub));
                cellSub.Border = Rectangle.NO_BORDER;
                headerText.AddCell(cellSub);

                var cellHeaderText = new PdfPCell(headerText);
                cellHeaderText.Border = Rectangle.NO_BORDER;
                headerTable.AddCell(cellHeaderText);

                doc.Add(headerTable);

                var line = new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, colorPrimario, Element.ALIGN_CENTER, -2);
                doc.Add(new Chunk(line));
                doc.Add(new Paragraph(" "));

                // ================================
                //  DATOS GENERALES
                // ================================
                var usuario = factura.Usuarios;

                PdfPTable infoTable = new PdfPTable(2);
                infoTable.WidthPercentage = 100;
                infoTable.SetWidths(new float[] { 50, 50 });

                PdfPTable leftInfo = new PdfPTable(1);
                leftInfo.AddCell(CellSinBorde(new Phrase("Datos del cliente", fontLabel)));
                leftInfo.AddCell(CellSinBorde(new Phrase(usuario.nombre_completo ?? "", fontNormal)));
                leftInfo.AddCell(CellSinBorde(new Phrase(usuario.email ?? "", fontNormal)));

                PdfPTable rightInfo = new PdfPTable(1);
                rightInfo.AddCell(CellSinBorde(new Phrase("Datos de factura", fontLabel)));
                rightInfo.AddCell(CellSinBorde(new Phrase($"Número: {factura.numero_factura}", fontNormal)));
                rightInfo.AddCell(CellSinBorde(new Phrase($"Fecha: {factura.fecha_emision:dd/MM/yyyy}", fontNormal)));
                rightInfo.AddCell(CellSinBorde(new Phrase($"Método de pago: {factura.metodo_pago}", fontNormal)));

                var cellLeft = new PdfPCell(leftInfo) { Border = Rectangle.NO_BORDER };
                var cellRight = new PdfPCell(rightInfo) { Border = Rectangle.NO_BORDER };

                infoTable.AddCell(cellLeft);
                infoTable.AddCell(cellRight);

                doc.Add(infoTable);
                doc.Add(new Paragraph(" "));

                // ================================
                //  TABLA DE DETALLE
                // ================================
                PdfPTable tabla = new PdfPTable(4);
                tabla.WidthPercentage = 100;
                tabla.SetWidths(new float[] { 40, 20, 20, 20 });

                tabla.AddCell(HeaderCell("Producto", fontTableH, colorTablaHead));
                tabla.AddCell(HeaderCell("Cantidad", fontTableH, colorTablaHead));
                tabla.AddCell(HeaderCell("Precio", fontTableH, colorTablaHead));
                tabla.AddCell(HeaderCell("Subtotal", fontTableH, colorTablaHead));

                foreach (var det in factura.Pedidos.Pedido_Detalle)
                {
                    var prod = det.Inventario_Detalle.Productos;
                    string nombre = prod != null ? prod.nombre_producto : "(Producto)";
                    int cantidad = det.cantidad;
                    decimal precio = det.precio_unitario;
                    decimal subtotal = precio * cantidad;

                    tabla.AddCell(BodyCell(nombre, fontNormal));
                    tabla.AddCell(BodyCell(cantidad.ToString(), fontNormal));
                    tabla.AddCell(BodyCell("$" + precio.ToString("0.00"), fontNormal));
                    tabla.AddCell(BodyCell("$" + subtotal.ToString("0.00"), fontNormal));
                }

                doc.Add(tabla);

                // ================================
                //  TOTAL  (ARREGLADO)
                // ================================
                doc.Add(new Paragraph(" "));
                PdfPTable totalTable = new PdfPTable(2);
                totalTable.WidthPercentage = 40;
                totalTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                totalTable.SetWidths(new float[] { 50, 50 });

                // Creamos las celdas por separado
                PdfPCell totalLabelCell = CellSinBorde(new Phrase("Total:", fontLabel));
                PdfPCell totalValueCell = CellSinBorde(new Phrase("$" + factura.monto_total.ToString("0.00"), fontTotal));

                // Alineamos la celda del valor a la derecha
                totalValueCell.HorizontalAlignment = Element.ALIGN_RIGHT;

                totalTable.AddCell(totalLabelCell);
                totalTable.AddCell(totalValueCell);

                doc.Add(totalTable);

                // ================================
                //  NOTA AL PIE
                // ================================
                doc.Add(new Paragraph(" "));
                doc.Add(new Chunk(line));

                var nota = new Paragraph(
                    "Nota: Esta factura es un comprobante electrónico generado por el sistema FitStyle. " +
                    "No requiere firma o sello para tener validez. Si tienes dudas sobre tu pedido, contáctanos a soporte@fitstyle.com.",
                    fontFooter
                );
                nota.Alignment = Element.ALIGN_CENTER;
                doc.Add(nota);

                doc.Close();

                return File(ms.ToArray(), "application/pdf", $"Factura_{factura.numero_factura}.pdf");
            }
        }

        // Helpers para celdas
        private PdfPCell CellSinBorde(Phrase phrase)
        {
            return new PdfPCell(phrase)
            {
                Border = Rectangle.NO_BORDER,
                PaddingBottom = 2f
            };
        }

        private PdfPCell HeaderCell(string text, Font font, BaseColor bg)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = bg,
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 5f
            };
        }

        private PdfPCell BodyCell(string text, Font font)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                Padding = 4f,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
        }

    }
}

