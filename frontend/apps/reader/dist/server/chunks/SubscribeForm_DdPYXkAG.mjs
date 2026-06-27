import { e as createComponent, m as maybeRenderHead, g as addAttribute, l as renderScript, r as renderTemplate, h as createAstro } from './astro/server_ynwNkiFT.mjs';

const $$Astro = createAstro();
const $$SubscribeForm = createComponent(async ($$result, $$props, $$slots) => {
  const Astro2 = $$result.createAstro($$Astro, $$props, $$slots);
  Astro2.self = $$SubscribeForm;
  const { slug } = Astro2.props;
  return renderTemplate`${maybeRenderHead()}<form class="subscribe-form" data-subscribe-form${addAttribute(slug, "data-slug")}> <div style="display: flex; gap: 8px; grid-column: 1 / -1;"> <input name="firstName" type="text" placeholder="First name" aria-label="First name" style="flex: 1;"> <input name="lastName" type="text" placeholder="Last name" aria-label="Last name" style="flex: 1;"> </div> <input name="email" type="email" required placeholder="reader@example.com" aria-label="Email"> <button type="submit">Subscribe</button> <p class="muted" data-message style="grid-column: 1 / -1; margin: 0;"></p> </form> ${renderScript($$result, "/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/components/SubscribeForm.astro?astro&type=script&index=0&lang.ts")}`;
}, "/home/abdeljalil/projects/rapid-newsletter/frontend/apps/reader/src/components/SubscribeForm.astro", void 0);

export { $$SubscribeForm as $ };
