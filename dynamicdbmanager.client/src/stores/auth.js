import { defineStore } from 'pinia'
import apiClient from '../api'
import { useAdminStore } from './admin'

function decodeJwtPayload(token) {
  const part = token?.split('.')[1]
  if (!part) return null
  const normalized = part.replace(/-/g, '+').replace(/_/g, '/')
  const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, '=')
  return JSON.parse(atob(padded))
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token') || '',
    user: null
  }),
  getters: {
    isAuthenticated: state => !!state.token,
    isAdmin: state => {
      if (!state.token) return false
      try {
        const payload = decodeJwtPayload(state.token)
        const roles = payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
        if (Array.isArray(roles)) return roles.includes('Admin')
        return typeof roles === 'string' && roles === 'Admin'
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
      this.user = null
      localStorage.removeItem('token')
      const adminStore = useAdminStore()
      adminStore.reset()
    }
  }
})
