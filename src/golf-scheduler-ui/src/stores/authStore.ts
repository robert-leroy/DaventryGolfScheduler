import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { authService } from '@/services/authService';
import { userApi } from '@/services/api';
import type { UserProfile } from '@/types';

const DEV_MODE = import.meta.env.VITE_BYPASS_AUTH === 'true';

export const useAuthStore = defineStore('auth', () => {
  const user = ref<UserProfile | null>(null);
  const loading = ref(false);
  const initialized = ref(false);

  const isAuthenticated = computed(() => user.value !== null);
  const isAdmin = computed(() => user.value?.isAdmin ?? false);

  async function initialize() {
    if (initialized.value) return;
    loading.value = true;
    try {
      if (DEV_MODE) {
        // In dev mode, automatically fetch the dev user
        await fetchCurrentUser();
      } else {
        await authService.initialize();
        if (authService.isAuthenticated()) {
          await fetchCurrentUser();
        }
      }
    } catch (error) {
      console.error('Auth initialization failed:', error);
    } finally {
      loading.value = false;
      initialized.value = true;
    }
  }

  async function login() {
    loading.value = true;
    try {
      if (DEV_MODE) {
        // In dev mode, just fetch the dev user
        await fetchCurrentUser();
      } else {
        await authService.login();
        await fetchCurrentUser();
      }
    } catch (error) {
      console.error('Login failed:', error);
      throw error;
    } finally {
      loading.value = false;
    }
  }

  async function logout() {
    loading.value = true;
    try {
      if (!DEV_MODE) {
        await authService.logout();
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
    } catch (error) {
      console.error('Failed to fetch current user:', error);
      user.value = null;
    }
  }

  return {
    user,
    loading,
    initialized,
    isAuthenticated,
    isAdmin,
    initialize,
    login,
    logout,
    fetchCurrentUser,
  };
});
