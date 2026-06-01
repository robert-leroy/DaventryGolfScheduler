import { defineStore } from 'pinia';
import { ref } from 'vue';
import { waitlistApi } from '@/services/api';
import { useTeeTimeStore } from '@/stores/teeTimeStore';
import type { WaitlistEntry } from '@/types';

export const useWaitlistStore = defineStore('waitlist', () => {
  const myWaitlist = ref<WaitlistEntry[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  async function fetchMyWaitlist() {
    loading.value = true;
    error.value = null;
    try {
      myWaitlist.value = await waitlistApi.getMyWaitlist();
    } catch (e) {
      error.value = 'Failed to load your waitlist';
      console.error(e);
    } finally {
      loading.value = false;
    }
  }

  async function joinWaitlist(teeTimeId: string) {
    loading.value = true;
    error.value = null;
    try {
      await waitlistApi.joinWaitlist(teeTimeId);
      const teeTimeStore = useTeeTimeStore();
      await teeTimeStore.fetchTeeTimes();
      if (teeTimeStore.currentTeeTime?.id === teeTimeId) {
        await teeTimeStore.fetchTeeTime(teeTimeId);
      }
    } catch (e: unknown) {
      const axiosError = e as { response?: { data?: string } };
      error.value = axiosError.response?.data || 'Failed to join waitlist';
      console.error(e);
      throw e;
    } finally {
      loading.value = false;
    }
  }

  async function leaveWaitlist(teeTimeId: string) {
    loading.value = true;
    error.value = null;
    try {
      await waitlistApi.leaveWaitlist(teeTimeId);
      const teeTimeStore = useTeeTimeStore();
      await teeTimeStore.fetchTeeTimes();
      if (teeTimeStore.currentTeeTime?.id === teeTimeId) {
        await teeTimeStore.fetchTeeTime(teeTimeId);
      }
    } catch (e) {
      error.value = 'Failed to leave waitlist';
      console.error(e);
      throw e;
    } finally {
      loading.value = false;
    }
  }

  async function leaveWaitlistByDate(date: string) {
    loading.value = true;
    error.value = null;
    try {
      await waitlistApi.leaveWaitlistByDate(date);
      myWaitlist.value = myWaitlist.value.filter((e) => e.teeDate !== date);
      const teeTimeStore = useTeeTimeStore();
      await teeTimeStore.fetchTeeTimes();
    } catch (e) {
      error.value = 'Failed to leave waitlist';
      console.error(e);
      throw e;
    } finally {
      loading.value = false;
    }
  }

  return {
    myWaitlist,
    loading,
    error,
    fetchMyWaitlist,
    joinWaitlist,
    leaveWaitlist,
    leaveWaitlistByDate,
  };
});
