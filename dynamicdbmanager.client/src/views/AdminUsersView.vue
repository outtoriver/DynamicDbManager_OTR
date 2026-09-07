<template>
  <div class="access-page">
    <section class="access-hero panel">
      <div class="hero-copy">
        <span class="eyebrow"><i></i> АДМИНИСТРИРОВАНИЕ</span>
        <h1>Доступ</h1>
        <p>Единое рабочее пространство для учетных записей и прав доступа.</p>
      </div>
      <div class="hero-actions">
        <button class="ui-btn ghost" type="button" :disabled="isRefreshing" @click="refreshData">
          <svg class="icon" :class="{spin:isRefreshing}" viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M4 4v6h6M20 20v-6h-6M6.5 17.5A8 8 0 0 0 20 14M17.5 6.5A8 8 0 0 0 4 10" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/></svg>
          Обновить
        </button>
      </div>
    </section>

    <section class="stats-grid">
      <div class="stat panel"><div class="stat-icon blue">👤</div><div><span>Учетные записи</span><strong>{{ adminStore.users.length }}</strong></div></div>
      <div class="stat panel"><div class="stat-icon violet">◈</div><div><span>Администраторы</span><strong>{{ administratorCount }}</strong></div></div>
      <div class="stat panel"><div class="stat-icon cyan">⌘</div><div><span>С правами</span><strong>{{ usersWithPermissionsCount }}</strong></div></div>
      <div class="stat panel"><div class="stat-icon emerald">✓</div><div><span>Назначено прав</span><strong>{{ adminStore.permissions.length }}</strong></div></div>
    </section>

    <section class="workspace panel">
      <div class="workspace-head">
        <div><span class="caption">КОНТРОЛЬ ДОСТУПА</span><h2>{{ activeTab==='accounts' ? 'Учетные записи' : 'Права доступа' }}</h2></div>
        <div class="tabs">
          <button class="tab" :class="{active:activeTab==='accounts'}" type="button" @click="switchTab('accounts')">👤 Учетные записи</button>
          <button class="tab" :class="{active:activeTab==='permissions'}" type="button" @click="switchTab('permissions')">🔐 Права доступа</button>
        </div>
      </div>

      <template v-if="activeTab==='accounts'">
        <div class="toolbar">
          <div class="search-wrap"><span>⌕</span><input v-model="searchQuery" placeholder="Поиск по имени, email или роли…"><button v-if="searchQuery" @click="searchQuery=''">×</button></div>
          <div class="toolbar-actions">
            <div class="segmented"><button :class="{active:roleFilter==='all'}" @click="roleFilter='all'">Все</button><button :class="{active:roleFilter==='admin'}" @click="roleFilter='admin'">Admin</button><button :class="{active:roleFilter==='user'}" @click="roleFilter='user'">User</button></div>
            <button class="ui-btn primary" type="button" @click="openCreateModal">＋ Добавить</button>
          </div>
        </div>

        <div v-if="adminStore.loading" class="users-grid">
          <div v-for="i in 4" :key="i" class="account-card skeleton"></div>
        </div>
        <div v-else-if="filteredUsers.length===0" class="empty">
          <div class="empty-icon">⌕</div><h3>Пользователи не найдены</h3><p>Измените поисковый запрос или фильтр.</p>
        </div>
        <div v-else class="users-grid">
          <article v-for="user in filteredUsers" :key="user.id" class="account-card">
            <div class="account-top">
              <div class="avatar" :style="avatarStyle(user.userName)">{{ initials(user.userName) }}</div>
              <div class="user-main"><div class="name-row"><h3>{{ user.userName }}</h3><span class="badge" :class="isAdminUser(user)?'admin':'user'">{{ isAdminUser(user)?'Admin':'User' }}</span></div><span class="email">{{ user.email || 'Email не указан' }}</span></div>
              <div class="menu-wrap">
                <button class="more" @click="toggleMenu(user.id)">⋯</button>
                <div v-if="openMenuId===user.id" class="menu">
                  <button @click="openEditModal(user)">✎ Изменить</button>
                  <button @click="openPasswordModal(user)">⌁ Сбросить пароль</button>
                  <div></div><button class="danger" @click="openDeleteModal(user)">⌫ Удалить</button>
                </div>
              </div>
            </div>
            <div class="account-meta">
              <div><span>Роли</span><div class="chips"><b v-for="r in user.roles||[]" :key="r" :class="{admin:r==='Admin'}">{{ r }}</b><b v-if="!user.roles?.length" class="muted">Нет ролей</b></div></div>
              <div><span>Права</span><strong>{{ userPermissionCount(user.id) }}</strong></div>
            </div>
            <div class="account-footer">
              <div class="rights-mini"><span>◉ {{ userViewCount(user.id) }}</span><span>✎ {{ userEditCount(user.id) }}</span><span>⌫ {{ userDeleteCount(user.id) }}</span></div>
              <button class="link-btn" @click="openUserPermissions(user.id)">Настроить доступ →</button>
            </div>
          </article>
        </div>
      </template>

      <template v-else>
        <div class="permissions-shell">
          <div class="user-picker panel-inset">
            <div class="picker-head"><div><span class="caption">ПОЛЬЗОВАТЕЛИ</span><h3>Выберите пользователя</h3></div><span class="count-pill">{{ filteredPermissionUsers.length }}</span></div>
            <div class="picker-scroll">
              <button v-for="user in filteredPermissionUsers" :key="user.id" class="user-card" :class="{active:selectedUserId===user.id}" @click="selectUser(user.id)">
                <span class="avatar small" :style="avatarStyle(user.userName)">{{ initials(user.userName) }}</span>
                <span class="user-card-copy"><strong>{{ user.userName }}</strong><small>{{ user.email || 'Без email' }}</small></span>
                <span class="user-count">{{ userPermissionCount(user.id) }}</span>
              </button>
            </div>
          </div>

          <div v-if="selectedUser" class="permission-workspace">
            <div class="selected-user panel-inset">
              <div class="selected-main"><div class="avatar large" :style="avatarStyle(selectedUser.userName)">{{ initials(selectedUser.userName) }}</div><div><span class="caption">АКТИВНЫЙ ПОЛЬЗОВАТЕЛЬ</span><h3>{{ selectedUser.userName }}</h3><small>{{ selectedUser.email || 'Email не указан' }}</small></div></div>
              <div class="selected-stats"><div><b>{{ selectedStats.view }}</b><span>просмотр</span></div><div><b>{{ selectedStats.edit }}</b><span>изменение</span></div><div><b>{{ selectedStats.delete }}</b><span>удаление</span></div></div>
            </div>

            <div class="bulk panel-inset">
              <div><span class="caption">МАССОВОЕ НАЗНАЧЕНИЕ</span><strong>Применить права к выбранным таблицам</strong></div>
              <div class="preset-row"><button :class="{active:bulkPreset==='view'}" @click="setPreset('view')">◉ Просмотр</button><button :class="{active:bulkPreset==='edit'}" @click="setPreset('edit')">✎ Просмотр + изменение</button><button :class="{active:bulkPreset==='full'}" @click="setPreset('full')">✓ Полный доступ</button></div>
              <div class="bulk-footer"><span>Выбрано: <b>{{ selectedTableIds.length }}</b></span><div><button class="ui-btn ghost" @click="selectAllTables">Все</button><button class="ui-btn ghost" @click="deselectAllTables">Снять</button><button class="ui-btn primary" :disabled="isApplying||!selectedTableIds.length" @click="applyMassPermissions">{{ isApplying?'Сохранение…':'Применить' }}</button></div></div>
            </div>

            <div class="tables-head"><div><span class="caption">ТАБЛИЦЫ</span><h3>Доступ пользователя</h3></div><div class="segmented"><button :class="{active:tableFilter==='all'}" @click="tableFilter='all'">Все</button><button :class="{active:tableFilter==='assigned'}" @click="tableFilter='assigned'">С доступом</button><button :class="{active:tableFilter==='unassigned'}" @click="tableFilter='unassigned'">Без доступа</button></div></div>
            <div class="table-cards">
              <article v-for="table in displayedTables" :key="table.id" class="perm-card" :class="{selected:selectedTableIds.includes(table.id),dirty:hasDirtyPermission(table.id)}">
                <div class="perm-top"><label class="check"><input type="checkbox" :value="table.id" v-model="selectedTableIds"><i></i></label><div class="table-title"><b>{{ table.name }}</b><span>{{ permissionLabel(table.id) }}</span></div><span class="access-dot" :class="accessClass(table.id)"></span></div>
                <div class="rights-row"><button :class="{enabled:permissionFor(table.id).canView}" @click="togglePermission(table.id,'canView')">◉ Просмотр <em>{{ permissionFor(table.id).canView?'Вкл.':'Выкл.' }}</em></button><button :class="{enabled:permissionFor(table.id).canEdit}" @click="togglePermission(table.id,'canEdit')">✎ Изменение <em>{{ permissionFor(table.id).canEdit?'Вкл.':'Выкл.' }}</em></button><button :class="{enabled:permissionFor(table.id).canDelete}" @click="togglePermission(table.id,'canDelete')">⌫ Удаление <em>{{ permissionFor(table.id).canDelete?'Вкл.':'Выкл.' }}</em></button></div>
                <div v-if="hasDirtyPermission(table.id)" class="dirty-note">● Несохранённые изменения</div>
              </article>
            </div>
          </div>
          <div v-else class="empty"><div class="empty-icon">◌</div><h3>Выберите пользователя</h3><p>Выберите пользователя выше, чтобы настроить доступ к таблицам.</p></div>
        </div>
      </template>
    </section>

    <Teleport to="body"><Transition name="modal"><div v-if="showUserModal" class="modal-backdrop" @click.self="closeUserModal(false)"><div class="modal-card">
      <div class="modal-head"><div><span class="caption">{{ editUserObj?'УЧЕТНАЯ ЗАПИСЬ':'НОВАЯ УЧЕТНАЯ ЗАПИСЬ' }}</span><h2>{{ editUserObj?'Редактирование пользователя':'Добавить пользователя' }}</h2></div><button class="close" @click="closeUserModal(false)">×</button></div>
      <form @submit.prevent="saveUser"><div class="form-grid"><label>Имя пользователя<input v-model.trim="form.userName" :disabled="!!editUserObj||isSaving" required></label><label>Email<input v-model.trim="form.email" type="email" :disabled="isSaving"></label><label v-if="!editUserObj">Пароль<input v-model="form.password" type="password" minlength="6" required></label><label v-else>Новый пароль<input v-model="form.newPassword" type="password" minlength="6"></label></div><div class="role-options"><label :class="{selected:form.roles.includes('User')}"><input v-model="form.roles" type="checkbox" value="User"> <span>User</span><small>Обычный пользователь</small></label><label :class="{selected:form.roles.includes('Admin')}"><input v-model="form.roles" type="checkbox" value="Admin"> <span>Admin</span><small>Полный доступ</small></label></div><div class="modal-footer"><button type="button" class="ui-btn ghost" @click="closeUserModal(false)">Отмена</button><button class="ui-btn primary" :disabled="isSaving">{{ isSaving?'Сохранение…':'Сохранить' }}</button></div></form>
    </div></div></Transition></Teleport>

    <Teleport to="body"><Transition name="modal"><div v-if="showPasswordModal" class="modal-backdrop" @click.self="closePasswordModal"><div class="modal-card small"><div class="modal-head"><div><span class="caption">БЕЗОПАСНОСТЬ</span><h2>Сброс пароля</h2></div><button class="close" @click="closePasswordModal">×</button></div><div class="selected-user mini"><div class="avatar" :style="avatarStyle(passwordUser?.userName)">{{ initials(passwordUser?.userName) }}</div><div><b>{{ passwordUser?.userName }}</b><small>Новый пароль будет применён сразу.</small></div></div><label>Новый пароль<input v-model="newPassword" type="password" minlength="6" @keyup.enter="resetPassword"></label><div class="modal-footer"><button class="ui-btn ghost" @click="closePasswordModal">Отмена</button><button class="ui-btn primary" :disabled="isResettingPassword||newPassword.length<6" @click="resetPassword">{{ isResettingPassword?'Сохранение…':'Изменить пароль' }}</button></div></div></div></Transition></Teleport>

    <Teleport to="body"><Transition name="modal"><div v-if="showDeleteModal" class="modal-backdrop" @click.self="closeDeleteModal"><div class="modal-card small"><div class="danger-icon">⌫</div><span class="caption">УДАЛЕНИЕ</span><h2>Удалить учетную запись?</h2><p class="delete-copy">Пользователь <b>{{ deleteUserTarget?.userName }}</b> будет удалён без возможности восстановления.</p><div class="modal-footer"><button class="ui-btn ghost" @click="closeDeleteModal">Отмена</button><button class="ui-btn danger" :disabled="isDeleting" @click="deleteUser">{{ isDeleting?'Удаление…':'Удалить' }}</button></div></div></div></Transition></Teleport>

    <Transition name="toast"><div v-if="toast.visible" class="toast" :class="toast.type"><b>{{ toast.type==='success'?'✓':'!' }}</b><span>{{ toast.message }}</span></div></Transition>
  </div>
