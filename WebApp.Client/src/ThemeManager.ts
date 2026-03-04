export class ThemeManager {
    public applyTheme(theme: 'theme-light' | 'theme-dark'): void {
        const root = document.documentElement;
        
        // On nettoie et on applique
        root.classList.remove('theme-light', 'theme-dark');
        root.classList.add(theme);
        
        localStorage.setItem('app-theme', theme);
        console.log(`[ThemeManager] Classe active sur HTML : ${root.className}`);
    }

    public toggle(): void {
        const root = document.documentElement;
        // On vérifie spécifiquement la présence de la classe
        const isDark = root.classList.contains('theme-dark');
        const next = isDark ? 'theme-light' : 'theme-dark';
        
        this.applyTheme(next);
    }
}