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
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
      meta: { guestOnly: true },
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('@/views/RegisterView.vue'),
      meta: { guestOnly: true },
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

  // Clear any login errors when navigating
  authStore.clearError();

  // Redirect authenticated users away from guest-only pages (login, register)
  if (to.meta.guestOnly && authStore.isAuthenticated) {
    next({ name: 'home' });
    return;
  }

  // Redirect unauthenticated users to login with return URL
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next({ name: 'login', query: { redirect: to.fullPath } });
    return;
  }

  // Redirect non-admin users away from admin pages
  if (to.meta.requiresAdmin && !authStore.isAdmin) {
    next({ name: 'home' });
    return;
  }

  next();
});

export default router;
