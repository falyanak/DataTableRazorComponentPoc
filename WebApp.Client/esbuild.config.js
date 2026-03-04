const esbuild = require('esbuild');

async function run() {
    const isWatch = process.argv.includes('--watch');

    const options = {
        // On définit les deux points d'entrée
        entryPoints: {
            'main.bundle': 'src/main.ts',
            'theme-init': 'src/theme-init.ts'
        },
        bundle: true,
        minify: true,
        sourcemap: true, // Recommandé pour le débugging TypeScript
        // On utilise outdir au lieu de outfile pour gérer plusieurs fichiers
        outdir: 'wwwroot/dist', 
        platform: 'browser',
        target: ['es2020'],
        format: 'iife', // Format immédiat pour theme-init (essentiel pour le <head>)
    };

    if (isWatch) {
        console.log("👀 Mode Watch activé...");
        let ctx = await esbuild.context(options);
        await ctx.watch();
    } else {
        await esbuild.build(options);
        console.log("🚀 Bundles générés dans wwwroot/dist/ :");
        console.log("   - main.bundle.js");
        console.log("   - theme-init.js");
        process.exit(0); 
    }
}

run().catch((err) => {
    console.error("❌ Erreur de build:", err);
    process.exit(1);
});