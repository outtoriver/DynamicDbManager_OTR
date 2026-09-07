<template>
  <div class="table-grid">
    <div v-if="tables.length === 0" class="empty-state">
      <div class="empty-icon">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path stroke-width="1.7" stroke-linecap="round" d="M5 6.5C5 5.12 8.13 4 12 4s7 1.12 7 2.5S15.87 9 12 9 5 7.88 5 6.5Zm0 0V12c0 1.38 3.13 2.5 7 2.5s7-1.12 7-2.5V6.5m-14 5.5v5.5c0 1.38 3.13 2.5 7 2.5s7-1.12 7-2.5V12" /></svg>
      </div>
      <h3>{{ isSearching ? 'Ничего не найдено' : 'Таблиц пока нет' }}</h3>
      <p>{{ isSearching ? 'Попробуйте изменить запрос.' : 'Создайте первую таблицу через кнопку в верхней панели.' }}</p>
    </div>

    <article v-for="table in tables" :key="table.id" class="table-card glass-panel" @dblclick="isAdmin ? startEdit(table) : openTable(table)">
      <template v-if="editingTableId === table.id">
        <div class="edit-state">
          <div class="edit-icon">✎</div>
          <input ref="editInput"
                 v-model="editingTableName"
                 class="glass-input"
                 @keyup.enter="saveEdit(table)"
                 @keyup.esc="cancelEdit"
                 autofocus />
          <div class="edit-actions">
            <button class="icon-action success" @click.stop="saveEdit(table)" title="Сохранить">✓</button>
            <button class="icon-action" @click.stop="cancelEdit" title="Отмена">✕</button>
          </div>
        </div>
      </template>
      <template v-else>
        <div class="table-card-head">
          <router-link :to="{ name: 'tableData', params: { id: table.id } }" class="table-main">
            <div class="table-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path stroke-width="1.7" stroke-linecap="round" d="M5 6.5C5 5.12 8.13 4 12 4s7 1.12 7 2.5S15.87 9 12 9 5 7.88 5 6.5Zm0 0V12c0 1.38 3.13 2.5 7 2.5s7-1.12 7-2.5V6.5m-14 5.5v5.5c0 1.38 3.13 2.5 7 2.5s7-1.12 7-2.5V12" /></svg>
            </div>
            <div class="min-w-0">
              <h3>{{ table.name }}</h3>
              <p>{{ table.tableColumns?.length || 0 }} колонок <span>·</span> ID {{ table.id }}</p>
            </div>
          </router-link>
          <span class="status-dot"></span>
        </div>

        <div class="table-card-foot">
          <span class="meta-chip"><span class="meta-dot"></span>{{ table.tableColumns?.length || 0 }} полей</span>
          <span v-if="table.createdAt" class="date-text">{{ formatDate(table.createdAt) }}</span>
        </div>

        <div class="card-actions">
          <router-link :to="{ name: 'tableData', params: { id: table.id } }" class="open-btn">
            Открыть
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" d="M5 12h13m-5-5 5 5-5 5" /></svg>
          </router-link>
          <div v-if="isAdmin" class="admin-actions">
            <button class="icon-action" @click.stop="startEdit(table)" title="Переименовать">✎</button>
            <button class="icon-action danger" @click.stop="$emit('delete-table', table)" title="Удалить">⌫</button>
          </div>
        </div>
      </template>
    </article>
  </div>
</template>

<script setup>
import { ref, computed, nextTick } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useRouter } from 'vue-router'
import apiClient from '../api'

const props = defineProps({
    tables: { type: Array, required: true }
})
const emit = defineEmits(['delete-table', 'table-updated'])
const authStore = useAuthStore()
const router = useRouter()
const isAdmin = computed(() => authStore.isAdmin)
const isSearching = computed(() => props.tables.length === 0)
const editingTableId = ref(null)
const editingTableName = ref('')
const editInput = ref(null)

function formatDate(value) {
    try {
      return new Intl.DateTimeFormat('ru-RU', { day:'2-digit', month:'short', year:'numeric' }).format(new Date(value))
    } catch {
      return ''
    }
}
function openTable(table) {
    router.push({ name: 'tableData', params: { id: table.id } })
}
function startEdit(table) {
    editingTableId.value = table.id
    editingTableName.value = table.name
    nextTick(() => editInput.value?.focus())
}
async function saveEdit(table) {
    const name = editingTableName.value.trim()
    if (!name) return
    try {
      await apiClient.put(`/admin/adminTables/${table.id}`, {
        name,
        tableColumns: table.tableColumns,
        modalColumns: table.modalColumns
      })
      table.name = name
      emit('table-updated', table)
    } catch (err) {
      console.error('Ошибка обновления названия:', err)
    } finally {
      cancelEdit()
    }
}
function cancelEdit() {
    editingTableId.value = null
    editingTableName.value = ''
}
</script>

