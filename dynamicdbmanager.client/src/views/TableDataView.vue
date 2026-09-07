<template>
  <div class="table-page">
    <div class="table-header">
      <div class="table-heading">
        <div class="eyebrow">Data workspace</div>
        <h2 class="table-title">{{ selectedTable?.name || 'Таблица' }}</h2>
        <div class="table-subtitle" v-if="selectedTable">{{ totalRows }} записей · {{ selectedTable.tableColumns?.length || 0 }} столбцов</div>
      </div>
    </div>

    <div class="tabs-panel glass-panel" v-if="tablesStore.tables.length">
      <button v-if="showLeftArrow" type="button" class="tab-arrow" @click="scrollTabs(-1)">‹</button>
      <div ref="tabsContainer" class="tabs-scroll" @scroll="updateArrows" @wheel="onWheel">
        <template v-for="table in tablesStore.tables" :key="table.id">
          <button v-if="editingTabId !== table.id" type="button" class="tab" :class="{ active: selectedTable?.id === table.id }" @click="selectTable(table)" @dblclick="startTabEdit(table)">{{ table.name }}</button>
          <div v-else class="tab-edit">
            <input v-model="editingTabName" type="text" @keyup.enter="saveTabEdit(table)" @keyup.esc="cancelTabEdit" ref="tabEditInput" autofocus />
            <button type="button" class="tab-mini" @click="saveTabEdit(table)">✓</button>
            <button type="button" class="tab-mini" @click="cancelTabEdit">×</button>
          </div>
        </template>
      </div>
      <button v-if="showRightArrow" type="button" class="tab-arrow" @click="scrollTabs(1)">›</button>
    </div>

    <div class="toolbar glass-panel" v-if="selectedTable">
      <div class="search-box">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><circle cx="11" cy="11" r="6.5" stroke-width="1.8"/><path d="m16 16 4.5 4.5" stroke-width="1.8" stroke-linecap="round"/></svg>
        <input v-model="searchQuery" type="text" placeholder="Поиск по текущей таблице..." @input="onSearchInput" />
        <button v-if="searchQuery" type="button" class="search-clear" @click="clearSearch">×</button>
      </div>

      <button type="button" class="toolbar-btn" :class="{active:globalSearchEnabled}" @click="toggleGlobalSearch">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><circle cx="11" cy="11" r="6.5" stroke-width="1.8"/><path d="m16 16 4.5 4.5" stroke-width="1.8" stroke-linecap="round"/></svg>
        <span>Глобальный</span>
      </button>
      <button type="button" class="toolbar-btn excel" @click="excelTransferOpen = true" title="Импорт / экспорт Excel">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M6 3.5h8.5L19 8v12.5H6z" stroke-width="1.7" stroke-linejoin="round"/><path d="M14 3.5V8h5M8.5 12.5l2 2 3.2-3.5M8.5 17h7" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"/></svg>
        <span>Excel</span>
      </button>
      <button type="button" class="toolbar-btn primary" @click="addRow">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M12 5v14M5 12h14" stroke-width="1.9" stroke-linecap="round"/></svg><span>Добавить</span>
      </button>
      <button type="button" class="toolbar-btn" @click="openTableColumnsEditor">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M4 6h16M4 12h16M4 18h16" stroke-width="1.7" stroke-linecap="round"/></svg><span>Колонки</span>
      </button>
      <button type="button" class="toolbar-btn" @click="openModalColumnsEditor">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><rect x="5" y="3.5" width="14" height="17" rx="2" stroke-width="1.7"/><path d="M8 8h8M8 12h5M8 16h8" stroke-width="1.7" stroke-linecap="round"/></svg><span>Модалка</span>
      </button>
      <button type="button" class="toolbar-btn danger" @click="deleteSelectedTable">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M6 6l12 12M18 6 6 18" stroke-width="1.9" stroke-linecap="round"/></svg><span>Удалить</span>
      </button>
      <span class="toolbar-note">Двойной клик по строке — редактирование</span>
    </div>

    <div v-if="globalSearchEnabled && globalSearchResults.length" class="search-results glass-panel">
      <div class="search-results-head">Глобальный поиск · {{ globalSearchResults.length }} результатов</div>
      <div class="search-result-list">
        <div v-for="item in globalSearchResults" :key="`${item.tableId}-${item.id}`" class="search-result">
          <div class="min-w-0">
            <div class="search-result-title"><strong>{{ item.tableName }}</strong> · ID {{ item.id }}</div>
            <div class="search-result-value">{{ item.dataJson }}</div>
          </div>
          <button type="button" class="jump-btn" @click="navigateToRow(item)">Открыть</button>
        </div>
      </div>
    </div>

    <section class="data-panel glass-panel">
      <div v-if="!selectedTable" class="table-empty">
        <div class="empty-icon">⌑</div><div class="empty-title">Выберите таблицу</div><div class="empty-help">Откройте вкладку выше или создайте новую таблицу.</div>
      </div>
      <div v-else class="table-scroll">
        <div v-if="rowsLoading" class="table-loading"><div class="loading-orb"></div><span>Загрузка данных…</span></div>
        <div v-else-if="paginatedRows.length===0" class="table-empty"><div class="empty-icon">⌕</div><div class="empty-title">Нет данных</div><div class="empty-help">Добавьте первую запись или измените поиск.</div></div>
        <table v-else class="data-table">
          <thead>
            <tr>
              <th v-for="col in visibleColumns" :key="col.name" :style="getColumnStyle(col)" class="data-column-head" :class="{'is-resizing': columnResize.active && columnResize.name === col.name}" @contextmenu.prevent.stop="openColumnMenu($event, col)">
                <button type="button" @click="sortBy(col.name)">
                  <span>{{ col.name }}</span>
                  <span v-if="sortColumn===col.name" class="sort-indicator">{{ sortDirection==='asc' ? '↑' : '↓' }}</span>
                </button>
                <span class="column-resizer" title="Изменить ширину" @pointerdown.stop.prevent="startColumnResize($event, col)" @dblclick.stop.prevent="autoFitColumn(col)"></span>
              </th>
              <th class="row-delete-col" aria-label="Удаление"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in paginatedRows" :key="row.id" @dblclick="editRow(row)">
              <td v-for="col in visibleColumns" :key="col.name" :style="getColumnStyle(col)" :class="{'is-wrapped': isColumnWrapped(col.name)}">
                <span v-if="col.type==='boolean'" class="bool-cell" :class="getCellValue(row,col.name)===true ? 'on' : 'off'">{{ getCellValue(row,col.name)===true ? '✓' : '—' }}</span>
                <span v-else class="cell-text" :class="{'cell-text-wrap': isColumnWrapped(col.name)}" :title="String(getCellValue(row,col.name))">{{ getCellValue(row,col.name) }}</span>
              </td>
              <td class="row-delete-col">
                <button type="button" class="row-delete-btn" title="Удалить запись" aria-label="Удалить запись" @click.stop="deleteRow(row.id)">×</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="selectedTable" class="statusbar">
        <span>{{ rowsLoading ? 'Обновление…' : `Показано ${paginatedRows.length} из ${totalRows}` }}</span>
        <Pagination :current="currentPage" :total="totalRows" :page-size="pageSize" @change="changePage" @update:pageSize="(val)=>{ pageSize=val; changePage(1) }" />
      </div>
    </section>

    <ExcelTransferModal
      v-model="excelTransferOpen"
      :tables="tablesStore.tables"
      :selected-table-id="selectedTable?.id"
      @completed="onExcelCompleted"
    />

    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showRowModal" class="modal-backdrop" @click.self="closeRowModal">
          <div class="modal-card">
            <div class="modal-header"><div><h2>{{ editingRow ? 'Редактировать запись' : 'Новая запись' }}</h2><p>{{ selectedTable?.name }}</p></div><button type="button" class="modal-close" @click="closeRowModal">×</button></div>
            <div class="form-grid">
              <div class="form-section"><h3>Основные поля</h3>
                <div v-for="col in selectedTable.tableColumns.filter(c=>c.type!=='boolean')" :key="`main-${col.name}`" class="field"><label>{{ col.name }}</label><input v-model="rowData[col.name]" /></div>
                <div v-for="field in modalTextFields" :key="`modal-${field.name}`" class="field"><label>{{ field.name }}</label><input v-model="rowData[field.name]" /></div>
              </div>
              <div class="form-section"><h3>Флаги</h3>
                <label v-for="col in selectedTable.tableColumns.filter(c=>c.type==='boolean')" :key="`bool-${col.name}`" class="check-line"><input type="checkbox" v-model="rowData[col.name]"/><span>{{ col.name }}</span></label>
                <label v-for="field in modalBooleanFields" :key="`mbool-${field.name}`" class="check-line"><input type="checkbox" v-model="rowData[field.name]"/><span>{{ field.name }}</span></label>
              </div>
            </div>
            <div class="attachments">
              <div class="attachments-title">Вложения</div>
              <div v-if="editingRow" class="attachment-list">
                <div v-if="attachmentsLoading" class="empty-help">Загрузка…</div>
                <template v-else>
                  <div v-for="file in attachmentsList" :key="file.id" class="attachment"><span>{{ file.fileName }}</span><button type="button" class="attachment-btn" @click="downloadAttachment(file)">↓</button><button type="button" class="attachment-btn danger" @click="deleteAttachment(file.id)">×</button></div>
                  <label class="file-btn"><input type="file" hidden @change="uploadAttachment($event)"/>＋ Добавить файл</label>
                </template>
              </div>
              <div v-else class="empty-help">Вложения можно добавить после сохранения записи.</div>
            </div>
            <div class="modal-actions"><button type="button" class="modal-btn" @click="closeRowModal" :disabled="isSaving">Отмена</button><button type="button" class="modal-btn primary" @click="saveRow" :disabled="isSaving">{{ isSaving ? 'Сохранение…' : 'Сохранить' }}</button></div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showTableColumnsModal" class="modal-backdrop" @click.self="closeTableColumnsModal">
          <div class="modal-card small">
            <div class="modal-header"><div><h2>Колонки таблицы</h2><p>Что показываем в основной таблице</p></div><button type="button" class="modal-close" @click="closeTableColumnsModal">×</button></div>
            <div class="columns-list"><div v-for="(col,index) in tableColumnDefs" :key="index" class="column-row"><input v-model="col.name" placeholder="Название"/><select v-model="col.type"><option value="text">Текст</option><option value="boolean">Флаг</option></select><button type="button" class="icon-btn danger" @click="removeTableColumn(index)">×</button></div></div>
            <button type="button" class="add-field" @click="addTableColumn">＋ Добавить колонку</button>
            <div class="modal-actions"><button type="button" class="modal-btn" @click="closeTableColumnsModal">Отмена</button><button type="button" class="modal-btn primary" @click="saveTableColumns" :disabled="isSavingColumns">{{ isSavingColumns ? 'Сохранение…' : 'Применить' }}</button></div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showModalColumnsModal" class="modal-backdrop" @click.self="closeModalColumnsModal">
          <div class="modal-card small">
            <div class="modal-header"><div><h2>Поля модалки</h2><p>Дополнительные поля формы записи</p></div><button type="button" class="modal-close" @click="closeModalColumnsModal">×</button></div>
            <div class="columns-list"><div v-for="(col,index) in modalColumnDefs" :key="index" class="column-row"><input v-model="col.name" placeholder="Название"/><select v-model="col.type"><option value="text">Текст</option><option value="boolean">Флаг</option></select><button type="button" class="icon-btn danger" @click="removeModalColumn(index)">×</button></div></div>
            <button type="button" class="add-field" @click="addModalColumn">＋ Добавить поле</button>
            <div class="modal-actions"><button type="button" class="modal-btn" @click="closeModalColumnsModal">Отмена</button><button type="button" class="modal-btn primary" @click="saveModalColumns" :disabled="isSavingModalCols">{{ isSavingModalCols ? 'Сохранение…' : 'Применить' }}</button></div>
          </div>
        </div>
      </Transition>
    </Teleport>
    <Teleport to="body">
      <Transition name="column-menu-fade">
        <div v-if="columnMenu.open" class="column-menu-backdrop" @click="closeColumnMenu" @contextmenu.prevent="closeColumnMenu">
          <div class="column-context-menu" :style="{ left: `${columnMenu.x}px`, top: `${columnMenu.y}px` }" @click.stop>
            <div class="column-menu-title"><span>Колонка</span><strong>{{ columnMenu.column?.name }}</strong></div>
            <button type="button" class="column-menu-item" @click="autoFitColumn(columnMenu.column); closeColumnMenu()"><span class="menu-icon">↔</span><span><b>Автоподбор ширины</b><small>По заголовку и данным</small></span></button>
            <button type="button" class="column-menu-item" @click="promptColumnWidth(columnMenu.column); closeColumnMenu()"><span class="menu-icon">↔</span><span><b>Задать ширину</b><small>{{ getColumnWidth(columnMenu.column) }} px</small></span></button>
            <button type="button" class="column-menu-item" @click="toggleColumnWrap(columnMenu.column); closeColumnMenu()"><span class="menu-icon">↕</span><span><b>{{ isColumnWrapped(columnMenu.column?.name) ? 'Отключить перенос' : 'Переносить текст' }}</b><small>Показывать длинный текст в ячейке</small></span></button>
            <div class="column-menu-separator"></div>
            <button type="button" class="column-menu-item" @click="hideColumn(columnMenu.column); closeColumnMenu()"><span class="menu-icon">◧</span><span><b>Скрыть колонку</b><small>Настройка останется сохранённой</small></span></button>
            <button type="button" class="column-menu-item" @click="resetColumn(columnMenu.column); closeColumnMenu()"><span class="menu-icon">↺</span><span><b>Сбросить настройки</b><small>Стандартная ширина и перенос</small></span></button>
            <button v-if="hiddenColumnNames.length" type="button" class="column-menu-item" @click="showAllColumns(); closeColumnMenu()"><span class="menu-icon">⊞</span><span><b>Показать скрытые</b><small>{{ hiddenColumnNames.length }} скрытых колонок</small></span></button>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount, nextTick, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTablesStore } from '../stores/tables'
