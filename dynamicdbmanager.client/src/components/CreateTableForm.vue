<template>
  <div class="create-form">
    <form @submit.prevent="submitForm">
      <div class="identity-grid">
        <label class="field">
          <span>Системное имя <b>*</b></span>
          <input ref="nameInput" v-model="form.tableName" class="glass-input" type="text" required pattern="[a-zA-Z_][a-zA-Z0-9_]*" placeholder="employees" />
          <small>Латиница, цифры и подчёркивание. Имя начинается с буквы или _.</small>
        </label>
        <label class="field">
          <span>Название для отображения</span>
          <input v-model="form.displayName" class="glass-input" type="text" placeholder="Сотрудники" />
          <small>Используется для удобного ориентирования в интерфейсе.</small>
        </label>
      </div>

      <div class="columns-head">
        <div>
          <span class="eyebrow">СТРУКТУРА</span>
          <h3>Столбцы таблицы</h3>
        </div>
        <button type="button" class="add-column" @click="addColumn">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path stroke-width="1.8" stroke-linecap="round" d="M12 5v14M5 12h14" /></svg>
          Добавить поле
        </button>
      </div>

      <div v-if="form.columns.length === 0" class="columns-empty">
        <div class="columns-empty-icon">＋</div>
        <strong>Поля пока не добавлены</strong>
        <span>Таблицу можно создать и без дополнительных полей, а структуру настроить позже.</span>
      </div>

      <div v-else class="columns-list">
        <article v-for="(col, index) in form.columns" :key="index" class="column-card">
          <div class="column-number">{{ String(index + 1).padStart(2, '0') }}</div>
          <div class="column-main">
            <input v-model="col.columnName" class="glass-input compact-input" placeholder="Имя поля" required pattern="[a-zA-Z_][a-zA-Z0-9_]*" />
            <input v-model="col.displayName" class="glass-input compact-input" placeholder="Отображаемое имя" />
          </div>
          <select v-model="col.dataType" class="glass-input type-select">
            <option value="nvarchar">Текст</option>
            <option value="int">Целое</option>
            <option value="datetime">Дата</option>
            <option value="decimal">Дробное</option>
            <option value="boolean">Флаг</option>
            <option value="uniqueidentifier">UID</option>
          </select>
          <input v-if="col.dataType === 'nvarchar' || col.dataType === 'varchar'" v-model.number="col.maxLength" class="glass-input length-input" type="number" min="1" placeholder="255" />
          <div class="column-switches">
            <label><input type="checkbox" v-model="col.isRequired" /><span>Обяз.</span></label>
            <label><input type="checkbox" v-model="col.isPrimaryKey" /><span>PK</span></label>
            <label><input type="checkbox" v-model="col.isVisibleInTable" /><span>Таблица</span></label>
            <label><input type="checkbox" v-model="col.isVisibleInModal" /><span>Форма</span></label>
          </div>
          <button type="button" class="remove-column" @click="removeColumn(index)" title="Удалить поле">✕</button>
        </article>
      </div>

      <div class="create-footer">
        <div>
          <span class="footer-count">{{ form.columns.length }}</span>
          <span>полей настроено</span>
        </div>
        <button type="submit" class="submit-button" :disabled="isSubmitting">
          <svg v-if="!isSubmitting" viewBox="0 0 24 24" fill="none" stroke="currentColor"><path stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" d="M5 12.5 9.3 17 19 7" /></svg>
          <svg v-else class="spin" viewBox="0 0 24 24" fill="none" stroke="currentColor"><path stroke-width="1.8" stroke-linecap="round" d="M12 4a8 8 0 1 0 8 8" /></svg>
          {{ isSubmitting ? 'Создание…' : 'Создать таблицу' }}
        </button>
      </div>

      <div v-if="error" class="form-error">
        <span>!</span>{{ errorMessage }}
      </div>
    </form>
  </div>
</template>

