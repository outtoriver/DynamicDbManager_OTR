import { defineStore } from 'pinia'

export const useBackgroundStore = defineStore('background', {
  state: () => ({
    type: 'galaxy' // 'galaxy', 'gradient', 'particles'
  }),
  actions: {
    setType(type) {
      this.type = type
      localStorage.setItem('backgroundType', type)
    },
    loadFromStorage() {
      const saved = localStorage.getItem('backgroundType')
      if (saved && ['galaxy', 'gradient', 'particles'].includes(saved)) {
        this.type = saved
      }
    }
  }
})
