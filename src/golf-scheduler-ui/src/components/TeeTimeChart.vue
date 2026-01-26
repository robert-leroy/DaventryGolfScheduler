<script setup lang="ts">
import { computed } from 'vue';
import { Chart } from 'vue-chartjs';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarController,
  LineController,
  BarElement,
  LineElement,
  PointElement,
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
  LineController,
  BarElement,
  LineElement,
  PointElement,
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

const chartData = computed<ChartData<'bar' | 'line', number[], string>>(() => {
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
        type: 'bar' as const,
        label: 'Total Slots',
        data: sorted.map((d) => d.totalSlots),
        backgroundColor: 'rgba(34, 139, 34, 0.6)',
        borderColor: 'rgba(34, 139, 34, 1)',
        borderWidth: 1,
        order: 2,
      },
      {
        type: 'line' as const,
        label: 'Spots Taken',
        data: sorted.map((d) => d.takenSlots),
        borderColor: 'rgba(255, 99, 132, 1)',
        backgroundColor: 'rgba(255, 99, 132, 0.2)',
        borderWidth: 3,
        pointRadius: 5,
        pointBackgroundColor: 'rgba(255, 99, 132, 1)',
        fill: false,
        order: 1,
      },
    ],
  } as ChartData<'bar' | 'line', number[], string>;
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
    y: {
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