</template>

<script setup>
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAdminStore } from '../stores/admin'
import { useTablesStore } from '../stores/tables'
import { permissionsApi } from '../api'
import apiClient from '../api'

const adminStore=useAdminStore(); const tablesStore=useTablesStore(); const router=useRouter(); const route=useRoute()
const activeTab=computed(()=>route.query.tab==='permissions'?'permissions':'accounts')
const searchQuery=ref(''); const roleFilter=ref('all'); const openMenuId=ref(null); const isRefreshing=ref(false)
const showUserModal=ref(false); const editUserObj=ref(null); const isSaving=ref(false)
const showPasswordModal=ref(false); const passwordUser=ref(null); const newPassword=ref(''); const isResettingPassword=ref(false)
const showDeleteModal=ref(false); const deleteUserTarget=ref(null); const isDeleting=ref(false)
const selectedUserId=ref(''); const selectedTableIds=ref([]); const tableFilter=ref('all'); const bulkPreset=ref('view'); const isApplying=ref(false); const dirtyPermissions=reactive({})
const toast=reactive({visible:false,message:'',type:'success'}); let toastTimer=null
const form=reactive({userName:'',email:'',password:'',newPassword:'',roles:[]}); const initialForm=ref(null)
const filteredUsers=computed(()=>{const q=searchQuery.value.trim().toLowerCase(); let a=[...adminStore.users]; if(roleFilter.value==='admin') a=a.filter(isAdminUser); if(roleFilter.value==='user') a=a.filter(u=>!isAdminUser(u)); if(!q) return a; return a.filter(u=>[u.userName,u.email,...(u.roles||[])].filter(Boolean).some(v=>String(v).toLowerCase().includes(q)))})
const filteredPermissionUsers=computed(()=>{const q=searchQuery.value.trim().toLowerCase(); return !q?adminStore.users:adminStore.users.filter(u=>[u.userName,u.email,...(u.roles||[])].filter(Boolean).some(v=>String(v).toLowerCase().includes(q)))})
const selectedUser=computed(()=>adminStore.users.find(u=>String(u.id)===String(selectedUserId.value))||null)
const administratorCount=computed(()=>adminStore.users.filter(isAdminUser).length)
const usersWithPermissionsCount=computed(()=>adminStore.users.filter(u=>adminStore.permissions.some(p=>p.userId===u.id&&(p.canView||p.canEdit||p.canDelete))).length)
const selectedStats=computed(()=>{if(!selectedUser.value)return {view:0,edit:0,delete:0}; const p=adminStore.permissions.filter(x=>x.userId===selectedUser.value.id); return {view:p.filter(x=>x.canView).length,edit:p.filter(x=>x.canEdit).length,delete:p.filter(x=>x.canDelete).length}})
const displayedTables=computed(()=>{if(!selectedUser.value)return []; const q=searchQuery.value.trim().toLowerCase(); return tablesStore.tables.filter(t=>{if(q&&!String(t.name||'').toLowerCase().includes(q))return false; const p=permissionFor(t.id); const a=p.canView||p.canEdit||p.canDelete; return tableFilter.value==='assigned'?a:tableFilter.value==='unassigned'?!a:true})})

