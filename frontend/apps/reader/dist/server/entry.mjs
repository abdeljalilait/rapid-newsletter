import { renderers } from './renderers.mjs';
import { c as createExports, s as serverEntrypointModule } from './chunks/_@astrojs-ssr-adapter_BAFOM1W2.mjs';
import { manifest } from './manifest_T_cYq72c.mjs';

const serverIslandMap = new Map();;

const _page0 = () => import('./pages/_image.astro.mjs');
const _page1 = () => import('./pages/_slug_/archive.astro.mjs');
const _page2 = () => import('./pages/_slug_/post/_id_.astro.mjs');
const _page3 = () => import('./pages/_slug_.astro.mjs');
const _page4 = () => import('./pages/index.astro.mjs');
const pageMap = new Map([
    ["../../node_modules/.pnpm/astro@5.18.2_rollup@4.62.2_typescript@5.9.3_yaml@2.9.0/node_modules/astro/dist/assets/endpoint/node.js", _page0],
    ["src/pages/[slug]/archive.astro", _page1],
    ["src/pages/[slug]/post/[id].astro", _page2],
    ["src/pages/[slug]/index.astro", _page3],
    ["src/pages/index.astro", _page4]
]);

const _manifest = Object.assign(manifest, {
    pageMap,
    serverIslandMap,
    renderers,
    actions: () => import('./noop-entrypoint.mjs'),
    middleware: () => import('./_noop-middleware.mjs')
});
const _args = {
    "mode": "standalone",
    "client": "file:///home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/dist/client/",
    "server": "file:///home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/dist/server/",
    "host": "127.0.0.1",
    "port": 4321,
    "assets": "_astro",
    "experimentalStaticHeaders": false
};
const _exports = createExports(_manifest, _args);
const handler = _exports['handler'];
const startServer = _exports['startServer'];
const options = _exports['options'];
const _start = 'start';
if (Object.prototype.hasOwnProperty.call(serverEntrypointModule, _start)) {
	serverEntrypointModule[_start](_manifest, _args);
}

export { handler, options, pageMap, startServer };
