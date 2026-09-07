import { defineStore } from 'pinia'

const STORAGE_KEY = 'themeMode'
const MODES = ['system', 'light', 'dark']

function getSystemTheme() {
  if (typeof window === 'undefined') return 'dark'
  return window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

function applyTheme(mode) {
  if (typeof document === 'undefined') return
  const effective = mode === 'system' ? getSystemTheme() : mode
  const root = document.documentElement
  root.dataset.theme = effective
  root.dataset.themeMode = mode
  root.classList.toggle('dark', effective === 'dark')
  root.style.colorScheme = effective
  window.dispatchEvent(new CustomEvent('themechange', { detail: { mode, effective } }))
}

export const useThemeStore = defineStore('theme', {
  state: () => ({
    mode: typeof localStorage !== 'undefined' && MODES.includes(localStorage.getItem(STORAGE_KEY)) ? localStorage.getItem(STORAGE_KEY) : 'system',
    systemTheme: getSystemTheme(),
    _media: null,
    _listener: null,
    _initialized: false
  }),
  getters: {
    effectiveTheme: state => state.mode === 'system' ? state.systemTheme : state.mode,
    isDark: state => state.mode === 'system' ? state.systemTheme === 'dark' : state.mode === 'dark',
    isLight: state => state.mode === 'system' ? state.systemTheme === 'light' : state.mode === 'light'
  },
  actions: {
    init() {
      const saved = localStorage.getItem(STORAGE_KEY)
      this.mode = MODES.includes(saved) ? saved : 'system'
      this.systemTheme = getSystemTheme()
      applyTheme(this.mode)
      if (this._initialized) return
      this._initialized = true
      if (window.matchMedia) {
        this._media = window.matchMedia('(prefers-color-scheme: dark)')
        this._listener = event => {
          this.systemTheme = event.matches ? 'dark' : 'light'
          if (this.mode === 'system') applyTheme(this.mode)
        }
        this._media.addEventListener?.('change', this._listener)
      }
    },
    setMode(mode) {
      const normalized = MODES.includes(mode) ? mode : 'system'
      this.mode = normalized
      this.systemTheme = getSystemTheme()
      localStorage.setItem(STORAGE_KEY, normalized)
      applyTheme(normalized)
    },
    toggle() {
      const next = this.mode === 'dark' ? 'light' : this.mode === 'light' ? 'system' : 'dark'
      this.setMode(next)
    },
    dispose() {
      this._media?.removeEventListener?.('change', this._listener)
      this._media = null
      this._listener = null
      this._initialized = false
    }
  }
})
