<script>
    document.addEventListener("DOMContentLoaded", function() {
        // --- LÓGICA DEL SIDEBAR (Ya existente) ---
        const sidebarToggle = document.body.querySelector('#sidebarToggle');
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', event => {
            event.preventDefault();
            document.body.classList.toggle('sidebar-toggled');
            document.querySelector('.sidebar').classList.toggle('toggled');

            const icon = sidebarToggle.querySelector('i');
            if (document.querySelector('.sidebar').classList.contains('toggled')) {
                icon.classList.remove('bi-chevron-left');
                icon.classList.add('bi-chevron-right');
            } else {
                icon.classList.remove('bi-chevron-right');
                icon.classList.add('bi-chevron-left');
            }
        });
        }

    // --- INICIALIZACIÓN AL CARGAR ---
    actualizarBadgeSolo(); // Para que el número aparezca apenas entras a la web
    });

    // --- LÓGICA DEL CARRITO (Moderna) ---

    function agregarAlCarrito(idProducto) {
        fetch('/Carrito/Agregar', {
            method: 'POST',
            body: JSON.stringify({ id: idProducto }),
            headers: { 'Content-Type': 'application/json' }
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // 1. Refrescar el contenido visual del modal
                    actualizarInterfazCarrito();

                    // 2. Abrir el modal automáticamente
                    var myOffcanvas = document.getElementById('cartCanvas');
                    var bsOffcanvas = bootstrap.Offcanvas.getInstance(myOffcanvas) || new bootstrap.Offcanvas(myOffcanvas);
                    bsOffcanvas.show();
                }
            });
    }

    function actualizarInterfazCarrito() {
        const container = document.getElementById('cartItemsContainer');

    // Llamada AJAX para obtener el HTML de los productos (Partial View)
    fetch('/Carrito/ObtenerResumen')
            .then(res => res.text())
            .then(html => {
        container.innerHTML = html;
    actualizarBadgeSolo(); // Actualizar el numerito del icono
            });
    }

    function actualizarBadgeSolo() {
        fetch('/Carrito/ObtenerInfoBasica')
            .then(res => res.json())
            .then(data => {
                const badge = document.getElementById('cartCountBadge');
                const totalLabel = document.getElementById('cartTotalLabel');

                if (badge && data.count > 0) {
                    badge.innerText = data.count;
                    badge.style.display = 'block';
                } else if (badge) {
                    badge.style.display = 'none';
                }

                if (totalLabel) {
                    totalLabel.innerText = 'C$ ' + data.total.toLocaleString('es-NI', { minimumFractionDigits: 2 });
                }
            });
    }
</script>