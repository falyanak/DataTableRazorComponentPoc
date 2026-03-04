import { initDataTable } from "../../Rcl.DataTable/src/main";
import { ThemeManager } from "./ThemeManager";
import { ProductUpdatePayload } from "./types/events";

declare global {
  interface Window {
    themeManager: ThemeManager;
  }
}

const AppManager = {
  init(): void {
    // 1. Initialisation de l'instance globale
    window.themeManager = new ThemeManager();

    // 2. Initialisation de la DataTable
    initDataTable();

    // 3. Délégation d'événements UNIQUE (Compatible CSP)
    document.addEventListener("click", (event: MouseEvent) => {
      const target = event.target as HTMLElement;

      // --- LOGIQUE THEME ---
      // On cherche si on a cliqué sur le bouton ou une icône à l'intérieur
      // On cherche le bouton ou n'importe quel parent qui est le bouton de thème
      const themeBtn = target.closest("#theme-toggle");

      if (themeBtn) {
        console.log("-> Clic détecté sur le bouton toggle");
        event.preventDefault(); // Empêche tout comportement par défaut
        window.themeManager.toggle();
      }

      // --- LOGIQUE ONGLETS ---
      const tab = target.closest("#productTabs .nav-link") as HTMLElement;
      if (tab) {
        this.handleTabClick(tab);
        return;
      }

      // ... reste de ta logique de suppression ...
    });

    // --- DANS AppManager.init() ---

    document.addEventListener("productUpdated", (event: any) => {
      // Note : HTMX met les données dans event.detail
      const data = event.detail as ProductUpdatePayload;

      if (data) {
        // 1. Badge Global (Layout/Index)
        const mainBadge = document.getElementById("main-product-badge");
        if (mainBadge) {
          mainBadge.textContent = `${data.total} items`;
        }

        // 2. Badge de la DataTable (RCL)
        const searchBadgeCount = document.querySelector(
          ".search-result-count strong",
        );
        if (searchBadgeCount) {
          searchBadgeCount.textContent = data.filtered.toLocaleString("fr-FR");
        }
      }
    });

    console.log("AppManager : Initialisé (ThemeManager prêt)");
  },

  handleTabClick(tab: HTMLElement): void {
    const container = tab.closest("#productTabs");
    if (container) {
      container
        .querySelectorAll(".nav-link")
        .forEach((el) => el.classList.remove("active"));
      tab.classList.add("active");
    }
  },
};

document.addEventListener("DOMContentLoaded", () => AppManager.init());