import apiClient, { attachmentsApi } from '../api'
import Pagination from '../components/Pagination.vue'
import ExcelTransferModal from '../components/ExcelTransferModal.vue'

const route = useRoute()
const router = useRouter()
const tablesStore = useTablesStore()

const selectedTable = ref(null)
const rows = ref([])
const totalRows = ref(0)
const rowsLoading = ref(false)
const searchQuery = ref('')
const globalSearchEnabled = ref(false)
const globalSearchResults = ref([])
const showRowModal = ref(false)
const editingRow = ref(null)
const showTableColumnsModal = ref(false)
const showModalColumnsModal = ref(false)
const tableColumnDefs = ref([])
const modalColumnDefs = ref([])
const rowData = ref({})
const sortColumn = ref(null)
const sortDirection = ref('asc')
const editingTabId = ref(null)
const editingTabName = ref('')
const tabEditInput = ref(null)
const isSaving = ref(false)
const isSavingColumns = ref(false)
const isSavingModalCols = ref(false)
const excelTransferOpen = ref(false)

// Пагинация
const currentPage = ref(1)
const pageSize = ref(100)

// Настройки колонок таблицы. table.css не изменяется — вся механика изолирована здесь.
const columnPreferences = ref({ widths: {}, hidden: [], wrapped: {} })
const columnMenu = ref({ open: false, x: 0, y: 0, column: null })
const columnResize = ref({ active: false, name: null, startX: 0, startWidth: 0 })
const columnResizeCleanup = ref(null)
const COLUMN_PREF_PREFIX = 'dynamicDbManager.tableColumns.'
const COLUMN_MIN_WIDTH = 90
const COLUMN_MAX_WIDTH = 760
const COLUMN_DEFAULT_WIDTH = 180

