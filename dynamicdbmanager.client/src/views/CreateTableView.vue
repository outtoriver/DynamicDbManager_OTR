<template>
  <div class="glass-card create-legacy">
    <form @submit.prevent="submitForm" class="space-y-5">
      <div>
        <label class="block text-sm font-medium text-gray-300 mb-1">Системное имя</label>
        <input v-model="form.tableName" type="text" required pattern="[a-zA-Z_][a-zA-Z0-9_]*" class="w-full px-3 py-2 border border-gray-600 bg-gray-700 text-white rounded-xl focus:ring-2 focus:ring-blue-500" placeholder="employees" />
      </div>
      <div>
        <label class="block text-sm font-medium text-gray-300 mb-1">Название для отображения</label>
        <input v-model="form.displayName" type="text" class="w-full px-3 py-2 border border-gray-600 bg-gray-700 text-white rounded-xl focus:ring-2 focus:ring-blue-500" placeholder="Сотрудники" />
      </div>

      <!-- Столбцы таблицы (единый список) -->
      <div>
        <div class="flex items-center justify-between mb-2">
          <h3 class="text-lg font-semibold text-white">Столбцы</h3>
          <button type="button" @click="addColumn" class="text-blue-400 text-sm font-medium hover:text-blue-300 flex items-center">
            <svg class="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" /></svg>
            Добавить
          </button>
        </div>
        <div class="space-y-3">
          <div v-for="(col, index) in form.columns" :key="index" class="flex flex-wrap items-center gap-2 bg-gray-800 rounded-xl p-3 border border-gray-700">
            <input v-model="col.columnName" placeholder="Имя" required pattern="[a-zA-Z_][a-zA-Z0-9_]*" class="flex-1 min-w-[100px] px-2 py-1 border border-gray-600 bg-gray-700 text-white rounded-lg text-sm" />
            <select v-model="col.dataType" class="px-2 py-1 border border-gray-600 bg-gray-700 text-white rounded-lg text-sm">
              <option value="nvarchar">Текст</option>
              <option value="int">Целое</option>
              <option value="datetime">Дата</option>
              <option value="decimal">Дробное</option>
              <option value="boolean">Флаг</option>
              <option value="uniqueidentifier">UID</option>
            </select>
            <input v-if="col.dataType === 'nvarchar' || col.dataType === 'varchar'" v-model.number="col.maxLength" type="number" placeholder="Длина" class="w-20 px-2 py-1 border border-gray-600 bg-gray-700 text-white rounded-lg text-sm" />
            <label class="flex items-center text-sm space-x-1"><input type="checkbox" v-model="col.isRequired" class="rounded" /><span class="text-gray-300">Обязательное</span></label>
            <label class="flex items-center text-sm space-x-1"><input type="checkbox" v-model="col.isPrimaryKey" class="rounded" /><span class="text-gray-300">PK</span></label>
            <label class="flex items-center text-sm space-x-1"><input type="checkbox" v-model="col.isVisibleInTable" class="rounded" /><span class="text-gray-300">В таблице</span></label>
            <label class="flex items-center text-sm space-x-1"><input type="checkbox" v-model="col.isVisibleInModal" class="rounded" /><span class="text-gray-300">В модалке</span></label>
            <button type="button" @click="removeColumn(index)" class="text-red-400 hover:text-red-600 p-1">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" /></svg>
            </button>
          </div>
        </div>
      </div>

      <div v-if="error" class="text-red-500 text-sm">{{ error }}</div>
      <button type="submit" class="w-full bg-blue-600 text-white py-3 rounded-xl font-semibold hover:bg-blue-700 active:scale-95 transition-all shadow-md">
        Создать таблицу
      </button>
    </form>
  </div>
</template>

<script setup>
    import { reactive, ref } from 'vue'
    import apiClient from '../api'

    const emit = defineEmits(['table-created'])

    const form = reactive({
      tableName: '',
      displayName: '',
      columns: []
    })

    const error = ref('')

    function addColumn() {
      form.columns.push({
        columnName: '', displayName: '', dataType: 'nvarchar', maxLength: 255,
        precision: null, scale: null, isRequired: false, isPrimaryKey: false,
        isVisibleInTable: true, isVisibleInModal: true,
        defaultValue: null, orderIndex: form.columns.length
      })
    }
    function removeColumn(i) { form.columns.splice(i, 1) }

    async function submitForm() {
      error.value = ''
      try {
        const payload = {
          tableName: form.tableName,
          displayName: form.displayName,
          columns: form.columns.map(col => ({
            columnName: col.columnName,
            displayName: col.displayName,
            dataType: col.dataType,
            maxLength: (col.dataType === 'nvarchar' || col.dataType === 'varchar') ? col.maxLength : null,
            precision: col.dataType === 'decimal' ? col.precision : null,
            scale: col.dataType === 'decimal' ? col.scale : null,
            isRequired: col.isRequired,
            isPrimaryKey: col.isPrimaryKey,
            isVisibleInTable: col.isVisibleInTable,
            isVisibleInModal: col.isVisibleInModal,
            defaultValue: col.defaultValue,
            orderIndex: col.orderIndex
          }))
        }
        await apiClient.post('/tables', payload)
        emit('table-created')
        form.tableName = ''; form.displayName = ''; form.columns = []
      } catch (err) {
        error.value = err.response?.data || 'Ошибка создания таблицы'
      }
    }
</script>

<style scoped>
  .create-legacy {
    max-width: 960px;
    margin: 0 auto;
  }
</style>
