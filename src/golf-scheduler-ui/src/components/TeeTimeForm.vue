<script setup lang="ts">
import { ref, watch } from 'vue';
import type { TeeTime, TeeTimeCreate } from '@/types';

const props = defineProps<{
  teeTime?: TeeTime | null;
  loading?: boolean;
}>();

const emit = defineEmits<{
  submit: [data: TeeTimeCreate];
  cancel: [];
}>();

const form = ref<TeeTimeCreate>({
  teeDate: '',
  teeTime: '',
  maxPlayers: 4,
  notes: '',
});

watch(
  () => props.teeTime,
  (teeTime) => {
    if (teeTime) {
      form.value = {
        teeDate: teeTime.teeDate,
        teeTime: teeTime.teeTime,
        maxPlayers: teeTime.maxPlayers,
        notes: teeTime.notes || '',
      };
    } else {
      form.value = {
        teeDate: '',
        teeTime: '',
        maxPlayers: 4,
        notes: '',
      };
    }
  },
  { immediate: true }
);

function handleSubmit() {
  emit('submit', { ...form.value });
}
</script>

<template>
  <form @submit.prevent="handleSubmit" class="tee-time-form">
    <div class="form-row">
      <div class="form-group">
        <label for="teeDate" class="form-label">Date</label>
        <input
          id="teeDate"
          v-model="form.teeDate"
          type="date"
          class="form-input"
          required
        />
      </div>

      <div class="form-group">
        <label for="teeTime" class="form-label">Time</label>
        <input
          id="teeTime"
          v-model="form.teeTime"
          type="time"
          class="form-input"
          required
        />
      </div>
    </div>

    <div class="form-group">
      <label for="maxPlayers" class="form-label">Maximum Players</label>
      <select id="maxPlayers" v-model.number="form.maxPlayers" class="form-select">
        <option :value="2">2 Players</option>
        <option :value="3">3 Players</option>
        <option :value="4">4 Players</option>
      </select>
    </div>

    <div class="form-group">
      <label for="notes" class="form-label">Notes (optional)</label>
      <textarea
        id="notes"
        v-model="form.notes"
        class="form-textarea"
        rows="3"
        placeholder="Any special instructions or notes..."
      ></textarea>
    </div>

    <div class="form-actions">
      <button type="button" @click="emit('cancel')" class="btn btn-outline">
        Cancel
      </button>
      <button type="submit" class="btn btn-primary" :disabled="loading">
        {{ teeTime ? 'Update' : 'Create' }} Tee Time
      </button>
    </div>
  </form>
</template>

<style scoped>
.tee-time-form {
  max-width: 500px;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.form-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  margin-top: 1.5rem;
}

@media (max-width: 480px) {
  .form-row {
    grid-template-columns: 1fr;
  }
}
</style>
