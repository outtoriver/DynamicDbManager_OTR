import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import { useThemeStore } from './stores/theme'
import './style.css'

const pinia = createPinia()
const app = createApp(App)

useThemeStore(pinia).init()

app.use(pinia)
app.use(router)
app.mount('#app')
