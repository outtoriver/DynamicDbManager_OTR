<template>
  <section class="login-page">
    <div class="login-glow login-glow-left"></div>
    <div class="login-glow login-glow-right"></div>

    <div class="login-card glass-panel">
      <div class="login-brand">
        <div class="login-brand-mark">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true">
            <path d="M5 6.5C5 5.12 8.13 4 12 4s7 1.12 7 2.5S15.87 9 12 9 5 7.88 5 6.5Zm0 0V12c0 1.38 3.13 2.5 7 2.5s7-1.12 7-2.5V6.5m-14 5.5v5.5c0 1.38 3.13 2.5 7 2.5s7-1.12 7-2.5V12" stroke-width="1.8" stroke-linecap="round"/>
          </svg>
        </div>
        <div>
          <strong>Dynamic DB Manager</strong>
          <small>Единое рабочее пространство</small>
        </div>
      </div>

      <div class="login-heading">
        <span>АВТОРИЗАЦИЯ</span>
        <h1>С возвращением</h1>
        <p>Введите учётные данные, чтобы продолжить работу.</p>
      </div>

      <form class="login-form" @submit.prevent="login">
        <label class="login-field">
          <span>Имя пользователя</span>
          <input ref="usernameInput" v-model="username" class="login-input" type="text" required autocomplete="username" placeholder="admin" />
        </label>

        <label class="login-field">
          <span>Пароль</span>
          <input v-model="password" class="login-input" type="password" required autocomplete="current-password" placeholder="Введите пароль" />
        </label>

        <div v-if="error" class="login-error" role="alert"><span>!</span>{{ error }}</div>

        <button class="login-submit" type="submit" :disabled="isLoading">
          <svg v-if="isLoading" class="spin" viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true"><path stroke-width="1.8" stroke-linecap="round" d="M12 4a8 8 0 1 0 8 8" /></svg>
          <span>{{ isLoading ? 'Проверяем…' : 'Войти' }}</span>
          <svg v-if="!isLoading" viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true"><path stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" d="M5 12h13m-5-5 5 5-5 5" /></svg>
        </button>
      </form>

      <div class="login-foot">
        <span class="login-dot"></span>
        Защищённый вход в систему
      </div>
    </div>
  </section>
</template>

<script setup>
import { ref, nextTick, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const username = ref('')
const password = ref('')
const error = ref('')
const isLoading = ref(false)
const usernameInput = ref(null)
const router = useRouter()
const authStore = useAuthStore()

onMounted(() => nextTick(() => usernameInput.value?.focus()))

async function login() {
  if (isLoading.value) return
  error.value = ''
  isLoading.value = true
  try {
    await authStore.login(username.value.trim(), password.value)
    router.push('/')
  } catch (e) {
    error.value = typeof e.response?.data === 'string'
      ? e.response.data
      : 'Не удалось выполнить вход. Проверьте имя пользователя и пароль.'
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
.login-page{position:relative;width:100%;min-height:100vh;display:flex;align-items:center;justify-content:center;box-sizing:border-box;overflow:hidden;padding:24px}
.login-card{position:relative;z-index:2;width:min(100%,430px);padding:30px;border-radius:26px;box-shadow:var(--app-shadow-lg),inset 0 1px 0 var(--app-highlight);backdrop-filter:blur(24px) saturate(140%);-webkit-backdrop-filter:blur(24px) saturate(140%)}
.login-brand{display:flex;align-items:center;gap:12px;margin-bottom:30px}.login-brand strong{display:block;font-size:.88rem;color:var(--app-text)}.login-brand small{display:block;margin-top:3px;color:var(--app-muted);font-size:.68rem}
.login-brand-mark{width:44px;height:44px;display:grid;place-items:center;flex:0 0 auto;border-radius:14px;color:#fff;background:linear-gradient(145deg,var(--app-primary),var(--app-primary-2));box-shadow:0 12px 34px var(--app-primary-shadow)}.login-brand-mark svg{width:21px;height:21px}
.login-heading>span{font-size:.66rem;font-weight:750;letter-spacing:.14em;color:var(--app-muted)}.login-heading h1{margin:8px 0 5px;font-size:1.85rem;line-height:1.15;font-weight:760;letter-spacing:-.04em;color:var(--app-text)}.login-heading p{margin:0;color:var(--app-muted);font-size:.82rem;line-height:1.5}
.login-form{display:flex;flex-direction:column;gap:14px;margin-top:22px}.login-field{display:flex;flex-direction:column;gap:7px}.login-field>span{font-size:.72rem;font-weight:650;color:var(--app-text-soft)}
.login-input{width:100%;height:45px;padding:0 13px;box-sizing:border-box;border:1px solid var(--app-border);border-radius:12px;background:var(--app-input-bg);color:var(--app-text);outline:none;font:inherit;font-size:.78rem;transition:border-color .18s ease,box-shadow .18s ease,background .18s ease}.login-input::placeholder{color:var(--app-muted)}.login-input:hover{border-color:var(--app-border-strong)}.login-input:focus{border-color:var(--app-primary-border);box-shadow:0 0 0 3px var(--app-primary-soft);background:var(--app-control)}
.login-error{display:flex;align-items:center;gap:8px;padding:9px 10px;border-radius:12px;color:var(--app-danger);background:var(--app-danger-soft);border:1px solid var(--app-danger-border);font-size:.72rem}.login-error span{width:19px;height:19px;display:grid;place-items:center;border-radius:6px;background:var(--app-danger-soft);font-weight:750;flex:0 0 auto}
.login-submit{display:flex;align-items:center;justify-content:center;gap:8px;width:100%;height:45px;margin-top:3px;border:1px solid var(--app-accent-border);border-radius:13px;color:#fff;background:linear-gradient(135deg,var(--app-primary),var(--app-primary-2));box-shadow:0 12px 35px var(--app-primary-shadow);font:inherit;font-weight:700;cursor:pointer;transition:transform .18s ease,box-shadow .18s ease,opacity .18s ease}.login-submit:hover:not(:disabled){transform:translateY(-1px);box-shadow:0 16px 42px var(--app-primary-shadow)}.login-submit:disabled{opacity:.55;cursor:not-allowed}.login-submit svg{width:17px;height:17px}
.login-foot{display:flex;align-items:center;justify-content:center;gap:7px;margin-top:20px;color:var(--app-muted);font-size:.68rem}.login-dot{width:6px;height:6px;border-radius:50%;background:var(--app-success);box-shadow:0 0 0 3px var(--app-success-soft)}
.login-glow{position:absolute;width:420px;height:420px;border-radius:50%;pointer-events:none;filter:blur(12px);opacity:.55}.login-glow-left{top:-250px;left:-230px;background:radial-gradient(circle,var(--app-primary-shadow) 0%,transparent 70%)}.login-glow-right{right:-250px;bottom:-250px;background:radial-gradient(circle,rgba(138,103,245,.18) 0%,transparent 70%)}
.spin{animation:spin .8s linear infinite}@keyframes spin{to{transform:rotate(360deg)}}
@media(max-width:600px){.login-page{padding:16px}.login-card{padding:24px;border-radius:22px}.login-heading h1{font-size:1.65rem}}
@media(max-height:680px) and (min-width:601px){.login-page{padding-top:18px;padding-bottom:18px}.login-card{padding-top:22px;padding-bottom:22px}.login-brand{margin-bottom:20px}}
</style>
