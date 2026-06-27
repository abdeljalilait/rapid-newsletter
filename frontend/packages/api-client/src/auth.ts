import { ApiClient, jsonBody } from "./client";
import type { AuthResponse } from "./types";

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName?: string | null;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export const authApi = (client: ApiClient) => ({
  register: (request: RegisterRequest) =>
    client.request<AuthResponse>("/api/auth/register", { method: "POST", body: jsonBody(request), auth: false }),
  login: (request: LoginRequest) =>
    client.request<AuthResponse>("/api/auth/login", { method: "POST", body: jsonBody(request), auth: false }),
  refresh: (refreshToken: string) =>
    client.request<AuthResponse>("/api/auth/refresh", { method: "POST", body: jsonBody({ refreshToken }), auth: false }),
  logout: (refreshToken: string) =>
    client.request<void>("/api/auth/logout", { method: "POST", body: jsonBody({ refreshToken }) }),
  me: () => client.request<AuthResponse>("/api/auth/me")
});
