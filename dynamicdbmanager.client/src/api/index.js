import axios from 'axios'
import { useAuthStore } from '../stores/auth'

const apiClient = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' }
})

// Интерцептор для добавления токена
apiClient.interceptors.request.use(config => {
  const authStore = useAuthStore()
  if (authStore.token) {
    try {
      const payload = JSON.parse(atob(authStore.token.split('.')[1]))
      if (payload.exp * 1000 < Date.now()) {
        console.warn('Токен истёк, требуется повторный вход')
        // можно выбросить ошибку, но лучше обработать в ответе
      } else {
        config.headers.Authorization = `Bearer ${authStore.token}`
      }
    } catch {
      config.headers.Authorization = `Bearer ${authStore.token}`
    }
  }
  return config
}, error => Promise.reject(error))

// Интерцептор для обработки ошибок
apiClient.interceptors.response.use(
  response => response,
  error => {
    const authStore = useAuthStore()
    if (error.response) {
      const status = error.response.status
      // 401 - неавторизован
      if (status === 401) {
        authStore.logout()
        alert('Сессия истекла. Пожалуйста, войдите снова.')
        if (window.location.pathname !== '/login') {
          window.location.href = '/login'
        }
      }
      // 403 - запрещено
      else if (status === 403) {
        alert('Доступ запрещён. У вас недостаточно прав.')
      }
      // 500 - внутренняя ошибка сервера
      else if (status >= 500) {
        alert('Внутренняя ошибка сервера. Попробуйте позже.')
      }
      // Другие ошибки
      else {
        // Показываем сообщение от сервера, если есть
        const msg = error.response.data || 'Произошла ошибка'
        if (typeof msg === 'string') alert(msg)
        else alert('Ошибка выполнения запроса')
      }
    } else if (error.request) {
      alert('Нет ответа от сервера. Проверьте соединение.')
    } else {
      alert('Ошибка: ' + error.message)
    }
    return Promise.reject(error)
  }
)

export default apiClient

// ========== Дополнительные API ==========
export const attachmentsApi = {
  upload: (rowId, file) => {
    const formData = new FormData()
    formData.append('file', file)
    return apiClient.post(`/attachments/upload/${rowId}`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
  },
  getList: (rowId) => apiClient.get(`/attachments/row/${rowId}`),
  download: (id) => apiClient.get(`/attachments/download/${id}`, { responseType: 'blob' }),
  delete: (id) => apiClient.delete(`/attachments/${id}`)
}

export const permissionsApi = {
  getAll: () => apiClient.get('/admin/tablepermissions'),
  getUser: (userId) => apiClient.get(`/admin/tablepermissions/user/${userId}`),
  set: (data) => apiClient.post('/admin/tablepermissions', data),
  delete: (id) => apiClient.delete(`/admin/tablepermissions/${id}`),
  batch: (data) => apiClient.post('/admin/tablepermissions/batch', data)
}

export const excelApi = {
  preview: (file, sheetName = '', range = '') => {
    const formData = new FormData()
    formData.append('file', file)
    if (sheetName) formData.append('sheetName', sheetName)
    if (range) formData.append('range', range)
    return apiClient.post('/admin/excel/preview', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
  },
  importNewTable: (file, { sheetName, range, name, hasHeaders = true }) => {
    const formData = new FormData()
    formData.append('file', file)
    formData.append('name', name || '')
    formData.append('hasHeaders', String(hasHeaders))
    if (sheetName) formData.append('sheetName', sheetName)
    if (range) formData.append('range', range)
    return apiClient.post('/admin/excel/import/new-table', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
  },
  importExisting: (tableId, file, { sheetName, range, hasHeaders = true, mapByHeader = true }) => {
    const formData = new FormData()
    formData.append('file', file)
    formData.append('tableId', String(tableId))
    formData.append('hasHeaders', String(hasHeaders))
    formData.append('mapByHeader', String(mapByHeader))
    if (sheetName) formData.append('sheetName', sheetName)
    if (range) formData.append('range', range)
    return apiClient.post(`/admin/excel/import/existing/${tableId}`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
  },
  exportTable: (tableId) => apiClient.get(`/admin/excel/export/${tableId}`, {
    responseType: 'blob'
  })
}

