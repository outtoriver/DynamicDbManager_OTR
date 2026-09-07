<template>
  <div class="theme-control" ref="root">
    <div class="theme-switch" role="group" aria-label="Тема интерфейса">
      <button type="button" class="theme-choice" :class="{ active: themeStore.effectiveTheme === 'light' && themeStore.mode !== 'dark' }" title="День" @click="select('light')">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true">
          <circle cx="12" cy="12" r="4" stroke-width="1.7"/>
          <path d="M12 2v2M12 20v2M4.93 4.93l1.41 1.41M17.66 17.66l1.41 1.41M2 12h2M20 12h2M4.93 19.07l1.41-1.41M17.66 6.34l1.41-1.41" stroke-width="1.7" stroke-linecap="round"/>
        </svg>
        <span>День</span>
      </button>

      <button type="button" class="theme-choice" :class="{ active: themeStore.effectiveTheme === 'dark' && themeStore.mode !== 'light' }" title="Ночь" @click="select('dark')">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true">
          <path d="M21 12.8A8.5 8.5 0 1 1 11.2 3 6.7 6.7 0 0 0 21 12.8Z" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
        <span>Ночь</span>
      </button>

      <button type="button" class="theme-auto" :class="{ active: themeStore.mode === 'system' }" title="Авто: тема Windows / браузера" @click="select('system')">
        <span class="auto-icon">◐</span>
        <span>Авто</span>
      </button>
    </div>
  </div>
</template>
<script setup>
import { useThemeStore } from '../stores/theme'

const themeStore = useThemeStore()

function select(mode) {
  themeStore.setMode(mode)
}
</script>
<style scoped>
.theme-control {
  display: inline-flex;
  align-items: center;
}

.theme-switch {
  display: inline-flex;
  align-items: center;
  gap: 2px;
  padding: 3px;
  min-height: 38px;
  border: 1px solid var(--app-border);
  border-radius: 12px;
  background: var(--app-control);
  box-shadow: inset 0 1px 0 var(--app-highlight);
  backdrop-filter: blur(14px) saturate(135%);
  -webkit-backdrop-filter: blur(14px) saturate(135%);
}

.theme-choice,
.theme-auto {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 5px;
  height: 30px;
  padding: 0 8px;
  border: 1px solid transparent;
  border-radius: 9px;
  background: transparent;
  color: var(--app-muted);
  font-size: 11px;
  font-weight: 700;
  cursor: pointer;
  transition: background .18s ease, color .18s ease, border-color .18s ease, transform .18s ease;
}

.theme-choice svg {
  width: 14px;
  height: 14px;
}

.theme-choice:hover,
.theme-auto:hover {
  color: var(--app-text);
  background: var(--app-hover);
}

.theme-choice.active,
.theme-auto.active {
  color: var(--app-text);
  background: var(--app-accent-soft);
  border-color: var(--app-accent-border);
  box-shadow: inset 0 1px 0 var(--app-highlight);
}

.theme-auto {
  min-width: 54px;
}

.auto-icon {
  font-size: 13px;
  line-height: 1;
  color: var(--app-primary);
}

.theme-choice:active,
.theme-auto:active {
  transform: scale(.97);
}

@media (max-width: 640px) {
  .theme-switch {
    min-height: 36px;
  }

  .theme-choice span:not(.auto-icon),
  .theme-auto > span:last-child {
    display: none;
  }

  .theme-choice,
  .theme-auto {
    width: 30px;
    padding: 0;
  }

  .theme-auto {
    min-width: 30px;
  }
}
</style>
