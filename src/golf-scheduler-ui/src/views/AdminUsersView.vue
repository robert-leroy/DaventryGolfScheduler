<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { RouterLink } from 'vue-router';
import { userApi } from '@/services/api';
import { useAuthStore } from '@/stores/authStore';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import type { User, UserCreate, UserUpdate } from '@/types';

const authStore = useAuthStore();
const users = ref<User[]>([]);
const loading = ref(false);
const error = ref<string | null>(null);

const showForm = ref(false);
const editingUser = ref<User | null>(null);
const saving = ref(false);

const form = ref<UserCreate>({
  email: '',
  firstName: '',
  lastName: '',
  phone: '',
  handicap: undefined,
  isAdmin: false,
});

onMounted(async () => {
  await fetchUsers();
});

async function fetchUsers() {
  loading.value = true;
  error.value = null;
  try {
    users.value = await userApi.getAllUsers();
  } catch {
    error.value = 'Failed to load users';
  } finally {
    loading.value = false;
  }
}

function openCreateForm() {
  editingUser.value = null;
  form.value = {
    email: '',
    firstName: '',
    lastName: '',
    phone: '',
    handicap: undefined,
    isAdmin: false,
  };
  showForm.value = true;
}

function openEditForm(user: User) {
  editingUser.value = user;
  form.value = {
    email: user.email,
    firstName: user.firstName,
    lastName: user.lastName,
    phone: user.phone || '',
    handicap: user.handicap || undefined,
    isAdmin: user.isAdmin,
  };
  showForm.value = true;
}

function closeForm() {
  showForm.value = false;
  editingUser.value = null;
}

async function handleSubmit() {
  saving.value = true;
  error.value = null;

  try {
    if (editingUser.value) {
      const updateData: UserUpdate = {
        email: form.value.email,
        firstName: form.value.firstName,
        lastName: form.value.lastName,
        phone: form.value.phone || undefined,
        handicap: form.value.handicap,
        isAdmin: form.value.isAdmin,
      };
      await userApi.updateUser(editingUser.value.id, updateData);
    } else {
      await userApi.createUser(form.value);
    }
    await fetchUsers();
    closeForm();
  } catch {
    error.value = editingUser.value ? 'Failed to update user' : 'Failed to create user';
  } finally {
    saving.value = false;
  }
}

async function handleDelete(user: User) {
  if (user.id === authStore.user?.id) {
    alert("You cannot delete your own account.");
    return;
  }

  if (!confirm(`Are you sure you want to delete ${user.displayName}? This action cannot be undone.`)) {
    return;
  }

  try {
    await userApi.deleteUser(user.id);
    await fetchUsers();
  } catch {
    error.value = 'Failed to delete user';
  }
}

async function toggleAdmin(user: User) {
  if (user.id === authStore.user?.id) {
    alert("You cannot change your own admin status.");
    return;
  }

  const action = user.isAdmin ? 'remove admin rights from' : 'make admin';
  if (!confirm(`Are you sure you want to ${action} ${user.displayName}?`)) {
    return;
  }

  try {
    const updated = await userApi.updateAdminStatus(user.id, !user.isAdmin);
    const index = users.value.findIndex((u) => u.id === user.id);
    if (index !== -1) {
      users.value[index] = updated;
    }
  } catch {
    error.value = 'Failed to update user';
  }
}
</script>

<template>
  <div class="admin-users-view">
    <div class="page-header flex flex-between flex-center">
      <h1 class="page-title">User Management</h1>
      <div class="header-actions">
        <button v-if="!showForm" @click="openCreateForm" class="btn btn-primary">
          Add User
        </button>
        <RouterLink to="/admin" class="btn btn-outline">
          &larr; Back to Dashboard
        </RouterLink>
      </div>
    </div>

    <div v-if="error" class="alert alert-error mb-2">
      {{ error }}
    </div>

    <!-- User Form -->
    <div v-if="showForm" class="card mb-2">
      <div class="card-header">
        <h2 class="card-title">{{ editingUser ? 'Edit User' : 'Add New User' }}</h2>
      </div>

      <form @submit.prevent="handleSubmit" class="user-form">
        <div class="form-row">
          <div class="form-group">
            <label for="firstName" class="form-label">First Name *</label>
            <input
              id="firstName"
              v-model="form.firstName"
              type="text"
              class="form-input"
              required
            />
          </div>

          <div class="form-group">
            <label for="lastName" class="form-label">Last Name *</label>
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
          <label for="email" class="form-label">Email *</label>
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

        <div class="form-group">
          <label class="checkbox-label">
            <input
              type="checkbox"
              v-model="form.isAdmin"
              :disabled="editingUser?.id === authStore.user?.id"
            />
            <span>Administrator</span>
          </label>
        </div>

        <div class="form-actions">
          <button type="button" @click="closeForm" class="btn btn-outline">
            Cancel
          </button>
          <button type="submit" class="btn btn-primary" :disabled="saving">
            {{ saving ? 'Saving...' : (editingUser ? 'Update User' : 'Create User') }}
          </button>
        </div>
      </form>
    </div>

    <LoadingSpinner v-if="loading" text="Loading users..." />

    <template v-else-if="!showForm">
      <div v-if="users.length === 0" class="empty-state">
        <p>No users found.</p>
        <button @click="openCreateForm" class="btn btn-primary mt-1">
          Add First User
        </button>
      </div>

      <div v-else class="users-table">
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Phone</th>
              <th>Handicap</th>
              <th>Role</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in users" :key="user.id">
              <td>
                {{ user.displayName }}
                <span v-if="user.id === authStore.user?.id" class="text-muted text-sm"> (You)</span>
              </td>
              <td>{{ user.email }}</td>
              <td>{{ user.phone || '-' }}</td>
              <td>{{ user.handicap ?? '-' }}</td>
              <td>
                <span :class="['badge', user.isAdmin ? 'badge-warning' : 'badge-success']">
                  {{ user.isAdmin ? 'Admin' : 'User' }}
                </span>
              </td>
              <td class="actions-cell">
                <button @click="openEditForm(user)" class="btn btn-outline btn-sm">
                  Edit
                </button>
                <button
                  @click="toggleAdmin(user)"
                  :class="['btn', 'btn-sm', user.isAdmin ? 'btn-outline' : 'btn-secondary']"
                  :disabled="user.id === authStore.user?.id"
                >
                  {{ user.isAdmin ? 'Remove Admin' : 'Make Admin' }}
                </button>
                <button
                  @click="handleDelete(user)"
                  class="btn btn-danger btn-sm"
                  :disabled="user.id === authStore.user?.id"
                >
                  Delete
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>
  </div>
</template>

<style scoped>
.header-actions {
  display: flex;
  gap: 0.5rem;
}

.user-form {
  max-width: 500px;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.form-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  margin-top: 1.5rem;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
}

.checkbox-label input {
  width: 1rem;
  height: 1rem;
}

.users-table {
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
  background-color: var(--surface);
  border-radius: 8px;
  overflow: hidden;
}

th, td {
  padding: 0.75rem 1rem;
  text-align: left;
  border-bottom: 1px solid var(--border-color);
}

th {
  background-color: var(--background);
  font-weight: 600;
  color: var(--text-secondary);
  font-size: 0.875rem;
}

tr:last-child td {
  border-bottom: none;
}

.actions-cell {
  white-space: nowrap;
}

.btn-sm {
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
  margin-right: 0.25rem;
}

@media (max-width: 480px) {
  .form-row {
    grid-template-columns: 1fr;
  }
}
</style>
