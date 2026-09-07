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
            <button type="button" class="excel-close" @click="close">×</button>
          </header>

          <div class="excel-tabs">
            <button type="button" :class="{ active: mode === 'new' }" @click="mode = 'new'">Новая таблица</button>
            <button type="button" :class="{ active: mode === 'existing' }" @click="mode = 'existing'">В существующую</button>
            <button type="button" :class="{ active: mode === 'export' }" @click="mode = 'export'">Экспорт</button>
          </div>

          <div class="excel-body">
            <!-- NEW TABLE -->
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
                  <input v-model="newTableName" class="excel-input" placeholder="Например: Сотрудники" />
                  <div class="field-help">Если оставить пустым, будет использовано имя Excel-файла.</div>
                </div>
              </div>

              <div v-if="preview" class="excel-grid two">
                <div class="field-card">
                  <label>Лист</label>
                  <select v-model="sheetName" class="excel-input" @change="loadPreview">
                    <option v-for="sheet in preview.sheets" :key="sheet.name" :value="sheet.name">{{ sheet.name }}</option>
                  </select>
                  <div class="field-help">Используемый диапазон: {{ preview.usedRange || 'не определён' }}</div>
                </div>
                <div class="field-card">
                  <label>Диапазон</label>
                  <input v-model="range" class="excel-input" placeholder="A1:F100 (пусто = весь используемый диапазон)" @change="loadPreview" @keyup.enter="loadPreview" />
                  <div class="field-help">Первая строка диапазона будет заголовками колонок.</div>
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

            <!-- EXISTING TABLE -->
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
                    <option v-for="sheet in preview.sheets" :key="sheet.name" :value="sheet.name">{{ sheet.name }}</option>
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
                  <span><strong>Первая строка — заголовки</strong><small>Заголовки используются для сопоставления колонок.</small></span>
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

            <!-- EXPORT -->
            <template v-else>
              <div class="export-hero">
                <div class="export-icon">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M7 3h8l4 4v14H7a3 3 0 0 1-3-3V6a3 3 0 0 1 3-3Z" stroke-width="1.7"/><path d="M15 3v5h5M8 12h8M8 16h8" stroke-width="1.7" stroke-linecap="round"/></svg>
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
import { computed, defineComponent, h, ref, watch } from 'vue'
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
  Array.isArray(targetTable.value?.tableColumns) ? targetTable.value.tableColumns : []
)

watch(() => props.selectedTableId, value => {
  if (value !== undefined && value !== null && value !== '') targetTableId.value = String(value)
})

