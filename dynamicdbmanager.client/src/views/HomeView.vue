<template>
  <div class="home-page">
    <section class="hero glass-panel">
      <div class="hero-copy">
        <span class="eyebrow"><span class="eyebrow-dot"></span>РАБОЧЕЕ ПРОСТРАНСТВО</span>
        <h1>{{ route.query.category ? `Таблицы · ${route.query.category}` : 'Учёты и таблицы' }}</h1>
        <p>Управляйте рабочими таблицами, открывайте записи и быстро переходите к нужным данным.</p>
      </div>

      <div class="hero-actions">
        <button v-if="authStore.isAdmin" type="button" class="hero-button primary" @click="openCreate">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true">
            <path stroke-width="1.8" stroke-linecap="round" d="M12 5v14M5 12h14" />
          </svg>
          Новая таблица
        </button>

        <button type="button" class="hero-button excel" @click="excelOpen = true">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true">
            <path d="M5 4h11l3 3v13H5z" stroke-width="1.7"/>
            <path d="M8 9h8M8 13h8M8 17h5" stroke-width="1.7" stroke-linecap="round"/>
          </svg>
          Excel
        </button>

        <button type="button" class="hero-button" :disabled="tablesStore.loading" @click="refreshTables">
          <svg :class="{ spin: tablesStore.loading }" viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true">
            <path stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" d="M20 12a8 8 0 1 1-2.34-5.66M20 5v5h-5" />
          </svg>
          {{ tablesStore.loading ? 'Обновление…' : 'Обновить' }}
        </button>
      </div>
    </section>

    <section class="stats-grid" aria-label="Статистика">
      <article class="stat-card glass-panel">
        <span class="stat-icon indigo">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path stroke-width="1.8" d="M5 6.5C5 5.12 8.13 4 12 4s7 1.12 7 2.5S15.87 9 12 9 5 7.88 5 6.5Zm0 0V12c0 1.38 3.13 2.5 7 2.5s7-1.12 7-2.5V6.5m-14 5.5v5.5c0 1.38 3.13 2.5 7 2.5s7-1.12 7-2.5V12" /></svg>
        </span>
        <span><small>ТАБЛИЦЫ</small><strong>{{ tablesStore.tables.length }}</strong></span>
      </article>

      <article class="stat-card glass-panel">
        <span class="stat-icon cyan">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path stroke-width="1.8" stroke-linecap="round" d="M5 6h14M5 12h14M5 18h14" /></svg>
        </span>
        <span><small>ПОЛЯ</small><strong>{{ totalColumns }}</strong></span>
      </article>

      <article class="stat-card glass-panel">
        <span class="stat-icon emerald">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><circle cx="12" cy="12" r="8" stroke-width="1.8" /><path stroke-width="1.8" stroke-linecap="round" d="M12 8v4l2.8 1.8" /></svg>
        </span>
        <span><small>КАТЕГОРИИ</small><strong>{{ tablesStore.categories.length }}</strong></span>
      </article>

      <article v-if="authStore.isAdmin" class="stat-card glass-panel">
        <span class="stat-icon violet">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path stroke-width="1.8" stroke-linecap="round" d="M16 20v-1.2a4.8 4.8 0 0 0-4.8-4.8H7.8A4.8 4.8 0 0 0 3 18.8V20m9-10a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7Zm6.8 2.4a3 3 0 0 0 0-5.8m2.2 11.4v-1.1a4.2 4.2 0 0 0-2.9-4" /></svg>
        </span>
        <span><small>ПОЛЬЗОВАТЕЛИ</small><strong>{{ adminStore.users.length }}</strong></span>
      </article>
    </section>

    <Transition name="fade">
      <section v-if="showCreateForm" class="create-section glass-panel">
        <div class="section-heading">
          <div>
            <span class="eyebrow">СОЗДАНИЕ</span>
            <h2>Новая таблица</h2>
            <p>Определите название и структуру полей. Набор отображаемых колонок можно изменить позже.</p>
          </div>
          <button type="button" class="circle-btn" @click="closeCreate" title="Закрыть" aria-label="Закрыть">×</button>
        </div>
        <CreateTableForm @table-created="onTableCreated" />
      </section>
    </Transition>

    <section class="table-section glass-panel">
      <div class="section-heading compact">
        <div>
          <span class="eyebrow">КАТАЛОГ</span>
          <h2>Ваши таблицы</h2>
        </div>

        <div class="section-tools">
          <div class="search-box">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true">
              <circle cx="11" cy="11" r="6.5" stroke-width="1.8" />
              <path stroke-width="1.8" stroke-linecap="round" d="m16 16 4.5 4.5" />
            </svg>
            <input v-model="searchQuery" type="search" placeholder="Поиск таблиц…" autocomplete="off" />
            <button v-if="searchQuery" type="button" @click="searchQuery = ''" aria-label="Очистить поиск">×</button>
          </div>
          <span class="result-count">{{ filteredTables.length }} из {{ tablesStore.tables.length }}</span>
        </div>
      </div>

      <div v-if="tablesStore.loading && !tablesStore.tables.length" class="skeleton-grid" aria-hidden="true">
        <div v-for="i in 6" :key="i" class="skeleton-card"></div>
      </div>

      <div v-else-if="!filteredTables.length" class="empty-state">
        <div class="empty-icon">⌕</div>
        <h3>{{ searchQuery || route.query.category ? 'Ничего не найдено' : 'Таблиц пока нет' }}</h3>
        <p>{{ searchQuery || route.query.category ? 'Попробуйте изменить поиск или категорию.' : 'Создайте первую таблицу, чтобы начать работу.' }}</p>
        <button v-if="authStore.isAdmin && !searchQuery" type="button" class="empty-action" @click="openCreate">Создать таблицу</button>
      </div>

      <TableList v-else
                 :tables="filteredTables"
                 @delete-table="handleDeleteTable"
                 @table-updated="loadTables" />
    </section>

    <ExcelTransferModal
      v-model="excelOpen"
      :tables="tablesStore.tables"
      @completed="onExcelCompleted"
    />
  </div>
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTablesStore } from '../stores/tables'
import { useAuthStore } from '../stores/auth'
import { useAdminStore } from '../stores/admin'
import CreateTableForm from '../components/CreateTableForm.vue'
import TableList from '../components/TableList.vue'
import ExcelTransferModal from '../components/ExcelTransferModal.vue'
import apiClient from '../api'

