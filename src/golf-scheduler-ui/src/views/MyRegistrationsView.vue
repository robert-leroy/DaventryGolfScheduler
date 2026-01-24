<script setup lang="ts">
import { onMounted } from 'vue';
import { RouterLink } from 'vue-router';
import { useRegistrationStore } from '@/stores/registrationStore';
import { useTeeTimeStore } from '@/stores/teeTimeStore';
import LoadingSpinner from '@/components/LoadingSpinner.vue';

const registrationStore = useRegistrationStore();
const teeTimeStore = useTeeTimeStore();

onMounted(async () => {
  await registrationStore.fetchMyRegistrations();
});

function formatDate(dateString: string) {
  const date = new Date(dateString);
  return date.toLocaleDateString('en-US', {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
  });
}

function formatTime(timeString: string) {
  const [hours, minutes] = timeString.split(':');
  const date = new Date();
  date.setHours(parseInt(hours), parseInt(minutes));
  return date.toLocaleTimeString('en-US', {
    hour: 'numeric',
    minute: '2-digit',
    hour12: true,
  });
}

async function handleCancel(teeTimeId: string) {
  await teeTimeStore.cancelRegistration(teeTimeId);
  await registrationStore.fetchMyRegistrations();
}
</script>

<template>
  <div class="my-registrations">
    <div class="page-header">
      <h1 class="page-title">My Registrations</h1>
    </div>

    <div v-if="registrationStore.error" class="alert alert-error">
      {{ registrationStore.error }}
    </div>

    <LoadingSpinner v-if="registrationStore.loading" text="Loading your registrations..." />

    <template v-else>
      <div v-if="registrationStore.myRegistrations.length === 0" class="empty-state">
        <p>You haven't registered for any tee times yet.</p>
        <RouterLink to="/tee-times" class="btn btn-primary mt-2">
          Browse Tee Times
        </RouterLink>
      </div>

      <div v-else class="registrations-list">
        <div
          v-for="reg in registrationStore.myRegistrations"
          :key="reg.id"
          class="registration-card card"
        >
          <div class="card-content">
            <div class="registration-info">
              <div class="date-time">
                <span class="date">{{ formatDate(reg.teeDate) }}</span>
                <span class="time">{{ formatTime(reg.teeTime) }}</span>
              </div>
              <p class="text-muted text-sm">
                Registered {{ new Date(reg.registeredAt).toLocaleDateString() }}
              </p>
            </div>

            <div class="card-actions">
              <RouterLink :to="`/tee-times/${reg.teeTimeId}`" class="btn btn-outline">
                View Details
              </RouterLink>
              <button
                @click="handleCancel(reg.teeTimeId)"
                class="btn btn-danger"
                :disabled="teeTimeStore.loading"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
.registrations-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.card-content {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 1rem;
  flex-wrap: wrap;
}

.registration-info {
  flex: 1;
}

.date-time {
  display: flex;
  align-items: baseline;
  gap: 0.75rem;
  margin-bottom: 0.25rem;
}

.date {
  font-size: 1.125rem;
  font-weight: 600;
}

.time {
  font-size: 1rem;
  color: var(--primary-color);
  font-weight: 500;
}

.card-actions {
  display: flex;
  gap: 0.5rem;
}
</style>
