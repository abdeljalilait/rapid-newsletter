import { ApiClient, jsonBody } from "./client";
import type { EmailProvider, ProviderAccountDto } from "./types";

export interface ProviderAccountRequest {
  provider: EmailProvider;
  accountName: string;
  apiKey: string;
  fromName: string;
  fromEmail: string;
  sendingDomain?: string | null;
  dailyLimit?: number | null;
  monthlyLimit?: number | null;
  ratePerMinute: number;
  enabled: boolean;
}

export const providersApi = (client: ApiClient, workspaceId: string) => ({
  list: () => client.request<ProviderAccountDto[]>(`/api/workspaces/${workspaceId}/provider-accounts`),
  create: (request: ProviderAccountRequest) =>
    client.request<ProviderAccountDto>(`/api/workspaces/${workspaceId}/provider-accounts`, {
      method: "POST",
      body: jsonBody(request)
    }),
  validate: (providerAccountId: string) =>
    client.request<ProviderAccountDto>(
      `/api/workspaces/${workspaceId}/provider-accounts/${providerAccountId}/validate`,
      { method: "POST" }
    )
});
