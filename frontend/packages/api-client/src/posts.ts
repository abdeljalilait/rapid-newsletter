import { ApiClient, jsonBody } from "./client";
import type { PostAudience, PostDto, PostStatus } from "./types";

export interface PostRequest {
  title: string;
  slug?: string | null;
  subtitle?: string | null;
  previewText?: string | null;
  coverImageUrl?: string | null;
  editorContentJson: string;
  renderedHtml: string;
  plainText: string;
  audience: PostAudience;
  status: PostStatus;
  publishOnWebsite: boolean;
  sendByEmail: boolean;
  scheduledAt?: string | null;
}

export const postsApi = (client: ApiClient, workspaceId: string) => ({
  list: () => client.request<PostDto[]>(`/api/workspaces/${workspaceId}/posts`),
  create: (request: PostRequest) =>
    client.request<PostDto>(`/api/workspaces/${workspaceId}/posts`, { method: "POST", body: jsonBody(request) })
});
