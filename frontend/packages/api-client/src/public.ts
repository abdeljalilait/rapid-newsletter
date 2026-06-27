import { ApiClient, jsonBody } from "./client";
import type { PlanDto, PostDto, PublicPostDetailDto, PublicUnsubscribeDto, PublicWorkspaceDto, SubscriberDto } from "./types";

export const publicApi = (client: ApiClient) => ({
  workspace: (slug: string) => client.request<PublicWorkspaceDto>(`/api/public/${slug}`, { auth: false }),
  plans: (slug: string) => client.request<PlanDto[]>(`/api/public/${slug}/plans`, { auth: false }),
  posts: (slug: string) => client.request<PostDto[]>(`/api/public/${slug}/posts`, { auth: false }),
  post: (slug: string, postId: string) => client.request<PublicPostDetailDto>(`/api/public/${slug}/posts/${postId}`, { auth: false }),
  subscribe: (slug: string, request: { email: string; firstName?: string | null; lastName?: string | null }) =>
    client.request<SubscriberDto>(`/api/public/${slug}/subscribe`, {
      method: "POST",
      auth: false,
      body: jsonBody(request)
    }),
  unsubscribe: (slug: string, request: { email: string }) =>
    client.request<PublicUnsubscribeDto>(`/api/public/${slug}/unsubscribe`, {
      method: "POST",
      auth: false,
      body: jsonBody(request)
    })
});
