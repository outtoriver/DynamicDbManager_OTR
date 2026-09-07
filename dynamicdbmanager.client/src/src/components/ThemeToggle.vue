<template>
  <div class="theme-control" ref="root">
    <button
      type="button"
      class="theme-button"
      :title="title"
      :aria-label="title"
      @click.stop="toggleMenu"
    >
      <svg v-if="themeStore.resolvedTheme === 'dark'" viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true">
        <path d="M21 15.4A8.5 8.5 0 0 1 8.6 3a8.5 8.5 0 1 0 12.4 12.4Z" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" />
      </svg>
      <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true">
        <circle cx="12" cy="12" r="4" stroke-width="1.8" />
        <path d="M12 2v2M12 20v2M4.93 4.93l1.42 1.42M17.65 17.65l1.42 1.42M2 12h2M20 12h2M4.93 19.07l1.42-1.42M17.65 6.35l1.42-1.42" stroke-width="1.8" stroke-linecap="round" />
      </svg>
    </button>

    <Transition name="theme-menu">
      <div v-if="open" class="theme-menu" @click.stop>
        <div class="theme-menu-title">Тема интерфейса</div>

        <button class="theme-option" :class="{active: themeStore.mode === 'light'}" @click="select('light')">
          <span class="theme-option-icon sun">☀</span>
          <span><strong>День</strong><small>Светлый glass</small></span>
          <span v-if="themeStore.mode === 'light'" class="theme-check">✓</span>
        </button>

        <button class="theme-option" :class="{active: themeStore.mode === 'system'}" @click="select('system')">
          <span class="theme-option-icon auto">◐</span>
          <span><strong>Авто</strong><small>По настройке системы</small></span>
          <span v-if="themeStore.mode === 'system'" class="theme-check">✓</span>
        </button>

        <button class="theme-option" :class="{active: themeStore.mode === 'dark'}" @click="select('dark')">
          <span class="theme-option-icon moon">☾</span>
          <span><strong>Ночь</strong><small>Тёмный glass</small></span>
          <span v-if="themeStore.mode === 'dark'" class="theme-check">✓</span>
        </button>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { useThemeStore } from '../stores/theme'

const themeStore = useThemeStore()
const open = ref(false)
const root = ref(null)
let unbindSystem = null

const title = computed(() => {
  if (themeStore.mode === 'light') return 'Тема: День'
  if (themeStore.mode === 'dark') return 'Тема: Ночь'
  return `Тема: Авто (${themeStore.resolvedTheme === 'dark' ? 'Ночь' : 'День'})`
})

function toggleMenu() {
  open.value = !open.value
}

function select(mode) {
  themeStore.setMode(mode)
  open.value = false
}

function onDocumentClick(event) {
  if (!root.value?.contains(event.target)) open.value = false
}

onMounted(() => {
  themeStore.load()
  unbindSystem = themeStore.bindSystemTheme()
  document.addEventListener('click', onDocumentClick)
})

onBeforeUnmount(() => {
  unbindSystem?.()
  document.removeEventListener('click', onDocumentClick)
})
</script>

<style scoped>
.theme-control { position: relative; }
.theme-button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 38px;
  height: 38px;
  padding: 0;
  border: 1px solid var(--app-border);
  border-radius: 11px;
  background: var(--app-control-bg);
  color: var(--app-muted);
  cursor: pointer;
  backdrop-filter: blur(14px) saturate(135%);
  -webkit-backdrop-filter: blur(14px) saturate(135%);
  box-shadow: inset 0 1px 0 var(--app-highlight);
  transition: .18s ease;
}
.theme-button:hover {
  color: var(--app-text);
  background: var(--app-control-hover);
  border-color: var(--app-border-strong);
  transform: translateY(-1px);
}
.theme-button svg { width: 17px; height: 17px; }
.theme-menu {
  position: absolute;
  right: 0;
  top: calc(100% + 8px);
  width: 236px;
  padding: 8px;
  border: 1px solid var(--app-border-strong);
  border-radius: 16px;
  background: var(--app-menu-bg);
  backdrop-filter: blur(24px) saturate(135%);
  -webkit-backdrop-filter: blur(24px) saturate(135%);
  box-shadow: var(--app-shadow-strong), inset 0 1px 0 var(--app-highlight);
}
.theme-menu-title {
  padding: 5px 8px 8px;
  color: var(--app-muted);
  font-size: 10px;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: .12em;
}
.theme-option {
  width: 100%;
  display: grid;
  grid-template-columns: 30px 1fr auto;
  align-items: center;
  gap: 9px;
  padding: 8px;
  border: 1px solid transparent;
  border-radius: 11px;
  background: transparent;
  color: var(--app-text);
  text-align: left;
  cursor: pointer;
}
.theme-option:hover { background: var(--app-hover); border-color: var(--app-border); }
.theme-option.active { background: var(--app-active); border-color: var(--app-primary-border); }
.theme-option > span:nth-child(2) { display: flex; flex-direction: column; gap: 1px; min-width: 0; }
.theme-option strong { font-size: 12px; }
.theme-option small { font-size: 10px; color: var(--app-muted); }
.theme-option-icon { display: grid; place-items: center; width: 28px; height: 28px; border-radius: 9px; background: var(--app-control-soft); font-size: 15px; }
.sun { color: #f59e0b; }
.moon { color: #8b9aff; }
.auto { color: var(--app-cyan); }
.theme-check { color: var(--app-primary); font-weight: 900; }
.theme-menu-enter-active,.theme-menu-leave-active { transition: opacity .14s ease, transform .14s ease; }
.theme-menu-enter-from,.theme-menu-leave-to { opacity: 0; transform: translateY(-5px) scale(.98); }
</style>
