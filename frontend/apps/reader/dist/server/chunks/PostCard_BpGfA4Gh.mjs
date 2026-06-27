import { e as createComponent, m as maybeRenderHead, g as addAttribute, r as renderTemplate, h as createAstro } from './astro/server_ynwNkiFT.mjs';

const $$Astro = createAstro();
const $$PostCard = createComponent(($$result, $$props, $$slots) => {
  const Astro2 = $$result.createAstro($$Astro, $$props, $$slots);
  Astro2.self = $$PostCard;
  const { slug, post } = Astro2.props;
  return renderTemplate`${maybeRenderHead()}<a class="post-card"${addAttribute(`/${slug}/post/${post.id}`, "href")}> <p class="muted">${post.publishedAt ? new Date(post.publishedAt).toLocaleDateString() : "Draft"}</p> <h2>${post.title}</h2> ${post.previewText && renderTemplate`<p class="muted">${post.previewText}</p>`} </a>`;
}, "/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/components/PostCard.astro", void 0);

export { $$PostCard as $ };
