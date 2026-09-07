<template>
  <div class="mobile-bar">
    <router-link to="/" class="bar-item" active-class="active"><span>⌂</span><small>Таблицы</small></router-link>
    <router-link v-if="authStore.isAdmin" to="/admin/users" class="bar-item" active-class="active"><span>◉</span><small>Доступ</small></router-link>
    <button class="bar-item" type="button" @click="themeStore.toggle"><span>{{ themeStore.isDark ? '☀' : '☾' }}</span><small>{{ themeStore.isDark ? 'День' : 'Ночь' }}</small></button>
    <button class="bar-item danger" type="button" @click="logout"><span>↪</span><small>Выход</small></button>
  </div>
</template>

<script setup>
import { useAuthStore } from '../stores/auth'
import { useThemeStore } from '../stores/theme'
import { useRouter } from 'vue-router'
const authStore=useAuthStore(); const themeStore=useThemeStore(); const router=useRouter()
function logout(){authStore.logout();router.push('/login')}
</script>

<style scoped>
.mobile-bar{position:fixed;left:50%;bottom:14px;transform:translateX(-50%);display:none;gap:4px;padding:5px;border-radius:17px;background:var(--app-header-bg);border:1px solid var(--app-border-strong);backdrop-filter:blur(18px) saturate(145%);-webkit-backdrop-filter:blur(18px) saturate(145%);box-shadow:var(--app-shadow-strong),inset 0 1px 0 var(--app-highlight);z-index:90;overflow:hidden}
.mobile-bar::before{content:"";position:absolute;inset:0;pointer-events:none;background:linear-gradient(135deg,var(--app-highlight),transparent 34%)}
.bar-item{position:relative;z-index:1;width:62px;height:50px;display:flex;flex-direction:column;align-items:center;justify-content:center;gap:2px;border:1px solid transparent;border-radius:13px;background:transparent;color:var(--app-muted);text-decoration:none;transition:.2s ease}
.bar-item span{font-size:17px;line-height:1}.bar-item small{font-size:9px;line-height:1}.bar-item:hover{color:var(--app-text);background:var(--app-hover);border-color:var(--app-border)}.bar-item:active{transform:scale(.96)}.bar-item.active{color:var(--app-primary);background:var(--app-active);border-color:var(--app-primary-border)}.bar-item.danger{color:var(--app-danger)}
@media(max-width:560px){.mobile-bar{display:flex}}
</style>
