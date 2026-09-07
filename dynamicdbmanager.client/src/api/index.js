import axios from 'axios'
import { useAuthStore } from '../stores/auth'

const apiClient = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' }
})

let handlingUnauthorized = false

function decodeJwtPayload(token) {
  const part = token?.split('.')[1]
  if (!part) return null
  const normalized = part.replace(/-/g, '+').replace(/_/g, '/')
  const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, '=')
  return JSON.parse(atob(padded))
}

function isTokenExpired(token) {
  try {
    const payload = decodeJwtPayload(token)
    return !payload?.exp || payload.exp * 1000 <= Date.now()
  } catch {
    return false
  }
}

apiClient.interceptors.request.use(config => {
  const authStore = useAuthStore()
  const url = String(config.url || '')
  const isAuthEndpoint = url.includes('/auth/login') || url.includes('/auth/register')

  if (authStore.token && !isAuthEndpoint && !isTokenExpired(authStore.token)) {
    config.headers = config.headers || {}
    config.headers.Authorization = `Bearer ${authStore.token}`
  }

  return config
}, error => Promise.reject(error))

apiClient.interceptors.response.use(
  response => response,
  async error => {
    const authStore = useAuthStore()
    const status = error.response?.status
    const config = error.config || {}
    const url = String(config.url || '')
    const isAuthEndpoint = url.includes('/auth/login') || url.includes('/auth/register')

    if (status === 401) {
      // Login itself may legitimately return 401 for wrong credentials.
      if (isAuthEndpoint) {
        return Promise.reject(error)
      }

      // Avoid an immediate logout on a single transient/incorrect 401.
      // Retry the same request once with the current token.
      if (!config._authRetry && authStore.token && !isTokenExpired(authStore.token)) {
        config._authRetry = true
        config.headers = config.headers || {}
        config.headers.Authorization = `Bearer ${authStore.token}`
        return apiClient.request(config)
      }

      if (!handlingUnauthorized) {
        handlingUnauthorized = true
        try {
          authStore.logout()
          alert('Сессия недействительна или истекла. Пожалуйста, войдите снова.')
          if (window.location.pathname !== '/login') {
            window.location.href = '/login'
          }
        } finally {
          setTimeout(() => { handlingUnauthorized = false }, 250)
        }
      }
    } else if (status === 403) {
      alert('Доступ запрещён. У вас недостаточно прав.')
    } else if (status >= 500) {
      alert('Внутренняя ошибка сервера. Попробуйте позже.')
    } else if (error.response) {
      const msg = error.response.data || 'Произошла ошибка'
      if (typeof msg === 'string') alert(msg)
      else alert('Ошибка выполнения запроса')
    } else if (error.request) {
      alert('Нет ответа от сервера. Проверьте соединение.')
    } else {
      alert('Ошибка: ' + error.message)
    }

    return Promise.reject(error)
  }
)

export default apiClient

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
