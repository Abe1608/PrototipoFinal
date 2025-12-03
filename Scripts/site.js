//Funcion para el scroll horizontal de las categorias en la pagina principal
document.addEventListener("DOMContentLoaded", function () {
    const navButtons = document.querySelectorAll(".category-nav-btn");

    navButtons.forEach(btn => {
        btn.addEventListener("click", function () {
            const targetId = this.getAttribute("data-target");
            const strip = document.getElementById(targetId);
            if (!strip) return;

            const direction = this.classList.contains("category-nav-next") ? 1 : -1;
            const scrollAmount = 260; // píxeles por click (ajusta si quieres)

            strip.scrollBy({
                left: direction * scrollAmount,
                behavior: "smooth"
            });
        });
    });
});


$(document).ready(function () {

    // Añadir mpas productos
    $(document).on("click", ".btn-qty-plus", function () {
        var idInv = $(this).data("idinventario");

        $.post("/Carrito/AgregarMasProducto", { idInventario: idInv }, function (r) {
            console.log("Añadir:", r);

            if (!r.success && r.notLogged) {
                alert("Debes iniciar sesión.");
                return;
            }

            if (r.success) {
                actualizarBadge(r.count);
                recargarCarrito();
            }
        });
    });

    // Quitar cantidad de productos
    $(document).on("click", ".btn-qty-minus", function () {
        var idInv = $(this).data("idinventario");

        $.post("/Carrito/RestarProducto", { idInventario: idInv }, function (r) {
            

            if (!r.success && r.notLogged) {
                alert("Debes iniciar sesión.");
                return;
            }

            if (r.success) {
                actualizarBadge(r.count);
                recargarCarrito();
            }
        });
    });

    // Eliminar producto del carrito
    $(document).on("click", ".btn-remove-item", function () {
        var idDet = $(this).data("iddetalle");

        $.post("/Carrito/EliminarProducto", { idDetalle: idDet }, function (r) {
           
            if (!r.success && r.notLogged) {
                alert("Debes iniciar sesión.");
                return;
            }

            if (r.success) {
                actualizarBadge(r.count);
                recargarCarrito();
            }
        });
    });

});


//  Funcion para agregar al carrito con AJAX y actualizar badge o mejor dicho el icono de notificación del carrito de compras
$(document).ready(function () {

    $(document).on('click', '.btn-add-cart', function (e) {
        e.preventDefault();

        var btn = $(this);
        var idProducto = btn.data('product-id');

        $.ajax({
            url: urlAgregarCarrito,
            type: 'POST',
            data: { idProducto: idProducto, cantidad: 1 },
            success: function (resp) {

                if (!resp.success && resp.notLogged) {
                    window.location.href = urlLogin;
                    return;
                }

                var $badge = $("#carrito-count");

                // Condicional para actualizar el icono de notificación
                if (resp.count > 0) {
                    $badge.text(resp.count)
                        .removeClass('d-none')
                        .show();

                    
                    $badge.addClass('pulse-badge');
                    setTimeout(function () {
                        $badge.removeClass('pulse-badge');
                    }, 300);

                } else {
                    $badge.addClass('d-none').hide();
                }

                recargarCarrito();
            },
            error: function () {
                console.error("Error en la petición AJAX.");
                alert("Error en la petición AJAX.");
            }
        });
    });

});

// Funcion para eliminar del carrito con AJAX y actualizar el icono de notificación del carrito de compras
function actualizarBadge(count) {
    var $badge = $("#carrito-count");

    if (count > 0) {
        $badge.text(count).removeClass("d-none").addClass("pulse-badge");
        setTimeout(function () {
            $badge.removeClass("pulse-badge");
        }, 300);
    } else {
        $badge.addClass("d-none");
    }
}


function recargarCarrito() {
    
    $("#carritoOffcanvasWrapper").load(urlOffcanvasCarrito, function () {

        var offcanvasEl = document.getElementById("carritoOffcanvas");
        if (offcanvasEl) {
            var offcanvas = bootstrap.Offcanvas.getOrCreateInstance(offcanvasEl);
            offcanvas.show();
        }
    });
}


