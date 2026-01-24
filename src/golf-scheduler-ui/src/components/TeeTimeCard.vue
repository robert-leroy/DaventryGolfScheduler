<script setup lang="ts">
import { computed } from 'vue';
import { RouterLink } from 'vue-router';
import type { TeeTimeListItem } from '@/types';

const props = defineProps<{
  teeTime: TeeTimeListItem;
}>();

const emit = defineEmits<{
  register: [id: string];
  cancel: [id: string];
}>();

const formattedDate = computed(() => {
  const date = new Date(props.teeTime.teeDate);
  return date.toLocaleDateString('en-US', {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
  });
});

const formattedTime = computed(() => {
  const [hours, minutes] = props.teeTime.teeTime.split(':');
  const date = new Date();
  date.setHours(parseInt(hours), parseInt(minutes));
  return date.toLocaleTimeString('en-US', {
    hour: 'numeric',
    minute: '2-digit',
    hour12: true,
  });
});

const isFull = computed(() => props.teeTime.availableSlots === 0);
</script>

<template>
  <div class="tee-time-card card">
    <div class="card-content">
      <div class="tee-time-info">
        <div class="date-time">
          <span class="date">{{ formattedDate }}</span>
          <span class="time">{{ formattedTime }}</span>
        </div>
        <div class="slots">
          <span :class="['badge', isFull ? 'badge-error' : 'badge-success']">
            {{ teeTime.availableSlots }} / {{ teeTime.maxPlayers }} spots available
          </span>
        </div>
        <p v-if="teeTime.notes" class="notes text-muted text-sm">{{ teeTime.notes }}</p>
      </div>

      <div class="card-actions">
        <RouterLink :to="`/tee-times/${teeTime.id}`" class="btn btn-outline">
          View Details
        </RouterLink>
        <button
          v-if="teeTime.isUserRegistered"
          @click="emit('cancel', teeTime.id)"
          class="btn btn-danger"
        >
          Cancel
        </button>
        <button
          v-else-if="!isFull"
          @click="emit('register', teeTime.id)"
          class="btn btn-primary"
        >
          Register
        </button>
        <span v-else class="text-muted text-sm">Full</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.card-content {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 1rem;
  flex-wrap: wrap;
}

.tee-time-info {
  flex: 1;
}

.date-time {
  display: flex;
  align-items: baseline;
  gap: 0.75rem;
  margin-bottom: 0.5rem;
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

.slots {
  margin-bottom: 0.5rem;
}

.notes {
  max-width: 400px;
}

.card-actions {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}
</style>
