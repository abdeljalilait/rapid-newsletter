import { ApiClient } from "./client";
import type { OverviewAnalyticsDto } from "./types";

export const analyticsApi = (client: ApiClient, workspaceId: string) => ({
  overview: () => client.request<OverviewAnalyticsDto>(`/api/workspaces/${workspaceId}/analytics/overview`)
});
