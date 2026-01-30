<script setup lang="ts">
import { ref } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useAuthStore } from '@/stores/authStore';

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();

const email = ref('');
const password = ref('');

async function handleSubmit() {
  try {
    await authStore.login(email.value, password.value);
    // Redirect to the original destination or home
    const redirect = route.query.redirect as string;
    router.push(redirect || '/');
  } catch {
    // Error is handled by the store
  }
}
</script>

<template>
  <div class="login-container">
    <div class="login-card">
      <h1>Sign In</h1>
      <p class="subtitle">Welcome back! Please sign in to your account.</p>

      <form @submit.prevent="handleSubmit" class="login-form">
        <div v-if="authStore.loginError" class="error-message">
          {{ authStore.loginError }}
        </div>

        <div class="form-group">
          <label for="email">Email</label>
          <input
            id="email"
            v-model="email"
            type="email"
            required
            placeholder="Enter your email"
            :disabled="authStore.loading"
          />
        </div>

        <div class="form-group">
          <label for="password">Password</label>
          <input
            id="password"
            v-model="password"
            type="password"
            required
            placeholder="Enter your password"
            :disabled="authStore.loading"
          />
        </div>

        <button type="submit" class="btn btn-primary btn-block" :disabled="authStore.loading">
          {{ authStore.loading ? 'Signing in...' : 'Sign In' }}
        </button>
      </form>

      <div class="login-footer">
        <p>Don't have an account? <RouterLink to="/register">Create one</RouterLink></p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.login-container {
  min-height: calc(100vh - 140px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
}

.login-card {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  padding: 2rem;
  width: 100%;
  max-width: 400px;
}

.login-card h1 {
  margin: 0 0 0.5rem 0;
  font-size: 1.75rem;
  color: #333;
}

.subtitle {
  color: #666;
  margin-bottom: 1.5rem;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-group label {
  font-weight: 500;
  color: #333;
}

.form-group input {
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
}

.form-group input:focus {
  outline: none;
  border-color: #2e7d32;
  box-shadow: 0 0 0 2px rgba(46, 125, 50, 0.1);
}

.form-group input:disabled {
  background-color: #f5f5f5;
  cursor: not-allowed;
}

.error-message {
  background-color: #ffebee;
  color: #c62828;
  padding: 0.75rem;
  border-radius: 4px;
  font-size: 0.875rem;
}

.btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.2s;
}

.btn-primary {
  background-color: #2e7d32;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background-color: #1b5e20;
}

.btn-primary:disabled {
  background-color: #a5d6a7;
  cursor: not-allowed;
}

.btn-block {
  width: 100%;
}

.login-footer {
  margin-top: 1.5rem;
  text-align: center;
  color: #666;
}

.login-footer a {
  color: #2e7d32;
  text-decoration: none;
  font-weight: 500;
}

.login-footer a:hover {
  text-decoration: underline;
}
</style>