// Вложения
const attachmentsList = ref([])
const attachmentsLoading = ref(false)

let searchTimeout = null
let rowsRequestController = null
let rowsRequestId = 0
const rowJsonCache = new WeakMap()

// -------------------- Вкладки --------------------
const tabsContainer = ref(null)
const showLeftArrow = ref(false)
const showRightArrow = ref(false)

function updateArrows() {
    if (!tabsContainer.value) return
    const { scrollLeft, scrollWidth, clientWidth } = tabsContainer.value
    showLeftArrow.value = scrollLeft > 0
    showRightArrow.value = scrollLeft + clientWidth < scrollWidth - 1
}

function scrollTabs(direction) {
    if (!tabsContainer.value) return
    tabsContainer.value.scrollBy({ left: direction * 200, behavior: 'smooth' })
}

function onWheel(event) {
    if (!tabsContainer.value) return
    event.preventDefault()
    tabsContainer.value.scrollBy({ left: event.deltaY, behavior: 'auto' })
}

watch(() => tablesStore.tables, () => {
    nextTick(updateArrows)
}, { immediate: true })

function startTabEdit(table) {
    editingTabId.value = table.id
    editingTabName.value = table.name
    nextTick(() => {
      if (tabEditInput.value) tabEditInput.value.focus()
    })
}

async function saveTabEdit(table) {
    if (!editingTabName.value.trim()) return
    try {
      await apiClient.put(`/admin/adminTables/${table.id}`, {
        name: editingTabName.value,
        tableColumns: table.tableColumns,
        modalColumns: table.modalColumns
      })
      table.name = editingTabName.value
      if (selectedTable.value?.id === table.id) {
        selectedTable.value.name = editingTabName.value
      }
      await tablesStore.loadTables(true)
    } catch (err) {
      console.error('Ошибка обновления имени таблицы:', err)
    } finally {
      cancelTabEdit()
    }
}

function cancelTabEdit() {
    editingTabId.value = null
    editingTabName.value = ''
}

// -------------------- Настройки и resize колонок --------------------
const visibleColumns = computed(() => {
    if (!selectedTable.value?.tableColumns) return []
    const hidden = new Set(columnPreferences.value.hidden || [])
    return selectedTable.value.tableColumns.filter(col => !hidden.has(col.name))
})

const hiddenColumnNames = computed(() => columnPreferences.value.hidden || [])

function columnStorageKey(tableId) {
    return `${COLUMN_PREF_PREFIX}${tableId}`
}

function loadColumnPreferences(tableId) {
    const defaults = { widths: {}, hidden: [], wrapped: {} }
    try {
      const raw = localStorage.getItem(columnStorageKey(tableId))
      if (!raw) { columnPreferences.value = defaults; return }
      const parsed = JSON.parse(raw)
      columnPreferences.value = {
        widths: parsed?.widths && typeof parsed.widths === 'object' ? parsed.widths : {},
        hidden: Array.isArray(parsed?.hidden) ? parsed.hidden : [],
        wrapped: parsed?.wrapped && typeof parsed.wrapped === 'object' ? parsed.wrapped : {}
      }
    } catch {
      columnPreferences.value = defaults
    }
}

function persistColumnPreferences() {
    if (!selectedTable.value?.id) return
    try { localStorage.setItem(columnStorageKey(selectedTable.value.id), JSON.stringify(columnPreferences.value)) }
    catch (err) { console.warn('Не удалось сохранить настройки колонок:', err) }
}

function clampColumnWidth(value) {
    const number = Number(value)
    if (!Number.isFinite(number)) return COLUMN_DEFAULT_WIDTH
    return Math.min(COLUMN_MAX_WIDTH, Math.max(COLUMN_MIN_WIDTH, Math.round(number)))
}

function getColumnWidth(col) {
    if (!col?.name) return COLUMN_DEFAULT_WIDTH
    return clampColumnWidth(columnPreferences.value.widths?.[col.name] ?? COLUMN_DEFAULT_WIDTH)
}

function getColumnStyle(col) {
    const width = getColumnWidth(col)
    return { width: `${width}px`, minWidth: `${width}px`, maxWidth: `${width}px` }
}

function isColumnWrapped(name) {
    return !!(name && columnPreferences.value.wrapped?.[name])
}

function openColumnMenu(event, col) {
    const menuWidth = 300, menuHeight = 350, gap = 6
    let x = event.clientX + gap, y = event.clientY + gap
    if (x + menuWidth > window.innerWidth - 8) x = Math.max(8, window.innerWidth - menuWidth - 8)
    if (y + menuHeight > window.innerHeight - 8) y = Math.max(8, window.innerHeight - menuHeight - 8)
    columnMenu.value = { open: true, x, y, column: col }
}

function closeColumnMenu() {
    columnMenu.value.open = false
    columnMenu.value.column = null
}

function onColumnMenuDocumentPointerDown(event) {
    if (!columnMenu.value.open) return
    const target = event.target
    if (target instanceof Element && target.closest('.column-context-menu')) return
    closeColumnMenu()
}

function onColumnMenuKeydown(event) {
    if (event.key === 'Escape' && columnMenu.value.open) closeColumnMenu()
}

function startColumnResize(event, col) {
    if (!col?.name) return
    closeColumnMenu()
    const startWidth = getColumnWidth(col)
    columnResize.value = { active: true, name: col.name, startX: event.clientX, startWidth }
    document.body.classList.add('column-resizing')
    const move = moveEvent => {
      columnPreferences.value.widths[col.name] = clampColumnWidth(startWidth + moveEvent.clientX - event.clientX)
    }
    const stop = () => {
      persistColumnPreferences()
      columnResize.value = { active: false, name: null, startX: 0, startWidth: 0 }
      document.body.classList.remove('column-resizing')
      window.removeEventListener('pointermove', move)
      window.removeEventListener('pointerup', stop)
      window.removeEventListener('pointercancel', stop)
      columnResizeCleanup.value = null
    }
    columnResizeCleanup.value = stop
    window.addEventListener('pointermove', move)
    window.addEventListener('pointerup', stop)
    window.addEventListener('pointercancel', stop)
}

