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

export const useBackgroundStore = defineStore('background', {
  state: () => ({ type: 'galaxy' }),
  actions: {
    setType(type) {
      const normalized = BACKGROUND_TYPES.includes(type) ? type : 'black'
      this.type = normalized
      localStorage.setItem('backgroundType', normalized)
    },
    loadFromStorage() {
      const saved = localStorage.getItem('backgroundType')
      this.type = BACKGROUND_TYPES.includes(saved) ? saved : 'galaxy'
    }
  }
})
