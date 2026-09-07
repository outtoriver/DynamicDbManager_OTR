<template>
  <Teleport to="body">
    <Transition name="excel-modal">
      <div v-if="open" class="excel-backdrop" @click.self="close">
        <section class="excel-modal" role="dialog" aria-modal="true" aria-labelledby="excel-title">
          <header class="excel-header">
            <div>
              <div class="excel-eyebrow">DATA TRANSFER</div>
              <h2 id="excel-title">Импорт / экспорт Excel</h2>
              <p>Перенос данных между Excel и Dynamic DB Manager без изменения существующей структуры.</p>
            </div>
            <button type="button" class="excel-close" @click="close" aria-label="Закрыть">×</button>
          </header>

          <div class="excel-tabs" role="tablist" aria-label="Операция Excel">
            <button type="button" :class="{ active: mode === 'new' }" @click="mode = 'new'">Новая таблица</button>
            <button type="button" :class="{ active: mode === 'existing' }" @click="mode = 'existing'">В существующую</button>
            <button type="button" :class="{ active: mode === 'export' }" @click="mode = 'export'">Экспорт</button>
          </div>

          <div class="excel-body">
            <template v-if="mode === 'new'">
              <div class="excel-grid two">
                <div class="field-card">
                  <label>Excel-файл</label>
                  <div class="file-drop" :class="{ selected: file }">
                    <input ref="fileInput" type="file" accept=".xlsx,.xls" @change="onFileChange" />
                    <div class="file-icon">XLS</div>
                    <div class="file-copy">
                      <strong>{{ file?.name || 'Выберите Excel-файл' }}</strong>
                      <span>{{ file ? formatBytes(file.size) : 'Поддерживаются .xlsx и .xls' }}</span>
                    </div>
                    <button type="button" class="mini-btn" @click="openFilePicker">Выбрать</button>
                  </div>
                </div>

                <div class="field-card">
                  <label>Название новой таблицы</label>
                  <input v-model.trim="newTableName" class="excel-input" placeholder="Например: Сотрудники" />
                  <div class="field-help">Если оставить пустым, будет использовано имя Excel-файла.</div>
                </div>
              </div>

              <div v-if="preview" class="excel-grid two">
                <div class="field-card">
                  <label>Лист</label>
                  <select v-model="sheetName" class="excel-input" @change="loadPreview">
                    <option v-for="sheet in preview.sheets || []" :key="sheet.name" :value="sheet.name">{{ sheet.name }}</option>
                  </select>
                  <div class="field-help">Используемый диапазон: {{ preview.usedRange || 'не определён' }}</div>
                </div>

                <div class="field-card">
                  <label>Диапазон</label>
                  <input v-model="range" class="excel-input" placeholder="A1:F100 (пусто = весь диапазон)" @change="loadPreview" @keyup.enter="loadPreview" />
                  <div class="field-help">Первая строка диапазона используется как заголовок.</div>
                </div>
              </div>

              <div class="switch-row">
                <label class="switch-label">
                  <input v-model="hasHeaders" type="checkbox" />
                  <span class="switch"></span>
                  <span><strong>Первая строка — заголовки</strong><small>Имена колонок будут взяты из Excel.</small></span>
                </label>
              </div>

              <PreviewTable v-if="preview" :preview="preview" />

              <div v-if="error" class="excel-error">{{ error }}</div>

              <footer class="excel-footer">
                <div class="excel-footer-info">
                  <span v-if="preview">{{ preview.rowCount }} строк · {{ preview.columnCount }} колонок</span>
                </div>
                <button type="button" class="primary-btn" :disabled="busy || !file || !hasHeaders" @click="importNewTable">
                  <span v-if="busy" class="spinner"></span>
                  {{ busy ? 'Импорт...' : 'Создать таблицу и импортировать' }}
                </button>
              </footer>
            </template>

            <template v-else-if="mode === 'existing'">
              <div class="excel-grid two">
                <div class="field-card">
                  <label>Excel-файл</label>
                  <div class="file-drop" :class="{ selected: file }">
                    <input ref="fileInput" type="file" accept=".xlsx,.xls" @change="onFileChange" />
                    <div class="file-icon">XLS</div>
                    <div class="file-copy">
                      <strong>{{ file?.name || 'Выберите Excel-файл' }}</strong>
                      <span>{{ file ? formatBytes(file.size) : 'Поддерживаются .xlsx и .xls' }}</span>
                    </div>
                    <button type="button" class="mini-btn" @click="openFilePicker">Выбрать</button>
                  </div>
                </div>

                <div class="field-card">
                  <label>Целевая таблица</label>
                  <select v-model="targetTableId" class="excel-input" @change="resetPreview">
                    <option value="">Выберите таблицу</option>
                    <option v-for="table in tables" :key="table.id" :value="String(table.id)">{{ table.name }}</option>
                  </select>
                  <div v-if="targetTable" class="field-help">Колонок в таблице: {{ targetColumns.length }}</div>
                </div>
              </div>

              <div v-if="preview" class="excel-grid two">
                <div class="field-card">
                  <label>Лист</label>
                  <select v-model="sheetName" class="excel-input" @change="loadPreview">
                    <option v-for="sheet in preview.sheets || []" :key="sheet.name" :value="sheet.name">{{ sheet.name }}</option>
                  </select>
                </div>
                <div class="field-card">
                  <label>Диапазон</label>
                  <input v-model="range" class="excel-input" placeholder="A1:F100" @change="loadPreview" @keyup.enter="loadPreview" />
                </div>
              </div>

              <div class="switch-row grouped">
                <label class="switch-label">
                  <input v-model="hasHeaders" type="checkbox" />
                  <span class="switch"></span>
                  <span><strong>Первая строка — заголовки</strong><small>Заголовки используются для сопоставления.</small></span>
                </label>
                <label class="switch-label">
                  <input v-model="mapByHeader" type="checkbox" :disabled="!hasHeaders" />
                  <span class="switch"></span>
                  <span><strong>Сопоставлять по названию</strong><small>Совпавшие названия Excel → колонка таблицы.</small></span>
                </label>
              </div>

              <div v-if="targetColumns.length" class="mapping-card">
                <div class="mapping-head">
                  <div><span class="excel-eyebrow">TARGET</span><strong>Колонки существующей таблицы</strong></div>
                  <span>{{ targetColumns.length }}</span>
                </div>
                <div class="mapping-list">
                  <span v-for="col in targetColumns" :key="col.name" class="mapping-pill">{{ col.name }}</span>
                </div>
              </div>

              <PreviewTable v-if="preview" :preview="preview" />

              <div v-if="error" class="excel-error">{{ error }}</div>

              <footer class="excel-footer">
                <div class="excel-footer-info">
                  <span v-if="preview">{{ preview.rowCount }} строк · {{ preview.columnCount }} колонок</span>
                </div>
                <button type="button" class="primary-btn" :disabled="busy || !file || !targetTableId" @click="importExisting">
                  <span v-if="busy" class="spinner"></span>
                  {{ busy ? 'Импорт...' : 'Импортировать в таблицу' }}
                </button>
              </footer>
            </template>

            <template v-else>
              <div class="export-hero">
                <div class="export-icon">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true"><path d="M7 3h8l4 4v14H7a3 3 0 0 1-3-3V6a3 3 0 0 1 3-3Z" stroke-width="1.7"/><path d="M15 3v5h5M8 12h8M8 16h8" stroke-width="1.7" stroke-linecap="round"/></svg>
                </div>
                <div>
                  <div class="excel-eyebrow">XLSX EXPORT</div>
                  <h3>Выгрузить таблицу целиком</h3>
                  <p>Будут экспортированы все записи, все колонки и их текущий порядок. Заголовок будет записан первой строкой.</p>
                </div>
              </div>

              <div class="export-list">
                <div class="export-row"><span>Таблица</span><strong>{{ targetTable?.name || 'Выберите таблицу' }}</strong></div>
                <div class="export-row"><span>Колонки</span><strong>{{ targetColumns.length }}</strong></div>
                <div class="export-row"><span>Формат</span><strong>Microsoft Excel (.xlsx)</strong></div>
              </div>

              <div class="field-card">
                <label>Таблица для экспорта</label>
                <select v-model="targetTableId" class="excel-input">
                  <option value="">Выберите таблицу</option>
                  <option v-for="table in tables" :key="table.id" :value="String(table.id)">{{ table.name }}</option>
                </select>
              </div>

              <div v-if="error" class="excel-error">{{ error }}</div>

              <footer class="excel-footer">
                <div></div>
                <button type="button" class="primary-btn" :disabled="busy || !targetTableId" @click="exportTable">
                  <span v-if="busy" class="spinner"></span>
                  {{ busy ? 'Формирование...' : 'Скачать Excel' }}
                </button>
              </footer>
            </template>
          </div>
        </section>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { computed, defineComponent, h, onMounted, onBeforeUnmount, ref, watch } from 'vue'
