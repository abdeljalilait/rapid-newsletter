import { ApiClient, jsonBody } from "./client";
import type { TagDto } from "./types";

export const tagsApi = (client: ApiClient, workspaceId: string) => ({
  list: () => client.request<TagDto[]>(`/api/workspaces/${workspaceId}/tags`),
  create: (name: string) =>
    client.request<TagDto>(`/api/workspaces/${workspaceId}/tags`, { method: "POST", body: jsonBody({ name }) })
});
