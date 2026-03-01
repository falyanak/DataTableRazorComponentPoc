document.addEventListener('htmx:afterSwap', (event: any) => {
    // Si le contenu injecté contient notre conteneur de tableau
    if (event.detail.target.id.includes('dt-') || event.detail.target.classList.contains('dt-container')) {
        const selected = document.querySelector('.table-active');
        if (selected) {
            // Repositionne la vue sur la ligne mémorisée
            selected.scrollIntoView({ block: 'center', behavior: 'smooth' });
        }
    }
});