function autoFitColumn(col) {
    if (!col?.name) return
    nextTick(() => {
      const table = document.querySelector('.data-table')
      if (!table) return
      const visibleIndex = visibleColumns.value.findIndex(item => item.name === col.name)
      if (visibleIndex < 0) return
      const header = table.querySelectorAll('thead th')[visibleIndex]
      const cells = Array.from(table.querySelectorAll('tbody tr')).slice(0, 80).map(row => row.children[visibleIndex]).filter(Boolean)
      const samples = [col.name, ...cells.map(cell => cell.textContent?.trim() || '').filter(Boolean)]
      const source = header || cells[0] || table
      const style = getComputedStyle(source)
      const canvas = document.createElement('canvas')
      const ctx = canvas.getContext('2d')
      if (!ctx) return
      ctx.font = `${style.fontWeight} ${style.fontSize} ${style.fontFamily}`
      const extra = sortColumn.value === col.name ? 46 : 32
      const measured = Math.max(...samples.map(value => ctx.measureText(value).width + extra))
      columnPreferences.value.widths[col.name] = clampColumnWidth(measured)
      persistColumnPreferences()
    })
}

function promptColumnWidth(col) {
    if (!col?.name) return
    const value = window.prompt(`Ширина «${col.name}» в пикселях (${COLUMN_MIN_WIDTH}–${COLUMN_MAX_WIDTH}):`, String(getColumnWidth(col)))
    if (value === null) return
    columnPreferences.value.widths[col.name] = clampColumnWidth(value)
    persistColumnPreferences()
}

function toggleColumnWrap(col) {
    if (!col?.name) return
    columnPreferences.value.wrapped[col.name] = !isColumnWrapped(col.name)
    persistColumnPreferences()
}

function hideColumn(col) {
    if (!col?.name) return
    const hidden = new Set(columnPreferences.value.hidden || [])
    hidden.add(col.name)
    if (hidden.size >= (selectedTable.value?.tableColumns?.length || 0)) {
      window.alert('Нельзя скрыть все колонки таблицы.')
      return
    }
    columnPreferences.value.hidden = Array.from(hidden)
    persistColumnPreferences()
}

function showAllColumns() { columnPreferences.value.hidden = []; persistColumnPreferences() }

function resetColumn(col) {
    if (!col?.name) return
    delete columnPreferences.value.widths[col.name]
    delete columnPreferences.value.wrapped[col.name]
    const hidden = new Set(columnPreferences.value.hidden || [])
    hidden.delete(col.name)
    columnPreferences.value.hidden = Array.from(hidden)
    persistColumnPreferences()
}

// -------------------- Загрузка данных --------------------
async function loadRows() {
    if (!selectedTable.value) return

    const requestId = ++rowsRequestId
    if (rowsRequestController) rowsRequestController.abort()
    rowsRequestController = new AbortController()
    rowsLoading.value = true

    try {
      const safePage = Number.isFinite(Number(currentPage.value))
        ? Math.max(1, Math.floor(Number(currentPage.value)))
        : 1
      const safePageSize = [100, 500, 1000].includes(Number(pageSize.value))
        ? Number(pageSize.value)
        : 100

      currentPage.value = safePage
      pageSize.value = safePageSize

      const skip = (safePage - 1) * safePageSize
      const params = new URLSearchParams({
        skip: String(skip),
        take: String(safePageSize)
      })

      const search = searchQuery.value.trim()
      if (search) params.append('search', search)

      const res = await apiClient.get(`/admin/adminTables/${selectedTable.value.id}/rows?${params.toString()}`, {
        signal: rowsRequestController.signal
      })

      if (requestId !== rowsRequestId) return
      rows.value = Array.isArray(res.data?.rows) ? res.data.rows : []
      totalRows.value = Number(res.data?.total) || 0
    } catch (err) {
      if (err?.code !== 'ERR_CANCELED' && err?.name !== 'CanceledError') {
        console.error('Ошибка загрузки строк:', err)
      }
    } finally {
      if (requestId === rowsRequestId) rowsLoading.value = false
    }
}

async function selectTable(table) {
    if (!table) return
    selectedTable.value = table
    loadColumnPreferences(table.id)
    ensureSelectedTableColumns()
    searchQuery.value = ''
    globalSearchEnabled.value = false
    globalSearchResults.value = []
    currentPage.value = 1
    sortColumn.value = null
    sortDirection.value = 'asc'
    await loadRows()
    if (route.params.id != table.id) {
      router.replace({ name: 'tableData', params: { id: table.id } })
    }
}

// -------------------- Поиск и глобальный поиск --------------------
function onSearchInput() {
    clearTimeout(searchTimeout)
    searchTimeout = setTimeout(() => {
      if (globalSearchEnabled.value) {
        performGlobalSearch()
      } else {
        currentPage.value = 1
        loadRows()
      }
    }, 400)
}

function clearSearch() {
    searchQuery.value = ''
    if (globalSearchEnabled.value) {
      globalSearchResults.value = []
    } else {
      currentPage.value = 1
      loadRows()
    }
}

async function performGlobalSearch() {
    if (!searchQuery.value.trim() || searchQuery.value.length < 2) {
      globalSearchResults.value = []
      return
    }
    try {
      const res = await apiClient.get(`/admin/adminTables/search?q=${encodeURIComponent(searchQuery.value)}`)
      globalSearchResults.value = res.data
    } catch (err) {
      console.error('Ошибка глобального поиска:', err)
    }
}

function toggleGlobalSearch() {
    globalSearchEnabled.value = !globalSearchEnabled.value
    if (globalSearchEnabled.value) {
      performGlobalSearch()
    } else {
      globalSearchResults.value = []
    }
}

function navigateToRow(item) {
    const table = tablesStore.tables.find(t => t.id === item.tableId)
    if (table) {
      router.push({ name: 'tableData', params: { id: table.id } })
    }
}

// -------------------- Пагинация --------------------
function changePage(page) {
    const nextPage = Number(page)
    if (!Number.isFinite(nextPage)) return

    const normalizedPage = Math.floor(nextPage)
    const safePageSize = [100, 500, 1000].includes(Number(pageSize.value))
      ? Number(pageSize.value)
      : 100
    const totalPages = Math.max(1, Math.ceil(Number(totalRows.value || 0) / safePageSize))

    if (normalizedPage < 1 || normalizedPage > totalPages) return

    currentPage.value = normalizedPage
    loadRows()
}

// -------------------- Сортировка (клиентская) --------------------
function sortBy(colName) {
    if (sortColumn.value === colName) {
      sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc'
    } else {
      sortColumn.value = colName
      sortDirection.value = 'asc'
    }
    // Сортировка применяется в computed paginatedRows
}

// -------------------- Computed --------------------
const sortedRows = computed(() => {
    let result = [...rows.value]
    if (sortColumn.value) {
      const col = sortColumn.value
      result.sort((a, b) => {
        let valA = getCellValue(a, col)
        let valB = getCellValue(b, col)
        if (typeof valA === 'boolean' && typeof valB === 'boolean') {
          return (valA === valB) ? 0 : valA ? -1 : 1
        }
        if (!isNaN(Number(valA)) && !isNaN(Number(valB)) && valA !== '' && valB !== '') {
          return Number(valA) - Number(valB)
        }
        return String(valA).localeCompare(String(valB))
      })
      if (sortDirection.value === 'desc') result.reverse()
    }
    return result
})

