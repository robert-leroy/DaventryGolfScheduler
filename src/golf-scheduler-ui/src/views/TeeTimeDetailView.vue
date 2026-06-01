<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useTeeTimeStore } from '@/stores/teeTimeStore';
import { useWaitlistStore } from '@/stores/waitlistStore';
import { useAuthStore } from '@/stores/authStore';
import RegistrationList from '@/components/RegistrationList.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import { parseLocalDate } from '@/utils/dateUtils';

const route = useRoute();
const router = useRouter();
const teeTimeStore = useTeeTimeStore();
const waitlistStore = useWaitlistStore();
const authStore = useAuthStore();

const teeTime = computed(() => teeTimeStore.currentTeeTime);

const formattedDate = computed(() => {
  if (!teeTime.value) return '';
  const date = parseLocalDate(teeTime.value.teeDate);
  return date.toLocaleDateString('en-US', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
});

const formattedTime = computed(() => {
  if (!teeTime.value) return '';
  const [hours, minutes] = teeTime.value.teeTime.split(':');
  const date = new Date();
  date.setHours(parseInt(hours), parseInt(minutes));
  return date.toLocaleTimeString('en-US', {
    hour: 'numeric',
    minute: '2-digit',
    hour12: true,
  });
});

const isUserRegistered = computed(() => {
  if (!teeTime.value || !authStore.user) return false;
  return teeTime.value.registrations.some((r) => r.userId === authStore.user?.id);
});

const isFull = computed(() => teeTime.value ? teeTime.value.availableSlots === 0 : false);
const isDayFull = computed(() => teeTime.value?.isDayFull ?? false);
const isUserRegisteredForDay = computed(() => teeTime.value?.isUserRegisteredForDay ?? false);
const canRegister = computed(() => !isFull.value && !isUserRegistered.value && !isUserRegisteredForDay.value);

onMounted(async () => {
  const id = route.params.id as string;
  await teeTimeStore.fetchTeeTime(id);
});

async function handleRegister() {
  if (!teeTime.value) return;
  await teeTimeStore.register(teeTime.value.id);
}

async function handleCancel() {
  if (!teeTime.value) return;
  await teeTimeStore.cancelRegistration(teeTime.value.id);
}

async function handleJoinWaitlist() {
  if (!teeTime.value) return;
  await waitlistStore.joinWaitlist(teeTime.value.id);
}

async function handleLeaveWaitlist() {
  if (!teeTime.value) return;
  await waitlistStore.leaveWaitlist(teeTime.value.id);
}

function goBack() {
  router.push('/tee-times');
}
</script>

<template>
  <div class="tee-time-detail">
    <button @click="goBack" class="back-link">
      &larr; Back to Tee Times
    </button>

    <LoadingSpinner v-if="teeTimeStore.loading" text="Loading tee time..." />

    <div v-else-if="teeTimeStore.error" class="alert alert-error">
      {{ teeTimeStore.error }}
    </div>

    <template v-else-if="teeTime">
      <div class="card">
        <div class="card-header">
          <h1 class="card-title">{{ formattedDate }}</h1>
          <span class="tee-time-value">{{ formattedTime }}</span>
        </div>

        <div class="tee-time-details">
          <div class="detail-item">
            <span class="label">Available Spots:</span>
            <span :class="['badge', teeTime.availableSlots > 0 ? 'badge-success' : 'badge-error']">
              {{ teeTime.availableSlots }} / {{ teeTime.maxPlayers }}
            </span>
          </div>

          <div v-if="teeTime.waitlistCount > 0" class="detail-item">
            <span class="label">Waitlist:</span>
            <span class="badge badge-warning">{{ teeTime.waitlistCount }} waiting</span>
          </div>

          <div v-if="teeTime.notes" class="detail-item">
            <span class="label">Notes:</span>
            <p class="notes">{{ teeTime.notes }}</p>
          </div>

          <div class="detail-item">
            <span class="label">Created by:</span>
            <span>{{ teeTime.createdBy.displayName }}</span>
          </div>
        </div>

        <div class="actions">
          <button
            v-if="isUserRegistered"
            @click="handleCancel"
            class="btn btn-danger"
            :disabled="teeTimeStore.loading"
          >
            Cancel My Registration
          </button>
          <span v-else-if="isUserRegisteredForDay" class="text-muted">
            You already have a tee time booked for this day
          </span>
          <button
            v-else-if="canRegister"
            @click="handleRegister"
            class="btn btn-primary"
            :disabled="teeTimeStore.loading"
          >
            Register for This Tee Time
          </button>
          <template v-else-if="isFull && !isUserRegistered">
            <div class="waitlist-actions" v-if="isDayFull">
              <button
                v-if="teeTime.isUserOnWaitlist"
                @click="handleLeaveWaitlist"
                class="btn btn-outline"
                :disabled="waitlistStore.loading"
              >
                Leave Waitlist
              </button>
              <button
                v-else
                @click="handleJoinWaitlist"
                class="btn btn-warning"
                :disabled="waitlistStore.loading"
              >
                Join Waitlist for This Day
              </button>
              <span v-if="teeTime.isUserOnWaitlist" class="waitlist-position text-muted">
                You are #{{ teeTime.waitlistPosition }} on the waitlist for this day
              </span>
              <span v-else class="text-muted text-sm">
                All tee times today are full — join the day's waitlist
              </span>
            </div>
            <span v-else class="text-muted">
              This tee time is full — other times today may still have spots
            </span>
          </template>
        </div>
      </div>

      <div class="card mt-2">
        <RegistrationList
          :registrations="teeTime.registrations"
          :max-players="teeTime.maxPlayers"
        />
      </div>
    </template>

    <div v-else class="empty-state">
      <p>Tee time not found.</p>
    </div>
  </div>
</template>

<style scoped>
.back-link {
  display: inline-block;
  margin-bottom: 1rem;
  color: var(--primary-color);
  text-decoration: none;
  background: none;
  border: none;
  cursor: pointer;
  font-size: 0.875rem;
}

.back-link:hover {
  text-decoration: underline;
}

.tee-time-value {
  font-size: 1.5rem;
  color: var(--primary-color);
  font-weight: 600;
}

.tee-time-details {
  margin: 1.5rem 0;
}

.detail-item {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  margin-bottom: 0.75rem;
}

.detail-item .label {
  font-weight: 500;
  color: var(--text-secondary);
  min-width: 120px;
}

.notes {
  margin: 0;
  color: var(--text-primary);
}

.actions {
  padding-top: 1rem;
  border-top: 1px solid var(--border-color);
}

.waitlist-actions {
  display: flex;
  align-items: center;
  gap: 1rem;
  flex-wrap: wrap;
}

.waitlist-position {
  font-size: 0.875rem;
}

.btn-warning {
  background-color: var(--warning-color, #f59e0b);
  color: white;
  border: none;
}

.btn-warning:hover:not(:disabled) {
  background-color: var(--warning-hover-color, #d97706);
}
</style>
