export class ThemeManager {
    private themeWrapper: HTMLElement | null;
    private styleTag: HTMLStyleElement | null;

    constructor(wrapperId: string, styleTagId: string) {
        this.themeWrapper = document.getElementById(wrapperId);
        this.styleTag = document.getElementById(styleTagId) as HTMLStyleElement;
        
        // Optionnel : Restaurer le thème précédent au démarrage
        const saved = localStorage.getItem('dt_theme');
        if (saved) this.applyTheme(saved);
    }

    public applyTheme(themeName: string): void {
        if (!this.themeWrapper || !this.styleTag) return;
        this.themeWrapper.className = themeName;
        this.styleTag.innerHTML = this.getThemeCSS(themeName);
        localStorage.setItem('dt_theme', themeName);
    }

    private getThemeCSS(theme: string): string {
        switch (theme) {
            case 'theme-dark-modern':
                return `.theme-dark-modern .dt-container { --dt-primary: #38bdf8; --dt-bg-header: #1e293b; --dt-text-header: #f1f5f9; --dt-footer-bg: #0f172a; --dt-border-color: #334155; }`;
            case 'theme-luxury':
                return `.theme-luxury .dt-container { --dt-primary: #b8860b; --dt-bg-header: #fff; --dt-radius: 1.5rem; }`;
            case 'theme-cyber':
                return `.theme-cyber .dt-container { --dt-primary: #00ff41; --dt-bg-header: #000; --dt-border-color: #00ff41; --dt-radius: 0; }`;
            default: return '';
        }
    }
}