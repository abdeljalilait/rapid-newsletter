import { e as createComponent, k as renderComponent, r as renderTemplate, m as maybeRenderHead } from '../chunks/astro/server_ynwNkiFT.mjs';
import { $ as $$BaseLayout } from '../chunks/BaseLayout_Zn6j3HLV.mjs';
export { renderers } from '../renderers.mjs';

const $$Index = createComponent(($$result, $$props, $$slots) => {
  return renderTemplate`${renderComponent($$result, "BaseLayout", $$BaseLayout, { "title": "Rapid Newsletter" }, { "default": ($$result2) => renderTemplate` ${maybeRenderHead()}<main class="container" id="newsletters" style="padding: 72px 0;"> <h1 style="font-size: clamp(42px, 8vw, 88px); margin: 0 0 16px;">Newsletter directory</h1> <p class="muted" style="max-width: 640px;">Open a workspace by URL, for example <code>/dotnet-weekly</code>.</p> </main> ` })}`;
}, "/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/pages/index.astro", void 0);

const $$file = "/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/pages/index.astro";
const $$url = "";

const _page = /*#__PURE__*/Object.freeze(/*#__PURE__*/Object.defineProperty({
  __proto__: null,
  default: $$Index,
  file: $$file,
  url: $$url
}, Symbol.toStringTag, { value: 'Module' }));

const page = () => _page;

export { page };
