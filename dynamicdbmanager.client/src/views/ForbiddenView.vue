<template>
  <div class="forbidden-page">
    <section class="forbidden-card glass-panel">
      <div class="forbidden-icon"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" d="M12 9v3m0 4h.01M5.7 20h12.6c1.43 0 2.34-1.54 1.62-2.77L13.62 5.3c-.71-1.23-2.49-1.23-3.2 0L4.08 17.23C3.36 18.46 4.27 20 5.7 20Z" /></svg></div>
      <span class="eyebrow">403 · ACCESS CONTROL</span>
      <h1>Доступ запрещён</h1>
      <p>У вашей учётной записи нет прав на просмотр этого раздела.</p>
      <div class="forbidden-actions"><router-link to="/" class="glass-btn primary">Вернуться к таблицам</router-link><button class="glass-btn" @click="logout">Выйти</button></div>
    </section>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
const router = useRouter()
const authStore = useAuthStore()
function logout(){authStore.logout();router.push('/login')}
</script>

<style scoped>
  .forbidden-page {
    min-height: calc(100vh - 2rem);
    display: grid;
    place-items: center;
    padding: 18px;
  }

  .forbidden-card {
    width: min(100%,620px);
    padding: 36px;
    border-radius: 28px;
    text-align: center
  }

  .forbidden-icon {
    width: 72px;
    height: 72px;
    display: grid;
    place-items: center;
    margin: 0 auto 18px;
    border-radius: 22px;
    background: rgba(255,101,132,.07);
    border: 1px solid rgba(255,101,132,.11);
    color: #ff8fa8
  }

    .forbidden-icon svg {
      width: 34px;
      height: 34px
    }

  .forbidden-card .eyebrow {
    color: #777f93;
    font-size: .67rem;
    letter-spacing: .14em
  }

  .forbidden-card h1 {
    margin: 9px 0 7px;
    font-size: 1.8rem;
    font-weight: 760;
    letter-spacing: -.035em
  }

  .forbidden-card p {
    margin: 0;
    color: #7f8aa0
  }

  .forbidden-actions {
    display: flex;
    justify-content: center;
    gap: 9px;
    margin-top: 22px
  }

  .glass-btn.primary {
    color: white;
    background: linear-gradient(135deg,rgba(115,133,255,.88),rgba(141,108,255,.82));
    border-color: rgba(115,133,255,.24)
  }
</style>
