<template>
  <div class="auth-page">
    <div class="floating-circle auth-orb one"></div>
    <div class="floating-circle auth-orb two"></div>
    <div class="auth-shell glass-panel">
      <div class="auth-brand"><span class="auth-mark"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path stroke-width="1.8" stroke-linecap="round" d="M12 5v14M5 12h14" /></svg></span><div><strong>Dynamic DB Manager</strong><small>Регистрация нового пользователя</small></div></div>
      <div class="auth-heading"><span>НОВАЯ УЧЕТНАЯ ЗАПИСЬ</span><h1>Создание аккаунта</h1><p>Заполните данные и сохраните учётные данные в системе.</p></div>
      <form @submit.prevent="register" class="auth-form">
        <label class="auth-field"><span>Имя пользователя</span><input ref="usernameInput" v-model="username" class="glass-input" type="text" required autocomplete="username" placeholder="Введите логин" /></label>
        <label class="auth-field"><span>Email</span><input v-model="email" class="glass-input" type="email" required autocomplete="email" placeholder="name@example.com" /></label>
        <label class="auth-field"><span>Пароль</span><input v-model="password" class="glass-input" type="password" required autocomplete="new-password" placeholder="Надёжный пароль" /></label>
        <div v-if="error" class="auth-error"><span>!</span>{{ error }}</div>
        <button class="auth-submit" type="submit" :disabled="isLoading"><svg v-if="isLoading" class="spin" viewBox="0 0 24 24" fill="none" stroke="currentColor"><path stroke-width="1.8" stroke-linecap="round" d="M12 4a8 8 0 1 0 8 8" /></svg><span>{{ isLoading ? 'Создаём…' : 'Зарегистрироваться' }}</span><svg v-if="!isLoading" viewBox="0 0 24 24" fill="none" stroke="currentColor"><path stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" d="M5 12h13m-5-5 5 5-5 5" /></svg></button>
      </form>
      <p class="auth-foot">Уже есть аккаунт? <router-link to="/login">Вернуться ко входу</router-link></p>
    </div>
  </div>
</template>

<script setup>
import { ref, nextTick, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const username = ref('')
const email = ref('')
const password = ref('')
const error = ref('')
const isLoading = ref(false)
const usernameInput = ref(null)
const router = useRouter()
const authStore = useAuthStore()

onMounted(() => nextTick(() => usernameInput.value?.focus()))

async function register() {
    if (isLoading.value) return
    error.value = ''
    isLoading.value = true
    try {
      await authStore.register(username.value.trim(), email.value.trim(), password.value)
      router.push('/login')
    } catch (e) {
      const data = e.response?.data
      error.value = Array.isArray(data) ? data.map(item => item.description).join(', ') : (typeof data === 'string' ? data : 'Ошибка регистрации.')
    } finally {
      isLoading.value = false
    }
}
</script>

<style scoped>
  .auth-page {
    min-height: calc(100vh - 2rem);
    display: grid;
    place-items: center;
    position: relative;
    overflow: hidden;
    padding: 18px;
  }

  .auth-orb {
    width: 360px;
    height: 360px;
    opacity: .5;
  }

    .auth-orb.one {
      top: -160px;
      left: -130px;
      background: radial-gradient(circle at 30% 30%,rgba(115,133,255,.42),rgba(115,133,255,0));
    }

    .auth-orb.two {
      right: -160px;
      bottom: -160px;
      animation-delay: -10s;
      background: radial-gradient(circle at 35% 35%,rgba(168,85,247,.34),rgba(168,85,247,0));
    }

  .auth-shell {
    width: min(100%,460px);
    padding: 26px;
    border-radius: 28px;
    position: relative;
    z-index: 2;
    box-shadow: 0 30px 90px rgba(0,0,0,.34),inset 0 1px 0 rgba(255,255,255,.04);
  }

  .auth-brand {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 30px;
  }

    .auth-brand strong {
      display: block;
      font-size: .87rem;
      color: #f4f6ff
    }

    .auth-brand small {
      display: block;
      margin-top: 2px;
      color: #6e7a91;
      font-size: .68rem
    }

  .auth-mark {
    width: 42px;
    height: 42px;
    display: grid;
    place-items: center;
    border-radius: 14px;
    color: white;
    background: linear-gradient(145deg,rgba(115,133,255,.9),rgba(141,108,255,.84));
    box-shadow: 0 12px 34px rgba(115,133,255,.22)
  }

    .auth-mark svg {
      width: 21px;
      height: 21px
    }

  .auth-heading > span {
    font-size: .66rem;
    font-weight: 700;
    letter-spacing: .14em;
    color: #7d89a2
  }

  .auth-heading h1 {
    margin: 8px 0 5px;
    font-size: 1.85rem;
    font-weight: 760;
    letter-spacing: -.04em
  }

  .auth-heading p {
    margin: 0;
    color: #7f8aa0;
    font-size: .82rem
  }

  .auth-form {
    display: flex;
    flex-direction: column;
    gap: 14px;
    margin-top: 22px
  }

  .auth-field {
    display: flex;
    flex-direction: column;
    gap: 7px
  }

    .auth-field > span {
      font-size: .72rem;
      font-weight: 650;
      color: #a3aec2
    }

    .auth-field .glass-input {
      padding: .76rem .82rem
    }

  .auth-submit {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    height: 43px;
    margin-top: 3px;
    border: 1px solid rgba(115,133,255,.26);
    border-radius: 13px;
    color: white;
    background: linear-gradient(135deg,rgba(115,133,255,.88),rgba(141,108,255,.82));
    box-shadow: 0 12px 35px rgba(115,133,255,.18);
    font-weight: 700
  }

    .auth-submit:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 16px 42px rgba(115,133,255,.25)
    }

    .auth-submit:disabled {
      opacity: .55;
      cursor: not-allowed
    }

    .auth-submit svg {
      width: 17px;
      height: 17px
    }

  .auth-error {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 9px 10px;
    border-radius: 12px;
    color: #ff9db1;
    background: rgba(255,101,132,.07);
    border: 1px solid rgba(255,101,132,.10);
    font-size: .72rem
  }

    .auth-error span {
      width: 19px;
      height: 19px;
      display: grid;
      place-items: center;
      border-radius: 6px;
      background: rgba(255,101,132,.11);
      font-weight: 750
    }

  .auth-foot {
    margin: 18px 0 0;
    text-align: center;
    color: #66738a;
    font-size: .72rem
  }

    .auth-foot a {
      color: #aab4ff
    }

      .auth-foot a:hover {
        text-decoration: underline
      }

  .spin {
    animation: spin .8s linear infinite
  }

  @keyframes spin {
    to {
      transform: rotate(360deg)
    }
  }
</style>