function isAdminUser(u){return (u.roles||[]).some(r=>String(r).toLowerCase()==='admin')}
function userPermissionCount(id){return adminStore.permissions.filter(p=>p.userId===id&&(p.canView||p.canEdit||p.canDelete)).length}
function userViewCount(id){return adminStore.permissions.filter(p=>p.userId===id&&p.canView).length}
function userEditCount(id){return adminStore.permissions.filter(p=>p.userId===id&&p.canEdit).length}
function userDeleteCount(id){return adminStore.permissions.filter(p=>p.userId===id&&p.canDelete).length}
function initials(v=''){const p=String(v).trim().split(/\s+/).filter(Boolean); return p.length>1?`${p[0][0]}${p[1][0]}`.toUpperCase():(p[0]||'U').slice(0,2).toUpperCase()}
function avatarStyle(v=''){const c=['linear-gradient(135deg,#4f46e5,#7c3aed)','linear-gradient(135deg,#2563eb,#0891b2)','linear-gradient(135deg,#0891b2,#0f766e)','linear-gradient(135deg,#7c3aed,#db2777)','linear-gradient(135deg,#0f766e,#2563eb)']; let h=0; for(let i=0;i<v.length;i++)h=(h<<5)-h+v.charCodeAt(i); return {background:c[Math.abs(h)%c.length]}}
function switchTab(tab){router.push({name:'adminUsers',query:tab==='permissions'?{tab:'permissions'}:{}})}
function openUserPermissions(id){selectedUserId.value=id; router.push({name:'adminUsers',query:{tab:'permissions',user:id}})}
function toggleMenu(id){openMenuId.value=openMenuId.value===id?null:id}
function resetForm(){Object.assign(form,{userName:'',email:'',password:'',newPassword:'',roles:[]})}
function openCreateModal(){resetForm();editUserObj.value=null;initialForm.value=JSON.parse(JSON.stringify(form));showUserModal.value=true;openMenuId.value=null}
function openEditModal(u){editUserObj.value=u;Object.assign(form,{userName:u.userName||'',email:u.email||'',password:'',newPassword:'',roles:[...(u.roles||[])]});initialForm.value=JSON.parse(JSON.stringify(form));showUserModal.value=true;openMenuId.value=null}
function closeUserModal(force=false){if(!force&&initialForm.value&&JSON.stringify(form)!==JSON.stringify(initialForm.value)&&!confirm('Есть несохранённые изменения. Закрыть окно?'))return;showUserModal.value=false;editUserObj.value=null;initialForm.value=null;resetForm()}
function openPasswordModal(u){passwordUser.value=u;newPassword.value='';showPasswordModal.value=true;openMenuId.value=null}
function closePasswordModal(){if(isResettingPassword.value)return;showPasswordModal.value=false;passwordUser.value=null;newPassword.value=''}
function openDeleteModal(u){deleteUserTarget.value=u;showDeleteModal.value=true;openMenuId.value=null}
function closeDeleteModal(){if(isDeleting.value)return;showDeleteModal.value=false;deleteUserTarget.value=null}
function showToast(message,type='success'){toast.message=message;toast.type=type;toast.visible=true;clearTimeout(toastTimer);toastTimer=setTimeout(()=>toast.visible=false,2800)}
function apiError(err,fallback){return typeof err?.response?.data==='string'&&err.response.data?err.response.data:err?.response?.data?.message||err?.message||fallback}
async function saveUser(){if(isSaving.value)return; if(!form.userName.trim()){showToast('Укажите имя пользователя','error');return} if(!editUserObj.value&&form.password.length<6){showToast('Пароль должен содержать минимум 6 символов','error');return} isSaving.value=true; try{if(editUserObj.value){await apiClient.put(`/admin/users/${editUserObj.value.id}`,{email:form.email||undefined,roles:form.roles,newPassword:form.newPassword||undefined});showToast('Учетная запись обновлена')}else{await apiClient.post('/admin/users',{userName:form.userName,email:form.email,password:form.password,roles:form.roles});showToast('Пользователь создан')} closeUserModal(true); await adminStore.loadAdminData(true)}catch(e){showToast(apiError(e,'Не удалось сохранить пользователя'),'error')}finally{isSaving.value=false}}
async function resetPassword(){if(isResettingPassword.value||!passwordUser.value)return; if(newPassword.value.length<6){showToast('Пароль должен содержать минимум 6 символов','error');return} isResettingPassword.value=true; try{await apiClient.post(`/admin/users/${passwordUser.value.id}/reset-password`,{newPassword:newPassword.value});closePasswordModal();showToast('Пароль успешно изменён')}catch(e){showToast(apiError(e,'Не удалось изменить пароль'),'error')}finally{isResettingPassword.value=false}}
async function deleteUser(){if(isDeleting.value||!deleteUserTarget.value)return; isDeleting.value=true; try{const name=deleteUserTarget.value.userName; await apiClient.delete(`/admin/users/${deleteUserTarget.value.id}`); closeDeleteModal(); await adminStore.loadAdminData(true); showToast(`Пользователь ${name} удалён`)}catch(e){showToast(apiError(e,'Не удалось удалить пользователя'),'error')}finally{isDeleting.value=false}}
function permissionFor(tableId){const key=`${selectedUserId.value}:${tableId}`; if(dirtyPermissions[key])return dirtyPermissions[key]; return adminStore.permissions.find(p=>p.userId===selectedUserId.value&&p.tableId===tableId)||{userId:selectedUserId.value,tableId,canView:false,canEdit:false,canDelete:false}}
function hasDirtyPermission(tableId){return !!dirtyPermissions[`${selectedUserId.value}:${tableId}`]}
function selectUser(id){selectedUserId.value=id;selectedTableIds.value=[];bulkPreset.value='view'}
async function togglePermission(tableId,field){const key=`${selectedUserId.value}:${tableId}`; const p={...permissionFor(tableId)}; p[field]=!p[field]; if((field==='canEdit'||field==='canDelete')&&p[field])p.canView=true; dirtyPermissions[key]=p; try{await permissionsApi.set({userId:selectedUserId.value,tableId,canView:p.canView,canEdit:p.canEdit,canDelete:p.canDelete}); delete dirtyPermissions[key]; await adminStore.loadAdminData(true); showToast('Право сохранено')}catch(e){showToast('Не удалось сохранить право','error')}}
function setPreset(p){bulkPreset.value=p}
function selectAllTables(){selectedTableIds.value=displayedTables.value.map(t=>t.id)}
function deselectAllTables(){selectedTableIds.value=[]}
async function applyMassPermissions(){if(isApplying.value||!selectedUser.value||!selectedTableIds.value.length)return; isApplying.value=true; const r={view:{canView:true,canEdit:false,canDelete:false},edit:{canView:true,canEdit:true,canDelete:false},full:{canView:true,canEdit:true,canDelete:true}}[bulkPreset.value]; try{await permissionsApi.batch({userId:selectedUser.value.id,permissions:selectedTableIds.value.map(tableId=>({tableId,...r}))}); selectedTableIds.value=[]; await adminStore.loadAdminData(true); showToast('Права успешно применены')}catch(e){showToast('Не удалось применить права','error')}finally{isApplying.value=false}}
function permissionLabel(id){const p=permissionFor(id);return p.canDelete?'Полный доступ':p.canEdit?'Просмотр и изменение':p.canView?'Только просмотр':'Нет доступа'}
function accessClass(id){const p=permissionFor(id);return p.canDelete?'full':p.canEdit?'edit':p.canView?'view':'none'}
async function refreshData(){if(isRefreshing.value)return;isRefreshing.value=true;try{await Promise.all([adminStore.loadAdminData(true),tablesStore.loadTables(true)]);showToast('Данные обновлены')}catch{showToast('Не удалось обновить данные','error')}finally{isRefreshing.value=false}}
onMounted(async()=>{try{await Promise.all([adminStore.loaded?Promise.resolve():adminStore.loadAdminData(),tablesStore.loaded?Promise.resolve():tablesStore.loadTables()]); if(route.query.user)selectedUserId.value=String(route.query.user); else if(adminStore.users.length&&!selectedUserId.value)selectedUserId.value=adminStore.users[0].id}catch(e){console.error(e)}})
onBeforeUnmount(()=>clearTimeout(toastTimer))
</script>

