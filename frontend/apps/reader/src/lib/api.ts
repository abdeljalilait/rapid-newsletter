import { ApiClient, publicApi, type PublicPostDetailDto, type PublicWorkspaceDto, type PlanDto, type PostDto } from "@rapid/api-client";

const baseUrl = import.meta.env.PUBLIC_API_URL ?? "http://localhost:5120";
const api = publicApi(new ApiClient(baseUrl));

export async function getWorkspace(slug: string): Promise<PublicWorkspaceDto | null> {
  return api.workspace(slug).catch(() => null);
}

export async function getPlans(slug: string): Promise<PlanDto[]> {
  return api.plans(slug).catch(() => []);
}

export async function getPosts(slug: string): Promise<PostDto[]> {
  return api.posts(slug).catch(() => []);
}

export async function getPost(slug: string, postId: string): Promise<PublicPostDetailDto | null> {
  return api.post(slug, postId).catch(() => null);
}
