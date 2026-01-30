<script setup lang="ts">
import { ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/authStore';

const router = useRouter();
const authStore = useAuthStore();

const email = ref('');
const password = ref('');
const confirmPassword = ref('');
const firstName = ref('');
const lastName = ref('');
const phone = ref('');
const handicap = ref<string>('');

const passwordMismatch = computed(() => {
  return Boolean(password.value && confirmPassword.value && password.value !== confirmPassword.value);
});

const passwordTooShort = computed(() => {
  return Boolean(password.value && password.value.length < 8);
});

async function handleSubmit() {
  if (passwordMismatch.value) {
    return;
  }

  try {
    await authStore.register({
      email: email.value,
      password: password.value,
      firstName: firstName.value,
      lastName: lastName.value,
      phone: phone.value || undefined,
      handicap: handicap.value ? parseInt(handicap.value) : undefined,
    });
    router.push('/');
  } catch {
    // Error is handled by the store
  }
}
</script>

<template>
  <div class="register-container">
    <div class="register-card">
      <h1>Create Account</h1>
      <p class="subtitle">Join us to start booking tee times.</p>

      <form @submit.prevent="handleSubmit" class="register-form">
        <div v-if="authStore.loginError" class="error-message">
          {{ authStore.loginError }}
        </div>

        <div class="form-row">
          <div class="form-group">
            <label for="firstName">First Name *</label>
            <input
              id="firstName"
              v-model="firstName"
              type="text"
              required
              placeholder="First name"
              :disabled="authStore.loading"
            />
          </div>

          <div class="form-group">
            <label for="lastName">Last Name *</label>
            <input
              id="lastName"
              v-model="lastName"
              type="text"
              required
              placeholder="Last name"
              :disabled="authStore.loading"
            />
          </div>
        </div>

        <div class="form-group">
          <label for="email">Email *</label>
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
          <label for="password">Password *</label>
          <input
            id="password"
            v-model="password"
            type="password"
            required
            minlength="8"
            placeholder="At least 8 characters"
            :disabled="authStore.loading"
            :class="{ 'input-error': passwordTooShort }"
          />
          <span v-if="passwordTooShort" class="field-error">Password must be at least 8 characters</span>
        </div>

        <div class="form-group">
          <label for="confirmPassword">Confirm Password *</label>
          <input
            id="confirmPassword"
            v-model="confirmPassword"
            type="password"
            required
            placeholder="Confirm your password"
            :disabled="authStore.loading"
            :class="{ 'input-error': passwordMismatch }"
          />
          <span v-if="passwordMismatch" class="field-error">Passwords do not match</span>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label for="phone">Phone</label>
            <input
              id="phone"
              v-model="phone"
              type="tel"
              placeholder="Optional"
              :disabled="authStore.loading"
            />
          </div>

          <div class="form-group">
            <label for="handicap">Handicap</label>
            <input
              id="handicap"
              v-model="handicap"
              type="number"
              min="0"
              max="54"
              placeholder="Optional"
              :disabled="authStore.loading"
            />
          </div>
        </div>

        <button
          type="submit"
          class="btn btn-primary btn-block"
          :disabled="authStore.loading || passwordMismatch || passwordTooShort"
        >
          {{ authStore.loading ? 'Creating account...' : 'Create Account' }}
        </button>
      </form>

      <div class="register-footer">
        <p>Already have an account? <RouterLink to="/login">Sign in</RouterLink></p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.register-container {
  min-height: calc(100vh - 140px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
}

.register-card {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  padding: 2rem;
  width: 100%;
  max-width: 500px;
}

.register-card h1 {
  margin: 0 0 0.5rem 0;
  font-size: 1.75rem;
  color: #333;
}

.subtitle {
  color: #666;
  margin-bottom: 1.5rem;
}

.register-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

@media (max-width: 480px) {
  .form-row {
    grid-template-columns: 1fr;
  }
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

.input-error {
  border-color: #c62828 !important;
}

.field-error {
  color: #c62828;
  font-size: 0.75rem;
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

.register-footer {
  margin-top: 1.5rem;
  text-align: center;
  color: #666;
}

.register-footer a {
  color: #2e7d32;
  text-decoration: none;
  font-weight: 500;
}

.register-footer a:hover {
  text-decoration: underline;
}
</style>
