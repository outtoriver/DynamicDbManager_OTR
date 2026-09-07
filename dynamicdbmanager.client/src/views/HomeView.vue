<template>
  <div class="home-page pb-8">
    <section class="hero glass-panel">
      <div class="hero-copy">
        <span class="eyebrow"><span class="eyebrow-dot"></span> РАБОЧЕЕ ПРОСТРАНСТВО</span>
        <h1>{{ route.query.category ? `Таблицы · ${route.query.category}` : 'Учеты и таблицы' }}</h1>
        <p>Управляйте рабочими таблицами, открывайте записи и быстро переходите к нужным данным.</p>
      </div>
      <div class="hero-actions">
        <button v-if="authStore.isAdmin" class="hero-button primary" @click="openCreate">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path stroke-width="1.8" stroke-linecap="round" d="M12 5v14M5 12h14" /></svg>
          Новая таблица
        </button>
        <button class="hero-button excel" @click="excelOpen = true">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M5 4h11l3 3v13H5z" stroke-width="1.7"/><path d="M8 9h8M8 13h8M8 17h5" stroke-width="1.7" stroke-linecap="round"/></svg>
          Excel
        </button>
        <button class="hero-button" @click="refreshTables" :disabled="tablesStore.loading">
          <svg :class="{ spin: tablesStore.loading }" viewBox="0 0 24 24" fill="none" stroke="currentColor"><path stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" d="M20 12a8 8 0 1 1-2.34-5.66M20 5v5h-5" /></svg>
          {{ tablesStore.loading ? 'Обновление…' : 'Обновить' }}
        </button>
      </div>
    </section>

    <section class="stats-grid">
      <article class="stat-card glass-panel">
        <span class="stat-icon indigo"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path stroke-width="1.8" d="M5 6.5C5 5.12 8.13 4 12 4s7 1.12 7 2.5S15.87 9 12 9 5 7.88 5 6.5Zm0 0V12c0 1.38 3.13 2.5 7 2.5s7-1.12 7-2.5V6.5m-14 5.5v5.5c0 1.38 3.13 2.5 7 2.5s7-1.12 7-2.5V12" /></svg></span>
        <span><small>ТАБЛИЦЫ</small><strong>{{ tablesStore.tables.length }}</strong></span>
      </article>
      <article class="stat-card glass-panel">
        <span class="stat-icon cyan"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path stroke-width="1.8" stroke-linecap="round" d="M5 6h14M5 12h14M5 18h14" /></svg></span>
        <span><small>ПОЛЯ</small><strong>{{ totalColumns }}</strong></span>
      </article>
      <article class="stat-card glass-panel">
        <span class="stat-icon emerald"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><circle cx="12" cy="12" r="8" stroke-width="1.8" /><path stroke-width="1.8" stroke-linecap="round" d="M12 8v4l2.8 1.8" /></svg></span>
        <span><small>КАТЕГОРИИ</small><strong>{{ tablesStore.categories.length }}</strong></span>
      </article>
      <article v-if="authStore.isAdmin" class="stat-card glass-panel">
        <span class="stat-icon violet"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path stroke-width="1.8" stroke-linecap="round" d="M16 20v-1.2a4.8 4.8 0 0 0-4.8-4.8H7.8A4.8 4.8 0 0 0 3 18.8V20m9-10a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7Zm6.8 2.4a3 3 0 0 0 0-5.8m2.2 11.4v-1.1a4.2 4.2 0 0 0-2.9-4" /></svg></span>
        <span><small>ПОЛЬЗОВАТЕЛИ</small><strong>{{ adminStore.users.length }}</strong></span>
      </article>
    </section>

    <Transition name="fade">
      <section v-if="showCreateForm" class="create-section glass-panel">
        <div class="section-heading">
          <div>
            <span class="eyebrow">СОЗДАНИЕ</span>
            <h2>Новая таблица</h2>
            <p>Определите название и структуру полей. Изменить набор отображаемых колонок можно позже.</p>
          </div>
          <button class="circle-btn" @click="closeCreate" title="Закрыть">✕</button>
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
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><circle cx="11" cy="11" r="6.5" stroke-width="1.8" /><path stroke-width="1.8" stroke-linecap="round" d="m16 16 4.5 4.5" /></svg>
            <input v-model="searchQuery" placeholder="Поиск таблиц…" autocomplete="off" />
            <button v-if="searchQuery" @click="searchQuery = ''">✕</button>
          </div>
          <span class="result-count">{{ filteredTables.length }} из {{ tablesStore.tables.length }}</span>
        </div>
      </div>

      <div v-if="tablesStore.loading && !tablesStore.tables.length" class="skeleton-grid">
        <div v-for="i in 6" :key="i" class="skeleton-card"></div>
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
import { ref, computed, onMounted, watch } from 'vue'
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
    const cat = String(route.query.category || '')
    const query = searchQuery.value.trim().toLowerCase()
    return tablesStore.tables.filter(t => {
      const name = String(t.name || '').trim()
      const categoryOk = !cat || name.startsWith(cat + ' ') || name === cat
      const searchOk = !query || name.toLowerCase().includes(query)
      return categoryOk && searchOk
    })
})

