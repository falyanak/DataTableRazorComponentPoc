export function initDataTable(): void {
  if ((window as any).__dataTableInitialized) return;

  document.addEventListener("htmx:afterSwap", (event: any) => {
    const target = event.detail.target as HTMLElement;
    const isDataTable = target.id.includes("dt-") || target.classList.contains("dt-container");

    if (isDataTable) {
      // 1. RESTAURATION DU FOCUS (Nouveau)
      // Si l'utilisateur était en train de taper dans la recherche, on lui redonne le focus
      const searchInput = target.querySelector('input[name="searchTerm"]') as HTMLInputElement;
      if (searchInput) {
        const val = searchInput.value;
        searchInput.focus();
        // Astuce pour placer le curseur à la fin du texte
        searchInput.value = '';
        searchInput.value = val;
      }

      // 2. SCROLL AUTOMATIQUE (Ton code existant)
      const selected = target.querySelector(".table-active");
      if (selected) {
        selected.scrollIntoView({ block: "center", behavior: "smooth" });
      }
    }
  });

  (window as any).__dataTableInitialized = true;
  console.log("Rcl.DataTable : Services d'infrastructure initialisés (Scroll + Focus).");
}