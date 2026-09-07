import { defineStore } from 'pinia'
import apiClient from '../api'
import { useAuthStore } from './auth'

export const useAdminStore = defineStore('admin', {
  state: () => ({ users: [], permissions: [], loaded: false, loading: false }),
  getters: {
    getUserById: state => id => state.users.find(u => u.id === id),
    getPermissionsForUser: state => userId => state.permissions.filter(p => p.userId === userId)
  },
  actions: {
    async loadAdminData(force = false) {
      if (this.loaded && !force) return
      this.loading = true
      try {
        const [usersRes, permissionsRes] = await Promise.all([
          apiClient.get('/admin/users'),
          apiClient.get('/admin/tablepermissions')
        ])
        this.users = Array.isArray(usersRes.data) ? usersRes.data : []
        this.permissions = Array.isArray(permissionsRes.data) ? permissionsRes.data : []
        this.loaded = true
        return { users: this.users, permissions: this.permissions }
      } catch (err) {
        console.error('Ошибка загрузки админ данных:', err)
        if (err.response?.status === 401) useAuthStore().logout()
        throw err
      } finally {
        this.loading = false
      }
    },
    async addUser() { return this.loadAdminData(true) },
    async updateUser() { return this.loadAdminData(true) },
    async deleteUser() { return this.loadAdminData(true) },
    async addPermission() { return this.loadAdminData(true) },
    async updatePermission() { return this.loadAdminData(true) },
    async deletePermission() { return this.loadAdminData(true) },
    reset() { this.users = []; this.permissions = []; this.loaded = false; this.loading = false }
  }
})
