import { defineStore } from 'pinia'
import { computed, watch } from 'vue'

export const THEME_TYPES = ['system', 'light', 'dark']

export const useThemeStore = defineStore('theme', {
  state: () => ({
    mode: 'system',
    systemDark: false,
  }),

  getters: {
    isDark: (state) => state.mode === 'dark' || (state.mode === 'system' && state.systemDark),
    resolvedTheme: (state) => (state.mode === 'dark' || (state.mode === 'system' && state.systemDark)) ? 'dark' : 'light',
  },

  actions: {
    setMode(mode) {
      this.mode = THEME_TYPES.includes(mode) ? mode : 'system'
      localStorage.setItem('themeMode', this.mode)
      this.apply()
    },

    toggle() {
      this.setMode(this.isDark ? 'light' : 'dark')
    },

    cycle() {
      const next = { system: 'light', light: 'dark', dark: 'system' }[this.mode] || 'system'
      this.setMode(next)
    },

    apply() {
      const resolved = this.resolvedTheme
      document.documentElement.dataset.theme = resolved
      document.documentElement.dataset.themeMode = this.mode
      document.documentElement.style.colorScheme = resolved
    },

    load() {
      const saved = localStorage.getItem('themeMode')
      this.mode = THEME_TYPES.includes(saved) ? saved : 'system'
      this.systemDark = window.matchMedia?.('(prefers-color-scheme: dark)').matches ?? true
      this.apply()
    },

    bindSystemTheme() {
      const media = window.matchMedia?.('(prefers-color-scheme: dark)')
      if (!media) return () => {}

      const handler = (event) => {
        this.systemDark = event.matches
        if (this.mode === 'system') this.apply()
      }

      media.addEventListener?.('change', handler)
      return () => media.removeEventListener?.('change', handler)
    },
  },
})
