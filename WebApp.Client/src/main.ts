const AppManager = {
    init(): void {
        document.addEventListener('click', (e: MouseEvent) => {
            const target = e.target as HTMLElement;

            // GESTION DES ONGLETS (Purement visuel, pas de data)
            const tab = target.closest('#productTabs .nav-link');
            if (tab) {
                this.handleTabClick(tab as HTMLElement);
            }
        });
    },

    handleTabClick(tab: HTMLElement): void {
        const container = tab.closest('#productTabs');
        if (container) {
            container.querySelectorAll('.nav-link').forEach(el => el.classList.remove('active'));
            tab.classList.add('active');
        }
    }
};

document.addEventListener('DOMContentLoaded', () => AppManager.init());