import { e as createComponent, n as renderHead, o as renderSlot, r as renderTemplate, h as createAstro } from './astro/server_ynwNkiFT.mjs';
/* empty css                           */

const $$Astro = createAstro();
const $$BaseLayout = createComponent(($$result, $$props, $$slots) => {
  const Astro2 = $$result.createAstro($$Astro, $$props, $$slots);
  Astro2.self = $$BaseLayout;
  const { title } = Astro2.props;
  return renderTemplate`<html lang="en"> <head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>${title}</title><link rel="preconnect" href="https://fonts.googleapis.com"><link rel="preconnect" href="https://fonts.gstatic.com" crossorigin><link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;700&family=Playfair+Display:wght@700&display=swap" rel="stylesheet">${renderHead()}</head> <body> <nav class="reader-nav"> <div class="container brand-row"> <a class="brand-title" href="/">Rapid Newsletter</a> <a href="/#newsletters">Directory</a> </div> </nav> ${renderSlot($$result, $$slots["default"])} <footer class="reader-footer"> <div class="container">Published with Rapid Newsletter.</div> </footer> </body></html>`;
}, "/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/layouts/BaseLayout.astro", void 0);

export { $$BaseLayout as $ };
