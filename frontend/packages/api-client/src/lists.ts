import { ApiClient, jsonBody } from "./client";
import type { ListDto } from "./types";

export const listsApi = (client: ApiClient, workspaceId: string) => ({
  list: () => client.request<ListDto[]>(`/api/workspaces/${workspaceId}/lists`),
  create: (request: { name: string; description?: string | null }) =>
    client.request<ListDto>(`/api/workspaces/${workspaceId}/lists`, { method: "POST", body: jsonBody(request) }),
  addMember: (listId: string, subscriberId: string) =>
    client.request<void>(`/api/workspaces/${workspaceId}/lists/${listId}/members/${subscriberId}`, { method: "POST" })
});
