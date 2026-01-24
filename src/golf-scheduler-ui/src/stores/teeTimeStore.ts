import { defineStore } from 'pinia';
import { ref } from 'vue';
import { teeTimeApi } from '@/services/api';
import type { TeeTime, TeeTimeListItem, TeeTimeCreate, TeeTimeUpdate } from '@/types';

export const useTeeTimeStore = defineStore('teeTime', () => {
  const teeTimes = ref<TeeTimeListItem[]>([]);
  const currentTeeTime = ref<TeeTime | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);

  async function fetchTeeTimes() {
    loading.value = true;
    error.value = null;
    try {
      teeTimes.value = await teeTimeApi.getAll();
    } catch (e) {
      error.value = 'Failed to load tee times';
      console.error(e);
    } finally {
      loading.value = false;
    }
  }

  async function fetchTeeTime(id: string) {
    loading.value = true;
    error.value = null;
    try {
      currentTeeTime.value = await teeTimeApi.getById(id);
    } catch (e) {
      error.value = 'Failed to load tee time';
      console.error(e);
    } finally {
      loading.value = false;
    }
  }

  async function createTeeTime(data: TeeTimeCreate) {
    loading.value = true;
    error.value = null;
    try {
      const created = await teeTimeApi.create(data);
      await fetchTeeTimes();
      return created;
    } catch (e) {
      error.value = 'Failed to create tee time';
      console.error(e);
      throw e;
    } finally {
      loading.value = false;
    }
  }

  async function updateTeeTime(id: string, data: TeeTimeUpdate) {
    loading.value = true;
    error.value = null;
    try {
      const updated = await teeTimeApi.update(id, data);
      await fetchTeeTimes();
      return updated;
    } catch (e) {
      error.value = 'Failed to update tee time';
      console.error(e);
      throw e;
    } finally {
      loading.value = false;
    }
  }

  async function deleteTeeTime(id: string) {
    loading.value = true;
    error.value = null;
    try {
      await teeTimeApi.delete(id);
      await fetchTeeTimes();
    } catch (e) {
      error.value = 'Failed to delete tee time';
      console.error(e);
      throw e;
    } finally {
      loading.value = false;
    }
  }

  async function register(id: string) {
    loading.value = true;
    error.value = null;
    try {
      await teeTimeApi.register(id);
      await fetchTeeTimes();
      if (currentTeeTime.value?.id === id) {
        await fetchTeeTime(id);
      }
    } catch (e: unknown) {
      const axiosError = e as { response?: { data?: string } };
      error.value = axiosError.response?.data || 'Failed to register';
      console.error(e);
      throw e;
    } finally {
      loading.value = false;
    }
  }

  async function cancelRegistration(id: string) {
    loading.value = true;
    error.value = null;
    try {
      await teeTimeApi.cancelRegistration(id);
      await fetchTeeTimes();
      if (currentTeeTime.value?.id === id) {
        await fetchTeeTime(id);
      }
    } catch (e) {
      error.value = 'Failed to cancel registration';
      console.error(e);
      throw e;
    } finally {
      loading.value = false;
    }
  }

  return {
    teeTimes,
    currentTeeTime,
    loading,
    error,
    fetchTeeTimes,
    fetchTeeTime,
    createTeeTime,
    updateTeeTime,
    deleteTeeTime,
    register,
    cancelRegistration,
  };
});
