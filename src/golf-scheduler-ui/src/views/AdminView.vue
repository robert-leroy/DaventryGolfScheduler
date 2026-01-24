<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { RouterLink } from 'vue-router';
import { useTeeTimeStore } from '@/stores/teeTimeStore';
import TeeTimeForm from '@/components/TeeTimeForm.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import type { TeeTimeCreate, TeeTimeListItem } from '@/types';

const teeTimeStore = useTeeTimeStore();

const showForm = ref(false);
const editingTeeTime = ref<TeeTimeListItem | null>(null);

onMounted(async () => {
  await teeTimeStore.fetchTeeTimes();
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

function openCreateForm() {
  editingTeeTime.value = null;
  showForm.value = true;
}

function openEditForm(teeTime: TeeTimeListItem) {
  editingTeeTime.value = teeTime;
  showForm.value = true;
}

function closeForm() {
  showForm.value = false;
  editingTeeTime.value = null;
}

async function handleSubmit(data: TeeTimeCreate) {
  try {
    if (editingTeeTime.value) {
      await teeTimeStore.updateTeeTime(editingTeeTime.value.id, data);
    } else {
      await teeTimeStore.createTeeTime(data);
    }
    closeForm();
  } catch {
    // Error handling is done in the store
  }
}

async function handleDelete(id: string) {
  if (confirm('Are you sure you want to delete this tee time? All registrations will be removed.')) {
    await teeTimeStore.deleteTeeTime(id);
  }
}
</script>

<template>
  <div class="admin-view">
    <div class="page-header flex flex-between flex-center">
      <h1 class="page-title">Admin Dashboard</h1>
      <RouterLink to="/admin/users" class="btn btn-outline">
        Manage Users
      </RouterLink>
    </div>

    <div v-if="teeTimeStore.error" class="alert alert-error">
      {{ teeTimeStore.error }}
    </div>

    <div class="card mb-2">
      <div class="card-header flex flex-between flex-center">
        <h2 class="card-title">Manage Tee Times</h2>
        <button v-if="!showForm" @click="openCreateForm" class="btn btn-primary">
          Add Tee Time
        </button>
      </div>

      <div v-if="showForm" class="form-container">
        <h3 class="mb-1">{{ editingTeeTime ? 'Edit' : 'Create' }} Tee Time</h3>
        <TeeTimeForm
          :tee-time="editingTeeTime as any"
          :loading="teeTimeStore.loading"
          @submit="handleSubmit"
          @cancel="closeForm"
        />
      </div>
    </div>

    <LoadingSpinner v-if="teeTimeStore.loading && !showForm" text="Loading tee times..." />

    <template v-else>
      <div v-if="teeTimeStore.teeTimes.length === 0" class="empty-state">
        <p>No tee times scheduled.</p>
        <button @click="openCreateForm" class="btn btn-primary mt-1">
          Create First Tee Time
        </button>
      </div>

      <div v-else class="tee-times-table">
        <table>
          <thead>
            <tr>
              <th>Date</th>
              <th>Time</th>
              <th>Players</th>
              <th>Notes</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="teeTime in teeTimeStore.teeTimes" :key="teeTime.id">
              <td>{{ formatDate(teeTime.teeDate) }}</td>
              <td>{{ formatTime(teeTime.teeTime) }}</td>
              <td>
                <span :class="['badge', teeTime.availableSlots > 0 ? 'badge-success' : 'badge-error']">
                  {{ teeTime.registeredCount }} / {{ teeTime.maxPlayers }}
                </span>
              </td>
              <td class="notes-cell">{{ teeTime.notes || '-' }}</td>
              <td class="actions-cell">
                <button @click="openEditForm(teeTime)" class="btn btn-outline btn-sm">
                  Edit
                </button>
                <button @click="handleDelete(teeTime.id)" class="btn btn-danger btn-sm">
                  Delete
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>
  </div>
</template>

<style scoped>
.form-container {
  padding: 1rem;
  background-color: var(--background);
  border-radius: 4px;
  margin-top: 1rem;
}

.tee-times-table {
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
  background-color: var(--surface);
  border-radius: 8px;
  overflow: hidden;
}

th, td {
  padding: 0.75rem 1rem;
  text-align: left;
  border-bottom: 1px solid var(--border-color);
}

th {
  background-color: var(--background);
  font-weight: 600;
  color: var(--text-secondary);
  font-size: 0.875rem;
}

tr:last-child td {
  border-bottom: none;
}

.notes-cell {
  max-width: 200px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.actions-cell {
  white-space: nowrap;
}

.btn-sm {
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
}

.actions-cell .btn {
  margin-right: 0.25rem;
}
</style>
