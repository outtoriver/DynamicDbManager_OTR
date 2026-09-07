<template>
  <div class="theme-control" aria-label="Выбор темы">
    <div class="theme-switch" role="group" aria-label="Тема интерфейса">
      <button
        type="button"
        class="theme-choice"
        :class="{ active: themeStore.mode === 'light' }"
        title="Светлая тема"
        @click="themeStore.setMode('light')"
      >
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true">
          <circle cx="12" cy="12" r="4" stroke-width="1.7" />
          <path d="M12 2v2M12 20v2M4.93 4.93l1.41 1.41M17.66 17.66l1.41 1.41M2 12h2M20 12h2M4.93 19.07l1.41-1.41M17.66 6.34l1.41-1.41" stroke-width="1.7" stroke-linecap="round" />
        </svg>
        <span>День</span>
      </button>

      <button
        type="button"
        class="theme-choice"
        :class="{ active: themeStore.mode === 'dark' }"
        title="Тёмная тема"
        @click="themeStore.setMode('dark')"
      >
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true">
          <path d="M21 12.8A8.5 8.5 0 1 1 11.2 3 6.7 6.7 0 0 0 21 12.8Z" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" />
        </svg>
        <span>Ночь</span>
      </button>

      <button
        type="button"
        class="theme-choice theme-auto"
        :class="{ active: themeStore.mode === 'system' }"
        title="Авто: тема Windows / браузера"
        @click="themeStore.setMode('system')"
      >
        <span class="auto-icon">◐</span>
        <span>Авто</span>
      </button>
    </div>
  </div>
</template>

<script setup>
import { useThemeStore } from '../stores/theme'

const themeStore = useThemeStore()
</script>

<style scoped>
.theme-control{display:flex;align-items:center}
.theme-switch{display:flex;align-items:center;gap:3px;padding:3px;border:1px solid var(--app-border);border-radius:13px;background:var(--app-control);box-shadow:inset 0 1px 0 var(--app-highlight)}
.theme-choice{height:30px;padding:0 9px;display:inline-flex;align-items:center;justify-content:center;gap:5px;border:1px solid transparent;border-radius:9px;background:transparent;color:var(--app-muted);font-size:10px;font-weight:700;cursor:pointer;transition:background .18s ease,color .18s ease,border-color .18s ease,box-shadow .18s ease}
.theme-choice svg{width:14px;height:14px;flex:none}
.theme-choice:hover{color:var(--app-text);background:var(--app-control-hover)}
.theme-choice.active{color:var(--app-primary-text);background:var(--app-primary-soft);border-color:var(--app-primary-border);box-shadow:0 5px 14px color-mix(in srgb,var(--app-primary) 10%,transparent)}
.auto-icon{font-size:13px;line-height:1}
@media(max-width:720px){.theme-choice span:last-child{display:none}.theme-choice{width:32px;padding:0}.theme-switch{gap:2px}}
</style>
