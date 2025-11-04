
document.addEventListener('DOMContentLoaded', function () {
    const itemsEl = document.getElementById('carritoItems');
    const totalEl = document.getElementById('carritoTotal');
    const pagarLink = document.getElementById('btnPagar');
    const offEl = document.getElementById('carritoOffcanvas');
    if (!itemsEl || !totalEl) return;
    let items = [];
    function render() {
        itemsEl.innerHTML = ''; let total = 0;
        items.forEach((it, ix) => {
            total += it.precio * it.cantidad;
            const d = document.createElement('div'); d.className = 'd-flex align-items-center gap-2';
            d.innerHTML = `<img src="${it.imagen}" style="width:56px;height:56px;object-fit:cover" class="rounded"/>
      <div class="flex-grow-1"><div class="fw-semibold text-truncate">${it.nombre}</</div><div class="text-muted">$${it.precio.toFixed(2)} c/u</div></div>
      <div class="d-flex align-items-center gap-2">
        <input type="number" min="1" value="${it.cantidad}" class="form-control form-control-sm" style="width:80px" data-idx="${ix}"/>
        <button class="btn btn-outline-secondary btn-sm" data-remove="${ix}">Quitar</button>
      </div>`; itemsEl.appendChild(d);
        });
        totalEl.textContent = `$${total.toFixed(2)}`;
        if (pagarLink) { if (items.length > 0) { pagarLink.classList.remove('disabled'); } else { pagarLink.classList.add('disabled'); } }
        sessionStorage.setItem('fitstyle_total', total.toFixed(2));
    }
    document.addEventListener('click', e => {
        const btn = e.target.closest('[data-addcart]');
        if (btn) {
            let d; try { d = JSON.parse(btn.getAttribute('data-addcart')); } catch (_) { return; }
            const ix = items.findIndex(x => x.nombre === d.Nombre);
            if (ix >= 0) items[ix].cantidad += 1; else items.push({ nombre: d.Nombre, precio: Number(d.Precio), imagen: d.Imagen, cantidad: 1 });
            render(); if (offEl && window.bootstrap && window.bootstrap.Offcanvas) { window.bootstrap.Offcanvas.getOrCreateInstance(offEl).show(); }
        }
        const rem = e.target.closest('[data-remove]'); if (rem) { items.splice(Number(rem.getAttribute('data-remove')), 1); render(); }
    });
    document.addEventListener('input', e => {
        if (e.target.matches('input[type=number][data-idx]')) {
            const ix = Number(e.target.getAttribute('data-idx')); items[ix].cantidad = Math.max(1, Number(e.target.value || 1)); render();
        }
    });
    render();
});
