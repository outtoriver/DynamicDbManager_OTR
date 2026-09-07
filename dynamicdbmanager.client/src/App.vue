<template>
  <div class="app-shell" :class="{ 'table-route': route.name === 'tableData' }">
    <component :is="currentBackground" class="app-background" />

    <header class="app-header">
      <div class="app-brand">
        <div class="brand-mark">DB</div>
        <h1>{{ pageTitle }}</h1>
      </div>
      <nav class="app-nav" aria-label="Основная навигация">
        <router-link to="/" class="nav-btn" :class="{ active: $route.name === 'home' && !$route.query.category }">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true"><path d="M4 6h16M4 12h16M4 18h16" stroke-width="1.8" stroke-linecap="round" /></svg><span>Таблицы</span>
        </router-link>
        <div class="nav-dropdown" @click.stop>
          <button type="button" class="nav-btn" :class="{ active: showCategories }" @click="toggleCategories">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true"><path d="M5 7.5h14M5 12h14M5 16.5h9" stroke-width="1.8" stroke-linecap="round" /></svg><span>Категории</span>
            <svg class="chevron" :class="{ open: showCategories }" viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true"><path d="m7 9 5 5 5-5" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" /></svg>
          </button>
          <div v-if="showCategories" class="category-menu">
            <button type="button" class="category-item" @click="selectCategory(null)">Все таблицы</button>
            <div v-if="tablesStore.loading" class="category-loading">Загрузка…</div>
            <button v-for="cat in tablesStore.categories" :key="cat" type="button" class="category-item" @click="selectCategory(cat)">{{ cat }}</button>
          </div>
        </div>
        <router-link to="/help" class="nav-btn" :class="{ active: $route.name === 'help' }">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true"><circle cx="12" cy="12" r="9" stroke-width="1.7"/><path d="M9.7 9a2.35 2.35 0 1 1 4.1 1.58c-.9.9-1.8 1.2-1.8 2.42" stroke-width="1.7" stroke-linecap="round"/><path d="M12 16.6h.01" stroke-width="2.2" stroke-linecap="round"/></svg><span>Справка</span>
        </router-link>
        <router-link v-if="authStore.isAdmin" to="/admin/users" class="nav-btn" :class="{ active: $route.name === 'adminUsers' || $route.name === 'permissions' }">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true"><path d="M16 20v-1.2a3.8 3.8 0 0 0-3.8-3.8H7.8A3.8 3.8 0 0 0 4 18.8V20M10 11.2a3.6 3.6 0 1 0 0-7.2 3.6 3.6 0 0 0 0 7.2ZM19 8v6M22 11h-6" stroke-width="1.7" stroke-linecap="round" /></svg><span>Доступ</span>
        </router-link>
        <button type="button" class="nav-btn danger" @click="logout"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true"><path d="M10 4H7a3 3 0 0 0-3 3v10a3 3 0 0 0 3 3h3M14 8l4 4-4 4M8 12h10" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" /></svg><span>Выход</span></button>
      </nav>
      <div class="app-header-actions">
        <ThemeToggle />
        <BackgroundSelector />
        <button v-if="authStore.isAdmin" type="button" class="new-table-btn" title="Новая таблица" @click="createNewTable"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true"><path d="M12 5v14M5 12h14" stroke-width="1.9" stroke-linecap="round" /></svg><span>Новая таблица</span></button>
      </div>
    </header>

    <main class="app-content"><router-view /></main>
    <TabBar />
  </div>
</template>

<script setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from './stores/auth'
import { useTablesStore } from './stores/tables'
import { useAdminStore } from './stores/admin'
import { useBackgroundStore } from './stores/background'
import { useThemeStore } from './stores/theme'
import Background3D from './components/Background3D.vue'
import BackgroundGradient from './components/BackgroundGradient.vue'
import BackgroundParticles from './components/BackgroundParticles.vue'
import BackgroundBlack from './components/BackgroundBlack.vue'
import BackgroundNebula3D from './components/BackgroundNebula3D.vue'
import BackgroundAurora from './components/BackgroundAurora.vue'
import BackgroundCyberGrid from './components/BackgroundCyberGrid.vue'
import BackgroundSelector from './components/BackgroundSelector.vue'
import ThemeToggle from './components/ThemeToggle.vue'
import TabBar from './components/TabBar.vue'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const tablesStore = useTablesStore()
const adminStore = useAdminStore()
const bgStore = useBackgroundStore()
const themeStore = useThemeStore()
const showCategories = ref(false)

const currentBackground = computed(() => ({
  galaxy: Background3D,
  gradient: BackgroundGradient,
  nebula3d: BackgroundNebula3D,
  aurora: BackgroundAurora,
  cybergrid: BackgroundCyberGrid,
  particles: BackgroundParticles,
  black: BackgroundBlack
}[bgStore.type] || BackgroundBlack))

const pageTitle = computed(() => {
  if (route.name === 'home') return route.query.category ? `Таблицы: ${route.query.category}` : 'Учеты и таблицы'
  if (route.name === 'adminUsers' || route.name === 'permissions') return 'Доступ'
  if (route.name === 'help') return 'Справка'
  if (route.name === 'tableData') {
    const table = tablesStore.getTableById(route.params.id)
    return table?.name || 'Таблица'
  }
  return 'Dynamic DB Manager'
})