const paginatedRows = computed(() => sortedRows.value) // уже пагинированы на сервере

function getParsedRowData(row) {
    if (!row || typeof row !== 'object') return {}
    const cached = rowJsonCache.get(row)
    if (cached) return cached

    let parsed = {}
    try {
      parsed = row.dataJson ? JSON.parse(row.dataJson) : {}
      if (!parsed || typeof parsed !== 'object' || Array.isArray(parsed)) parsed = {}
    } catch {
      parsed = {}
    }

    rowJsonCache.set(row, parsed)
    return parsed
}

function getCellValue(row, columnName) {
    const obj = getParsedRowData(row)
    const val = obj[columnName]
    return val ?? ''
}

// -------------------- Модалка записи --------------------
const modalTextFields = computed(() => {
    if (!selectedTable.value?.modalColumns) return []
    return selectedTable.value.modalColumns.filter(f => f.type === 'text')
})
const modalBooleanFields = computed(() => {
    if (!selectedTable.value?.modalColumns) return []
    return selectedTable.value.modalColumns.filter(f => f.type === 'boolean')
})

function normalizeColumns(value) {
    if (Array.isArray(value)) {
      return value
        .filter(Boolean)
        .map(col => ({
          name: String(col?.name ?? '').trim(),
          type: col?.type === 'boolean' ? 'boolean' : 'text'
        }))
        .filter(col => col.name)
    }

    if (typeof value === 'string') {
      try {
        const parsed = JSON.parse(value)
        return normalizeColumns(parsed)
      } catch {
        return []
      }
    }

    if (value && typeof value === 'object') {
      if (Array.isArray(value.columns)) return normalizeColumns(value.columns)
      return Object.values(value).length ? normalizeColumns(Object.values(value)) : []
    }

    return []
}

function ensureSelectedTableColumns() {
    if (!selectedTable.value) return { tableColumns: [], modalColumns: [] }

    const tableColumns = normalizeColumns(selectedTable.value.tableColumns)
    const modalColumns = normalizeColumns(selectedTable.value.modalColumns)

    selectedTable.value.tableColumns = tableColumns
    selectedTable.value.modalColumns = modalColumns

    return { tableColumns, modalColumns }
}

function createDefaultRowData() {
    const data = {}
    const { tableColumns, modalColumns } = ensureSelectedTableColumns()

    tableColumns.forEach(col => {
      data[col.name] = col.type === 'boolean' ? false : ''
    })

    modalColumns.forEach(f => {
      data[f.name] = f.type === 'boolean' ? false : ''
    })

    return data
}

const initialRowData = ref({})

async function addRow() {
    editingRow.value = null
    const defaultData = createDefaultRowData()
    rowData.value = { ...defaultData }
    initialRowData.value = { ...defaultData }
    attachmentsList.value = []
    attachmentsLoading.value = false
    showRowModal.value = true
}

async function editRow(row) {
    editingRow.value = row
    try {
      const obj = JSON.parse(row.dataJson)
      const merged = createDefaultRowData()
      Object.keys(merged).forEach(key => {
        if (key in obj) merged[key] = obj[key]
      })
      rowData.value = merged
      initialRowData.value = { ...merged }
    } catch {
      rowData.value = createDefaultRowData()
      initialRowData.value = { ...rowData.value }
    }
    attachmentsList.value = []
    showRowModal.value = true
    loadAttachments(row.id)
}

function closeRowModal() {
    if (JSON.stringify(rowData.value) !== JSON.stringify(initialRowData.value)) {
      if (!confirm('Есть несохранённые изменения. Закрыть?')) return
    }
    showRowModal.value = false
}

async function saveRow() {
    if (isSaving.value) return
    isSaving.value = true
    try {
      const payload = { ...rowData.value }
      if (editingRow.value) {
        await apiClient.put(`/admin/adminTables/rows/${editingRow.value.id}`, payload)
      } else {
        await apiClient.post(`/admin/adminTables/${selectedTable.value.id}/rows`, payload)
      }
      showRowModal.value = false
      tablesStore.invalidateRows(selectedTable.value.id)
      await loadRows()
    } catch (err) {
      alert('Ошибка сохранения: ' + (err.response?.data || err.message))
    } finally {
      isSaving.value = false
    }
}

async function deleteRow(id) {
    if (!confirm('Удалить запись?')) return
    await apiClient.delete(`/admin/adminTables/rows/${id}`)
    tablesStore.invalidateRows(selectedTable.value.id)
    await loadRows()
}

// -------------------- Вложения --------------------
async function loadAttachments(rowId) {
    if (!rowId) return
    attachmentsLoading.value = true
    try {
      const res = await attachmentsApi.getList(rowId)
      attachmentsList.value = res.data
    } catch (err) {
      console.error('Ошибка загрузки вложений', err)
    } finally {
      attachmentsLoading.value = false
    }
}

async function uploadAttachment(event) {
    const file = event.target.files[0]
    if (!file) return
    if (!editingRow.value) {
      alert('Сначала сохраните запись, чтобы добавить вложения')
      event.target.value = ''
      return
    }
    try {
      await attachmentsApi.upload(editingRow.value.id, file)
      await loadAttachments(editingRow.value.id)
    } catch (err) {
      console.error('Ошибка загрузки файла', err)
    }
    event.target.value = ''
}

async function downloadAttachment(file) {
    try {
      const res = await attachmentsApi.download(file.id)
      const blob = new Blob([res.data])
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = file.fileName
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)
    } catch (err) {
      console.error('Ошибка скачивания', err)
    }
}

async function deleteAttachment(id) {
    if (!confirm('Удалить вложение?')) return
    try {
      await attachmentsApi.delete(id)
      await loadAttachments(editingRow.value.id)
    } catch (err) {
      console.error('Ошибка удаления', err)
    }
}

// -------------------- Удаление таблицы --------------------
async function deleteSelectedTable() {
    if (!selectedTable.value) return
    if (!confirm(`Удалить таблицу "${selectedTable.value.name}"?`)) return
    await apiClient.delete(`/admin/adminTables/${selectedTable.value.id}`)
    selectedTable.value = null
    await tablesStore.loadTables(true)
    router.push({ name: 'home' })
}

async function onExcelCompleted() {
    await tablesStore.loadTables(true)
    if (selectedTable.value) {
        const refreshed = tablesStore.getTableById(selectedTable.value.id)
        if (refreshed) selectedTable.value = refreshed
        currentPage.value = 1
        await loadRows()
    }
}

