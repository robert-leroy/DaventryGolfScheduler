<script setup lang="ts">
import { onMounted } from 'vue';
import { RouterLink } from 'vue-router';
import { useWaitlistStore } from '@/stores/waitlistStore';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import { parseLocalDate } from '@/utils/dateUtils';

const waitlistStore = useWaitlistStore();

onMounted(async () => {
  await waitlistStore.fetchMyWaitlist();
});

function formatDate(dateString: string) {
  const date = parseLocalDate(dateString);
  return date.toLocaleDateString('en-US', {
    weekday: 'long',
    month: 'short',
    day: 'numeric',
  });
}

async function handleLeave(teeDate: string) {
  await waitlistStore.leaveWaitlistByDate(teeDate);
}
</script>

<template>
  <div class="my-waitlist">
    <div class="page-header">
      <h1 class="page-title">My Waitlist</h1>
    </div>

    <div v-if="waitlistStore.error" class="alert alert-error">
      {{ waitlistStore.error }}
    </div>

    <LoadingSpinner v-if="waitlistStore.loading" text="Loading your waitlist..." />

    <template v-else>
      <div v-if="waitlistStore.myWaitlist.length === 0" class="empty-state">
        <p>You are not on any waitlists.</p>
        <RouterLink to="/tee-times" class="btn btn-primary mt-2">
          Browse Tee Times
        </RouterLink>
      </div>

      <div v-else class="waitlist-list">
        <div
          v-for="entry in waitlistStore.myWaitlist"
          :key="entry.id"
          class="waitlist-card card"
        >
          <div class="card-content">
            <div class="waitlist-info">
              <div class="date">{{ formatDate(entry.teeDate) }}</div>
              <div class="position-badge">
                <span class="badge badge-warning">Position #{{ entry.position }}</span>
              </div>
              <p class="text-muted text-sm">
                Joined {{ new Date(entry.joinedAt).toLocaleDateString() }}
              </p>
            </div>

            <div class="card-actions">
              <button
                @click="handleLeave(entry.teeDate)"
                class="btn btn-danger"
                :disabled="waitlistStore.loading"
              >
                Leave Waitlist
              </button>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
.waitlist-list {
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

.waitlist-info {
  flex: 1;
}

.date {
  font-size: 1.125rem;
  font-weight: 600;
  margin-bottom: 0.5rem;
}

.position-badge {
  margin-bottom: 0.25rem;
}

.card-actions {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}
</style>
