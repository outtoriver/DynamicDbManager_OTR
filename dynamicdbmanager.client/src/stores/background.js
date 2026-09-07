import { defineStore } from 'pinia'

const TYPES = ['galaxy','nebula3d','aurora','gradient','particles','cybergrid','black']
const STORAGE_KEY = 'backgroundType'

export const useBackgroundStore = defineStore('background', {
  state: () => ({ type: 'particles' }),
  actions: {
    setType(type) {
      if (!TYPES.includes(type)) return
      this.type = type
      localStorage.setItem(STORAGE_KEY, type)
    },
    loadFromStorage() {
      const saved = localStorage.getItem(STORAGE_KEY)
      this.type = TYPES.includes(saved) ? saved : 'particles'
    }
  }
})
