import { ApiClient, jsonBody } from "./client";
import type { BillingInterval, PlanDto } from "./types";

export interface PlanRequest {
  name: string;
  description?: string | null;
  price: number;
  currency: string;
  billingInterval: BillingInterval;
  dodoProductId?: string | null;
  benefits?: string[] | null;
  isActive: boolean;
  sortOrder: number;
}

export const plansApi = (client: ApiClient, workspaceId: string) => ({
  list: () => client.request<PlanDto[]>(`/api/workspaces/${workspaceId}/plans`),
  create: (request: PlanRequest) =>
    client.request<PlanDto>(`/api/workspaces/${workspaceId}/plans`, { method: "POST", body: jsonBody(request) })
});
