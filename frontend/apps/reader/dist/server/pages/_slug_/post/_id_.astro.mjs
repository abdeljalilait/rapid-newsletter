import { e as createComponent, k as renderComponent, r as renderTemplate, h as createAstro, m as maybeRenderHead, g as addAttribute, u as unescapeHTML } from '../../../chunks/astro/server_ynwNkiFT.mjs';
import { $ as $$SubscribeForm } from '../../../chunks/SubscribeForm_DdPYXkAG.mjs';
import { $ as $$BaseLayout } from '../../../chunks/BaseLayout_Zn6j3HLV.mjs';
import { g as getWorkspace, c as getPost } from '../../../chunks/api_QzgRt07h.mjs';
export { renderers } from '../../../renderers.mjs';

const $$Astro = createAstro();
const $$id = createComponent(async ($$result, $$props, $$slots) => {
  const Astro2 = $$result.createAstro($$Astro, $$props, $$slots);
  Astro2.self = $$id;
  const slug = Astro2.params.slug;
  const id = Astro2.params.id;
  const [workspace, post] = await Promise.all([getWorkspace(slug), getPost(slug, id)]);
  if (!workspace) return Astro2.redirect("/");
  if (!post) return Astro2.redirect(`/${slug}/archive`);
  return renderTemplate`${renderComponent($$result, "BaseLayout", $$BaseLayout, { "title": `${post.title} \xB7 ${workspace.name}` }, { "default": async ($$result2) => renderTemplate` ${maybeRenderHead()}<article class="article"> <p class="muted"><a${addAttribute(`/${slug}`, "href")}>${workspace.name}</a></p> <h1>${post.title}</h1> ${post.subtitle && renderTemplate`<p class="muted">${post.subtitle}</p>`} ${post.coverImageUrl && renderTemplate`<img${addAttribute(post.coverImageUrl, "src")} alt="" style="width: 100%; border-radius: 8px; margin: 24px 0;">`} ${post.renderedHtml ? renderTemplate`<div>${unescapeHTML(post.renderedHtml)}</div>` : renderTemplate`<p>${post.previewText || "No content available."}</p>`} ${post.publishedAt && renderTemplate`<p class="muted" style="margin-top: 32px;">
Published ${new Date(post.publishedAt).toLocaleDateString("en-US", { year: "numeric", month: "long", day: "numeric" })} </p>`} </article> <aside class="cta-bar"> ${renderComponent($$result2, "SubscribeForm", $$SubscribeForm, { "slug": slug })} </aside> ` })}`;
}, "/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/pages/[slug]/post/[id].astro", void 0);

const $$file = "/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/pages/[slug]/post/[id].astro";
const $$url = "/[slug]/post/[id]";

const _page = /*#__PURE__*/Object.freeze(/*#__PURE__*/Object.defineProperty({
  __proto__: null,
  default: $$id,
  file: $$file,
  url: $$url
}, Symbol.toStringTag, { value: 'Module' }));

const page = () => _page;

export { page };
