import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import LoginView from '../views/LoginView.vue'
import RegisterView from '../views/RegisterView.vue'
import AdminUsersView from '../views/AdminUsersView.vue'
import TableDataView from '../views/TableDataView.vue'
import HelpView from '../views/HelpView.vue'
import ForbiddenView from '../views/ForbiddenView.vue'
import { useAuthStore } from '../stores/auth'

const routes = [
  { path: '/', name: 'home', component: HomeView, meta: { requiresAuth: true } },
  { path: '/table/:id', name: 'tableData', component: TableDataView, meta: { requiresAuth: true } },
  { path: '/help', name: 'help', component: HelpView, meta: { requiresAuth: true } },
  { path: '/login', name: 'login', component: LoginView, meta: { guest: true } },
  { path: '/register', name: 'register', component: RegisterView, meta: { guest: true } },
  { path: '/admin/users', name: 'adminUsers', component: AdminUsersView, meta: { requiresAuth: true, requiresAdmin: true } },
  {
    path: '/admin/permissions',
    redirect: to => ({ name: 'adminUsers', query: { tab: 'permissions', user: to.query.user } })
  },
  { path: '/forbidden', name: 'forbidden', component: ForbiddenView, meta: { requiresAuth: true } }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to) => {
  const authStore = useAuthStore()
  if (to.meta.requiresAuth && !authStore.isAuthenticated) return '/login'
  if (to.meta.guest && authStore.isAuthenticated) return '/'
  if (to.meta.requiresAdmin && !authStore.isAdmin) return '/forbidden'
  return true
})

export default router
