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

const STORAGE_KEY = 'backgroundType'
const DEFAULT_BACKGROUND = 'particles'

export const useBackgroundStore = defineStore('background', {
  state: () => ({
    type: DEFAULT_BACKGROUND
  }),

  actions: {
    setType(type) {
      const normalized = BACKGROUND_TYPES.includes(type)
        ? type
        : DEFAULT_BACKGROUND

      this.type = normalized

      if (typeof localStorage !== 'undefined') {
        localStorage.setItem(STORAGE_KEY, normalized)
      }
    },

    loadFromStorage() {
      if (typeof localStorage === 'undefined') {
        this.type = DEFAULT_BACKGROUND
        return
      }

      const saved = localStorage.getItem(STORAGE_KEY)
      this.type = BACKGROUND_TYPES.includes(saved)
        ? saved
        : DEFAULT_BACKGROUND
    }
  }
})
