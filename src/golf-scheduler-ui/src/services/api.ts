import axios, { AxiosInstance, InternalAxiosRequestConfig } from 'axios';
import { authService } from './authService';
import type { AuthResponse, RegisterRequest } from './authService';
import type { TeeTime, TeeTimeListItem, TeeTimeCreate, TeeTimeUpdate, User, UserProfile, UserProfileUpdate, UserCreate, UserUpdate, UserRegistration, GolfersByDay } from '@/types';

const apiClient: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - attach token synchronously
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = authService.getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor - handle 401 and token refresh
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // If 401 and we haven't already retried
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      const refreshToken = authService.getRefreshToken();
      if (refreshToken) {
        try {
          // Try to refresh the token
          const response = await axios.post<AuthResponse>(
            `${import.meta.env.VITE_API_URL || '/api'}/api/auth/refresh`,
            { refreshToken }
          );

          const { accessToken, refreshToken: newRefreshToken, user } = response.data;
          authService.saveTokens(accessToken, newRefreshToken);
          authService.saveUser(user);

          // Retry the original request with new token
          originalRequest.headers.Authorization = `Bearer ${accessToken}`;
          return apiClient(originalRequest);
        } catch {
          // Refresh failed, clear tokens
          authService.clearTokens();
        }
      }

      // No refresh token or refresh failed - redirect to login
      if (typeof window !== 'undefined' && !window.location.pathname.includes('/login')) {
        window.location.href = '/login';
      }
    }

    return Promise.reject(error);
  }
);

export const authApi = {
  login: async (email: string, password: string): Promise<AuthResponse> => {
    const response = await apiClient.post<AuthResponse>('/api/auth/login', { email, password });
    const { accessToken, refreshToken, user } = response.data;
    authService.saveTokens(accessToken, refreshToken);
    authService.saveUser(user);
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const response = await apiClient.post<AuthResponse>('/api/auth/register', data);
    const { accessToken, refreshToken, user } = response.data;
    authService.saveTokens(accessToken, refreshToken);
    authService.saveUser(user);
    return response.data;
  },

  logout: async (): Promise<void> => {
    const refreshToken = authService.getRefreshToken();
    if (refreshToken) {
      try {
        await apiClient.post('/api/auth/logout', { refreshToken });
      } catch {
        // Ignore errors on logout
      }
    }
    authService.clearTokens();
  },

  changePassword: async (currentPassword: string, newPassword: string): Promise<void> => {
    await apiClient.post('/api/auth/change-password', { currentPassword, newPassword });
  },
};

export const teeTimeApi = {
  getAll: async (): Promise<TeeTimeListItem[]> => {
    const response = await apiClient.get<TeeTimeListItem[]>('/api/teetimes');
    return response.data;
  },

  getById: async (id: string): Promise<TeeTime> => {
    const response = await apiClient.get<TeeTime>(`/api/teetimes/${id}`);
    return response.data;
  },

  create: async (data: TeeTimeCreate): Promise<TeeTime> => {
    const response = await apiClient.post<TeeTime>('/api/teetimes', data);
    return response.data;
  },

  update: async (id: string, data: TeeTimeUpdate): Promise<TeeTime> => {
    const response = await apiClient.put<TeeTime>(`/api/teetimes/${id}`, data);
    return response.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/teetimes/${id}`);
  },

  register: async (id: string): Promise<void> => {
    await apiClient.post(`/api/teetimes/${id}/register`);
  },

  cancelRegistration: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/teetimes/${id}/register`);
  },

  getGolfersByDay: async (): Promise<GolfersByDay[]> => {
    const response = await apiClient.get<GolfersByDay[]>('/api/teetimes/by-day');
    return response.data;
  },
};

export const registrationApi = {
  getMyRegistrations: async (): Promise<UserRegistration[]> => {
    const response = await apiClient.get<UserRegistration[]>('/api/registrations/me');
    return response.data;
  },
};

export const userApi = {
  getCurrentUser: async (): Promise<UserProfile> => {
    const response = await apiClient.get<UserProfile>('/api/users/me');
    return response.data;
  },

  updateCurrentUserProfile: async (data: UserProfileUpdate): Promise<UserProfile> => {
    const response = await apiClient.put<UserProfile>('/api/users/me', data);
    return response.data;
  },

  getAllUsers: async (): Promise<User[]> => {
    const response = await apiClient.get<User[]>('/api/users');
    return response.data;
  },

  getUser: async (id: string): Promise<User> => {
    const response = await apiClient.get<User>(`/api/users/${id}`);
    return response.data;
  },

  createUser: async (data: UserCreate): Promise<User> => {
    const response = await apiClient.post<User>('/api/users', data);
    return response.data;
  },

  updateUser: async (id: string, data: UserUpdate): Promise<User> => {
    const response = await apiClient.put<User>(`/api/users/${id}`, data);
    return response.data;
  },

  updateAdminStatus: async (id: string, isAdmin: boolean): Promise<User> => {
    const response = await apiClient.put<User>(`/api/users/${id}/admin`, { isAdmin });
    return response.data;
  },

  deleteUser: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/users/${id}`);
  },
};

export default apiClient;
