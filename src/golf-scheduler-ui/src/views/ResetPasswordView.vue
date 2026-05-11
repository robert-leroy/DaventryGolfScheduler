<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { authApi } from '@/services/api';

const route = useRoute();

const token = ref('');
const newPassword = ref('');
const confirmPassword = ref('');
const loading = ref(false);
const errorMessage = ref('');
const success = ref(false);

const passwordMismatch = computed(
  () => confirmPassword.value.length > 0 && newPassword.value !== confirmPassword.value
);

onMounted(() => {
  token.value = (route.query.token as string) ?? '';
  if (!token.value) {
    errorMessage.value = 'Invalid password reset link. Please request a new one.';
  }
});

async function handleSubmit() {
  if (passwordMismatch.value) return;

  loading.value = true;
  errorMessage.value = '';

  try {
    await authApi.resetPassword(token.value, newPassword.value);
    success.value = true;
  } catch (err: unknown) {
    const error = err as { response?: { data?: { message?: string } } };
    errorMessage.value = error.response?.data?.message ?? 'This reset link is invalid or has expired.';
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <div class="page-container">
    <div class="card">
      <h1>Reset Password</h1>

      <div v-if="success" class="success-message">
        <p>Your password has been reset successfully.</p>
        <RouterLink to="/login" class="btn btn-primary">Sign In</RouterLink>
      </div>

      <template v-else>
        <p class="subtitle">Enter your new password below.</p>

        <div v-if="errorMessage" class="error-message">{{ errorMessage }}</div>

        <form v-if="token" @submit.prevent="handleSubmit" class="form">
          <div class="form-group">
            <label for="new-password">New Password</label>
            <input
              id="new-password"
              v-model="newPassword"
              type="password"
              required
              minlength="8"
              placeholder="At least 8 characters"
              :disabled="loading"
            />
          </div>

          <div class="form-group">
            <label for="confirm-password">Confirm Password</label>
            <input
              id="confirm-password"
              v-model="confirmPassword"
              type="password"
              required
              placeholder="Confirm your new password"
              :disabled="loading"
              :class="{ 'input-error': passwordMismatch }"
            />
            <span v-if="passwordMismatch" class="field-error">Passwords do not match.</span>
          </div>

          <button
            type="submit"
            class="btn btn-primary btn-block"
            :disabled="loading || passwordMismatch || !newPassword || !confirmPassword"
          >
            {{ loading ? 'Resetting...' : 'Reset Password' }}
          </button>
        </form>

        <div class="footer" v-if="!token">
          <RouterLink to="/forgot-password">Request a new reset link</RouterLink>
        </div>
        <div class="footer" v-else>
          <RouterLink to="/login">Back to Sign In</RouterLink>
        </div>
      </template>
    </div>
  </div>
</template>

<style scoped>
.page-container {
  min-height: calc(100vh - 140px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
}

.card {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  padding: 2rem;
  width: 100%;
  max-width: 400px;
}

.card h1 {
  margin: 0 0 0.5rem 0;
  font-size: 1.75rem;
  color: #333;
}

.subtitle {
  color: #666;
  margin-bottom: 1.5rem;
}

.form {
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

.form-group input.input-error {
  border-color: #c62828;
}

.field-error {
  font-size: 0.8rem;
  color: #c62828;
}

.error-message {
  background-color: #ffebee;
  color: #c62828;
  padding: 0.75rem;
  border-radius: 4px;
  font-size: 0.875rem;
  margin-bottom: 1rem;
}

.btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.2s;
  text-decoration: none;
  display: inline-block;
  text-align: center;
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

.success-message {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  color: #333;
}

.success-message p {
  background-color: #e8f5e9;
  color: #1b5e20;
  padding: 1rem;
  border-radius: 4px;
  margin: 0;
}

.footer {
  margin-top: 1.5rem;
  text-align: center;
}

.footer a {
  color: #2e7d32;
  text-decoration: none;
  font-weight: 500;
  font-size: 0.9rem;
}

.footer a:hover {
  text-decoration: underline;
}
</style>