<script setup>
import { reactive, ref, computed, onMounted, nextTick } from 'vue'
import apiClient from '../api'

const emit = defineEmits(['table-created'])
const form = reactive({ tableName:'', displayName:'', category:'', columns:[] })
const error = ref('')
const isSubmitting = ref(false)
const nameInput = ref(null)

const errorMessage = computed(() => {
    if (!error.value) return ''
    if (typeof error.value === 'string') return error.value
    return 'Не удалось создать таблицу.'
})

onMounted(() => nextTick(() => nameInput.value?.focus()))

function addColumn() {
    form.columns.push({
      columnName:'', displayName:'', dataType:'nvarchar', maxLength:255,
      precision:null, scale:null, isRequired:false, isPrimaryKey:false,
      isVisibleInTable:true, isVisibleInModal:true, defaultValue:null,
      orderIndex:form.columns.length
    })
}
function removeColumn(index) { form.columns.splice(index, 1) }

async function submitForm() {
    const name = form.tableName.trim()
    if (!name) {
      error.value = 'Системное имя обязательно для заполнения.'
      nameInput.value?.focus()
      return
    }
    if (isSubmitting.value) return

    error.value = ''
    isSubmitting.value = true
    try {
      await apiClient.post('/admin/adminTables', {
        name,
        tableColumns: form.columns.map(col => ({
          name: col.columnName,
          type: col.dataType === 'boolean' ? 'boolean' : 'text'
        })),
        modalColumns: form.columns
          .filter(col => col.isVisibleInModal)
          .map(col => ({ name: col.columnName, type: col.dataType === 'boolean' ? 'boolean' : 'text' }))
      })
      emit('table-created')
      form.tableName = ''
      form.displayName = ''
      form.category = ''
      form.columns = []
    } catch (err) {
      error.value = err.response?.data || err.message || 'Ошибка создания таблицы.'
      console.error('Ошибка создания:', err)
    } finally {
      isSubmitting.value = false
      nextTick(() => nameInput.value?.focus())
    }
}
</script>