watch(mode, () => {
  error.value = ''
  if (mode.value === 'export' && props.selectedTableId) targetTableId.value = String(props.selectedTableId)
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
  if (!file.value) return
  busy.value = true
  error.value = ''
  try {
    const { data } = await excelApi.preview(file.value, sheetName.value, range.value)
    preview.value = data
    if (!sheetName.value && data.sheets?.length) sheetName.value = data.sheets[0].name
  } catch (err) {
    preview.value = null
    error.value = getError(err, 'Не удалось прочитать Excel-файл')
  } finally {
    busy.value = false
  }
}

async function importNewTable() {
  if (!file.value) return
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
  if (!file.value || !targetTableId.value) return
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
  if (!targetTableId.value) return
  busy.value = true
  error.value = ''
  try {
    const response = await excelApi.exportTable(Number(targetTableId.value))
    const blob = new Blob([response.data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
    const url = URL.createObjectURL(blob)
    const anchor = document.createElement('a')
    anchor.href = url
    anchor.download = `${sanitizeFileName(targetTable.value?.name || 'table')}.xlsx`
    document.body.appendChild(anchor)
    anchor.click()
    anchor.remove()
    URL.revokeObjectURL(url)
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

const PreviewTable = defineComponent({
  name: 'PreviewTable',
  props: { preview: { type: Object, required: true } },
  setup(previewProps) {
    return () => h('div', { class: 'preview-card' }, [
      h('div', { class: 'preview-head' }, [
        h('div', [h('span', { class: 'excel-eyebrow' }, 'PREVIEW'), h('strong', 'Первые строки Excel')]),
        h('span', `${previewProps.preview.rows?.length || 0} из ${previewProps.preview.rowCount || 0}`)
      ]),
      h('div', { class: 'preview-scroll' }, [
        h('table', { class: 'preview-table' }, [
          h('thead', [h('tr', (previewProps.preview.headers || []).map((header, index) =>
            h('th', { key: `${header}-${index}` }, header || `Колонка ${index + 1}`)
          ))]),
          h('tbody', (previewProps.preview.rows || []).map((row, rowIndex) =>
            h('tr', { key: rowIndex }, (row || []).map((value, colIndex) => h('td', { key: colIndex, title: String(value ?? '') }, String(value ?? ''))))
          ))
        ])
      ])
    ])
  }
})
</script>

<style scoped>
.excel-backdrop{position:fixed;inset:0;z-index:2000;display:flex;align-items:center;justify-content:center;padding:24px;background:rgba(0,0,0,.72);backdrop-filter:blur(14px);-webkit-backdrop-filter:blur(14px)}
.excel-modal{width:min(1080px,100%);max-height:min(900px,calc(100vh - 48px));overflow:auto;border:1px solid rgba(255,255,255,.09);border-radius:24px;background:linear-gradient(180deg,rgba(12,16,25,.98),rgba(5,7,11,.99));box-shadow:0 30px 100px rgba(0,0,0,.7);color:#e5e7eb}
.excel-header{display:flex;align-items:flex-start;justify-content:space-between;gap:18px;padding:24px 26px 18px;border-bottom:1px solid rgba(255,255,255,.07)}
.excel-header h2{margin:3px 0 5px;font-size:22px;line-height:1.15;color:#f8fafc}.excel-header p{margin:0;max-width:720px;color:#7f8ba0;font-size:12px}.excel-eyebrow{color:#64748b;font-size:9px;font-weight:800;letter-spacing:.16em;text-transform:uppercase}.excel-close{width:34px;height:34px;border:1px solid #202938;border-radius:10px;background:#0b0f15;color:#94a3b8;font-size:23px;cursor:pointer}.excel-close:hover{color:#fff;background:#121824}
.excel-tabs{display:flex;gap:6px;padding:12px 18px;border-bottom:1px solid rgba(255,255,255,.06);overflow:auto}.excel-tabs button{height:36px;padding:0 14px;border:1px solid transparent;border-radius:10px;background:transparent;color:#8190a5;font-size:12px;font-weight:700;white-space:nowrap;cursor:pointer}.excel-tabs button:hover{background:#0f141c;color:#dbe4f1}.excel-tabs button.active{background:#121c2d;color:#c7d2fe;border-color:#283858;box-shadow:0 8px 24px rgba(37,99,235,.08)}
.excel-body{padding:20px 24px 24px}.excel-grid{display:grid;gap:12px}.excel-grid.two{grid-template-columns:1fr 1fr}.field-card{padding:14px;border:1px solid rgba(255,255,255,.07);border-radius:15px;background:rgba(10,14,21,.72)}.field-card>label{display:block;margin-bottom:7px;color:#a8b3c4;font-size:11px;font-weight:700}.field-help{margin-top:6px;color:#586579;font-size:10px}.excel-input{width:100%;height:40px;padding:0 11px;border:1px solid #202938;border-radius:10px;background:#080b10;color:#e5e7eb;outline:none;font-size:12px}.excel-input:focus{border-color:#40517a;box-shadow:0 0 0 3px rgba(99,102,241,.1)}
.file-drop{display:flex;align-items:center;gap:11px;min-height:64px;padding:10px;border:1px dashed #263247;border-radius:13px;background:#080b10}.file-drop input{display:none}.file-drop.selected{border-color:#36518a;background:#0b1220}.file-icon{display:grid;place-items:center;width:38px;height:38px;border-radius:9px;background:#18233b;color:#93c5fd;font-size:9px;font-weight:900}.file-copy{min-width:0;flex:1;display:flex;flex-direction:column;gap:3px}.file-copy strong{overflow:hidden;color:#dbe4f1;font-size:11px;white-space:nowrap;text-overflow:ellipsis}.file-copy span{color:#59677c;font-size:10px}.mini-btn{height:32px;padding:0 10px;border:1px solid #253149;border-radius:9px;background:#101722;color:#cbd5e1;font-size:11px;font-weight:700;cursor:pointer}.mini-btn:hover{background:#162033;color:#fff}
.switch-row{margin-top:12px;padding:12px 14px;border:1px solid rgba(255,255,255,.06);border-radius:14px;background:#080c12}.switch-row.grouped{display:grid;grid-template-columns:1fr 1fr;gap:12px}.switch-label{display:flex;align-items:flex-start;gap:10px;cursor:pointer}.switch-label input{position:absolute;opacity:0;pointer-events:none}.switch{position:relative;flex:0 0 34px;width:34px;height:20px;margin-top:1px;border-radius:99px;background:#202938;border:1px solid #2b3546;transition:.18s}.switch:after{content:"";position:absolute;top:3px;left:3px;width:12px;height:12px;border-radius:50%;background:#69778d;transition:.18s}.switch-label input:checked + .switch{background:#304a8b;border-color:#4965ae}.switch-label input:checked + .switch:after{left:17px;background:#e0e7ff}.switch-label input:disabled + .switch{opacity:.35}.switch-label strong{display:block;color:#dbe4f1;font-size:11px}.switch-label small{display:block;margin-top:2px;color:#59677c;font-size:10px}
.preview-card{margin-top:14px;border:1px solid rgba(255,255,255,.07);border-radius:15px;overflow:hidden;background:#070a0f}.preview-head,.mapping-head{display:flex;align-items:center;justify-content:space-between;gap:10px;padding:11px 13px;border-bottom:1px solid rgba(255,255,255,.06)}.preview-head>div{display:flex;align-items:center;gap:9px}.preview-head strong,.mapping-head strong{color:#cbd5e1;font-size:11px}.preview-head>span,.mapping-head>span{color:#586579;font-size:10px}.preview-scroll{max-height:240px;overflow:auto}.preview-table{width:max-content;min-width:100%;border-collapse:separate;border-spacing:0}.preview-table th,.preview-table td{max-width:280px;padding:8px 10px;border-right:1px solid rgba(255,255,255,.05);border-bottom:1px solid rgba(255,255,255,.05);text-align:left;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;font-size:10px}.preview-table th{position:sticky;top:0;background:#0d131d;color:#8fa0b6;font-weight:800;z-index:2}.preview-table td{color:#b7c2d2}.preview-table tr:hover td{background:#0b1119}
.mapping-card{margin-top:12px;border:1px solid rgba(255,255,255,.07);border-radius:15px;background:#070a0f;overflow:hidden}.mapping-list{display:flex;flex-wrap:wrap;gap:6px;padding:12px}.mapping-pill{padding:5px 8px;border:1px solid #263247;border-radius:8px;background:#0b1119;color:#9fb0c8;font-size:10px}
.export-hero{display:flex;align-items:flex-start;gap:15px;padding:18px;border:1px solid rgba(255,255,255,.07);border-radius:16px;background:linear-gradient(135deg,rgba(30,64,175,.12),rgba(9,12,18,.8))}.export-icon{display:grid;place-items:center;flex:0 0 48px;width:48px;height:48px;border:1px solid #29416f;border-radius:13px;background:#0c1730;color:#93c5fd}.export-icon svg{width:23px;height:23px}.export-hero h3{margin:4px 0 5px;color:#f1f5f9;font-size:16px}.export-hero p{margin:0;color:#718096;font-size:11px;line-height:1.6}.export-list{margin-top:12px;border:1px solid rgba(255,255,255,.07);border-radius:15px;overflow:hidden}.export-row{display:flex;justify-content:space-between;gap:20px;padding:11px 14px;border-bottom:1px solid rgba(255,255,255,.05);font-size:11px}.export-row:last-child{border-bottom:0}.export-row span{color:#59677c}.export-row strong{color:#cbd5e1;font-weight:700}
.excel-error{margin-top:12px;padding:10px 12px;border:1px solid rgba(244,63,94,.2);border-radius:11px;background:rgba(127,29,29,.18);color:#fda4af;font-size:11px}.excel-footer{display:flex;align-items:center;justify-content:space-between;gap:12px;margin-top:16px}.excel-footer-info{color:#59677c;font-size:10px}.primary-btn{display:inline-flex;align-items:center;justify-content:center;gap:8px;height:40px;padding:0 15px;border:1px solid #314c91;border-radius:10px;background:#1c3270;color:#e0e7ff;font-size:11px;font-weight:800;cursor:pointer;box-shadow:0 8px 24px rgba(37,99,235,.12)}.primary-btn:hover{background:#24418d}.primary-btn:disabled{opacity:.45;cursor:not-allowed}.spinner{width:13px;height:13px;border:2px solid rgba(255,255,255,.3);border-top-color:#fff;border-radius:50%;animation:spin .7s linear infinite}@keyframes spin{to{transform:rotate(360deg)}}
.excel-modal-enter-active,.excel-modal-leave-active{transition:opacity .16s ease}.excel-modal-enter-active .excel-modal,.excel-modal-leave-active .excel-modal{transition:transform .16s ease}.excel-modal-enter-from,.excel-modal-leave-to{opacity:0}.excel-modal-enter-from .excel-modal,.excel-modal-leave-to .excel-modal{transform:translateY(8px) scale(.985)}
@media(max-width:760px){.excel-backdrop{padding:10px}.excel-modal{max-height:calc(100vh - 20px);border-radius:18px}.excel-body{padding:14px}.excel-grid.two,.switch-row.grouped{grid-template-columns:1fr}.excel-header{padding:18px}.excel-footer{flex-direction:column-reverse;align-items:stretch}.primary-btn{width:100%}}
</style>
