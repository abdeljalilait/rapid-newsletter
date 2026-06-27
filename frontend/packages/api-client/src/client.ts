import type { AuthResponse } from "./types";

export interface TokenStore {
  getAccessToken(): string | null;
  getRefreshToken(): string | null;
  setAuth(auth: AuthResponse): void;
  clearAuth(): void;
}

export interface RequestOptions extends RequestInit {
  auth?: boolean;
  retry?: boolean;
}

export class ApiError extends Error {
  constructor(
    public status: number,
    public payload: unknown,
    message = "API request failed"
  ) {
    super(message);
  }
}

export class ApiClient {
  constructor(
    public readonly baseUrl = "http://localhost:5120",
    private readonly tokenStore?: TokenStore
  ) {}

  async request<T>(path: string, options: RequestOptions = {}): Promise<T> {
    const headers = new Headers(options.headers);
    if (options.body && !headers.has("Content-Type")) {
      headers.set("Content-Type", "application/json");
    }

    if (options.auth !== false) {
      const token = this.tokenStore?.getAccessToken();
      if (token) headers.set("Authorization", `Bearer ${token}`);
    }

    const response = await fetch(`${this.baseUrl}${path}`, { ...options, headers });
    if (response.status === 401 && options.retry !== false && this.tokenStore?.getRefreshToken()) {
      await this.refreshToken();
      return this.request<T>(path, { ...options, retry: false });
    }

    if (response.status === 204) return undefined as T;

    const text = await response.text();
    const payload = text ? JSON.parse(text) : null;
    if (!response.ok) {
      throw new ApiError(response.status, payload, extractError(payload));
    }
    return payload as T;
  }

  private async refreshToken() {
    const refreshToken = this.tokenStore?.getRefreshToken();
    if (!refreshToken) throw new ApiError(401, null, "Missing refresh token");

    const response = await fetch(`${this.baseUrl}/api/auth/refresh`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken })
    });

    const auth = (await response.json()) as AuthResponse;
    if (!response.ok) {
      this.tokenStore?.clearAuth();
      throw new ApiError(response.status, auth, "Session expired");
    }
    this.tokenStore?.setAuth(auth);
  }
}

export function jsonBody(value: unknown) {
  return JSON.stringify(value);
}

function extractError(payload: unknown) {
  if (payload && typeof payload === "object" && "error" in payload) {
    return String((payload as { error: unknown }).error);
  }
  return "API request failed";
}
