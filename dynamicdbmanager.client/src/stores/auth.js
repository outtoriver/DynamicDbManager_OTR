import { defineStore } from 'pinia'

function decodeJwtPayload(token) {
  try {
    const part = String(token || '').split('.')[1]
    if (!part) return null

    const base64 = part.replace(/-/g, '+').replace(/_/g, '/')
    const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, '=')
    const binary = atob(padded)
    const bytes = Uint8Array.from(binary, char => char.charCodeAt(0))
    return JSON.parse(new TextDecoder().decode(bytes))
  } catch {
    return null
  }
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token') || '',
    user: null
  }),

  getters: {
    isAuthenticated: (state) => Boolean(state.token),

    isAdmin: (state) => {
      const payload = decodeJwtPayload(state.token)
      const role = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
      const roles = payload?.[role] ?? payload?.role ?? payload?.roles
      return Array.isArray(roles)
        ? roles.some(value => String(value).toLowerCase() === 'admin')
        : String(roles || '').toLowerCase() === 'admin'
    },

    userName: (state) => {
      const payload = decodeJwtPayload(state.token)
      return payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || payload?.name || ''
    }
  },

  actions: {
    setToken(token) {
      this.token = token || ''
      if (this.token) localStorage.setItem('token', this.token)
      else localStorage.removeItem('token')
    },

    login(token, user = null) {
      this.setToken(token)
      this.user = user
    },

    logout() {
      this.token = ''
      this.user = null
      localStorage.removeItem('token')
    }
  }
})
