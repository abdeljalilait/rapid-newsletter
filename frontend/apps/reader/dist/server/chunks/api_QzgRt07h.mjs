class ApiError extends Error {
  constructor(status, payload, message = "API request failed") {
    super(message);
    this.status = status;
    this.payload = payload;
  }
}
class ApiClient {
  constructor(baseUrl = "http://localhost:5120", tokenStore) {
    this.baseUrl = baseUrl;
    this.tokenStore = tokenStore;
  }
  async request(path, options = {}) {
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
      return this.request(path, { ...options, retry: false });
    }
    if (response.status === 204) return void 0;
    const text = await response.text();
    const payload = text ? JSON.parse(text) : null;
    if (!response.ok) {
      throw new ApiError(response.status, payload, extractError(payload));
    }
    return payload;
  }
  async refreshToken() {
    const refreshToken = this.tokenStore?.getRefreshToken();
    if (!refreshToken) throw new ApiError(401, null, "Missing refresh token");
    const response = await fetch(`${this.baseUrl}/api/auth/refresh`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken })
    });
    const auth = await response.json();
    if (!response.ok) {
      this.tokenStore?.clearAuth();
      throw new ApiError(response.status, auth, "Session expired");
    }
    this.tokenStore?.setAuth(auth);
  }
}
function jsonBody(value) {
  return JSON.stringify(value);
}
function extractError(payload) {
  if (payload && typeof payload === "object" && "error" in payload) {
    return String(payload.error);
  }
  return "API request failed";
}

const publicApi = (client) => ({
  workspace: (slug) => client.request(`/api/public/${slug}`, { auth: false }),
  plans: (slug) => client.request(`/api/public/${slug}/plans`, { auth: false }),
  posts: (slug) => client.request(`/api/public/${slug}/posts`, { auth: false }),
  post: (slug, postId) => client.request(`/api/public/${slug}/posts/${postId}`, { auth: false }),
  subscribe: (slug, request) => client.request(`/api/public/${slug}/subscribe`, {
    method: "POST",
    auth: false,
    body: jsonBody(request)
  })
});

const baseUrl = "http://localhost:5120";
const api = publicApi(new ApiClient(baseUrl));
async function getWorkspace(slug) {
  return api.workspace(slug).catch(() => null);
}
async function getPlans(slug) {
  return api.plans(slug).catch(() => []);
}
async function getPosts(slug) {
  return api.posts(slug).catch(() => []);
}
async function getPost(slug, postId) {
  return api.post(slug, postId).catch(() => null);
}

export { getPosts as a, getPlans as b, getPost as c, getWorkspace as g };
