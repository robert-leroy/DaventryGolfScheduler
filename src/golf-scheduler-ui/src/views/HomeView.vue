<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { useAuthStore } from '@/stores/authStore';
import { useTeeTimeStore } from '@/stores/teeTimeStore';
import TeeTimeCard from '@/components/TeeTimeCard.vue';
import TeeTimeChart from '@/components/TeeTimeChart.vue';
import LoadingSpinner from '@/components/LoadingSpinner.vue';

const authStore = useAuthStore();
const teeTimeStore = useTeeTimeStore();

const nextTeeTime = computed(() => {
  return teeTimeStore.teeTimes[0] || null;
});

onMounted(async () => {
  if (authStore.isAuthenticated) {
    await teeTimeStore.fetchTeeTimes();
  }
});

async function handleRegister(id: string) {
  await teeTimeStore.register(id);
}

async function handleCancel(id: string) {
  await teeTimeStore.cancelRegistration(id);
}
</script>

<template>
  <div class="home">
    <section v-if="!authStore.isAuthenticated" class="hero">
      <h1 class="hero-title">Neighborhood Golf Scheduler</h1>
      <p class="hero-subtitle">Sign up for weekly tee times with your neighbors</p>

      <div class="hero-actions">
        <RouterLink to="/login" class="btn btn-primary btn-lg">
          Sign In to Get Started
        </RouterLink>
      </div>
    </section>

    <template v-if="authStore.isAuthenticated">
      <div v-if="!teeTimeStore.loading" class="dashboard-top">
        <section class="next-tee-time">
          <h2 class="section-title">Next Tee Time</h2>

          <div v-if="nextTeeTime">
            <TeeTimeCard
              :tee-time="nextTeeTime"
              @register="handleRegister"
              @cancel="handleCancel"
            />
          </div>

          <div v-else class="empty-state">
            <p>No upcoming tee times scheduled.</p>
            <p class="text-muted text-sm">Check back later or ask an admin to add some!</p>
          </div>
        </section>
        <section class="dashboard-chart">
          <TeeTimeChart :tee-times="teeTimeStore.teeTimes" />
        </section>
      </div>

      <LoadingSpinner v-else text="Loading tee times..." />

      <section class="quick-links">
        <h2 class="section-title">Quick Links</h2>
        <div class="links-grid">
          <RouterLink to="/tee-times" class="link-card">
            <h3>All Tee Times</h3>
            <p class="text-muted">View and register for all upcoming tee times</p>
          </RouterLink>
          <RouterLink to="/my-registrations" class="link-card">
            <h3>My Registrations</h3>
            <p class="text-muted">See all your upcoming golf rounds</p>
          </RouterLink>
          <RouterLink v-if="authStore.isAdmin" to="/admin" class="link-card">
            <h3>Admin Dashboard</h3>
            <p class="text-muted">Manage tee times and users</p>
          </RouterLink>
        </div>
      </section>
    </template>
  </div>
</template>

<style scoped>
.dashboard-top {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
  align-items: start;
  margin-bottom: 2rem;
}

.dashboard-chart {
  min-height: 0;
}

.hero {
  text-align: center;
  padding: 3rem 0;
  background: linear-gradient(135deg, var(--primary-color) 0%, var(--primary-dark) 100%);
  color: white;
  border-radius: 8px;
  margin-bottom: 2rem;
}

.hero-title {
  font-size: 2.5rem;
  font-weight: 700;
  margin-bottom: 0.5rem;
}

.hero-subtitle {
  font-size: 1.25rem;
  opacity: 0.9;
  margin-bottom: 1.5rem;
}

.hero-actions {
  margin-top: 1.5rem;
}

.btn-lg {
  padding: 0.75rem 2rem;
  font-size: 1rem;
}

.section-title {
  font-size: 1.5rem;
  font-weight: 600;
  margin-bottom: 1rem;
  color: var(--text-primary);
}

.next-tee-time {
  min-width: 0;
}

.quick-links {
  margin-bottom: 2rem;
}

.links-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1rem;
}

.link-card {
  display: block;
  padding: 1.5rem;
  background-color: var(--surface);
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  text-decoration: none;
  transition: box-shadow 0.2s, transform 0.2s;
}

.link-card:hover {
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
  transform: translateY(-2px);
}

.link-card h3 {
  color: var(--primary-color);
  font-size: 1.125rem;
  margin-bottom: 0.5rem;
}

.link-card p {
  font-size: 0.875rem;
}

@media (max-width: 768px) {
  .dashboard-top {
    grid-template-columns: 1fr;
  }

  .hero-title {
    font-size: 1.75rem;
  }

  .hero-subtitle {
    font-size: 1rem;
  }
}
</style>
