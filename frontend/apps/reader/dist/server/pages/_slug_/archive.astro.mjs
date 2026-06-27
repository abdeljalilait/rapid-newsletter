import { e as createComponent, k as renderComponent, r as renderTemplate, h as createAstro, m as maybeRenderHead } from '../../chunks/astro/server_ynwNkiFT.mjs';
import { $ as $$PostCard } from '../../chunks/PostCard_BpGfA4Gh.mjs';
import { $ as $$BaseLayout } from '../../chunks/BaseLayout_Zn6j3HLV.mjs';
import { g as getWorkspace, a as getPosts } from '../../chunks/api_QzgRt07h.mjs';
export { renderers } from '../../renderers.mjs';

const $$Astro = createAstro();
const $$Archive = createComponent(async ($$result, $$props, $$slots) => {
  const Astro2 = $$result.createAstro($$Astro, $$props, $$slots);
  Astro2.self = $$Archive;
  const slug = Astro2.params.slug;
  const [workspace, posts] = await Promise.all([getWorkspace(slug), getPosts(slug)]);
  if (!workspace) return Astro2.redirect("/");
  return renderTemplate`${renderComponent($$result, "BaseLayout", $$BaseLayout, { "title": `${workspace.name} archive` }, { "default": async ($$result2) => renderTemplate` ${maybeRenderHead()}<main class="container" style="padding: 56px 0;"> <h1>${workspace.name} archive</h1> <div class="post-grid"> ${posts.map((post) => renderTemplate`${renderComponent($$result2, "PostCard", $$PostCard, { "slug": slug, "post": post })}`)} </div> </main> ` })}`;
}, "/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/pages/[slug]/archive.astro", void 0);

const $$file = "/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/pages/[slug]/archive.astro";
const $$url = "/[slug]/archive";

const _page = /*#__PURE__*/Object.freeze(/*#__PURE__*/Object.defineProperty({
  __proto__: null,
  default: $$Archive,
  file: $$file,
  url: $$url
}, Symbol.toStringTag, { value: 'Module' }));

const page = () => _page;

export { page };