import { excelApi } from '../api'

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  tables: { type: Array, default: () => [] },
  selectedTableId: { type: [String, Number], default: '' }
})

const emit = defineEmits(['update:modelValue', 'completed'])

const open = computed({
  get: () => props.modelValue,
  set: value => emit('update:modelValue', value)
})

const mode = ref('new')
const file = ref(null)
const fileInput = ref(null)
const preview = ref(null)
const sheetName = ref('')
const range = ref('')
const newTableName = ref('')
const targetTableId = ref(props.selectedTableId ? String(props.selectedTableId) : '')
const hasHeaders = ref(true)
const mapByHeader = ref(true)
const busy = ref(false)
const error = ref('')

const targetTable = computed(() =>
  props.tables.find(table => String(table.id) === String(targetTableId.value)) || null
)

const targetColumns = computed(() =>
  Array.isArray(targetTable.value?.tableColumns)
    ? targetTable.value.tableColumns
    : []
)

watch(
  () => props.selectedTableId,
  value => {
    if (value !== undefined && value !== null && value !== '') {
      targetTableId.value = String(value)
    }
  }
)

watch(mode, value => {
  error.value = ''
  if (value === 'export' && props.selectedTableId) {
    targetTableId.value = String(props.selectedTableId)
  }
})

function close() {
  if (busy.value) return
  open.value = false
  reset()
}

