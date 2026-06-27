import { ApiClient, jsonBody } from "./client";
import type { PaymentConfigurationDto, PaymentEnvironment } from "./types";

export interface PaymentConfigurationRequest {
  apiKey: string;
  webhookSecret: string;
  environment: PaymentEnvironment;
}

export const paymentsApi = (client: ApiClient, workspaceId: string) => ({
  getDodoConfiguration: () =>
    client.request<PaymentConfigurationDto>(`/api/workspaces/${workspaceId}/payments/dodo/configuration`),
  upsertDodoConfiguration: (request: PaymentConfigurationRequest) =>
    client.request<PaymentConfigurationDto>(`/api/workspaces/${workspaceId}/payments/dodo/configuration`, {
      method: "PUT",
      body: jsonBody(request)
    })
});
