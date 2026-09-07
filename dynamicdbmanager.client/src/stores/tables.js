import { defineStore } from 'pinia'
import apiClient from '../api'

export const useTablesStore = defineStore('tables', {
  state: () => ({
    tables: [],
    loading: false,
    loaded: false,
    error: null,

    // { [tableId]: rows[] }
    rowsCache: {},

    // Promise текущей загрузки списка таблиц.
    // Не даёт нескольким компонентам одновременно
    // запрашивать один и тот же список.
    loadingPromise: null
  }),

  getters: {
    categories: (state) => {
      const cats = new Set()

      state.tables.forEach(table => {
        const name = String(table?.name || '').trim()

        if (!name) {
          return
        }

        const firstWord = name.split(/\s+/)[0]

        if (firstWord) {
          cats.add(firstWord)
        }
      })

      return Array.from(cats).sort((a, b) =>
        a.localeCompare(b, 'ru')
      )
    },

    getTableById: (state) => (id) => {
      if (
        id === null ||
        id === undefined ||
        id === ''
      ) {
        return null
      }

      return state.tables.find(
        table =>
          String(table.id) === String(id)
      ) || null
    },

    getRows: (state) => (tableId) => {
      return state.rowsCache[tableId] || []
    }
  },

  actions: {
    // ==========================================================
    // LOAD TABLES
    // ==========================================================

    async loadTables(force = false) {
      if (
        this.loaded &&
        !force
      ) {
        return this.tables
      }

      // Если загрузка уже выполняется,
      // ждём существующий запрос.
      if (
        this.loading &&
        this.loadingPromise
      ) {
        return this.loadingPromise
      }

      this.loading = true
      this.error = null

      this.loadingPromise =
        (async () => {
          try {
            const { data } =
              await apiClient.get(
                '/admin/adminTables'
              )

            this.tables =
              Array.isArray(data)
                ? data
                : []

            this.loaded = true

            return this.tables
          }
          catch (error) {
            console.error(
              'Ошибка загрузки таблиц:',
              error
            )

            this.error =
              error

            // Важно:
            // состояние загрузки завершено,
            // чтобы UI мог показать ошибку,
            // а не бесконечный spinner.
            this.loaded = false

            throw error
          }
          finally {
            this.loading = false
            this.loadingPromise = null
          }
        })()

      return this.loadingPromise
    },

    // ==========================================================
    // LOAD ROWS
    // ==========================================================

    async loadRows(
      tableId,
      force = false
    ) {
      if (
        tableId === null ||
        tableId === undefined ||
        tableId === ''
      ) {
        return []
      }

      if (
        !force &&
        this.rowsCache[tableId]
      ) {
        return this.rowsCache[tableId]
      }

      try {
        const { data } =
          await apiClient.get(
            `/admin/adminTables/${tableId}/rows`
          )

        this.rowsCache[tableId] =
          Array.isArray(data)
            ? data
            : []

        return this.rowsCache[tableId]
      }
      catch (error) {
        console.error(
          'Ошибка загрузки строк:',
          error
        )

        throw error
      }
    },

    // ==========================================================
    // CACHE
    // ==========================================================

    invalidateRows(tableId) {
      if (
        tableId === null ||
        tableId === undefined
      ) {
        return
      }

      delete this.rowsCache[tableId]
    },

    clearRowsCache() {
      this.rowsCache = {}
    },

    // ==========================================================
    // LOCAL TABLE STATE
    // ==========================================================

    async addTable(table) {
      if (!table) {
        return
      }

      this.tables.push(table)
    },

    async updateTable(updated) {
      if (!updated) {
        return
      }

      const index =
        this.tables.findIndex(
          table =>
            String(table.id) ===
            String(updated.id)
        )

      if (index !== -1) {
        this.tables[index] =
          updated
      }
    },

    async removeTable(id) {
      this.tables =
        this.tables.filter(
          table =>
            String(table.id) !==
            String(id)
        )

      this.invalidateRows(id)
    },

    // ==========================================================
    // RESET
    // ==========================================================

    reset() {
      this.tables = []
      this.loading = false
      this.loaded = false
      this.error = null
      this.loadingPromise = null
      this.rowsCache = {}
    }
  }
})
