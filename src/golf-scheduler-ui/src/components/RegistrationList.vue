<script setup lang="ts">
import type { Registration } from '@/types';

defineProps<{
  registrations: Registration[];
  maxPlayers: number;
}>();

function formatDate(dateString: string) {
  return new Date(dateString).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  });
}
</script>

<template>
  <div class="registration-list">
    <h3 class="list-title">
      Registered Players ({{ registrations.length }} / {{ maxPlayers }})
    </h3>

    <div v-if="registrations.length === 0" class="empty-state">
      <p class="text-muted">No one has registered yet. Be the first!</p>
    </div>

    <ul v-else class="player-list">
      <li v-for="reg in registrations" :key="reg.id" class="player-item">
        <span class="player-name">{{ reg.userDisplayName }}</span>
        <span class="registered-at text-muted text-sm">
          Registered {{ formatDate(reg.registeredAt) }}
        </span>
      </li>
    </ul>

    <div v-if="registrations.length < maxPlayers" class="open-slots">
      <div
        v-for="n in (maxPlayers - registrations.length)"
        :key="n"
        class="open-slot"
      >
        <span class="text-muted">Open spot</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.list-title {
  font-size: 1rem;
  font-weight: 600;
  margin-bottom: 1rem;
  color: var(--text-primary);
}

.player-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.player-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem;
  background-color: #e8f5e9;
  border-radius: 4px;
  margin-bottom: 0.5rem;
}

.player-name {
  font-weight: 500;
}

.open-slots {
  margin-top: 0.5rem;
}

.open-slot {
  padding: 0.75rem;
  background-color: var(--background);
  border: 1px dashed var(--border-color);
  border-radius: 4px;
  margin-bottom: 0.5rem;
  text-align: center;
}
</style>