<style scoped>
.access-page{max-width:1700px;margin:0 auto;padding:10px 4px 36px;color:var(--app-text)}
.panel,.panel-inset{border:1px solid var(--app-border);background:var(--app-surface);box-shadow:var(--app-shadow);backdrop-filter:blur(20px) saturate(120%);-webkit-backdrop-filter:blur(20px) saturate(120%)}
.access-hero{min-height:140px;padding:28px 30px;border-radius:26px;display:flex;align-items:center;justify-content:space-between;gap:20px}.hero-copy h1{margin:4px 0 0;font-size:30px;letter-spacing:-.035em}.hero-copy p{margin:8px 0 0;color:var(--app-muted);font-size:13px}.eyebrow,.caption{font-size:10px;font-weight:800;letter-spacing:.15em;text-transform:uppercase;color:var(--app-primary)}.eyebrow{display:flex;align-items:center;gap:7px}.eyebrow i{width:7px;height:7px;border-radius:50%;background:var(--app-primary);box-shadow:0 0 10px rgba(99,102,241,.45)}
.stats-grid{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:12px;margin:12px 0}.stat{min-height:88px;padding:15px;border-radius:20px;display:flex;align-items:center;gap:12px}.stat>div:last-child{min-width:0}.stat span{display:block;color:var(--app-muted);font-size:11px}.stat strong{display:block;margin-top:2px;font-size:21px}.stat-icon{width:42px;height:42px;border-radius:13px;display:grid;place-items:center;background:var(--app-surface-strong);font-size:17px}.stat-icon.blue{color:#4f8cff}.stat-icon.violet{color:#8b6cff}.stat-icon.cyan{color:#06a6c7}.stat-icon.emerald{color:#0eaa78}
.workspace{border-radius:26px;overflow:hidden}.workspace-head{padding:18px 20px;display:flex;align-items:center;justify-content:space-between;gap:18px;border-bottom:1px solid var(--app-border)}.workspace-head h2{margin:3px 0 0;font-size:18px}.tabs,.segmented{display:flex;gap:4px;padding:4px;border:1px solid var(--app-border);border-radius:13px;background:var(--app-surface-strong)}.tab,.segmented button{border:0;border-radius:9px;background:transparent;color:var(--app-muted);padding:9px 12px;font-size:11px;cursor:pointer}.tab.active,.segmented button.active{background:rgba(99,102,241,.14);color:var(--app-text);box-shadow:inset 0 1px 0 rgba(255,255,255,.05)}
.toolbar{padding:16px 20px;display:flex;align-items:center;justify-content:space-between;gap:12px}.search-wrap{position:relative;flex:1;max-width:620px}.search-wrap>span{position:absolute;left:13px;top:10px;color:var(--app-muted)}.search-wrap input{width:100%;height:42px;border:1px solid var(--app-border);border-radius:13px;background:var(--app-surface-strong);color:var(--app-text);padding:0 40px;outline:none}.search-wrap input::placeholder{color:var(--app-muted)}.search-wrap>button{position:absolute;right:7px;top:6px;width:30px;height:30px;border:0;border-radius:8px;background:transparent;color:var(--app-muted);cursor:pointer}.toolbar-actions{display:flex;gap:8px;align-items:center}
.ui-btn{height:38px;padding:0 13px;border-radius:11px;border:1px solid var(--app-border);background:var(--app-surface-strong);color:var(--app-text);font-size:11px;font-weight:650;cursor:pointer}.ui-btn.primary{color:#fff;border-color:transparent;background:linear-gradient(135deg,#4f46e5,#6366f1);box-shadow:0 8px 20px rgba(79,70,229,.18)}.ui-btn.ghost:hover{background:rgba(99,102,241,.08)}.ui-btn.danger{color:#dc2626;background:rgba(239,68,68,.08);border-color:rgba(239,68,68,.15)}.ui-btn:disabled{opacity:.5;cursor:not-allowed}.icon{width:16px;height:16px;vertical-align:-3px;margin-right:6px}.spin{animation:spin .8s linear infinite}@keyframes spin{to{transform:rotate(360deg)}}
.users-grid{padding:0 20px 20px;display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:11px}.account-card{padding:15px;border:1px solid var(--app-border);background:color-mix(in srgb,var(--app-surface) 70%,transparent);border-radius:18px}.account-card:hover{border-color:rgba(99,102,241,.18)}.account-top{display:flex;align-items:center;gap:11px}.avatar{width:46px;height:46px;border-radius:14px;display:grid;place-items:center;color:#fff;font-weight:800;box-shadow:inset 0 1px 0 rgba(255,255,255,.2),0 8px 18px rgba(15,23,42,.13);flex:none}.avatar.small{width:38px;height:38px;border-radius:12px;font-size:12px}.avatar.large{width:50px;height:50px}.user-main{min-width:0;flex:1}.name-row{display:flex;gap:7px;align-items:center}.name-row h3{margin:0;font-size:13px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}.email{display:block;margin-top:3px;color:var(--app-muted);font-size:10px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}.badge,.chips b{border-radius:999px;padding:4px 7px;font-size:8px;font-weight:800}.badge.admin,.chips b.admin{color:#7c3aed;background:rgba(139,92,246,.1)}.badge.user{color:#2563eb;background:rgba(59,130,246,.1)}.menu-wrap{position:relative}.more{width:31px;height:31px;border:1px solid var(--app-border);border-radius:9px;background:transparent;color:var(--app-muted);cursor:pointer}.menu{position:absolute;right:0;top:36px;z-index:30;width:185px;padding:6px;border-radius:12px;border:1px solid var(--app-border);background:var(--app-surface-strong);box-shadow:0 18px 45px rgba(15,23,42,.2)}.menu button{width:100%;height:34px;text-align:left;padding:0 9px;border:0;border-radius:8px;background:transparent;color:var(--app-text);font-size:10px;cursor:pointer}.menu button:hover{background:rgba(99,102,241,.08)}.menu div{height:1px;background:var(--app-border);margin:5px}.menu .danger{color:#dc2626}
.account-meta{display:grid;grid-template-columns:1.3fr .5fr;gap:12px;margin-top:15px;padding-top:12px;border-top:1px solid var(--app-border)}.account-meta>div>span{display:block;color:var(--app-muted);font-size:9px;text-transform:uppercase;letter-spacing:.08em;margin-bottom:6px}.chips{display:flex;gap:5px;flex-wrap:wrap}.chips b{color:#4f46e5;background:rgba(99,102,241,.08);border:1px solid rgba(99,102,241,.12)}.chips b.muted{color:var(--app-muted);background:var(--app-surface-strong)}.account-footer{display:flex;justify-content:space-between;align-items:center;gap:12px;margin-top:13px}.rights-mini{display:flex;gap:9px;color:var(--app-muted);font-size:10px}.link-btn{border:0;background:transparent;color:var(--app-primary);font-size:10px;cursor:pointer}.skeleton{min-height:155px;background:linear-gradient(90deg,var(--app-surface),var(--app-surface-strong),var(--app-surface));background-size:200% 100%;animation:shimmer 1.6s linear infinite}@keyframes shimmer{to{background-position:-200% 0}}
.permissions-shell{padding:16px 20px 20px}.user-picker{padding:14px;border-radius:18px}.picker-head,.selected-user,.bulk-footer,.tables-head{display:flex;justify-content:space-between;align-items:center;gap:12px}.picker-head h3,.tables-head h3{margin:3px 0 0;font-size:16px}.count-pill{min-width:28px;padding:4px 8px;border-radius:999px;background:var(--app-surface-strong);color:var(--app-muted);font-size:10px;text-align:center}.picker-scroll{display:flex;gap:8px;overflow-x:auto;padding:10px 2px 2px;scrollbar-width:thin}.user-card{flex:0 0 220px;display:flex;align-items:center;gap:9px;padding:9px;border:1px solid var(--app-border);background:var(--app-surface-strong);border-radius:13px;color:var(--app-text);text-align:left;cursor:pointer}.user-card.active{border-color:rgba(99,102,241,.3);background:linear-gradient(135deg,rgba(99,102,241,.13),rgba(99,102,241,.04))}.user-card-copy{min-width:0;flex:1}.user-card-copy strong,.user-card-copy small{display:block;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}.user-card-copy strong{font-size:11px}.user-card-copy small{margin-top:2px;color:var(--app-muted);font-size:9px}.user-count{min-width:24px;padding:4px 6px;border-radius:999px;background:rgba(99,102,241,.08);color:var(--app-primary);font-size:9px;text-align:center}.permission-workspace{margin-top:12px}.selected-user{padding:15px;border-radius:17px}.selected-main{display:flex;align-items:center;gap:11px}.selected-main h3{margin:3px 0 1px;font-size:17px}.selected-main small{color:var(--app-muted);font-size:10px}.selected-stats{display:flex;gap:20px}.selected-stats b,.selected-stats span{display:block}.selected-stats b{font-size:18px}.selected-stats span{font-size:9px;color:var(--app-muted)}.bulk{margin-top:11px;padding:14px;border-radius:17px;background:rgba(99,102,241,.05)}.bulk strong{display:block;margin-top:3px;font-size:12px}.preset-row{display:grid;grid-template-columns:repeat(3,1fr);gap:7px;margin-top:12px}.preset-row button{min-height:38px;border:1px solid var(--app-border);border-radius:11px;background:var(--app-surface-strong);color:var(--app-muted);cursor:pointer;font-size:10px}.preset-row button.active{background:rgba(99,102,241,.13);border-color:rgba(99,102,241,.22);color:var(--app-text)}.bulk-footer{margin-top:12px;color:var(--app-muted);font-size:10px}.bulk-footer b{color:var(--app-text)}.bulk-footer>div{display:flex;gap:6px}.tables-head{margin:18px 0 10px}.table-cards{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:9px}.perm-card{padding:12px;border:1px solid var(--app-border);background:var(--app-surface-strong);border-radius:16px}.perm-card.selected{border-color:rgba(99,102,241,.26);background:rgba(99,102,241,.05)}.perm-card.dirty{box-shadow:inset 3px 0 #f59e0b}.perm-top{display:flex;align-items:center;gap:8px}.check{width:18px;height:18px;position:relative;flex:none}.check input{position:absolute;opacity:0}.check i{display:block;width:18px;height:18px;border-radius:6px;border:1px solid var(--app-border);background:var(--app-surface)}.check input:checked+i{background:#6366f1;border-color:#818cf8}.check input:checked+i:after{content:'✓';display:grid;place-items:center;height:100%;color:white;font-size:10px;font-weight:800}.table-title{min-width:0;flex:1}.table-title b,.table-title span{display:block;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}.table-title b{font-size:11px}.table-title span{margin-top:2px;color:var(--app-muted);font-size:9px}.access-dot{width:7px;height:7px;border-radius:50%;flex:none}.access-dot.none{background:#94a3b8}.access-dot.view{background:#06b6d4}.access-dot.edit{background:#8b5cf6}.access-dot.full{background:#10b981}.rights-row{display:grid;grid-template-columns:repeat(3,1fr);gap:5px;margin-top:9px}.rights-row button{min-height:34px;padding:0 6px;border:1px solid var(--app-border);border-radius:9px;background:transparent;color:var(--app-muted);font-size:9px;cursor:pointer;text-align:left}.rights-row button.enabled{background:rgba(99,102,241,.08);border-color:rgba(99,102,241,.16);color:var(--app-text)}.rights-row em{display:block;margin-top:2px;color:var(--app-muted);font-size:8px;font-style:normal}.dirty-note{margin-top:7px;color:#b45309;font-size:9px}.empty{padding:50px 20px;text-align:center;color:var(--app-muted)}.empty-icon{width:54px;height:54px;margin:0 auto 12px;display:grid;place-items:center;border-radius:16px;background:var(--app-surface-strong);border:1px solid var(--app-border);font-size:23px;color:var(--app-primary)}.empty h3{margin:0 0 5px;color:var(--app-text);font-size:15px}.empty p{margin:0;font-size:11px}
.modal-backdrop{position:fixed;inset:0;z-index:999;display:flex;align-items:center;justify-content:center;padding:20px;background:rgba(2,6,23,.56);backdrop-filter:blur(10px)}.modal-card{width:min(560px,100%);max-height:90vh;overflow:auto;padding:22px;border-radius:22px;background:var(--app-surface-strong);border:1px solid var(--app-border-strong);box-shadow:0 30px 90px rgba(2,6,23,.35)}.modal-card.small{width:min(440px,100%)}.modal-head{display:flex;justify-content:space-between;gap:12px;margin-bottom:18px}.modal-head h2{margin:4px 0 0;font-size:19px}.close{width:32px;height:32px;border:1px solid var(--app-border);border-radius:9px;background:transparent;color:var(--app-muted);font-size:18px;cursor:pointer}.form-grid{display:grid;gap:13px}.form-grid label,.modal-card>label{display:grid;gap:6px;color:var(--app-muted);font-size:10px}.form-grid input,.modal-card>label input{width:100%;height:41px;padding:0 11px;border-radius:11px;border:1px solid var(--app-border);background:var(--app-surface);color:var(--app-text);outline:none}.role-options{display:grid;grid-template-columns:1fr 1fr;gap:8px;margin-top:14px}.role-options label{padding:11px;border-radius:13px;border:1px solid var(--app-border);background:var(--app-surface);cursor:pointer}.role-options label.selected{border-color:rgba(99,102,241,.22);background:rgba(99,102,241,.08)}.role-options input{display:none}.role-options span{font-size:11px;font-weight:700;color:var(--app-text)}.role-options small{display:block;margin-top:3px;color:var(--app-muted);font-size:9px}.modal-footer{display:flex;justify-content:flex-end;gap:7px;margin-top:20px;padding-top:14px;border-top:1px solid var(--app-border)}.selected-user.mini{justify-content:flex-start;margin-bottom:15px}.selected-user.mini small{display:block;margin-top:2px;color:var(--app-muted);font-size:9px}.danger-icon{width:52px;height:52px;border-radius:16px;display:grid;place-items:center;color:#dc2626;background:rgba(239,68,68,.09);margin-bottom:14px}.delete-copy{color:var(--app-muted);font-size:11px;line-height:1.6}.delete-copy b{color:var(--app-text)}.toast{position:fixed;right:22px;bottom:22px;z-index:1100;display:flex;align-items:center;gap:9px;padding:11px 13px;border-radius:13px;border:1px solid var(--app-border);background:var(--app-surface-strong);box-shadow:0 18px 50px rgba(15,23,42,.18);font-size:11px}.toast b{width:23px;height:23px;display:grid;place-items:center;border-radius:7px;background:rgba(255,255,255,.06)}.toast.success b{color:#10b981}.toast.error b{color:#ef4444}.modal-enter-active,.modal-leave-active,.toast-enter-active,.toast-leave-active{transition:.2s ease}.modal-enter-from,.modal-leave-to,.toast-enter-from,.toast-leave-to{opacity:0}.modal-enter-from .modal-card,.modal-leave-to .modal-card{transform:translateY(8px) scale(.985)}
@media(max-width:1200px){.stats-grid{grid-template-columns:repeat(2,1fr)}.users-grid,.table-cards{grid-template-columns:1fr}}
@media(max-width:900px){.workspace-head,.toolbar,.selected-user{align-items:flex-start;flex-direction:column}.tabs,.toolbar-actions{width:100%}.tabs .tab{flex:1}.search-wrap{max-width:none;width:100%}.toolbar-actions{justify-content:space-between}.selected-stats{width:100%;justify-content:space-between}.preset-row{grid-template-columns:1fr}.bulk-footer{align-items:flex-start;flex-direction:column}.bulk-footer>div{width:100%;flex-wrap:wrap}}
@media(max-width:640px){.access-page{padding:4px 0 26px}.access-hero{padding:21px 18px;border-radius:21px}.hero-copy h1{font-size:25px}.stats-grid{grid-template-columns:1fr 1fr}.stat{min-height:74px;padding:11px}.stat-icon{width:36px;height:36px}.users-grid,.permissions-shell{padding-left:12px;padding-right:12px}.account-footer{align-items:flex-start;flex-direction:column}.link-btn{width:100%;text-align:left}.role-options{grid-template-columns:1fr}.picker-scroll{margin-inline:-2px}.user-card{flex-basis:205px}.rights-row{grid-template-columns:1fr}.modal-card{padding:18px;border-radius:20px}}
</style>