<style scoped>
  .create-form {
    padding: 0 2px;
  }

  .identity-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 12px;
  }

  .field {
    display: flex;
    flex-direction: column;
    gap: 7px;
  }

    .field > span {
      font-size: .74rem;
      color: #a1abc0;
      font-weight: 650;
    }

    .field b {
      color: #ff91a9;
      font-weight: 700;
    }

    .field small {
      font-size: .68rem;
      color: #616d84;
    }

    .field .glass-input {
      padding: .72rem .8rem;
    }

  .columns-head {
    display: flex;
    align-items: flex-end;
    justify-content: space-between;
    gap: 10px;
    margin: 24px 0 12px;
  }

    .columns-head h3 {
      margin: 4px 0 0;
      font-size: .95rem;
      font-weight: 700;
    }

  .add-column {
    display: inline-flex;
    align-items: center;
    gap: 7px;
    padding: .55rem .72rem;
    border-radius: 11px;
    border: 1px solid rgba(115,133,255,.15);
    background: rgba(115,133,255,.07);
    color: #aab4ff;
  }

    .add-column:hover {
      background: rgba(115,133,255,.12);
    }

    .add-column svg {
      width: 16px;
      height: 16px;
    }

  .columns-empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 4px;
    min-height: 130px;
    border: 1px dashed rgba(255,255,255,.08);
    border-radius: 18px;
    background: rgba(255,255,255,.018);
    color: #78839a;
    text-align: center;
    padding: 20px;
  }

    .columns-empty strong {
      color: #aab3c5;
      font-size: .82rem;
    }

    .columns-empty span {
      font-size: .72rem;
      max-width: 520px;
    }

  .columns-empty-icon {
    width: 36px;
    height: 36px;
    display: grid;
    place-items: center;
    border-radius: 11px;
    background: rgba(115,133,255,.08);
    color: #929eff;
    margin-bottom: 3px;
  }

  .columns-list {
    display: flex;
    flex-direction: column;
    gap: 9px;
  }

  .column-card {
    display: grid;
    grid-template-columns: auto minmax(200px,1fr) 125px 74px auto;
    gap: 8px;
    align-items: center;
    padding: 10px;
    border-radius: 15px;
    background: rgba(255,255,255,.025);
    border: 1px solid rgba(255,255,255,.06);
  }

  .column-number {
    width: 34px;
    height: 34px;
    display: grid;
    place-items: center;
    border-radius: 10px;
    background: rgba(255,255,255,.04);
    color: #6f7b91;
    font-size: .67rem;
    font-weight: 700;
  }

  .column-main {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 7px;
    min-width: 0;
  }

  .compact-input {
    padding: .62rem .7rem;
  }

  .type-select {
    padding: .62rem .7rem;
  }

  .length-input {
    padding: .62rem .55rem;
  }

  .column-switches {
    display: flex;
    align-items: center;
    gap: 7px;
  }

    .column-switches label {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      color: #7f8aa0;
      font-size: .66rem;
    }

    .column-switches input {
      accent-color: #7d8aff;
    }

  .remove-column {
    width: 30px;
    height: 30px;
    border-radius: 9px;
    border: 1px solid rgba(255,255,255,.05);
    background: rgba(255,255,255,.025);
    color: #727d93;
  }

    .remove-column:hover {
      color: #ff92aa;
      background: rgba(255,101,132,.07);
    }

  .create-footer {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 10px;
    margin-top: 16px;
    padding-top: 14px;
    border-top: 1px solid rgba(255,255,255,.06);
    color: #69758d;
    font-size: .72rem;
  }

  .footer-count {
    display: inline-grid;
    place-items: center;
    min-width: 24px;
    height: 24px;
    padding: 0 7px;
    border-radius: 999px;
    background: rgba(115,133,255,.10);
    color: #adb6ff;
    font-weight: 700;
    margin-right: 6px;
  }

  .submit-button {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 7px;
    min-height: 40px;
    padding: 0 14px;
    border-radius: 12px;
    border: 1px solid rgba(115,133,255,.23);
    background: linear-gradient(135deg,rgba(115,133,255,.86),rgba(141,108,255,.80));
    color: #fff;
    font-weight: 650;
    box-shadow: 0 10px 30px rgba(115,133,255,.16);
  }

    .submit-button:hover {
      box-shadow: 0 14px 36px rgba(115,133,255,.23);
    }

    .submit-button:disabled {
      opacity: .55;
      cursor: not-allowed;
    }

    .submit-button svg {
      width: 16px;
      height: 16px;
    }

  .form-error {
    margin-top: 10px;
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 9px 11px;
    border-radius: 12px;
    background: rgba(255,101,132,.07);
    border: 1px solid rgba(255,101,132,.10);
    color: #ff9aaf;
    font-size: .72rem;
  }

    .form-error > span {
      width: 20px;
      height: 20px;
      display: grid;
      place-items: center;
      border-radius: 7px;
      background: rgba(255,101,132,.11);
      font-weight: 700;
    }

  @media (max-width:1020px) {
    .column-card {
      grid-template-columns: auto minmax(0,1fr) 125px auto
    }

    .column-switches {
      grid-column: 2/-1;
    }

    .remove-column {
      grid-column: 4;
      grid-row: 1;
    }

    .column-main {
      grid-template-columns: 1fr;
    }
  }

  @media (max-width:720px) {
    .identity-grid {
      grid-template-columns: 1fr
    }

    .column-card {
      grid-template-columns: auto 1fr auto
    }

    .type-select, .length-input {
      grid-column: 2
    }

    .column-switches {
      grid-column: 2/-1;
      flex-wrap: wrap
    }

    .remove-column {
      grid-column: 3;
      grid-row: 1;
    }

    .create-footer {
      align-items: stretch;
      flex-direction: column
    }

    .submit-button {
      width: 100%;
    }
  }
</style>