function reset() {
  file.value = null
  preview.value = null
  sheetName.value = ''
  range.value = ''
  newTableName.value = ''
  hasHeaders.value = true
  mapByHeader.value = true
  busy.value = false
  error.value = ''
  if (props.selectedTableId) targetTableId.value = String(props.selectedTableId)
}

function resetPreview() {
  preview.value = null
  error.value = ''
}

function openFilePicker() {
  fileInput.value?.click()
}

async function onFileChange(event) {
  const selected = event.target.files?.[0]
  if (!selected) return

  file.value = selected
  error.value = ''
  preview.value = null
  sheetName.value = ''
  range.value = ''

  if (!newTableName.value) {
    newTableName.value = selected.name.replace(/\.(xlsx|xls)$/i, '')
  }

  await loadPreview()
}

async function loadPreview() {
  if (!file.value || busy.value) return

  busy.value = true
  error.value = ''

  try {
    const { data } = await excelApi.preview(file.value, sheetName.value, range.value)
    preview.value = data

    if (!sheetName.value && Array.isArray(data?.sheets) && data.sheets.length) {
      sheetName.value = data.sheets[0].name
    }
  } catch (err) {
    preview.value = null
    error.value = getError(err, 'Не удалось прочитать Excel-файл')
  } finally {
    busy.value = false
  }
}

async function importNewTable() {
  if (!file.value || busy.value || !hasHeaders.value) return

  busy.value = true
  error.value = ''

  try {
    await excelApi.importNewTable(file.value, {
      sheetName: sheetName.value,
      range: range.value,
      name: newTableName.value.trim() || file.value.name.replace(/\.(xlsx|xls)$/i, ''),
      hasHeaders: hasHeaders.value
    })

    emit('completed')
    close()
  } catch (err) {
    error.value = getError(err, 'Не удалось импортировать Excel в новую таблицу')
  } finally {
    busy.value = false
  }
}

async function importExisting() {
  if (!file.value || !targetTableId.value || busy.value) return

  busy.value = true
  error.value = ''

  try {
    await excelApi.importExisting(Number(targetTableId.value), file.value, {
      sheetName: sheetName.value,
      range: range.value,
      hasHeaders: hasHeaders.value,
      mapByHeader: mapByHeader.value
    })

    emit('completed')
    close()
  } catch (err) {
    error.value = getError(err, 'Не удалось импортировать данные в таблицу')
  } finally {
    busy.value = false
  }
}

