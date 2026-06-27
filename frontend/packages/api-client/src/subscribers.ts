import { ApiClient, jsonBody } from "./client";
import type { ImportSummaryDto, SubscriberAccessLevel, SubscriberDto, SubscriberStatus } from "./types";

export interface UpsertSubscriberRequest {
  email: string;
  firstName?: string | null;
  lastName?: string | null;
  status: SubscriberStatus;
  accessLevel: SubscriberAccessLevel;
  consentSource?: string | null;
  consentAt?: string | null;
}

export interface ImportSubscriberRow {
  email: string;
  firstName?: string | null;
  lastName?: string | null;
  accessLevel: SubscriberAccessLevel;
}

export const subscribersApi = (client: ApiClient, workspaceId: string) => ({
  list: () => client.request<SubscriberDto[]>(`/api/workspaces/${workspaceId}/subscribers`),
  create: (request: UpsertSubscriberRequest) =>
    client.request<SubscriberDto>(`/api/workspaces/${workspaceId}/subscribers`, { method: "POST", body: jsonBody(request) }),
  importJson: (rows: ImportSubscriberRow[]) =>
    client.request<ImportSummaryDto>(`/api/workspaces/${workspaceId}/subscribers/import-json`, {
      method: "POST",
      body: jsonBody({ rows })
    })
});
