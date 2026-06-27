import { ApiClient, AuthResponse, authApi, TokenStore } from "@rapid/api-client";
import { createContext, ReactNode, useContext, useMemo, useState } from "react";

const storageKey = "rapid.auth";

interface AuthState {
  auth: AuthResponse | null;
  client: ApiClient;
  login(email: string, password: string): Promise<void>;
  register(input: { email: string; password: string; firstName: string; lastName?: string }): Promise<void>;
  logout(): Promise<void>;
}

const AuthContext = createContext<AuthState | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [auth, setAuthState] = useState<AuthResponse | null>(() => readStoredAuth());

  const tokenStore = useMemo<TokenStore>(() => ({
    getAccessToken: () => readStoredAuth()?.accessToken ?? null,
    getRefreshToken: () => readStoredAuth()?.refreshToken ?? null,
    setAuth: (next) => {
      localStorage.setItem(storageKey, JSON.stringify(next));
      setAuthState(next);
    },
    clearAuth: () => {
      localStorage.removeItem(storageKey);
      setAuthState(null);
    }
  }), []);

  const client = useMemo(() => new ApiClient(import.meta.env.VITE_API_URL ?? "http://localhost:5120", tokenStore), [tokenStore]);
  const api = authApi(client);

  const value: AuthState = {
    auth,
    client,
    async login(email, password) {
      tokenStore.setAuth(await api.login({ email, password }));
    },
    async register(input) {
      tokenStore.setAuth(await api.register(input));
    },
    async logout() {
      const refreshToken = tokenStore.getRefreshToken();
      if (refreshToken) await api.logout(refreshToken).catch(() => undefined);
      tokenStore.clearAuth();
    }
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used inside AuthProvider");
  return context;
}

function readStoredAuth() {
  const raw = localStorage.getItem(storageKey);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as AuthResponse;
  } catch {
    localStorage.removeItem(storageKey);
    return null;
  }
}
