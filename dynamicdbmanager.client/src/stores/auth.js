import { defineStore } from 'pinia'
import apiClient from '../api'
import { useAdminStore } from './admin'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token') || '',
    user: null
  }),
  getters: {
    isAuthenticated: (state) => !!state.token,
    isAdmin: (state) => {
      if (!state.token) return false
      try {
        const payload = JSON.parse(atob(state.token.split('.')[1]))
        const roles = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
        if (Array.isArray(roles)) return roles.includes('Admin')
        if (typeof roles === 'string') return roles === 'Admin'
        return false
      } catch {
        return false
      }
    }
  },
  actions: {
    async login(username, password) {
      const res = await apiClient.post('/auth/login', { userName: username, password })
      this.token = res.data.token
      localStorage.setItem('token', this.token)
    },
    async register(username, email, password) {
      await apiClient.post('/auth/register', { userName: username, email, password })
    },
    logout() {
      this.token = ''
      localStorage.removeItem('token')
      const adminStore = useAdminStore()
      adminStore.reset()
    }
  }
})
