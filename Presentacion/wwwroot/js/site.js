<script>
    document.addEventListener("DOMContentLoaded", function() {
        const sidebarToggle = document.body.querySelector('#sidebarToggle');
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', event => {
            event.preventDefault();
            document.body.classList.toggle('sidebar-toggled');
            document.querySelector('.sidebar').classList.toggle('toggled');

            // Cambiar el ícono del botón
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
    });
</script>