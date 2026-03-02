export function initDataTable(): void {
    // Gestion autonome du scroll après swap HTMX
    document.addEventListener('htmx:afterSwap', (event: any) => {
        const target = event.detail.target as HTMLElement;
        if (target.id.includes('dt-') || target.classList.contains('dt-container')) {
            const selected = target.querySelector('.table-active');
            if (selected) {
                selected.scrollIntoView({ block: 'center', behavior: 'smooth' });
            }
        }
    });

    console.log("Rcl.DataTable : Moteur initialisé.");
}