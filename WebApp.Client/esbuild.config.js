const esbuild = require('esbuild');

async function run() {
    const isWatch = process.argv.includes('--watch');

    const options = {
        entryPoints: ['src/main.ts'],
        bundle: true,
        minify: true,
        outfile: 'wwwroot/dist/main.bundle.js',
        platform: 'browser',
        target: ['es2020'],
    };

    if (isWatch) {
        let ctx = await esbuild.context(options);
        await ctx.watch();
    } else {
        // On utilise build() qui est une promesse qui se termine
        await esbuild.build(options);
        console.log("🚀 Bundle généré. Fin du processus.");
        process.exit(0); // <--- FORCE LA SORTIE POUR LIBÉRER VS CODE
    }
}

run().catch(() => process.exit(1));