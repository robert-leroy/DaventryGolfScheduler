import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { authService } from '@/services/authService';
import { authApi, userApi } from '@/services/api';
import type { UserProfile } from '@/types';
import type { RegisterRequest } from '@/services/authService';

const DEV_MODE = import.meta.env.VITE_BYPASS_AUTH === 'true';

export const useAuthStore = defineStore('auth', () => {
  const user = ref<UserProfile | null>(null);
  const loading = ref(false);
  const initialized = ref(false);
  const loginError = ref<string | null>(null);

  const isAuthenticated = computed(() => user.value !== null);
  const isAdmin = computed(() => user.value?.isAdmin ?? false);

  async function initialize() {
    if (initialized.value) return;
    loading.value = true;
    try {
      if (DEV_MODE) {
        // In dev mode, automatically fetch the dev user
        await fetchCurrentUser();
      } else if (authService.isAuthenticated()) {
        // Try to load from storage first
        const storedUser = authService.getStoredUser();
        if (storedUser) {
          user.value = storedUser;
        }
        // Then verify with server
        await fetchCurrentUser();
      }
    } catch (error) {
      console.error('Auth initialization failed:', error);
      // Clear tokens if initialization fails
      if (!DEV_MODE) {
        authService.clearTokens();
      }
      user.value = null;
    } finally {
      loading.value = false;
      initialized.value = true;
    }
  }

  async function login(email: string, password: string) {
    loading.value = true;
    loginError.value = null;
    try {
      if (DEV_MODE) {
        // In dev mode, just fetch the dev user
        await fetchCurrentUser();
      } else {
        const response = await authApi.login(email, password);
        user.value = response.user;
      }
    } catch (error: unknown) {
      const axiosError = error as { response?: { data?: { message?: string } } };
      loginError.value = axiosError.response?.data?.message || 'Login failed. Please check your credentials.';
      throw error;
    } finally {
      loading.value = false;
    }
  }

  async function register(data: RegisterRequest) {
    loading.value = true;
    loginError.value = null;
    try {
      const response = await authApi.register(data);
      user.value = response.user;
    } catch (error: unknown) {
      const axiosError = error as { response?: { data?: { message?: string } } };
      loginError.value = axiosError.response?.data?.message || 'Registration failed. Please try again.';
      throw error;
    } finally {
      loading.value = false;
    }
  }

  async function logout() {
    loading.value = true;
    try {
      if (!DEV_MODE) {
        await authApi.logout();
      }
      user.value = null;
    } catch (error) {
      console.error('Logout failed:', error);
    } finally {
      loading.value = false;
    }
  }

  async function fetchCurrentUser() {
    try {
      user.value = await userApi.getCurrentUser();
      // Update stored user
      if (user.value && !DEV_MODE) {
        authService.saveUser(user.value);
      }
    } catch (error) {
      console.error('Failed to fetch current user:', error);
      user.value = null;
    }
  }

  function clearError() {
    loginError.value = null;
  }

  return {
    user,
    loading,
    initialized,
    loginError,
    isAuthenticated,
    isAdmin,
    initialize,
    login,
    register,
    logout,
    fetchCurrentUser,
    clearError,
  };
});
