import { initDataTable } from '../../Rcl.DataTable/src/main'; 
import { ThemeManager } from './ThemeManager';

declare global {
    interface Window { 
        themeManager: ThemeManager; 
    }
}

const AppManager = {
    init(): void {
        // 1. On initialise la structure technique de la DataTable (Scroll HTMX, etc.)
        initDataTable(); 

        // 2. Thèmes
        window.themeManager = new ThemeManager('theme-wrapper', 'client-theme-styles');   

        // 3. UNIQUE LISTENER pour toute l'application (Délégation)
        document.addEventListener('click', (event: MouseEvent) => {
            const target = event.target as HTMLElement;

            // --- LOGIQUE DE SUPPRESSION (MÉTIER) ---
            // Le client sait qu'il utilise la classe '.dt-form-delete'
            const deleteBtn = target.closest('.dt-form-delete button[type="submit"]');
            if (deleteBtn) {
                const message = deleteBtn.getAttribute('data-confirm');
                if (message && !confirm(message)) {
                    event.preventDefault();
                    event.stopPropagation();
                    return; // On s'arrête là
                }
            }

            // --- LOGIQUE DES ONGLETS (MÉTIER) ---
            const tab = target.closest('#productTabs .nav-link') as HTMLElement;
            if (tab) {
                this.handleTabClick(tab);
                return;
            }
        });

        console.log("AppManager : Initialisé (Logique métier centralisée)");
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
