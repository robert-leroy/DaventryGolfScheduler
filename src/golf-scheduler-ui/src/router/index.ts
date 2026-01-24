import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '@/stores/authStore';
import HomeView from '@/views/HomeView.vue';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/tee-times',
      name: 'tee-times',
      component: () => import('@/views/TeeTimesView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/tee-times/:id',
      name: 'tee-time-detail',
      component: () => import('@/views/TeeTimeDetailView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/my-registrations',
      name: 'my-registrations',
      component: () => import('@/views/MyRegistrationsView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/golfers-by-day',
      name: 'golfers-by-day',
      component: () => import('@/views/GolfersByDayView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/profile',
      name: 'profile',
      component: () => import('@/views/ProfileView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/admin',
      name: 'admin',
      component: () => import('@/views/AdminView.vue'),
      meta: { requiresAuth: true, requiresAdmin: true },
    },
    {
      path: '/admin/users',
      name: 'admin-users',
      component: () => import('@/views/AdminUsersView.vue'),
      meta: { requiresAuth: true, requiresAdmin: true },
    },
  ],
});

router.beforeEach(async (to, _from, next) => {
  const authStore = useAuthStore();

  if (!authStore.initialized) {
    await authStore.initialize();
  }

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next({ name: 'home' });
    return;
  }

  if (to.meta.requiresAdmin && !authStore.isAdmin) {
    next({ name: 'home' });
    return;
  }

  next();
});

export default router;