const totalColumns = computed(() => tablesStore.tables.reduce((sum, t) => sum + (Array.isArray(t.tableColumns) ? t.tableColumns.length : 0), 0))

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
    syncCreateQuery(false)
}

watch(() => route.query.create, val => {
    if (val === 'true') showCreateForm.value = true
} , { immediate: true })

async function refreshTables() {
    await tablesStore.loadTables(true)
}

async function loadTables() {
    await tablesStore.loadTables(true)
}

async function handleDeleteTable(table) {
    if (!authStore.isAdmin) return
    if (!confirm(`Удалить таблицу «${table.name}»? Все записи и вложения этой таблицы также будут удалены.`)) return
    try {
      await apiClient.delete(`/admin/adminTables/${table.id}`)
      await loadTables()
    } catch (err) {
      console.error('Ошибка удаления таблицы:', err)
      alert('Ошибка удаления таблицы: ' + (err.response?.data || err.message))
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
    if (!tablesStore.loaded) await tablesStore.loadTables()
    if (authStore.isAdmin && !adminStore.loaded) await adminStore.loadAdminData()
})
</script>

<style scoped>
.glass-panel {
  background: var(--app-surface);
  border: 1px solid var(--app-border);
  box-shadow: var(--app-shadow), inset 0 1px 0 var(--app-highlight);
}
  .home-page {
    display: flex;
    flex-direction: column;
    gap: 14px;
  }

  .hero, .table-section, .create-section {
    border-radius: 26px;
    overflow: hidden;
  }

  .hero {
    padding: 22px;
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 16px;
  }

  .hero-copy { min-width: 0; }

  .hero h1 {
    margin: 8px 0 6px;
    font-size: clamp(1.45rem,2vw,2rem);
    font-weight: 750;
    letter-spacing: -.035em;
  }

  .hero p {
    margin: 0;
    max-width: 760px;
    color: var(--app-muted);
  }

  .eyebrow {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    font-size: .68rem;
    letter-spacing: .14em;
    font-weight: 700;
    color: #74809a;
  }

  .eyebrow-dot {
    width: 7px;
    height: 7px;
    border-radius: 50%;
    background: #7b8aff;
    box-shadow: 0 0 14px rgba(123,138,255,.7);
  }

  .hero-actions {
    display: flex;
    gap: 8px;
    flex-shrink: 0;
  }

  .hero-button {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    height: 40px;
    padding: 0 13px;
    border-radius: 13px;
    border: 1px solid rgba(255,255,255,.08);
    background: rgba(255,255,255,.04);
    color: #b8c1d3;
    transition: .18s;
  }

  .hero-button:hover {
    background: rgba(255,255,255,.075);
    color: #fff;
    transform: translateY(-1px);
  }

  .hero-button:disabled {
    opacity: .55;
    cursor: not-allowed;
    transform: none;
  }

  .hero-button.primary {
    color: white;
    border-color: rgba(115,133,255,.26);
    background: linear-gradient(135deg,rgba(115,133,255,.84),rgba(141,108,255,.78));
    box-shadow: 0 12px 35px rgba(115,133,255,.16);
  }

  .hero-button.excel {
    color: #bbf7d0;
    border-color: rgba(74,222,128,.18);
    background: rgba(16,185,129,.10);
  }

  .hero-button.excel:hover {
    background: rgba(16,185,129,.16);
  }

  .hero-button svg {
    width: 17px;
    height: 17px;
  }

  .spin { animation: spin .8s linear infinite; }

  @keyframes spin { to { transform: rotate(360deg); } }

  .stats-grid {
    display: grid;
    grid-template-columns: repeat(4,minmax(0,1fr));
    gap: 12px;
  }

  .stat-card {
    min-height: 78px;
    border-radius: 20px;
    padding: 15px;
    display: flex;
    align-items: center;
    gap: 12px;
  }

  .stat-card > span:last-child {
    display: flex;
    flex-direction: column;
    min-width: 0;
  }

  .stat-card small {
    color: #6f7b92;
    font-size: .63rem;
    letter-spacing: .13em;
    font-weight: 700;
  }

  .stat-card strong {
    margin-top: 2px;
    font-size: 1.35rem;
    line-height: 1;
  }

  .stat-icon {
    width: 38px;
    height: 38px;
    display: grid;
    place-items: center;
    border-radius: 13px;
    border: 1px solid rgba(255,255,255,.07);
  }

  .stat-icon svg { width: 19px; height: 19px; }
  .stat-icon.indigo { color: #aab2ff; background: rgba(115,133,255,.12); }
  .stat-icon.cyan { color: #77dbff; background: rgba(69,201,255,.10); }
  .stat-icon.emerald { color: #74e1ba; background: rgba(82,215,167,.10); }
  .stat-icon.violet { color: #c1a5ff; background: rgba(141,108,255,.11); }

  .section-heading {
    display: flex;
    justify-content: space-between;
    gap: 15px;
    align-items: flex-start;
    padding: 20px 20px 0;
  }

  .section-heading.compact {
    align-items: center;
    padding-bottom: 16px;
  }

  .section-heading h2 {
    margin: 5px 0 0;
    font-size: 1.05rem;
    font-weight: 700;
    letter-spacing: -.015em;
  }

  .section-heading p {
    margin: 5px 0 0;
    color: #7f8aa0;
    max-width: 760px;
    font-size: .86rem;
  }

  .circle-btn {
    width: 34px;
    height: 34px;
    border-radius: 11px;
    border: 1px solid rgba(255,255,255,.08);
    background: rgba(255,255,255,.04);
    color: #8994aa;
  }

  .circle-btn:hover { color: #fff; background: rgba(255,255,255,.08); }

  .section-tools {
    display: flex;
    align-items: center;
    gap: 9px;
  }

  .search-box {
    display: flex;
    align-items: center;
    gap: 8px;
    min-width: min(290px,45vw);
    height: 38px;
    padding: 0 10px;
    border-radius: 12px;
    border: 1px solid rgba(255,255,255,.07);
    background: rgba(6,10,18,.38);
  }

  .search-box svg { width: 16px; height: 16px; color: #68748b; flex-shrink: 0; }

  .search-box input {
    flex: 1;
    min-width: 0;
    border: 0;
    outline: 0;
    background: transparent;
    color: #eef2ff;
  }

  .search-box input::placeholder { color: #58637a; }

  .search-box button { border: 0; background: transparent; color: #6f7a91; cursor: pointer; }

  .result-count { color: #6f7b92; font-size: .75rem; white-space: nowrap; }

  .create-section { padding-bottom: 20px; }

  .create-section :deep(.glass-card) {
    margin: 16px 20px 0;
    background: rgba(5,8,16,.30);
    box-shadow: none;
    border-color: rgba(255,255,255,.07);
  }

  .table-section { padding-bottom: 18px; }

  .skeleton-grid {
    display: grid;
    grid-template-columns: repeat(3,minmax(0,1fr));
    gap: 12px;
    padding: 0 20px;
  }

  .skeleton-card {
    height: 130px;
    border-radius: 20px;
    background: linear-gradient(110deg,rgba(255,255,255,.04) 25%,rgba(255,255,255,.07) 37%,rgba(255,255,255,.04) 63%);
    background-size: 400% 100%;
    animation: skeleton 1.25s ease infinite;
  }

  @keyframes skeleton { to { background-position: -135% 0; } }

  @media (max-width:980px) {
    .stats-grid { grid-template-columns: repeat(2,1fr); }
    .skeleton-grid { grid-template-columns: repeat(2,1fr); }
    .hero { flex-direction: column; }
  }

  @media (max-width:640px) {
    .stats-grid { grid-template-columns: 1fr 1fr; }
    .skeleton-grid { grid-template-columns: 1fr; }
    .section-heading.compact { align-items: flex-start; flex-direction: column; }
    .section-tools { width: 100%; }
    .search-box { min-width: 0; flex: 1; }
    .result-count { display: none; }
    .hero { padding: 18px; }
    .hero-actions { width: 100%; flex-wrap: wrap; }
    .hero-button { flex: 1 1 calc(50% - 4px); justify-content: center; }
    .section-heading { padding-inline: 15px; }
  }

.fade-enter-active,.fade-leave-active { transition: opacity .2s ease; }
.fade-enter-from,.fade-leave-to { opacity: 0; }
</style>
