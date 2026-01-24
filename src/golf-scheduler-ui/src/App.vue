<script setup lang="ts">
import { onMounted } from 'vue';
import { useAuthStore } from '@/stores/authStore';
import AppHeader from '@/components/AppHeader.vue';

const authStore = useAuthStore();

onMounted(async () => {
  await authStore.initialize();
});
</script>

<template>
  <div class="app">
    <AppHeader />
    <main class="main-content">
      <div class="container">
        <RouterView />
      </div>
    </main>
  </div>
</template>

<style scoped>
.app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.main-content {
  flex: 1;
  padding: 2rem 0;
}
</style>
