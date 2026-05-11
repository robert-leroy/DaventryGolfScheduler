<script setup lang="ts">
import { ref } from 'vue';
import { authApi } from '@/services/api';

const email = ref('');
const loading = ref(false);
const submitted = ref(false);

async function handleSubmit() {
  loading.value = true;
  try {
    await authApi.forgotPassword(email.value);
  } catch {
    // Swallow all errors — we always show the generic message
  } finally {
    loading.value = false;
    submitted.value = true;
  }
}
</script>

<template>
  <div class="page-container">
    <div class="card">
      <h1>Forgot Password</h1>

      <div v-if="submitted" class="success-message">
        <p>
          If that email is a valid user for this application, you will receive a reset email shortly.
        </p>
        <RouterLink to="/login" class="btn btn-primary">Back to Sign In</RouterLink>
      </div>

      <template v-else>
        <p class="subtitle">Enter the email address associated with your account and we'll send you a reset link.</p>

        <form @submit.prevent="handleSubmit" class="form">
          <div class="form-group">
            <label for="email">Email</label>
            <input
              id="email"
              v-model="email"
              type="email"
              required
              placeholder="Enter your email"
              :disabled="loading"
            />
          </div>

          <button type="submit" class="btn btn-primary btn-block" :disabled="loading">
            {{ loading ? 'Sending...' : 'Send Reset Link' }}
          </button>
        </form>

        <div class="footer">
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
