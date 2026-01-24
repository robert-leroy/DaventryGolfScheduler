import { defineStore } from 'pinia';
import { ref } from 'vue';
import { registrationApi } from '@/services/api';
import type { UserRegistration } from '@/types';

export const useRegistrationStore = defineStore('registration', () => {
  const myRegistrations = ref<UserRegistration[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  async function fetchMyRegistrations() {
    loading.value = true;
    error.value = null;
    try {
      myRegistrations.value = await registrationApi.getMyRegistrations();
    } catch (e) {
      error.value = 'Failed to load your registrations';
      console.error(e);
    } finally {
      loading.value = false;
    }
  }

  return {
    myRegistrations,
    loading,
    error,
    fetchMyRegistrations,
  };
});
