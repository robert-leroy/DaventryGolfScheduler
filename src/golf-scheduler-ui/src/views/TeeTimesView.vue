<script setup lang="ts">
import { onMounted } from 'vue';
import { useTeeTimeStore } from '@/stores/teeTimeStore';
import { useWaitlistStore } from '@/stores/waitlistStore';
import TeeTimeCard from '@/components/TeeTimeCard.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';

const teeTimeStore = useTeeTimeStore();
const waitlistStore = useWaitlistStore();

onMounted(async () => {
  await teeTimeStore.fetchTeeTimes();
});

async function handleRegister(id: string) {
  await teeTimeStore.register(id);
}

async function handleCancel(id: string) {
  await teeTimeStore.cancelRegistration(id);
}

async function handleJoinWaitlist(id: string) {
  await waitlistStore.joinWaitlist(id);
}

async function handleLeaveWaitlist(id: string) {
  await waitlistStore.leaveWaitlist(id);
}
</script>

<template>
  <div class="tee-times-view">
    <div class="page-header">
      <h1 class="page-title">Upcoming Tee Times</h1>
    </div>

    <div v-if="teeTimeStore.error || waitlistStore.error" class="alert alert-error">
      {{ teeTimeStore.error || waitlistStore.error }}
    </div>

    <LoadingSpinner v-if="teeTimeStore.loading" text="Loading tee times..." />

    <template v-else>
      <div v-if="teeTimeStore.teeTimes.length === 0" class="empty-state">
        <p>No upcoming tee times available.</p>
        <p class="text-muted text-sm">Check back later for new schedules!</p>
      </div>

      <div v-else class="tee-times-list">
        <TeeTimeCard
          v-for="teeTime in teeTimeStore.teeTimes"
          :key="teeTime.id"
          :tee-time="teeTime"
          @register="handleRegister"
          @cancel="handleCancel"
          @join-waitlist="handleJoinWaitlist"
          @leave-waitlist="handleLeaveWaitlist"
        />
      </div>
    </template>
  </div>
</template>

<style scoped>
.tee-times-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
</style>
