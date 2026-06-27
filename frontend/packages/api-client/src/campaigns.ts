import { ApiClient, jsonBody } from "./client";
import type {
  AudienceFilterDto,
  CampaignCapacityDto,
  CampaignDto,
  CampaignLaunchRequest,
  Guid
} from "./types";

export interface CampaignRequest {
  postId?: Guid | null;
  name: string;
  subject: string;
  previewText?: string | null;
  fromName: string;
  fromEmail: string;
  replyTo?: string | null;
  bodyHtml: string;
  plainText: string;
  audienceFilter: AudienceFilterDto;
  scheduledAt?: string | null;
  allowPartialCampaign: boolean;
}

export const campaignsApi = (client: ApiClient, workspaceId: string) => ({
  list: () => client.request<CampaignDto[]>(`/api/workspaces/${workspaceId}/campaigns`),
  create: (request: CampaignRequest) =>
    client.request<CampaignDto>(`/api/workspaces/${workspaceId}/campaigns`, { method: "POST", body: jsonBody(request) }),
  estimate: (campaignId: string, request: CampaignLaunchRequest) =>
    client.request<CampaignCapacityDto>(`/api/workspaces/${workspaceId}/campaigns/${campaignId}/estimate`, {
      method: "POST",
      body: jsonBody(request)
    }),
  launch: (campaignId: string, request: CampaignLaunchRequest) =>
    client.request<CampaignDto>(`/api/workspaces/${workspaceId}/campaigns/${campaignId}/launch`, {
      method: "POST",
      body: jsonBody(request)
    }),
  pause: (campaignId: string) =>
    client.request<CampaignDto>(`/api/workspaces/${workspaceId}/campaigns/${campaignId}/pause`, { method: "POST" }),
  resume: (campaignId: string) =>
    client.request<CampaignDto>(`/api/workspaces/${workspaceId}/campaigns/${campaignId}/resume`, { method: "POST" }),
  cancel: (campaignId: string) =>
    client.request<CampaignDto>(`/api/workspaces/${workspaceId}/campaigns/${campaignId}/cancel`, { method: "POST" })
});