// -------------------- Редакторы колонок --------------------
function openTableColumnsEditor() {
    if (!selectedTable.value) return
    const { tableColumns } = ensureSelectedTableColumns()
    tableColumnDefs.value = tableColumns.map(c => ({ name: c.name, type: c.type }))
    showTableColumnsModal.value = true
}
function closeTableColumnsModal() {
    showTableColumnsModal.value = false
}
function addTableColumn() { tableColumnDefs.value.push({ name: '', type: 'text' }) }
function removeTableColumn(index) { tableColumnDefs.value.splice(index, 1) }
async function saveTableColumns() {
    if (isSavingColumns.value) return
    isSavingColumns.value = true
    try {
      const newCols = tableColumnDefs.value.filter(c => c.name).map(c => ({ name: c.name, type: c.type }))
      selectedTable.value.tableColumns = newCols
      await apiClient.put(`/admin/adminTables/${selectedTable.value.id}`, {
        name: selectedTable.value.name,
        tableColumns: newCols,
        modalColumns: selectedTable.value.modalColumns
      })
      showTableColumnsModal.value = false
      await loadRows()
      await tablesStore.loadTables(true)
    } catch (err) {
      alert('Ошибка сохранения колонок: ' + (err.response?.data || err.message))
    } finally {
      isSavingColumns.value = false
    }
}

function openModalColumnsEditor() {
    if (!selectedTable.value) return
    const { modalColumns } = ensureSelectedTableColumns()
    modalColumnDefs.value = modalColumns.map(c => ({ name: c.name, type: c.type }))
    showModalColumnsModal.value = true
}
function closeModalColumnsModal() {
    showModalColumnsModal.value = false
}
function addModalColumn() { modalColumnDefs.value.push({ name: '', type: 'text' }) }
function removeModalColumn(index) { modalColumnDefs.value.splice(index, 1) }
async function saveModalColumns() {
    if (isSavingModalCols.value) return
    isSavingModalCols.value = true
    try {
      const newCols = modalColumnDefs.value.filter(c => c.name)
      selectedTable.value.modalColumns = newCols
      await apiClient.put(`/admin/adminTables/${selectedTable.value.id}`, {
        name: selectedTable.value.name,
        tableColumns: selectedTable.value.tableColumns,
        modalColumns: newCols
      })
      showModalColumnsModal.value = false
      await tablesStore.loadTables(true)
    } catch (err) {
      alert('Ошибка сохранения колонок модалки: ' + (err.response?.data || err.message))
    } finally {
      isSavingModalCols.value = false
    }
}

onBeforeUnmount(() => {
    if (searchTimeout) {
      clearTimeout(searchTimeout)
      searchTimeout = null
    }
    if (rowsRequestController) {
      rowsRequestController.abort()
      rowsRequestController = null
    }
    closeColumnMenu()
    document.removeEventListener('pointerdown', onColumnMenuDocumentPointerDown, true)
    document.removeEventListener('keydown', onColumnMenuKeydown)
    if (columnResizeCleanup.value) columnResizeCleanup.value()
    document.body.classList.remove('column-resizing')
})

// -------------------- Инициализация --------------------
onMounted(async () => {
    document.addEventListener('pointerdown', onColumnMenuDocumentPointerDown, true)
    document.addEventListener('keydown', onColumnMenuKeydown)
    if (!tablesStore.loaded) {
      await tablesStore.loadTables()
    }
    const idParam = route.params.id
    if (idParam && idParam !== 'new') {
      const table = tablesStore.getTableById(idParam)
      if (table) {
        await selectTable(table)
      }
    }
})

