<script setup lang="ts">
import { computed } from 'vue';
import { Chart } from 'vue-chartjs';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarController,
  BarElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js';
import type { ChartData, ChartOptions } from 'chart.js';
import type { TeeTimeListItem } from '@/types';
import { parseLocalDate } from '@/utils/dateUtils';

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarController,
  BarElement,
  Title,
  Tooltip,
  Legend
);

const props = defineProps<{
  teeTimes: TeeTimeListItem[];
}>();

interface DayStats {
  date: string;
  label: string;
  totalSlots: number;
  takenSlots: number;
}

const chartData = computed<ChartData<'bar', number[], string>>(() => {
  // Aggregate tee times by day
  const byDay = new Map<string, DayStats>();

  for (const tt of props.teeTimes) {
    const date = tt.teeDate;
    if (!byDay.has(date)) {
      const dateObj = parseLocalDate(date);
      const label = dateObj.toLocaleDateString('en-US', {
        weekday: 'short',
        month: 'short',
        day: 'numeric',
      });
      byDay.set(date, {
        date,
        label,
        totalSlots: 0,
        takenSlots: 0,
      });
    }
    const stats = byDay.get(date)!;
    stats.totalSlots += tt.maxPlayers;
    stats.takenSlots += tt.registeredCount;
  }

  // Sort by date and take up to 7 days
  const sorted = Array.from(byDay.values())
    .sort((a, b) => a.date.localeCompare(b.date))
    .slice(0, 7);

  return {
    labels: sorted.map((d) => d.label),
    datasets: [
      {
        label: 'Spots Taken',
        data: sorted.map((d) => d.takenSlots),
        backgroundColor: 'rgba(220, 53, 69, 0.7)',
        borderColor: 'rgba(220, 53, 69, 1)',
        borderWidth: 1,
      },
      {
        label: 'Available',
        data: sorted.map((d) => d.totalSlots - d.takenSlots),
        backgroundColor: 'rgba(34, 139, 34, 0.6)',
        borderColor: 'rgba(34, 139, 34, 1)',
        borderWidth: 1,
      },
    ],
  };
});

const chartOptions: ChartOptions<'bar'> = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'top',
    },
    title: {
      display: true,
      text: 'Tee Time Availability',
      font: {
        size: 18,
      },
    },
  },
  scales: {
    x: {
      stacked: true,
    },
    y: {
      stacked: true,
      beginAtZero: true,
      title: {
        display: true,
        text: 'Number of Slots',
      },
      ticks: {
        stepSize: 1,
      },
    },
  },
};
</script>

<template>
  <div class="chart-container">
    <Chart v-if="teeTimes.length > 0" type="bar" :data="chartData" :options="chartOptions" />
    <div v-else class="no-data">
      <p>No tee times scheduled yet.</p>
    </div>
  </div>
</template>

<style scoped>
.chart-container {
  background-color: var(--surface);
  border-radius: 8px;
  padding: 1.5rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  height: 350px;
}

.no-data {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: var(--text-muted);
}
</style>
