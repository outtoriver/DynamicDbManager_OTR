import { defineStore } from 'pinia'

export const BACKGROUND_TYPES = [
  'galaxy',
  'nebula3d',
  'aurora',
  'gradient',
  'particles',
  'cybergrid',
  'black'
]

export const DEFAULT_BACKGROUND = 'particles'

export const useBackgroundStore = defineStore('background', {
  state: () => ({
    // Частицы — безопасный и нейтральный фон по умолчанию.
    type: DEFAULT_BACKGROUND
  }),

  actions: {
    setType(type) {
      const normalized = BACKGROUND_TYPES.includes(type)
        ? type
        : DEFAULT_BACKGROUND

      this.type = normalized
      localStorage.setItem('backgroundType', normalized)
    },

    loadFromStorage() {
      const saved = localStorage.getItem('backgroundType')

      this.type = BACKGROUND_TYPES.includes(saved)
        ? saved
        : DEFAULT_BACKGROUND
    }
  }
})