function toggleCategories() { showCategories.value = !showCategories.value }
function selectCategory(category) {
  showCategories.value = false
  router.push({ name: 'home', query: category ? { category } : {} })
}
function logout() {
  authStore.logout()
  tablesStore.reset()
  router.push('/login')
}
function createNewTable() {
  router.push({ name: 'home', query: { create: 'true' } })
}
function handleClickOutside(event) {
  if (!event.target.closest('.nav-dropdown')) showCategories.value = false
}

onMounted(async () => {
  bgStore.loadFromStorage()
  await Promise.all([
    tablesStore.loadTables(),
    authStore.isAdmin ? adminStore.loadAdminData() : Promise.resolve()
  ])
  document.addEventListener('click', handleClickOutside)
})

onBeforeUnmount(() => document.removeEventListener('click', handleClickOutside))
</script>

<style scoped>
.app-shell{position:relative;min-height:100vh;background:var(--app-bg);color:var(--app-text);isolation:isolate}
.app-background{position:fixed;inset:0;z-index:-10;pointer-events:none}
.app-header{position:sticky;top:0;z-index:40;display:grid;grid-template-columns:minmax(190px,1fr) auto minmax(230px,1fr);align-items:center;gap:16px;margin:12px;padding:10px 12px;border:1px solid var(--app-border);border-radius:18px;background:var(--app-header);backdrop-filter:blur(18px) saturate(135%);-webkit-backdrop-filter:blur(18px) saturate(135%);box-shadow:var(--app-shadow),inset 0 1px 0 var(--app-highlight)}
.app-brand{display:flex;align-items:center;gap:10px;min-width:0}.brand-mark{width:30px;height:30px;border-radius:9px;display:grid;place-items:center;background:var(--app-brand-bg);border:1px solid var(--app-border-strong);color:var(--app-primary);font-size:10px;font-weight:800;letter-spacing:.05em}.app-brand h1{margin:0;min-width:0;font-size:14px;font-weight:700;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;color:var(--app-text)}
.app-nav{display:flex;align-items:center;gap:5px}.nav-btn,.new-table-btn{display:inline-flex;align-items:center;justify-content:center;gap:8px;height:38px;padding:0 11px;border:1px solid transparent;border-radius:11px;background:transparent;color:var(--app-muted);font-size:12px;font-weight:600;white-space:nowrap;cursor:pointer;text-decoration:none;transition:.18s}.nav-btn svg,.new-table-btn svg{width:16px;height:16px;flex:none}.nav-btn:hover{background:var(--app-hover);color:var(--app-text);border-color:var(--app-border)}.nav-btn.active{background:var(--app-accent-soft);color:var(--app-text);border-color:var(--app-accent-border)}.nav-btn.danger:hover{color:var(--app-danger);background:var(--app-danger-soft);border-color:var(--app-danger-border)}
.app-header-actions{display:flex;align-items:center;justify-content:flex-end;gap:8px}.new-table-btn{background:var(--app-primary-soft);color:var(--app-primary-text);border-color:var(--app-accent-border);box-shadow:0 8px 24px var(--app-primary-shadow)}.new-table-btn:hover{background:var(--app-primary-soft-strong)}.nav-dropdown{position:relative}.chevron{width:13px!important;height:13px!important;transition:transform .18s}.chevron.open{transform:rotate(180deg)}
.category-menu{position:absolute;top:calc(100% + 8px);left:0;min-width:185px;padding:6px;border:1px solid var(--app-border);border-radius:13px;background:var(--app-menu);backdrop-filter:blur(22px);-webkit-backdrop-filter:blur(22px);box-shadow:var(--app-shadow-lg)}.category-item{display:block;width:100%;padding:9px 10px;border:0;border-radius:9px;background:transparent;color:var(--app-text-soft);text-align:left;font-size:12px;cursor:pointer}.category-item:hover{background:var(--app-hover);color:var(--app-text)}.category-loading{padding:9px 10px;color:var(--app-muted);font-size:11px}

/* Table route: give the table workspace a real viewport-constrained flex chain.
   This keeps the page itself fixed and lets .table-scroll own vertical scrolling. */
.app-shell.table-route{height:100dvh;min-height:100dvh;overflow:hidden;display:flex;flex-direction:column}
.app-shell.table-route .app-content{position:relative;z-index:1;flex:1 1 auto;min-height:0;height:auto;overflow:hidden;padding:0 12px 20px;display:flex;flex-direction:column}
.app-shell.table-route .app-content > *{flex:1 1 auto;min-height:0;min-width:0}

.app-content{position:relative;z-index:1;padding:0 12px 20px}
@media(max-width:1000px){.app-header{grid-template-columns:1fr auto}.app-nav{grid-column:1/-1;grid-row:2;justify-content:center}.app-header-actions{grid-column:2;grid-row:1}.app-header{gap:8px}}
@media(max-width:640px){.app-header{margin:8px;padding:8px}.nav-btn span,.new-table-btn span{display:none}.nav-btn,.new-table-btn{width:38px;padding:0}.app-nav{overflow:auto;justify-content:flex-start}.app-brand h1{font-size:13px}.app-content{padding-inline:6px}.app-shell.table-route .app-content{padding-inline:6px;padding-bottom:0}}
</style>