watch(() => route.params.id, async (newId) => {
    if (!newId || newId === 'new') return
    if (String(selectedTable.value?.id) === String(newId)) return

    const table = tablesStore.getTableById(newId)
    if (table) {
      await selectTable(table)
    } else {
      await tablesStore.loadTables(true)
      const updatedTable = tablesStore.getTableById(newId)
      if (updatedTable) await selectTable(updatedTable)
    }
})
</script>
<style scoped>
.table-page{height:100%;min-height:0;display:flex;flex-direction:column;gap:14px;color:#e5e7eb}.table-header{display:flex;align-items:center;justify-content:space-between;gap:12px}.table-heading{min-width:0}.eyebrow{font-size:10px;letter-spacing:.16em;text-transform:uppercase;color:#64748b}.table-title{margin:2px 0 0;font-size:22px;font-weight:700;color:#f8fafc;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}.table-subtitle{margin-top:2px;font-size:12px;color:#64748b}.glass-panel{background:rgba(7,10,18,.82);border:1px solid rgba(255,255,255,.08);border-radius:18px;box-shadow:0 12px 32px rgba(0,0,0,.22),inset 0 1px 0 rgba(255,255,255,.03)}
.tabs-panel{display:flex;align-items:center;gap:8px;padding:7px;min-width:0}.tabs-scroll{display:flex;gap:7px;overflow-x:auto;scrollbar-width:thin;scrollbar-color:#334155 transparent;min-width:0;flex:1}.tabs-scroll::-webkit-scrollbar{height:7px}.tabs-scroll::-webkit-scrollbar-thumb{background:#334155;border-radius:99px}.tab{border:1px solid transparent;background:rgba(255,255,255,.025);color:#94a3b8;border-radius:12px;padding:8px 12px;white-space:nowrap;font-size:13px;font-weight:600;transition:.18s;cursor:pointer}.tab:hover{background:rgba(255,255,255,.06);color:#e2e8f0}.tab.active{background:linear-gradient(135deg,rgba(99,102,241,.24),rgba(56,189,248,.12));border-color:rgba(99,102,241,.25);color:#eef2ff;box-shadow:0 6px 18px rgba(79,70,229,.12)}.tab-edit{display:flex;align-items:center;gap:6px;padding:2px 4px;border-radius:12px;background:rgba(99,102,241,.12);border:1px solid rgba(99,102,241,.25)}.tab-edit input{width:150px;background:#090d15;border:1px solid #334155;color:#f8fafc;border-radius:9px;padding:6px 9px;outline:none}.tab-mini{border:0;background:transparent;color:#94a3b8;cursor:pointer;padding:6px;border-radius:8px}.tab-mini:hover{background:rgba(255,255,255,.06);color:#fff}.tab-arrow{flex:0 0 auto;width:30px;height:30px;border-radius:10px;border:1px solid rgba(255,255,255,.08);background:#111827;color:#cbd5e1;cursor:pointer}.tab-arrow:hover{background:#1e293b;color:#fff}
.toolbar{display:flex;align-items:center;gap:8px;flex-wrap:wrap;padding:10px}.search-box{position:relative;flex:1 1 260px;min-width:220px}.search-box input{width:100%;height:40px;padding:0 38px;border-radius:12px;border:1px solid rgba(255,255,255,.08);background:#080b12;color:#f8fafc;outline:none;transition:.18s}.search-box input:focus{border-color:rgba(99,102,241,.48);box-shadow:0 0 0 3px rgba(99,102,241,.1)}.search-box svg{position:absolute;left:13px;top:12px;width:16px;height:16px;color:#64748b}.search-clear{position:absolute;right:8px;top:7px;border:0;background:transparent;color:#64748b;cursor:pointer;border-radius:8px;width:26px;height:26px}.search-clear:hover{background:rgba(255,255,255,.06);color:#fff}.toolbar-btn{height:40px;display:inline-flex;align-items:center;justify-content:center;gap:8px;padding:0 13px;border-radius:12px;border:1px solid rgba(255,255,255,.08);background:#0d111a;color:#cbd5e1;cursor:pointer;font-size:12px;font-weight:600;transition:.18s;white-space:nowrap}.toolbar-btn:hover{background:#151b27;border-color:rgba(255,255,255,.14);color:#fff;transform:translateY(-1px)}.toolbar-btn.excel{background:#0f251d;border-color:#1c533a;color:#86efac}.toolbar-btn.excel:hover{background:#123125;color:#bbf7d0}.toolbar-btn.primary{background:linear-gradient(135deg,#4f46e5,#2563eb);border-color:rgba(129,140,248,.38);color:#fff;box-shadow:0 8px 22px rgba(37,99,235,.18)}.toolbar-btn.primary:hover{filter:brightness(1.08)}.toolbar-btn.danger{background:rgba(127,29,29,.16);color:#fca5a5;border-color:rgba(248,113,113,.12)}.toolbar-btn.active{background:rgba(79,70,229,.24);border-color:rgba(129,140,248,.28);color:#e0e7ff}.toolbar-note{font-size:11px;color:#64748b;margin-left:auto;padding:0 6px}
.search-results{overflow:hidden}.search-results-head{padding:12px 14px;border-bottom:1px solid rgba(255,255,255,.06);font-size:11px;color:#94a3b8;text-transform:uppercase;letter-spacing:.08em}.search-result-list{max-height:220px;overflow:auto}.search-result{display:flex;justify-content:space-between;gap:10px;padding:11px 14px;border-bottom:1px solid rgba(255,255,255,.04)}.search-result:hover{background:rgba(255,255,255,.025)}.search-result-title{font-size:12px;color:#e2e8f0}.search-result-title strong{color:#a5b4fc}.search-result-value{margin-top:3px;font-size:11px;color:#64748b;max-width:820px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}.jump-btn{border:1px solid rgba(99,102,241,.2);background:rgba(79,70,229,.12);color:#c7d2fe;border-radius:9px;padding:7px 10px;font-size:11px;cursor:pointer;flex:none}.jump-btn:hover{background:rgba(79,70,229,.22)}
.data-panel{display:flex;flex-direction:column;min-height:0;flex:1;overflow:hidden}.table-scroll{flex:1;min-height:0;overflow:auto;overscroll-behavior:contain;background:#05070b;border-radius:18px 18px 0 0}.table-scroll::-webkit-scrollbar{width:12px;height:12px}.table-scroll::-webkit-scrollbar-track{background:#080b10}.table-scroll::-webkit-scrollbar-thumb{background:#263244;border:3px solid #080b10;border-radius:99px}.table-scroll::-webkit-scrollbar-thumb:hover{background:#3b4a61}.data-table{border-collapse:separate;border-spacing:0;min-width:max-content;width:max-content;font-size:12px}.data-table th{position:sticky;top:0;z-index:2;background:#0b0f17;color:#94a3b8;border-bottom:1px solid #202938;border-right:1px solid rgba(255,255,255,.04);text-align:left;white-space:nowrap;padding:0}.data-table th:first-child{border-top-left-radius:0}.data-table th>button{width:100%;height:46px;padding:0 14px;border:0;background:transparent;color:inherit;text-align:left;font:inherit;font-weight:700;cursor:pointer}.data-table th>button:hover{background:#111827;color:#e2e8f0}.data-table td{min-width:150px;max-width:320px;padding:11px 14px;color:#d1d5db;border-bottom:1px solid rgba(255,255,255,.045);border-right:1px solid rgba(255,255,255,.035);vertical-align:middle}.data-table tbody tr{background:#06090e}.data-table tbody tr:nth-child(even){background:#080c12}.data-table tbody tr:hover{background:#0d1420}.cell-text{display:block;max-width:280px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}.bool-cell{display:inline-flex;align-items:center;justify-content:center;width:26px;height:26px;border-radius:8px;font-size:13px}.bool-cell.on{background:rgba(16,185,129,.12);color:#6ee7b7}.bool-cell.off{background:rgba(100,116,139,.1);color:#64748b}.icon-btn{width:30px;height:30px;border-radius:9px;border:1px solid transparent;background:transparent;color:#94a3b8;display:inline-flex;align-items:center;justify-content:center;cursor:pointer}.icon-btn:hover{background:rgba(99,102,241,.1);color:#a5b4fc;border-color:rgba(129,140,248,.12)}.icon-btn.danger:hover{background:rgba(239,68,68,.1);color:#fca5a5;border-color:rgba(248,113,113,.12)}.table-loading,.table-empty{height:220px;display:flex;align-items:center;justify-content:center;flex-direction:column;color:#64748b;font-size:13px;gap:10px}.loading-orb{width:26px;height:26px;border:3px solid #263244;border-top-color:#818cf8;border-radius:50%;animation:spin .8s linear infinite}@keyframes spin{to{transform:rotate(360deg)}}
.empty-icon{width:44px;height:44px;border-radius:14px;display:grid;place-items:center;background:#0d111a;border:1px solid rgba(255,255,255,.06);color:#64748b}.empty-title{font-weight:700;color:#cbd5e1}.empty-help{font-size:11px;color:#475569}.statusbar{display:flex;align-items:center;justify-content:space-between;gap:12px;padding:10px 14px;background:#080b11;border-top:1px solid rgba(255,255,255,.06);font-size:11px;color:#64748b}
.modal-backdrop{position:fixed;inset:0;z-index:60;background:rgba(0,0,0,.78);display:flex;align-items:center;justify-content:center;padding:20px}.modal-card{width:min(760px,100%);max-height:88vh;overflow:auto;background:#090c13;border:1px solid rgba(255,255,255,.09);border-radius:20px;box-shadow:0 30px 80px rgba(0,0,0,.55);padding:20px}.modal-card.small{width:min(560px,100%)}.modal-header{display:flex;align-items:center;justify-content:space-between;gap:12px;margin-bottom:16px}.modal-header h2{font-size:18px;font-weight:700;color:#f8fafc}.modal-header p{margin-top:2px;color:#64748b;font-size:11px}.modal-close{width:34px;height:34px;border-radius:10px;border:1px solid rgba(255,255,255,.06);background:#0f1420;color:#94a3b8;cursor:pointer}.modal-close:hover{color:#fff;background:#161d2b}.form-grid{display:grid;grid-template-columns:1fr 1fr;gap:16px}.form-section{padding:14px;border:1px solid rgba(255,255,255,.06);background:#070a10;border-radius:14px}.form-section h3{font-size:10px;letter-spacing:.12em;text-transform:uppercase;color:#64748b;margin-bottom:12px}.field{margin-bottom:11px}.field:last-child{margin-bottom:0}.field label{display:block;margin-bottom:5px;font-size:11px;color:#94a3b8}.field input,.column-row input,.column-row select{width:100%;height:38px;padding:0 10px;border-radius:10px;border:1px solid rgba(255,255,255,.08);background:#0b0f17;color:#f8fafc;outline:none}.field input:focus,.column-row input:focus,.column-row select:focus{border-color:rgba(99,102,241,.4);box-shadow:0 0 0 3px rgba(99,102,241,.08)}.check-line{display:flex;align-items:center;gap:9px;padding:8px 0;color:#cbd5e1;font-size:12px}.check-line input{accent-color:#6366f1;width:17px;height:17px}.attachments{margin-top:16px;padding-top:16px;border-top:1px solid rgba(255,255,255,.06)}.attachments-title{font-size:11px;letter-spacing:.1em;text-transform:uppercase;color:#64748b;margin-bottom:10px}.attachment-list{display:flex;flex-wrap:wrap;gap:7px}.attachment{display:flex;align-items:center;gap:7px;background:#0c111a;border:1px solid rgba(255,255,255,.06);border-radius:10px;padding:6px 8px;color:#cbd5e1;font-size:11px}.attachment span{max-width:180px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}.attachment-btn{border:0;background:transparent;color:#94a3b8;cursor:pointer}.attachment-btn:hover{color:#fff}.attachment-btn.danger:hover{color:#fca5a5}.file-btn{display:inline-flex;align-items:center;gap:7px;padding:7px 10px;background:rgba(79,70,229,.1);border:1px solid rgba(99,102,241,.15);border-radius:10px;color:#a5b4fc;font-size:11px;cursor:pointer}.modal-actions{display:flex;justify-content:flex-end;gap:8px;margin-top:18px}.modal-btn{height:38px;padding:0 14px;border-radius:10px;border:1px solid rgba(255,255,255,.08);background:#0f1420;color:#cbd5e1;cursor:pointer;font-size:12px;font-weight:600}.modal-btn.primary{background:linear-gradient(135deg,#4f46e5,#2563eb);color:#fff;border-color:rgba(129,140,248,.3)}.modal-btn.danger{background:rgba(127,29,29,.16);color:#fca5a5}.modal-btn:disabled{opacity:.5;cursor:not-allowed}.columns-list{display:flex;flex-direction:column;gap:8px;max-height:52vh;overflow:auto}.column-row{display:grid;grid-template-columns:1fr 120px 38px;gap:8px}.add-field{margin-top:10px;border:0;background:transparent;color:#a5b4fc;font-size:11px;cursor:pointer;display:inline-flex;align-items:center;gap:6px}.add-field:hover{color:#c7d2fe}
@media(max-width:900px){.form-grid{grid-template-columns:1fr}.toolbar-note{width:100%;margin-left:0}.table-title{font-size:18px}}
@media(max-width:640px){.table-page{padding:8px}.table-header{align-items:flex-start}.table-subtitle{display:none}.toolbar{padding:8px}.search-box{min-width:100%}.toolbar-btn{flex:1}.toolbar-btn.excel{background:#0f251d;border-color:#1c533a;color:#86efac}.toolbar-btn.excel:hover{background:#123125;color:#bbf7d0}.toolbar-btn.primary{flex:1.2}.statusbar{flex-direction:column;align-items:flex-start}.data-table td{min-width:130px}}

/* Row delete control: compact X at the far right. Editing remains on row double-click. */
.data-table .row-delete-col{width:42px;min-width:42px;max-width:42px;position:sticky;right:0;z-index:3;padding:0!important;text-align:center;background:inherit;border-right:0;box-shadow:-10px 0 18px rgba(0,0,0,.14)}
.data-table thead .row-delete-col{background:#0b0f17;z-index:4}
.data-table .row-delete-btn{width:28px;min-width:28px;height:28px;margin:0 auto;padding:0;border:1px solid transparent;border-radius:8px;background:transparent;color:var(--app-muted,#94a3b8);display:inline-flex;align-items:center;justify-content:center;font-size:19px;font-weight:400;line-height:1;cursor:pointer;transition:background .16s ease,color .16s ease,border-color .16s ease,transform .16s ease}
.data-table .row-delete-btn:hover,.data-table .row-delete-btn:focus-visible{background:var(--app-danger-soft,rgba(239,68,68,.10));color:var(--app-danger,#ef4444);border-color:var(--app-danger-border,rgba(239,68,68,.22));transform:scale(1.04);outline:none}
.data-table .row-delete-btn:active{transform:scale(.96)}
@media(max-width:640px){.data-table .row-delete-col{width:38px;min-width:38px;max-width:38px}.data-table .row-delete-btn{width:26px;min-width:26px;height:26px;font-size:18px}}

/* Excel-like column controls — isolated inside TableDataView. table.css is not changed. */
.data-table th.data-column-head{position:sticky;box-sizing:border-box;overflow:visible}.data-table th.data-column-head>button{padding-right:22px}.data-table th.data-column-head.is-resizing{box-shadow:inset -2px 0 0 var(--app-primary,#6366f1)}
.data-table th.data-column-head .column-resizer{position:absolute;top:0;right:-4px;width:9px;height:100%;z-index:6;cursor:col-resize}.data-table th.data-column-head .column-resizer::after{content:"";position:absolute;top:8px;bottom:8px;left:3px;width:2px;border-radius:4px;background:transparent;transition:background .15s,box-shadow .15s}.data-table th.data-column-head:hover .column-resizer::after,.data-table th.data-column-head.is-resizing .column-resizer::after{background:var(--app-primary,#6366f1);box-shadow:0 0 0 3px color-mix(in srgb,var(--app-primary,#6366f1) 10%,transparent)}
.data-table td.is-wrapped .cell-text-wrap{white-space:normal;overflow:visible;text-overflow:clip;overflow-wrap:anywhere;word-break:break-word;display:block;max-width:none;line-height:1.45}.data-table td.is-wrapped{vertical-align:top}
.column-menu-backdrop{position:fixed;inset:0;z-index:120;pointer-events:auto}.column-context-menu{position:fixed;width:300px;max-width:calc(100vw - 16px);padding:8px;box-sizing:border-box;border:1px solid var(--app-border,rgba(255,255,255,.12));border-radius:14px;background:var(--app-surface,rgba(12,16,24,.98));color:var(--app-text,#e5e7eb);box-shadow:0 22px 55px rgba(0,0,0,.28);backdrop-filter:blur(18px);pointer-events:auto}.column-menu-title{padding:8px 10px 10px;border-bottom:1px solid var(--app-border,rgba(255,255,255,.08));margin-bottom:4px;overflow:hidden}.column-menu-title span{display:block;margin-bottom:2px;font-size:9px;letter-spacing:.14em;text-transform:uppercase;color:var(--app-muted,#64748b)}.column-menu-title strong{display:block;font-size:13px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}.column-menu-item{width:100%;display:flex;align-items:center;gap:10px;text-align:left;border:0;background:transparent;color:inherit;padding:9px 10px;border-radius:10px;cursor:pointer;font:inherit}.column-menu-item:hover{background:var(--app-surface-strong,rgba(99,102,241,.08))}.column-menu-item .menu-icon{width:24px;height:24px;flex:0 0 24px;display:inline-flex;align-items:center;justify-content:center;border-radius:7px;background:var(--app-surface-soft,rgba(99,102,241,.08));color:var(--app-primary,#818cf8);font-size:14px}.column-menu-item span:last-child{min-width:0}.column-menu-item b{display:block;font-size:11px;line-height:1.25}.column-menu-item small{display:block;margin-top:2px;color:var(--app-muted,#64748b);font-size:9px;line-height:1.25}.column-menu-separator{height:1px;background:var(--app-border,rgba(255,255,255,.08));margin:4px 6px}.column-menu-fade-enter-active,.column-menu-fade-leave-active{transition:opacity .12s ease,transform .12s ease}.column-menu-fade-enter-from,.column-menu-fade-leave-to{opacity:0;transform:translateY(-4px) scale(.985)}:global(body.column-resizing),:global(body.column-resizing *){cursor:col-resize!important;user-select:none!important}
@media(max-width:640px){.column-context-menu{width:min(300px,calc(100vw - 16px))}.data-table th.data-column-head .column-resizer{width:12px;right:-6px}}
</style>
