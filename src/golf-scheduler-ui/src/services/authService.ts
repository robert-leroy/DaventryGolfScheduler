const TOKEN_KEY = 'golf_scheduler_token';
const REFRESH_TOKEN_KEY = 'golf_scheduler_refresh_token';
const USER_KEY = 'golf_scheduler_user';

export interface UserProfile {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  displayName: string;
  phone: string | null;
  handicap: number | null;
  isAdmin: boolean;
  createdAt: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserProfile;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phone?: string;
  handicap?: number;
}

class AuthService {
  private accessToken: string | null = null;
  private refreshToken: string | null = null;

  constructor() {
    this.loadTokensFromStorage();
  }

  private loadTokensFromStorage(): void {
    this.accessToken = localStorage.getItem(TOKEN_KEY);
    this.refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  saveTokens(accessToken: string, refreshToken: string): void {
    this.accessToken = accessToken;
    this.refreshToken = refreshToken;
    localStorage.setItem(TOKEN_KEY, accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  }

  saveUser(user: UserProfile): void {
    localStorage.setItem(USER_KEY, JSON.stringify(user));
  }

  clearTokens(): void {
    this.accessToken = null;
    this.refreshToken = null;
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }

  getAccessToken(): string | null {
    return this.accessToken;
  }

  getRefreshToken(): string | null {
    return this.refreshToken;
  }

  isAuthenticated(): boolean {
    return this.accessToken !== null;
  }

  getStoredUser(): UserProfile | null {
    const userJson = localStorage.getItem(USER_KEY);
    return userJson ? JSON.parse(userJson) : null;
  }
}

export const authService = new AuthService();
