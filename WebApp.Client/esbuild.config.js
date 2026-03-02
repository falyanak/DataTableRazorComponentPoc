const esbuild = require('esbuild');

async function run() {
    let ctx = await esbuild.context({
        entryPoints: ['src/main.ts'],
        bundle: true,
        minify: true,
        sourcemap: true,
        outfile: 'wwwroot/dist/main.bundle.js',
        platform: 'browser',
        target: ['es2020'],
    });

    await ctx.watch();
    console.log("👀 Surveillance active... (Ctrl+C pour arrêter)");
}

run().catch(() => process.exit(1));