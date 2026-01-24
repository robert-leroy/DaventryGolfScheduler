<script setup lang="ts">
import { useAuthStore } from '@/stores/authStore';
import { RouterLink } from 'vue-router';

const authStore = useAuthStore();

async function handleLogin() {
  await authStore.login();
}

async function handleLogout() {
  await authStore.logout();
}
</script>

<template>
  <header class="header">
    <div class="container">
      <nav class="nav">
        <RouterLink to="/" class="logo">Golf Scheduler</RouterLink>

        <div class="nav-links" v-if="authStore.isAuthenticated">
          <RouterLink to="/tee-times" class="nav-link">Tee Times</RouterLink>
          <RouterLink to="/golfers-by-day" class="nav-link">By Day</RouterLink>
          <RouterLink to="/my-registrations" class="nav-link">My Registrations</RouterLink>
          <RouterLink to="/admin" class="nav-link" v-if="authStore.isAdmin">Admin</RouterLink>
        </div>

        <div class="nav-actions">
          <template v-if="authStore.isAuthenticated">
            <RouterLink to="/profile" class="user-name-link">{{ authStore.user?.displayName }}</RouterLink>
            <button @click="handleLogout" class="btn btn-outline" :disabled="authStore.loading">
              Sign Out
            </button>
          </template>
          <template v-else>
            <button @click="handleLogin" class="btn btn-primary" :disabled="authStore.loading">
              Sign In
            </button>
          </template>
        </div>
      </nav>
    </div>
  </header>
</template>

<style scoped>
.header {
  background-color: var(--surface);
  border-bottom: 1px solid var(--border-color);
  padding: 1rem 0;
}

.nav {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 1rem;
}

.logo {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--primary-color);
  text-decoration: none;
}

.nav-links {
  display: flex;
  gap: 1.5rem;
}

.nav-link {
  color: var(--text-primary);
  text-decoration: none;
  font-weight: 500;
  transition: color 0.2s;
}

.nav-link:hover {
  color: var(--primary-color);
}

.nav-link.router-link-active {
  color: var(--primary-color);
}

.nav-actions {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.user-name-link {
  color: var(--text-secondary);
  font-size: 0.875rem;
  text-decoration: none;
  transition: color 0.2s;
}

.user-name-link:hover {
  color: var(--primary-color);
}

@media (max-width: 768px) {
  .nav-links {
    order: 3;
    width: 100%;
    justify-content: center;
  }
}
</style>
