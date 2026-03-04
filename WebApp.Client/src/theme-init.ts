((): void => {
    const savedTheme = localStorage.getItem('app-theme') || 'theme-light';
    // On applique sur <html> (document.documentElement)
    document.documentElement.classList.add(savedTheme);
})();