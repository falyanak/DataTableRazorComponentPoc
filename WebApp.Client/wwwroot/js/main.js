const AppManager = {
    init() {
        document.addEventListener('click', (e) => {
            const target = e.target;
            // GESTION DES ONGLETS (Purement visuel, pas de data)
            const tab = target.closest('#productTabs .nav-link');
            if (tab) {
                this.handleTabClick(tab);
            }
        });
    },
    handleTabClick(tab) {
        const container = tab.closest('#productTabs');
        if (container) {
            container.querySelectorAll('.nav-link').forEach(el => el.classList.remove('active'));
            tab.classList.add('active');
        }
    }
};
document.addEventListener('DOMContentLoaded', () => AppManager.init());
