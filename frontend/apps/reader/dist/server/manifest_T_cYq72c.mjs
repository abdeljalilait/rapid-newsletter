import { p as decodeKey } from './chunks/astro/server_ynwNkiFT.mjs';
import { N as NOOP_MIDDLEWARE_FN } from './chunks/astro-designed-error-pages_o73DDEh2.mjs';

function sanitizeParams(params) {
  return Object.fromEntries(
    Object.entries(params).map(([key, value]) => {
      if (typeof value === "string") {
        return [key, value.normalize().replace(/#/g, "%23").replace(/\?/g, "%3F")];
      }
      return [key, value];
    })
  );
}
function getParameter(part, params) {
  if (part.spread) {
    return params[part.content.slice(3)] || "";
  }
  if (part.dynamic) {
    if (!params[part.content]) {
      throw new TypeError(`Missing parameter: ${part.content}`);
    }
    return params[part.content];
  }
  return part.content.normalize().replace(/\?/g, "%3F").replace(/#/g, "%23").replace(/%5B/g, "[").replace(/%5D/g, "]");
}
function getSegment(segment, params) {
  const segmentPath = segment.map((part) => getParameter(part, params)).join("");
  return segmentPath ? "/" + segmentPath : "";
}
function getRouteGenerator(segments, addTrailingSlash) {
  return (params) => {
    const sanitizedParams = sanitizeParams(params);
    let trailing = "";
    if (addTrailingSlash === "always" && segments.length) {
      trailing = "/";
    }
    const path = segments.map((segment) => getSegment(segment, sanitizedParams)).join("") + trailing;
    return path || "/";
  };
}

function deserializeRouteData(rawRouteData) {
  return {
    route: rawRouteData.route,
    type: rawRouteData.type,
    pattern: new RegExp(rawRouteData.pattern),
    params: rawRouteData.params,
    component: rawRouteData.component,
    generate: getRouteGenerator(rawRouteData.segments, rawRouteData._meta.trailingSlash),
    pathname: rawRouteData.pathname || void 0,
    segments: rawRouteData.segments,
    prerender: rawRouteData.prerender,
    redirect: rawRouteData.redirect,
    redirectRoute: rawRouteData.redirectRoute ? deserializeRouteData(rawRouteData.redirectRoute) : void 0,
    fallbackRoutes: rawRouteData.fallbackRoutes.map((fallback) => {
      return deserializeRouteData(fallback);
    }),
    isIndex: rawRouteData.isIndex,
    origin: rawRouteData.origin
  };
}

function deserializeManifest(serializedManifest) {
  const routes = [];
  for (const serializedRoute of serializedManifest.routes) {
    routes.push({
      ...serializedRoute,
      routeData: deserializeRouteData(serializedRoute.routeData)
    });
    const route = serializedRoute;
    route.routeData = deserializeRouteData(serializedRoute.routeData);
  }
  const assets = new Set(serializedManifest.assets);
  const componentMetadata = new Map(serializedManifest.componentMetadata);
  const inlinedScripts = new Map(serializedManifest.inlinedScripts);
  const clientDirectives = new Map(serializedManifest.clientDirectives);
  const serverIslandNameMap = new Map(serializedManifest.serverIslandNameMap);
  const key = decodeKey(serializedManifest.key);
  return {
    // in case user middleware exists, this no-op middleware will be reassigned (see plugin-ssr.ts)
    middleware() {
      return { onRequest: NOOP_MIDDLEWARE_FN };
    },
    ...serializedManifest,
    assets,
    componentMetadata,
    inlinedScripts,
    clientDirectives,
    routes,
    serverIslandNameMap,
    key
  };
}

const manifest = deserializeManifest({"hrefRoot":"file:///home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/","cacheDir":"file:///home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/node_modules/.astro/","outDir":"file:///home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/dist/","srcDir":"file:///home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/","publicDir":"file:///home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/public/","buildClientDir":"file:///home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/dist/client/","buildServerDir":"file:///home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/dist/server/","adapterName":"@astrojs/node","routes":[{"file":"","links":[],"scripts":[],"styles":[],"routeData":{"type":"page","component":"_server-islands.astro","params":["name"],"segments":[[{"content":"_server-islands","dynamic":false,"spread":false}],[{"content":"name","dynamic":true,"spread":false}]],"pattern":"^\\/_server-islands\\/([^/]+?)\\/?$","prerender":false,"isIndex":false,"fallbackRoutes":[],"route":"/_server-islands/[name]","origin":"internal","_meta":{"trailingSlash":"ignore"}}},{"file":"","links":[],"scripts":[],"styles":[],"routeData":{"type":"endpoint","isIndex":false,"route":"/_image","pattern":"^\\/_image\\/?$","segments":[[{"content":"_image","dynamic":false,"spread":false}]],"params":[],"component":"../../node_modules/.pnpm/astro@5.18.2_rollup@4.62.2_typescript@5.9.3_yaml@2.9.0/node_modules/astro/dist/assets/endpoint/node.js","pathname":"/_image","prerender":false,"fallbackRoutes":[],"origin":"internal","_meta":{"trailingSlash":"ignore"}}},{"file":"","links":[],"scripts":[],"styles":[{"type":"inline","content":":root{color-scheme:dark;--bg: #0f0e17;--panel: rgba(255, 255, 255, .07);--text: #faf8f5;--muted: #bdb7ae;--accent: #e6a817;--highlight: #ff8906;--border: rgba(250, 248, 245, .16);font-family:Inter,system-ui,sans-serif}body{margin:0;background:var(--bg);color:var(--text)}a{color:inherit}.container{width:min(1120px,calc(100% - 32px));margin:0 auto}.reader-nav,.reader-footer{border-bottom:1px solid var(--border);padding:18px 0}.reader-footer{border-top:1px solid var(--border);border-bottom:0;margin-top:72px;color:var(--muted)}.brand-row{display:flex;align-items:center;justify-content:space-between;gap:16px}.brand-title,h1,h2,h3{font-family:Playfair Display,Georgia,serif}.newsletter-header{padding:56px 0 32px}.newsletter-header h1{font-size:clamp(42px,7vw,82px);line-height:1;margin:0 0 18px}.muted{color:var(--muted)}.post-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(260px,1fr));gap:18px}.post-card{min-height:190px;padding:22px;border:1px solid var(--border);border-radius:8px;background:var(--panel);backdrop-filter:blur(12px);text-decoration:none;transition:transform .16s ease,border-color .16s ease}.post-card:hover{transform:translateY(-3px);border-color:var(--accent)}.article{width:min(780px,calc(100% - 32px));margin:56px auto;font-size:1.08rem;line-height:1.8}.cta-bar{position:sticky;bottom:16px;width:min(760px,calc(100% - 32px));margin:32px auto 0;border:1px solid var(--border);border-radius:8px;background:#0f0e17eb;padding:12px}.subscribe-form{display:grid;grid-template-columns:1fr auto;gap:8px}input,button{border-radius:6px;border:1px solid var(--border);padding:11px 12px;font:inherit}button{background:linear-gradient(135deg,var(--accent),var(--highlight));color:#120f09;font-weight:700;cursor:pointer}\n"}],"routeData":{"route":"/[slug]/archive","isIndex":false,"type":"page","pattern":"^\\/([^/]+?)\\/archive\\/?$","segments":[[{"content":"slug","dynamic":true,"spread":false}],[{"content":"archive","dynamic":false,"spread":false}]],"params":["slug"],"component":"src/pages/[slug]/archive.astro","prerender":false,"fallbackRoutes":[],"distURL":[],"origin":"project","_meta":{"trailingSlash":"ignore"}}},{"file":"","links":[],"scripts":[],"styles":[{"type":"inline","content":":root{color-scheme:dark;--bg: #0f0e17;--panel: rgba(255, 255, 255, .07);--text: #faf8f5;--muted: #bdb7ae;--accent: #e6a817;--highlight: #ff8906;--border: rgba(250, 248, 245, .16);font-family:Inter,system-ui,sans-serif}body{margin:0;background:var(--bg);color:var(--text)}a{color:inherit}.container{width:min(1120px,calc(100% - 32px));margin:0 auto}.reader-nav,.reader-footer{border-bottom:1px solid var(--border);padding:18px 0}.reader-footer{border-top:1px solid var(--border);border-bottom:0;margin-top:72px;color:var(--muted)}.brand-row{display:flex;align-items:center;justify-content:space-between;gap:16px}.brand-title,h1,h2,h3{font-family:Playfair Display,Georgia,serif}.newsletter-header{padding:56px 0 32px}.newsletter-header h1{font-size:clamp(42px,7vw,82px);line-height:1;margin:0 0 18px}.muted{color:var(--muted)}.post-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(260px,1fr));gap:18px}.post-card{min-height:190px;padding:22px;border:1px solid var(--border);border-radius:8px;background:var(--panel);backdrop-filter:blur(12px);text-decoration:none;transition:transform .16s ease,border-color .16s ease}.post-card:hover{transform:translateY(-3px);border-color:var(--accent)}.article{width:min(780px,calc(100% - 32px));margin:56px auto;font-size:1.08rem;line-height:1.8}.cta-bar{position:sticky;bottom:16px;width:min(760px,calc(100% - 32px));margin:32px auto 0;border:1px solid var(--border);border-radius:8px;background:#0f0e17eb;padding:12px}.subscribe-form{display:grid;grid-template-columns:1fr auto;gap:8px}input,button{border-radius:6px;border:1px solid var(--border);padding:11px 12px;font:inherit}button{background:linear-gradient(135deg,var(--accent),var(--highlight));color:#120f09;font-weight:700;cursor:pointer}\n"}],"routeData":{"route":"/[slug]/post/[id]","isIndex":false,"type":"page","pattern":"^\\/([^/]+?)\\/post\\/([^/]+?)\\/?$","segments":[[{"content":"slug","dynamic":true,"spread":false}],[{"content":"post","dynamic":false,"spread":false}],[{"content":"id","dynamic":true,"spread":false}]],"params":["slug","id"],"component":"src/pages/[slug]/post/[id].astro","prerender":false,"fallbackRoutes":[],"distURL":[],"origin":"project","_meta":{"trailingSlash":"ignore"}}},{"file":"","links":[],"scripts":[],"styles":[{"type":"inline","content":":root{color-scheme:dark;--bg: #0f0e17;--panel: rgba(255, 255, 255, .07);--text: #faf8f5;--muted: #bdb7ae;--accent: #e6a817;--highlight: #ff8906;--border: rgba(250, 248, 245, .16);font-family:Inter,system-ui,sans-serif}body{margin:0;background:var(--bg);color:var(--text)}a{color:inherit}.container{width:min(1120px,calc(100% - 32px));margin:0 auto}.reader-nav,.reader-footer{border-bottom:1px solid var(--border);padding:18px 0}.reader-footer{border-top:1px solid var(--border);border-bottom:0;margin-top:72px;color:var(--muted)}.brand-row{display:flex;align-items:center;justify-content:space-between;gap:16px}.brand-title,h1,h2,h3{font-family:Playfair Display,Georgia,serif}.newsletter-header{padding:56px 0 32px}.newsletter-header h1{font-size:clamp(42px,7vw,82px);line-height:1;margin:0 0 18px}.muted{color:var(--muted)}.post-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(260px,1fr));gap:18px}.post-card{min-height:190px;padding:22px;border:1px solid var(--border);border-radius:8px;background:var(--panel);backdrop-filter:blur(12px);text-decoration:none;transition:transform .16s ease,border-color .16s ease}.post-card:hover{transform:translateY(-3px);border-color:var(--accent)}.article{width:min(780px,calc(100% - 32px));margin:56px auto;font-size:1.08rem;line-height:1.8}.cta-bar{position:sticky;bottom:16px;width:min(760px,calc(100% - 32px));margin:32px auto 0;border:1px solid var(--border);border-radius:8px;background:#0f0e17eb;padding:12px}.subscribe-form{display:grid;grid-template-columns:1fr auto;gap:8px}input,button{border-radius:6px;border:1px solid var(--border);padding:11px 12px;font:inherit}button{background:linear-gradient(135deg,var(--accent),var(--highlight));color:#120f09;font-weight:700;cursor:pointer}\n"}],"routeData":{"route":"/[slug]","isIndex":true,"type":"page","pattern":"^\\/([^/]+?)\\/?$","segments":[[{"content":"slug","dynamic":true,"spread":false}]],"params":["slug"],"component":"src/pages/[slug]/index.astro","prerender":false,"fallbackRoutes":[],"distURL":[],"origin":"project","_meta":{"trailingSlash":"ignore"}}},{"file":"","links":[],"scripts":[],"styles":[{"type":"inline","content":":root{color-scheme:dark;--bg: #0f0e17;--panel: rgba(255, 255, 255, .07);--text: #faf8f5;--muted: #bdb7ae;--accent: #e6a817;--highlight: #ff8906;--border: rgba(250, 248, 245, .16);font-family:Inter,system-ui,sans-serif}body{margin:0;background:var(--bg);color:var(--text)}a{color:inherit}.container{width:min(1120px,calc(100% - 32px));margin:0 auto}.reader-nav,.reader-footer{border-bottom:1px solid var(--border);padding:18px 0}.reader-footer{border-top:1px solid var(--border);border-bottom:0;margin-top:72px;color:var(--muted)}.brand-row{display:flex;align-items:center;justify-content:space-between;gap:16px}.brand-title,h1,h2,h3{font-family:Playfair Display,Georgia,serif}.newsletter-header{padding:56px 0 32px}.newsletter-header h1{font-size:clamp(42px,7vw,82px);line-height:1;margin:0 0 18px}.muted{color:var(--muted)}.post-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(260px,1fr));gap:18px}.post-card{min-height:190px;padding:22px;border:1px solid var(--border);border-radius:8px;background:var(--panel);backdrop-filter:blur(12px);text-decoration:none;transition:transform .16s ease,border-color .16s ease}.post-card:hover{transform:translateY(-3px);border-color:var(--accent)}.article{width:min(780px,calc(100% - 32px));margin:56px auto;font-size:1.08rem;line-height:1.8}.cta-bar{position:sticky;bottom:16px;width:min(760px,calc(100% - 32px));margin:32px auto 0;border:1px solid var(--border);border-radius:8px;background:#0f0e17eb;padding:12px}.subscribe-form{display:grid;grid-template-columns:1fr auto;gap:8px}input,button{border-radius:6px;border:1px solid var(--border);padding:11px 12px;font:inherit}button{background:linear-gradient(135deg,var(--accent),var(--highlight));color:#120f09;font-weight:700;cursor:pointer}\n"}],"routeData":{"route":"/","isIndex":true,"type":"page","pattern":"^\\/$","segments":[],"params":[],"component":"src/pages/index.astro","pathname":"/","prerender":false,"fallbackRoutes":[],"distURL":[],"origin":"project","_meta":{"trailingSlash":"ignore"}}}],"base":"/","trailingSlash":"ignore","compressHTML":true,"componentMetadata":[["/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/pages/[slug]/archive.astro",{"propagation":"none","containsHead":true}],["/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/pages/[slug]/index.astro",{"propagation":"none","containsHead":true}],["/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/pages/[slug]/post/[id].astro",{"propagation":"none","containsHead":true}],["/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/pages/index.astro",{"propagation":"none","containsHead":true}]],"renderers":[],"clientDirectives":[["idle","(()=>{var l=(n,t)=>{let i=async()=>{await(await n())()},e=typeof t.value==\"object\"?t.value:void 0,s={timeout:e==null?void 0:e.timeout};\"requestIdleCallback\"in window?window.requestIdleCallback(i,s):setTimeout(i,s.timeout||200)};(self.Astro||(self.Astro={})).idle=l;window.dispatchEvent(new Event(\"astro:idle\"));})();"],["load","(()=>{var e=async t=>{await(await t())()};(self.Astro||(self.Astro={})).load=e;window.dispatchEvent(new Event(\"astro:load\"));})();"],["media","(()=>{var n=(a,t)=>{let i=async()=>{await(await a())()};if(t.value){let e=matchMedia(t.value);e.matches?i():e.addEventListener(\"change\",i,{once:!0})}};(self.Astro||(self.Astro={})).media=n;window.dispatchEvent(new Event(\"astro:media\"));})();"],["only","(()=>{var e=async t=>{await(await t())()};(self.Astro||(self.Astro={})).only=e;window.dispatchEvent(new Event(\"astro:only\"));})();"],["visible","(()=>{var a=(s,i,o)=>{let r=async()=>{await(await s())()},t=typeof i.value==\"object\"?i.value:void 0,c={rootMargin:t==null?void 0:t.rootMargin},n=new IntersectionObserver(e=>{for(let l of e)if(l.isIntersecting){n.disconnect(),r();break}},c);for(let e of o.children)n.observe(e)};(self.Astro||(self.Astro={})).visible=a;window.dispatchEvent(new Event(\"astro:visible\"));})();"]],"entryModules":{"\u0000@astro-page:src/pages/[slug]/archive@_@astro":"pages/_slug_/archive.astro.mjs","\u0000@astro-page:src/pages/[slug]/index@_@astro":"pages/_slug_.astro.mjs","\u0000@astro-page:src/pages/[slug]/post/[id]@_@astro":"pages/_slug_/post/_id_.astro.mjs","\u0000@astro-page:src/pages/index@_@astro":"pages/index.astro.mjs","\u0000@astrojs-ssr-virtual-entry":"entry.mjs","\u0000@astro-renderers":"renderers.mjs","\u0000noop-middleware":"_noop-middleware.mjs","\u0000virtual:astro:actions/noop-entrypoint":"noop-entrypoint.mjs","\u0000@astro-page:../../node_modules/.pnpm/astro@5.18.2_rollup@4.62.2_typescript@5.9.3_yaml@2.9.0/node_modules/astro/dist/assets/endpoint/node@_@js":"pages/_image.astro.mjs","\u0000@astrojs-ssr-adapter":"_@astrojs-ssr-adapter.mjs","\u0000@astrojs-manifest":"manifest_T_cYq72c.mjs","/home/abdeljalil/projects/rapid-newsletter/frontend/node_modules/.pnpm/astro@5.18.2_rollup@4.62.2_typescript@5.9.3_yaml@2.9.0/node_modules/astro/dist/assets/services/sharp.js":"chunks/sharp_C5LktDFR.mjs","/home/abdeljalil/projects/rapid-newsletter/frontend/node_modules/.pnpm/unstorage@1.17.5/node_modules/unstorage/drivers/fs-lite.mjs":"chunks/fs-lite_COtHaKzy.mjs","/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/components/SubscribeForm.astro?astro&type=script&index=0&lang.ts":"_astro/SubscribeForm.astro_astro_type_script_index_0_lang.DqLZdmwp.js","astro:scripts/before-hydration.js":""},"inlinedScripts":[["/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/components/SubscribeForm.astro?astro&type=script&index=0&lang.ts","document.querySelectorAll(\"[data-subscribe-form]\").forEach(r=>{r.addEventListener(\"submit\",async a=>{a.preventDefault();const e=a.currentTarget;if(!(e instanceof HTMLFormElement))return;const s=e.querySelector(\"[data-message]\"),t=new FormData(e),n={email:t.get(\"email\"),firstName:t.get(\"firstName\")||null,lastName:t.get(\"lastName\")||null},o=await fetch(`http://localhost:5120/api/public/${e.dataset.slug}/subscribe`,{method:\"POST\",headers:{\"Content-Type\":\"application/json\"},body:JSON.stringify(n)});s&&(s.textContent=o.ok?\"You're subscribed!\":\"Subscription failed. Try again.\")})});"]],"assets":[],"buildFormat":"directory","checkOrigin":true,"allowedDomains":[],"actionBodySizeLimit":1048576,"serverIslandNameMap":[],"key":"uTQYZHCgQq/E+qGnd0CWp2cU4lx09KAdli1UWjgA+Ew=","sessionConfig":{"driver":"fs-lite","options":{"base":"/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/node_modules/.astro/sessions"}}});
if (manifest.sessionConfig) manifest.sessionConfig.driverModule = () => import('./chunks/fs-lite_COtHaKzy.mjs');

export { manifest };
