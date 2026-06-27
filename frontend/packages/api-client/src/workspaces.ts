import { ApiClient, jsonBody } from "./client";
import type { WorkspaceDto } from "./types";

export interface CreateWorkspaceRequest {
  name: string;
  slug?: string | null;
  description?: string | null;
  defaultSenderName: string;
  defaultSenderEmail: string;
  logoUrl?: string | null;
  timezone: string;
  defaultCurrency: string;
}

export interface UpdateWorkspaceRequest {
  name: string;
  defaultSenderName: string;
  defaultSenderEmail: string;
  description?: string | null;
  logoUrl?: string | null;
  timezone: string;
  defaultCurrency: string;
}

export const workspacesApi = (client: ApiClient) => ({
  list: () => client.request<WorkspaceDto[]>("/api/workspaces/"),
  get: (id: string) => client.request<WorkspaceDto>(`/api/workspaces/${id}`),
  create: (request: CreateWorkspaceRequest) =>
    client.request<WorkspaceDto>("/api/workspaces/", { method: "POST", body: jsonBody(request) }),
  update: (id: string, request: UpdateWorkspaceRequest) =>
    client.request<WorkspaceDto>(`/api/workspaces/${id}`, { method: "PUT", body: jsonBody(request) })
});
