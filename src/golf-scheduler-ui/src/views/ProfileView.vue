<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useAuthStore } from '@/stores/authStore';
import { userApi } from '@/services/api';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import type { UserProfileUpdate } from '@/types';

const authStore = useAuthStore();
const loading = ref(false);
const saving = ref(false);
const error = ref<string | null>(null);
const success = ref<string | null>(null);

const form = ref<UserProfileUpdate>({
  firstName: '',
  lastName: '',
  email: '',
  phone: null,
  handicap: null,
});

onMounted(() => {
  if (authStore.user) {
    form.value = {
      firstName: authStore.user.firstName,
      lastName: authStore.user.lastName,
      email: authStore.user.email,
      phone: authStore.user.phone,
      handicap: authStore.user.handicap,
    };
  }
});

async function handleSubmit() {
  saving.value = true;
  error.value = null;
  success.value = null;

  try {
    await userApi.updateCurrentUserProfile(form.value);
    await authStore.fetchCurrentUser();
    success.value = 'Profile updated successfully';
  } catch (e) {
    error.value = 'Failed to update profile';
    console.error(e);
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <div class="profile-view">
    <div class="page-header">
      <h1 class="page-title">My Profile</h1>
    </div>

    <LoadingSpinner v-if="loading" text="Loading profile..." />

    <div v-else class="card">
      <div v-if="error" class="alert alert-error mb-2">
        {{ error }}
      </div>

      <div v-if="success" class="alert alert-success mb-2">
        {{ success }}
      </div>

      <form @submit.prevent="handleSubmit" class="profile-form">
        <div class="form-row">
          <div class="form-group">
            <label for="firstName" class="form-label">First Name</label>
            <input
              id="firstName"
              v-model="form.firstName"
              type="text"
              class="form-input"
              required
            />
          </div>

          <div class="form-group">
            <label for="lastName" class="form-label">Last Name</label>
            <input
              id="lastName"
              v-model="form.lastName"
              type="text"
              class="form-input"
              required
            />
          </div>
        </div>

        <div class="form-group">
          <label for="email" class="form-label">Email</label>
          <input
            id="email"
            v-model="form.email"
            type="email"
            class="form-input"
            required
          />
        </div>

        <div class="form-row">
          <div class="form-group">
            <label for="phone" class="form-label">Phone</label>
            <input
              id="phone"
              v-model="form.phone"
              type="tel"
              class="form-input"
              placeholder="(555) 555-5555"
            />
          </div>

          <div class="form-group">
            <label for="handicap" class="form-label">Handicap</label>
            <input
              id="handicap"
              v-model.number="form.handicap"
              type="number"
              class="form-input"
              min="0"
              max="54"
              placeholder="0-54"
            />
          </div>
        </div>

        <div class="form-actions">
          <button type="submit" class="btn btn-primary" :disabled="saving">
            {{ saving ? 'Saving...' : 'Save Changes' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.profile-form {
  max-width: 500px;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.form-actions {
  margin-top: 1.5rem;
}

@media (max-width: 480px) {
  .form-row {
    grid-template-columns: 1fr;
  }
}
</style>