<style scoped>
  .table-grid {
    display: grid;
    grid-template-columns: repeat(3,minmax(0,1fr));
    gap: 12px;
    padding: 0 20px;
  }

  .table-card {
    position: relative;
    min-height: 180px;
    border-radius: 20px;
    padding: 16px;
    overflow: hidden;
    transition: transform .2s ease, border-color .2s ease, box-shadow .2s ease;
  }

    .table-card:hover {
      transform: translateY(-2px);
      border-color: rgba(115,133,255,.18);
      box-shadow: 0 24px 54px rgba(0,0,0,.25),inset 0 1px 0 rgba(255,255,255,.04);
    }

  .table-card-head {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 10px;
  }

  .table-main {
    display: flex;
    align-items: center;
    gap: 11px;
    min-width: 0;
  }

  .table-icon {
    width: 42px;
    height: 42px;
    flex-shrink: 0;
    display: grid;
    place-items: center;
    border-radius: 14px;
    color: #aeb7ff;
    background: linear-gradient(145deg,rgba(115,133,255,.13),rgba(141,108,255,.09));
    border: 1px solid rgba(115,133,255,.12);
  }

    .table-icon svg {
      width: 20px;
      height: 20px;
    }

  .table-main h3 {
    margin: 0;
    color: #edf1ff;
    font-size: .93rem;
    font-weight: 700;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .table-main p {
    margin: 4px 0 0;
    color: #737e94;
    font-size: .72rem;
  }

    .table-main p span {
      color: #4e596d;
      padding-inline: 3px;
    }

  .status-dot {
    width: 7px;
    height: 7px;
    border-radius: 50%;
    background: #54d7a7;
    box-shadow: 0 0 12px rgba(84,215,167,.65);
    margin-top: 7px;
  }

  .table-card-foot {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
    margin: 18px 0 13px;
  }

  .meta-chip {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    padding: 4px 8px;
    border-radius: 999px;
    color: #8490a7;
    background: rgba(255,255,255,.035);
    border: 1px solid rgba(255,255,255,.06);
    font-size: .68rem;
  }

  .meta-dot {
    width: 5px;
    height: 5px;
    border-radius: 50%;
    background: #7f8bff;
  }

  .date-text {
    color: #606b81;
    font-size: .67rem;
  }

  .card-actions {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
  }

  .open-btn {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
    height: 36px;
    padding: 0 10px;
    border-radius: 11px;
    color: #cbd3e8;
    background: rgba(255,255,255,.035);
    border: 1px solid rgba(255,255,255,.06);
    font-size: .75rem;
  }

    .open-btn:hover {
      color: #fff;
      background: rgba(115,133,255,.11);
      border-color: rgba(115,133,255,.14);
    }

    .open-btn svg {
      width: 15px;
      height: 15px;
    }

  .admin-actions, .edit-actions {
    display: flex;
    gap: 6px;
  }

  .icon-action {
    width: 36px;
    height: 36px;
    display: grid;
    place-items: center;
    border-radius: 11px;
    border: 1px solid rgba(255,255,255,.06);
    background: rgba(255,255,255,.035);
    color: #818ca2;
  }

    .icon-action:hover {
      color: #eef2ff;
      background: rgba(255,255,255,.08);
    }

    .icon-action.success {
      color: #71dfb5;
      background: rgba(82,215,167,.08);
    }

    .icon-action.danger {
      color: #ff92aa;
      background: rgba(255,101,132,.06);
    }

  .edit-state {
    display: grid;
    grid-template-columns: auto minmax(0,1fr) auto;
    gap: 8px;
    align-items: center;
    height: 100%;
    min-height: 145px;
  }

  .edit-icon {
    width: 34px;
    height: 34px;
    display: grid;
    place-items: center;
    border-radius: 10px;
    background: rgba(115,133,255,.10);
    color: #aab4ff;
  }

  .empty-state {
    grid-column: 1/-1;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 250px;
    padding: 35px 15px;
    color: #768197;
    text-align: center;
  }

  .empty-icon {
    width: 60px;
    height: 60px;
    display: grid;
    place-items: center;
    border-radius: 18px;
    background: rgba(255,255,255,.035);
    border: 1px solid rgba(255,255,255,.06);
    color: #7f8bff;
    margin-bottom: 14px;
  }

    .empty-icon svg {
      width: 28px;
      height: 28px;
    }

  .empty-state h3 {
    margin: 0;
    color: #d7ddef;
    font-size: .95rem;
  }

  .empty-state p {
    margin: 6px 0 0;
    font-size: .76rem;
    color: #6d788e;
  }

  @media (max-width:1100px) {
    .table-grid {
      grid-template-columns: repeat(2,minmax(0,1fr));
    }
  }

  @media (max-width:640px) {
    .table-grid {
      grid-template-columns: 1fr;
      padding: 0 14px;
    }

    .table-card {
      min-height: 168px;
    }
  }
</style>
