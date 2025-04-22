// Add dropdown toggle functionality
document.addEventListener('DOMContentLoaded', function() {
    const dropdowns = document.querySelectorAll('.dropdown');

    dropdowns.forEach(dropdown => {
        const button = dropdown.querySelector('button');
        const menu = dropdown.querySelector('.dropdown-menu');

        button.addEventListener('click', function() {
            menu.classList.toggle('hidden');
        });

        // Close when clicking outside
        document.addEventListener('click', function(event) {
            if (!dropdown.contains(event.target)) {
                menu.classList.add('hidden');
            }
        });
    });
});