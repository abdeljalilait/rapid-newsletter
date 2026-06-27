import { e as createComponent, m as maybeRenderHead, r as renderTemplate, h as createAstro, k as renderComponent } from '../chunks/astro/server_ynwNkiFT.mjs';
import { $ as $$PostCard } from '../chunks/PostCard_BpGfA4Gh.mjs';
import { $ as $$SubscribeForm } from '../chunks/SubscribeForm_DdPYXkAG.mjs';
import { $ as $$BaseLayout } from '../chunks/BaseLayout_Zn6j3HLV.mjs';
import { g as getWorkspace, a as getPosts, b as getPlans } from '../chunks/api_QzgRt07h.mjs';
export { renderers } from '../renderers.mjs';

const $$Astro$1 = createAstro();
const $$NewsletterShell = createComponent(($$result, $$props, $$slots) => {
  const Astro2 = $$result.createAstro($$Astro$1, $$props, $$slots);
  Astro2.self = $$NewsletterShell;
  const { workspace } = Astro2.props;
  return renderTemplate`${maybeRenderHead()}<header class="newsletter-header"> <div class="container"> <p class="muted">/${workspace.slug}</p> <h1>${workspace.name}</h1> ${workspace.description && renderTemplate`<p class="muted" style="max-width: 680px; font-size: 1.15rem;">${workspace.description}</p>`} </div> </header>`;
}, "/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/components/NewsletterShell.astro", void 0);

const $$Astro = createAstro();
const $$Index = createComponent(async ($$result, $$props, $$slots) => {
  const Astro2 = $$result.createAstro($$Astro, $$props, $$slots);
  Astro2.self = $$Index;
  const slug = Astro2.params.slug;
  const [workspace, posts, plans] = await Promise.all([getWorkspace(slug), getPosts(slug), getPlans(slug)]);
  if (!workspace) return Astro2.redirect("/");
  return renderTemplate`${renderComponent($$result, "BaseLayout", $$BaseLayout, { "title": workspace.name }, { "default": async ($$result2) => renderTemplate` ${renderComponent($$result2, "NewsletterShell", $$NewsletterShell, { "workspace": workspace })} ${maybeRenderHead()}<main class="container"> <section class="cta-bar" style="position: static; width: auto; margin: 0 0 32px;"> ${renderComponent($$result2, "SubscribeForm", $$SubscribeForm, { "slug": slug })} </section> <section style="margin: 36px 0;"> <h2>Latest posts</h2> <div class="post-grid"> ${posts.map((post) => renderTemplate`${renderComponent($$result2, "PostCard", $$PostCard, { "slug": slug, "post": post })}`)} </div> </section> <section style="margin: 48px 0;"> <h2>Plans</h2> <div class="post-grid"> ${plans.map((plan) => renderTemplate`<article class="post-card"> <h3>${plan.name}</h3> <p class="muted">${plan.description}</p> <strong>${plan.currency} ${plan.price}</strong> </article>`)} </div> </section> </main> ` })}`;
}, "/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/pages/[slug]/index.astro", void 0);

const $$file = "/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/pages/[slug]/index.astro";
const $$url = "/[slug]";

const _page = /*#__PURE__*/Object.freeze(/*#__PURE__*/Object.defineProperty({
  __proto__: null,
  default: $$Index,
  file: $$file,
  url: $$url
}, Symbol.toStringTag, { value: 'Module' }));

const page = () => _page;

export { page };