const route = useRoute()
const router = useRouter()
const tablesStore = useTablesStore()
const authStore = useAuthStore()
const adminStore = useAdminStore()

const showCreateForm = ref(false)
const searchQuery = ref('')
const excelOpen = ref(false)

const filteredTables = computed(() => {
  const category = String(route.query.category || '').trim()
  const query = searchQuery.value.trim().toLowerCase()

  return tablesStore.tables.filter(table => {
    const name = String(table?.name || '').trim()
    const categoryOk = !category || name === category || name.startsWith(`${category} `)
    const searchOk = !query || name.toLowerCase().includes(query)
    return categoryOk && searchOk
  })
})

const totalColumns = computed(() =>
  tablesStore.tables.reduce(
    (sum, table) => sum + (Array.isArray(table?.tableColumns) ? table.tableColumns.length : 0),
    0
  )
)

function syncCreateQuery(value) {
  showCreateForm.value = value

  if (value && route.query.create === 'true') {
    const query = { ...route.query }
    delete query.create
    router.replace({ name: 'home', query })
  }
}

function openCreate() {
  syncCreateQuery(true)
}

function closeCreate() {
  showCreateForm.value = false
}

watch(
  () => route.query.create,
  value => {
    showCreateForm.value = value === 'true'
  },
  { immediate: true }
)

async function refreshTables() {
  try {
    await tablesStore.loadTables(true)
  } catch (error) {
    console.error('Ошибка обновления таблиц:', error)
  }
}

async function loadTables() {
  try {
    await tablesStore.loadTables(true)
  } catch (error) {
    console.error('Ошибка загрузки таблиц:', error)
  }
}

async function handleDeleteTable(table) {
  if (!authStore.isAdmin || !table) return

  const confirmed = window.confirm(
    `Удалить таблицу «${table.name}»? Все записи и вложения этой таблицы также будут удалены.`
  )

  if (!confirmed) return

  try {
    await apiClient.delete(`/admin/adminTables/${table.id}`)
    await loadTables()
  } catch (error) {
    console.error('Ошибка удаления таблицы:', error)
    window.alert(`Ошибка удаления таблицы: ${error?.response?.data || error?.message || 'Неизвестная ошибка'}`)
  }
}

