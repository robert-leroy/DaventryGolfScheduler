<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { teeTimeApi } from '@/services/api';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import type { GolfersByDay } from '@/types';
import { parseLocalDate } from '@/utils/dateUtils';

const golfersByDay = ref<GolfersByDay[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

onMounted(async () => {
  await fetchGolfersByDay();
});

async function fetchGolfersByDay() {
  loading.value = true;
  error.value = null;
  try {
    golfersByDay.value = await teeTimeApi.getGolfersByDay();
  } catch (e) {
    error.value = 'Failed to load golfers';
    console.error(e);
  } finally {
    loading.value = false;
  }
}

function formatDate(dateString: string) {
  const date = parseLocalDate(dateString);
  return date.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
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

function printSchedule() {
  window.print();
}
</script>

<template>
  <div class="golfers-by-day">
    <div class="page-header">
      <h1 class="page-title">Golfers by Day</h1>
      <button
        v-if="golfersByDay.length > 0 && !loading"
        class="btn btn-secondary print-btn"
        @click="printSchedule"
      >
        Print Schedule
      </button>
    </div>

    <div v-if="error" class="alert alert-error">
      {{ error }}
    </div>

    <LoadingSpinner v-if="loading" text="Loading golfers..." />

    <template v-else>
      <div v-if="golfersByDay.length === 0" class="empty-state">
        <p>No upcoming tee times with registered golfers.</p>
      </div>

      <div v-else class="days-list">
        <div v-for="day in golfersByDay" :key="day.date" class="day-card card">
          <div class="day-header">
            <h2 class="day-title">{{ day.dayOfWeek }}</h2>
            <span class="day-date">{{ formatDate(day.date) }}</span>
          </div>

          <div class="tee-times-list">
            <div v-for="teeTime in day.teeTimes" :key="teeTime.id" class="tee-time-slot">
              <div class="tee-time-header">
                <span class="tee-time-value">{{ formatTime(teeTime.teeTime) }}</span>
                <span v-if="teeTime.notes" class="tee-time-notes text-muted text-sm">
                  {{ teeTime.notes }}
                </span>
              </div>

              <div v-if="teeTime.golfers.length === 0" class="no-golfers text-muted">
                No golfers registered
              </div>

              <ul v-else class="golfers-list">
                <li v-for="golfer in teeTime.golfers" :key="golfer" class="golfer-item">
                  {{ golfer }}
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
.days-list {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.day-card {
  padding: 0;
  overflow: hidden;
}

.day-header {
  background: linear-gradient(135deg, var(--primary-color) 0%, var(--primary-dark) 100%);
  color: white;
  padding: 1rem 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.day-title {
  font-size: 1.25rem;
  font-weight: 600;
  margin: 0;
}

.day-date {
  font-size: 0.875rem;
  opacity: 0.9;
}

.tee-times-list {
  padding: 1rem 1.5rem;
}

.tee-time-slot {
  padding: 1rem 0;
  border-bottom: 1px solid var(--border-color);
}

.tee-time-slot:last-child {
  border-bottom: none;
  padding-bottom: 0;
}

.tee-time-slot:first-child {
  padding-top: 0;
}

.tee-time-header {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 0.5rem;
}

.tee-time-value {
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--primary-color);
}

.tee-time-notes {
  font-style: italic;
}

.golfers-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.golfer-item {
  background-color: #e8f5e9;
  color: var(--primary-dark);
  padding: 0.25rem 0.75rem;
  border-radius: 16px;
  font-size: 0.875rem;
  font-weight: 500;
}

.no-golfers {
  font-style: italic;
  font-size: 0.875rem;
}

/* Print styles */
@media print {
  .print-btn {
    display: none !important;
  }

  .golfers-by-day {
    padding: 0;
  }

  .page-header {
    margin-bottom: 1rem;
  }

  .page-title {
    font-size: 1.5rem;
    color: black;
  }

  .days-list {
    gap: 1rem;
  }

  .day-card {
    break-inside: avoid;
    box-shadow: none;
    border: 1px solid #ccc;
  }

  .day-header {
    background: #f0f0f0 !important;
    color: black !important;
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
  }

  .day-title {
    color: black;
  }

  .tee-time-value {
    color: black;
    font-weight: bold;
  }

  .golfer-item {
    background-color: #f5f5f5 !important;
    color: black !important;
    border: 1px solid #ccc;
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
  }

  .tee-times-list {
    padding: 0.75rem 1rem;
  }

  .tee-time-slot {
    padding: 0.5rem 0;
  }
}
</style>
