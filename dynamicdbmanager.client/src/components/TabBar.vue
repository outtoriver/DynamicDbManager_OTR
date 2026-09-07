<template>
  <div class="mobile-bar">
    <router-link to="/" class="bar-item" active-class="active">
      <span>⌂</span><small>Таблицы</small>
    </router-link>

    <router-link v-if="authStore.isAdmin" to="/admin/users" class="bar-item" active-class="active">
      <span>◉</span><small>Доступ</small>
    </router-link>

    <button class="bar-item theme-mobile" type="button" :title="themeTitle" @click="cycleTheme">
      <span>{{ themeIcon }}</span><small>{{ themeLabel }}</small>
    </button>

    <button class="bar-item danger" type="button" @click="logout">
      <span>↪</span><small>Выход</small>
    </button>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useThemeStore } from '../stores/theme'

const authStore = useAuthStore()
const themeStore = useThemeStore()
const router = useRouter()

const themeIcon = computed(() => themeStore.mode === 'light' ? '☀' : themeStore.mode === 'dark' ? '☾' : '◐')
const themeLabel = computed(() => themeStore.mode === 'light' ? 'День' : themeStore.mode === 'dark' ? 'Ночь' : 'Авто')
const themeTitle = computed(() => `Тема: ${themeLabel.value}. Нажмите для переключения`)

function cycleTheme() {
  const next = themeStore.mode === 'dark' ? 'light' : themeStore.mode === 'light' ? 'system' : 'dark'
  themeStore.setMode(next)
}

function logout() {
  authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
.mobile-bar{position:fixed;left:50%;bottom:14px;transform:translateX(-50%);display:none;gap:5px;padding:5px;border:1px solid var(--app-border);border-radius:18px;background:var(--app-header-bg);box-shadow:var(--app-shadow-strong),inset 0 1px var(--app-highlight);backdrop-filter:blur(18px) saturate(135%);-webkit-backdrop-filter:blur(18px) saturate(135%);z-index:90}
.bar-item{position:relative;display:flex;z-index:1;width:66px;height:50px;flex-direction:column;align-items:center;justify-content:center;gap:3px;border:1px solid transparent;border-radius:13px;background:transparent;color:var(--app-muted);text-decoration:none;cursor:pointer;transition:.18s}
.bar-item span{font-size:17px;line-height:1}.bar-item small{font-size:9px;line-height:1}
.bar-item:hover{background:var(--app-hover);color:var(--app-text);border-color:var(--app-border)}
.bar-item.active{background:var(--app-active);color:var(--app-primary-text);border-color:var(--app-primary-border)}
.bar-item.theme-mobile{color:var(--app-primary)}.bar-item.theme-mobile:hover{background:var(--app-active)}
.bar-item.danger{color:var(--app-danger)}
@media(max-width:560px){.mobile-bar{display:flex}}
</style>
