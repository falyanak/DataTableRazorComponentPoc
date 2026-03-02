import { initDataTable } from '../../Rcl.DataTable/src/main'; // Import du moteur de DataTable depuis le projet Rcl.DataTable
import { ThemeManager } from './ThemeManager';

// Déclaration pour l'accès global depuis le HTML (Boutons de thème)
declare global {
    interface Window { 
        themeManager: ThemeManager; 
    }
}

const AppManager = {
    init(): void {
        initDataTable(); // Initialisation du moteur de DataTable

        // Initialisation du gestionnaire de thèmes
        window.themeManager = new ThemeManager('theme-wrapper', 'client-theme-styles');   

        console.log("AppManager : Initialisé avec succès (Themes + Tabs)");
    },

    handleTabClick(tab: HTMLElement): void {
        const container = tab.closest('#productTabs');
        if (container) {
            container.querySelectorAll('.nav-link').forEach(el => el.classList.remove('active'));
            tab.classList.add('active');
        }
    }
};

// Lancement au chargement du DOM
document.addEventListener('DOMContentLoaded', () => AppManager.init());