async function exportTable() {
  if (!targetTableId.value || busy.value) return

  busy.value = true
  error.value = ''

  try {
    const response = await excelApi.exportTable(Number(targetTableId.value))
    const blob = new Blob([response.data], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    })
    const url = URL.createObjectURL(blob)
    const anchor = document.createElement('a')
    anchor.href = url
    anchor.download = `${sanitizeFileName(targetTable.value?.name || 'table')}.xlsx`
    document.body.appendChild(anchor)
    anchor.click()
    anchor.remove()
    setTimeout(() => URL.revokeObjectURL(url), 0)
  } catch (err) {
    error.value = getError(err, 'Не удалось экспортировать таблицу')
  } finally {
    busy.value = false
  }
}

function getError(err, fallback) {
  const data = err?.response?.data
  if (typeof data === 'string' && data.trim()) return data
  if (data?.message) return data.message
  if (data?.title) return data.title
  return err?.message || fallback
}

function formatBytes(bytes) {
  if (!Number.isFinite(bytes)) return ''
  if (bytes < 1024) return `${bytes} Б`
  if (bytes < 1024 ** 2) return `${(bytes / 1024).toFixed(1)} КБ`
  return `${(bytes / 1024 ** 2).toFixed(1)} МБ`
}

function sanitizeFileName(value) {
  return String(value).replace(/[\\/:*?"<>|]/g, '_').trim() || 'table'
}

function handleEscape(event) {
  if (event.key === 'Escape' && open.value && !busy.value) close()
}

onMounted(() => window.addEventListener('keydown', handleEscape))
onBeforeUnmount(() => window.removeEventListener('keydown', handleEscape))

const PreviewTable = defineComponent({
  name: 'PreviewTable',
  props: { preview: { type: Object, required: true } },
  setup(previewProps) {
    return () => h('div', { class: 'preview-card' }, [
      h('div', { class: 'preview-head' }, [
        h('div', [
          h('span', { class: 'excel-eyebrow' }, 'PREVIEW'),
          h('strong', 'Первые строки Excel')
        ]),
        h('span', `${previewProps.preview.rows?.length || 0} из ${previewProps.preview.rowCount || 0}`)
      ]),
      h('div', { class: 'preview-scroll' }, [
        h('table', { class: 'preview-table' }, [
          h('thead', [
            h('tr', (previewProps.preview.headers || []).map((header, index) =>
              h('th', { key: `${header}-${index}` }, header || `Колонка ${index + 1}`)
            ))
          ]),
          h('tbody', (previewProps.preview.rows || []).map((row, rowIndex) =>
            h('tr', { key: rowIndex }, (row || []).map((value, colIndex) =>
              h('td', { key: colIndex, title: String(value ?? '') }, String(value ?? ''))
            ))
          ))
        ])
      ])
    ])
  }
})
</script>

<style scoped>
.excel-backdrop{position:fixed;inset:0;z-index:1000;display:flex;align-items:center;justify-content:center;padding:20px;background:rgba(15,23,42,.32);backdrop-filter:blur(10px);-webkit-backdrop-filter:blur(10px)}
.excel-modal{width:min(980px,100%);max-height:min(900px,calc(100vh - 40px));overflow:auto;border:1px solid var(--app-border-strong);border-radius:26px;background:var(--app-surface-strong);color:var(--app-text);box-shadow:var(--app-shadow-lg);}
.excel-header{display:flex;align-items:flex-start;justify-content:space-between;gap:16px;padding:22px 24px;border-bottom:1px solid var(--app-border)}
.excel-eyebrow{display:block;color:var(--app-primary);font-size:10px;font-weight:800;letter-spacing:.14em;text-transform:uppercase}
.excel-header h2{margin:5px 0 4px;font-size:20px;letter-spacing:-.03em}
.excel-header p{margin:0;color:var(--app-muted);font-size:12px;line-height:1.5}
.excel-close{width:36px;height:36px;border-radius:11px;border:1px solid var(--app-border);background:var(--app-control);color:var(--app-muted);font-size:22px;line-height:1;cursor:pointer}
.excel-close:hover{background:var(--app-control-hover);color:var(--app-text)}
.excel-tabs{display:flex;gap:5px;padding:10px 24px;border-bottom:1px solid var(--app-border);background:var(--app-bg-soft);overflow:auto}
.excel-tabs button{min-height:38px;padding:0 13px;border:1px solid transparent;border-radius:11px;background:transparent;color:var(--app-muted);white-space:nowrap;cursor:pointer}
.excel-tabs button:hover{background:var(--app-hover);color:var(--app-text-soft)}
.excel-tabs button.active{background:var(--app-active);border-color:var(--app-accent-border);color:var(--app-primary-text)}
.excel-body{padding:18px 24px 22px}
.excel-grid{display:grid;gap:12px;margin-bottom:12px}.excel-grid.two{grid-template-columns:repeat(2,minmax(0,1fr))}
.field-card{padding:14px;border:1px solid var(--app-border);border-radius:17px;background:var(--app-control)}
.field-card>label{display:block;margin-bottom:7px;color:var(--app-text-soft);font-size:10px;font-weight:800}
.field-help{margin-top:6px;color:var(--app-muted);font-size:9px;line-height:1.45}
.excel-input{width:100%;height:42px;padding:0 11px;border:1px solid var(--app-border-strong);border-radius:12px;background:var(--app-input-bg);color:var(--app-text);outline:none}
.excel-input:focus{border-color:var(--app-primary-border);box-shadow:0 0 0 3px var(--app-active)}
.excel-input::placeholder{color:var(--app-muted)}
.file-drop{position:relative;display:flex;align-items:center;gap:10px;min-height:65px;padding:10px;border:1px dashed var(--app-border-strong);border-radius:14px;background:var(--app-control-soft)}
.file-drop.selected{border-color:var(--app-primary-border);background:var(--app-active)}
.file-drop input{position:absolute;inset:0;opacity:0;cursor:pointer}
.file-icon{width:38px;height:38px;display:grid;place-items:center;border-radius:11px;background:var(--app-primary-soft);color:var(--app-primary-text);font-size:10px;font-weight:900}
.file-copy{min-width:0;flex:1}.file-copy strong,.file-copy span{display:block;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}.file-copy strong{color:var(--app-text);font-size:11px}.file-copy span{margin-top:2px;color:var(--app-muted);font-size:9px}
.mini-btn{position:relative;z-index:1;min-height:32px;padding:0 10px;border:1px solid var(--app-border);border-radius:10px;background:var(--app-control);color:var(--app-text-soft);cursor:pointer}
.mini-btn:hover{background:var(--app-control-hover)}
.switch-row{padding:12px;border:1px solid var(--app-border);border-radius:15px;background:var(--app-control-soft);margin-bottom:12px}.switch-row.grouped{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:10px}
.switch-label{display:flex;align-items:center;gap:10px;min-width:0}.switch-label input{position:absolute;opacity:0;pointer-events:none}.switch{position:relative;width:40px;height:22px;flex:0 0 auto;border-radius:999px;background:var(--app-border-strong);border:1px solid var(--app-border);cursor:pointer}.switch::after{content:"";position:absolute;top:2px;left:2px;width:16px;height:16px;border-radius:50%;background:var(--app-highlight);box-shadow:0 1px 3px rgba(0,0,0,.15);transition:.18s}.switch-label input:checked+.switch{background:var(--app-primary);border-color:var(--app-primary)}.switch-label input:checked+.switch::after{transform:translateX(18px);background:#fff}.switch-label input:disabled+.switch{opacity:.5}.switch-label strong,.switch-label small{display:block}.switch-label strong{color:var(--app-text);font-size:10px}.switch-label small{margin-top:2px;color:var(--app-muted);font-size:9px}
.mapping-card{margin-bottom:12px;padding:14px;border:1px solid var(--app-border);border-radius:17px;background:var(--app-control)}.mapping-head{display:flex;align-items:center;justify-content:space-between;gap:10px;padding-bottom:9px;border-bottom:1px solid var(--app-border)}.mapping-head strong{display:block;margin-top:3px;color:var(--app-text);font-size:11px}.mapping-head>span{color:var(--app-muted);font-size:10px}.mapping-list{display:flex;flex-wrap:wrap;gap:6px;margin-top:10px}.mapping-pill{padding:5px 8px;border:1px solid var(--app-border);border-radius:999px;background:var(--app-control-soft);color:var(--app-text-soft);font-size:9px}
.preview-card{margin-bottom:12px;border:1px solid var(--app-border);border-radius:18px;overflow:hidden;background:var(--app-control)}.preview-head{display:flex;align-items:center;justify-content:space-between;gap:10px;padding:12px 14px;border-bottom:1px solid var(--app-border)}.preview-head strong{display:block;margin-top:3px;color:var(--app-text);font-size:11px}.preview-head>span{color:var(--app-muted);font-size:9px}.preview-scroll{max-width:100%;overflow:auto}.preview-table{min-width:600px;width:max-content;border-collapse:separate;border-spacing:0;font-size:9px}.preview-table th,.preview-table td{padding:8px 10px;border-right:1px solid var(--app-border);border-bottom:1px solid var(--app-border);text-align:left;white-space:nowrap}.preview-table th{background:var(--app-bg-soft);color:var(--app-text-soft);font-weight:800}.preview-table td{color:var(--app-text);background:var(--app-control)}.preview-table tbody tr:hover td{background:var(--app-hover)}
.excel-error{margin-bottom:12px;padding:10px 12px;border:1px solid color-mix(in srgb,#ef4444 22%,var(--app-border));border-radius:12px;background:color-mix(in srgb,#ef4444 8%,var(--app-control));color:var(--app-danger);font-size:10px}
.excel-footer{display:flex;align-items:center;justify-content:space-between;gap:12px;padding-top:14px;border-top:1px solid var(--app-border)}.excel-footer-info{color:var(--app-muted);font-size:10px}.primary-btn{min-height:38px;padding:0 13px;border:1px solid var(--app-primary-border);border-radius:12px;background:linear-gradient(135deg,var(--app-primary),var(--app-primary-2));color:#fff;cursor:pointer;box-shadow:0 8px 20px color-mix(in srgb,var(--app-primary) 18%,transparent)}.primary-btn:hover:not(:disabled){transform:translateY(-1px)}.primary-btn:disabled{opacity:.5;cursor:not-allowed}.spinner{display:inline-block;width:13px;height:13px;margin-right:7px;border:2px solid rgba(255,255,255,.35);border-top-color:#fff;border-radius:50%;vertical-align:-2px;animation:spin .8s linear infinite}
.export-hero{display:flex;align-items:flex-start;gap:13px;padding:16px;border:1px solid var(--app-border);border-radius:18px;background:var(--app-primary-soft);margin-bottom:12px}.export-icon{width:44px;height:44px;display:grid;place-items:center;flex:0 0 auto;border-radius:13px;background:var(--app-control);color:var(--app-primary)}.export-icon svg{width:21px;height:21px}.export-hero h3{margin:4px 0 4px;color:var(--app-text);font-size:16px}.export-hero p{margin:0;color:var(--app-muted);font-size:10px;line-height:1.5}.export-list{display:grid;gap:6px;margin-bottom:12px}.export-row{display:flex;justify-content:space-between;gap:12px;padding:9px 11px;border:1px solid var(--app-border);border-radius:12px;background:var(--app-control-soft)}.export-row span{color:var(--app-muted);font-size:10px}.export-row strong{color:var(--app-text);font-size:10px;text-align:right}
.excel-modal-enter-active,.excel-modal-leave-active{transition:opacity .18s ease}.excel-modal-enter-active .excel-modal,.excel-modal-leave-active .excel-modal{transition:transform .18s ease,opacity .18s ease}.excel-modal-enter-from,.excel-modal-leave-to{opacity:0}.excel-modal-enter-from .excel-modal,.excel-modal-leave-to .excel-modal{opacity:0;transform:translateY(8px) scale(.985)}
@media(max-width:760px){.excel-body{padding:14px}.excel-header{padding:18px}.excel-tabs{padding-inline:14px}.excel-grid.two,.switch-row.grouped{grid-template-columns:1fr}.excel-footer{align-items:stretch;flex-direction:column}.primary-btn{width:100%}}
</style>