async function onTableCreated() {
  showCreateForm.value = false
  await loadTables()
}

async function onExcelCompleted() {
  await loadTables()
}

onMounted(async () => {
  try {
    if (!tablesStore.loaded) await tablesStore.loadTables()
    if (authStore.isAdmin && !adminStore.loaded) await adminStore.loadAdminData()
  } catch (error) {
    console.error('Ошибка инициализации главной страницы:', error)
  }
})
</script>

<style scoped>
.home-page { display:flex; flex-direction:column; gap:14px; padding-bottom:32px; }
.glass-panel { background:var(--app-surface); border:1px solid var(--app-border); box-shadow:var(--app-shadow), inset 0 1px 0 var(--app-highlight); color:var(--app-text); }
.hero,.table-section,.create-section { border-radius:26px; overflow:hidden; }
.hero { padding:22px; display:flex; justify-content:space-between; align-items:flex-start; gap:20px; }
.hero-copy { min-width:0; }
.hero h1 { margin:8px 0 6px; font-size:clamp(1.45rem,2vw,2rem); font-weight:750; line-height:1.12; letter-spacing:-.035em; }
.hero p { margin:0; max-width:760px; color:var(--app-muted); }
.eyebrow { display:inline-flex; align-items:center; gap:8px; font-size:.68rem; letter-spacing:.14em; font-weight:800; color:var(--app-primary); }
.eyebrow-dot { width:7px; height:7px; border-radius:50%; background:var(--app-primary); box-shadow:0 0 12px color-mix(in srgb,var(--app-primary) 55%,transparent); }
.hero-actions { display:flex; gap:8px; flex-shrink:0; flex-wrap:wrap; }
.hero-button { display:inline-flex; align-items:center; justify-content:center; gap:8px; min-height:40px; padding:0 13px; border-radius:13px; border:1px solid var(--app-border); background:var(--app-control); color:var(--app-text-soft); cursor:pointer; transition:transform .18s ease,background .18s ease,border-color .18s ease,box-shadow .18s ease; }
.hero-button:hover:not(:disabled) { background:var(--app-control-hover); border-color:var(--app-border-strong); transform:translateY(-1px); }
.hero-button:disabled { opacity:.55; cursor:not-allowed; }
.hero-button svg { width:17px; height:17px; }
.hero-button.primary { color:#fff; border-color:color-mix(in srgb,var(--app-primary) 48%,var(--app-border)); background:linear-gradient(135deg,var(--app-primary),var(--app-primary-2)); box-shadow:0 12px 28px color-mix(in srgb,var(--app-primary) 22%,transparent); }
.hero-button.excel { color:var(--app-success); border-color:color-mix(in srgb,var(--app-success) 30%,var(--app-border)); background:color-mix(in srgb,var(--app-success) 8%,var(--app-control)); }
.stats-grid { display:grid; grid-template-columns:repeat(4,minmax(0,1fr)); gap:12px; }
.stat-card { min-height:78px; border-radius:20px; padding:15px; display:flex; align-items:center; gap:12px; }
.stat-card>span:last-child { display:flex; flex-direction:column; min-width:0; }
.stat-card small { color:var(--app-muted); font-size:.63rem; letter-spacing:.13em; font-weight:800; }
.stat-card strong { margin-top:2px; font-size:1.35rem; line-height:1; color:var(--app-text); }
.stat-icon { width:38px; height:38px; flex:0 0 auto; display:grid; place-items:center; border-radius:13px; border:1px solid var(--app-border); background:var(--app-control-soft); }
.stat-icon svg { width:19px; height:19px; }
.stat-icon.indigo { color:#6475ff; background:color-mix(in srgb,#6475ff 10%,var(--app-control)); }
.stat-icon.cyan { color:#0891b2; background:color-mix(in srgb,#0891b2 10%,var(--app-control)); }
.stat-icon.emerald { color:#059669; background:color-mix(in srgb,#059669 10%,var(--app-control)); }
.stat-icon.violet { color:#7c3aed; background:color-mix(in srgb,#7c3aed 10%,var(--app-control)); }
.section-heading { display:flex; justify-content:space-between; gap:15px; align-items:flex-start; padding:20px 20px 0; }
.section-heading.compact { align-items:center; padding-bottom:16px; }
.section-heading h2 { margin:5px 0 0; font-size:1.05rem; font-weight:750; letter-spacing:-.015em; color:var(--app-text); }
.section-heading p { margin:5px 0 0; color:var(--app-muted); max-width:760px; font-size:.86rem; }
.circle-btn { width:34px; height:34px; border-radius:11px; border:1px solid var(--app-border); background:var(--app-control); color:var(--app-muted); cursor:pointer; }
.circle-btn:hover { color:var(--app-text); background:var(--app-control-hover); }
.section-tools { display:flex; align-items:center; gap:9px; min-width:0; }
.search-box { display:flex; align-items:center; gap:8px; min-width:min(290px,45vw); height:38px; padding:0 10px; border-radius:12px; border:1px solid var(--app-border); background:var(--app-control); }
.search-box:focus-within { border-color:var(--app-primary-border); box-shadow:0 0 0 3px var(--app-active); }
.search-box svg { width:16px; height:16px; color:var(--app-muted); flex-shrink:0; }
.search-box input { flex:1; min-width:0; border:0; outline:0; background:transparent; color:var(--app-text); }
.search-box input::placeholder { color:var(--app-muted); }
.search-box button { border:0; background:transparent; color:var(--app-muted); cursor:pointer; }
.result-count { color:var(--app-muted); font-size:.75rem; white-space:nowrap; }
.create-section { padding-bottom:20px; }
.create-section :deep(.glass-card) { margin:16px 20px 0; background:var(--app-control-soft); border-color:var(--app-border); box-shadow:none; }
.table-section { padding-bottom:18px; }
.skeleton-grid { display:grid; grid-template-columns:repeat(3,minmax(0,1fr)); gap:12px; padding:0 20px; }
.skeleton-card { height:130px; border-radius:20px; background:linear-gradient(110deg,var(--app-control-soft) 25%,var(--app-hover) 37%,var(--app-control-soft) 63%); background-size:400% 100%; animation:skeleton 1.25s ease infinite; }
.empty-state { min-height:270px; display:flex; flex-direction:column; align-items:center; justify-content:center; text-align:center; padding:40px 20px; }
.empty-icon { width:54px; height:54px; display:grid; place-items:center; border-radius:17px; background:var(--app-control-soft); border:1px solid var(--app-border); color:var(--app-primary); font-size:22px; }
.empty-state h3 { margin:12px 0 4px; color:var(--app-text); font-size:15px; }
.empty-state p { margin:0; color:var(--app-muted); font-size:11px; }
.empty-action { margin-top:14px; min-height:38px; padding:0 13px; border-radius:12px; border:1px solid var(--app-primary-border); background:var(--app-primary-soft); color:var(--app-primary-text); cursor:pointer; }
.empty-action:hover { background:var(--app-primary-soft-strong); }
.spin { animation:spin .8s linear infinite; }
@keyframes spin { to { transform:rotate(360deg); } }
@keyframes skeleton { to { background-position:-135% 0; } }
.fade-enter-active,.fade-leave-active { transition:opacity .2s ease,transform .2s ease; }
.fade-enter-from,.fade-leave-to { opacity:0; transform:translateY(6px); }
@media (max-width:980px){ .stats-grid{grid-template-columns:repeat(2,1fr)} .skeleton-grid{grid-template-columns:repeat(2,1fr)} .hero{flex-direction:column} .hero-actions{width:100%} }
@media (max-width:640px){ .stats-grid{grid-template-columns:1fr 1fr} .skeleton-grid{grid-template-columns:1fr} .section-heading.compact{align-items:flex-start;flex-direction:column} .section-tools{width:100%} .search-box{min-width:0;flex:1} .result-count{display:none} .hero{padding:18px} .hero-button{flex:1 1 calc(50% - 4px)} .section-heading{padding-inline:15px} .create-section :deep(.glass-card){margin-inline:15px} }
</style>
