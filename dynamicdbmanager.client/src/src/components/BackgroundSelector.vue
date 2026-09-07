<template>
  <div class="bg-control" ref="root">
    <button type="button" class="bg-button" title="Сменить фон" aria-label="Сменить фон" @click.stop="toggle">
      <span class="current-preview" :class="`current-${bgStore.type}`" aria-hidden="true">
        <span v-if="bgStore.type==='galaxy'||bgStore.type==='nebula3d'||bgStore.type==='aurora'" class="current-stars"></span>
        <span v-else-if="bgStore.type==='gradient'" class="current-gradient"></span>
        <span v-else-if="bgStore.type==='particles'" class="current-particles"><i></i><i></i><i></i></span>
        <span v-else-if="bgStore.type==='cybergrid'" class="current-grid"></span>
        <span v-else class="current-black"></span>
      </span>
      <svg class="bg-button-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true"><path d="M4 6h16M4 12h16M4 18h16" stroke-width="1.7" stroke-linecap="round"/><path d="M8 4v4M16 10v4M8 16v4" stroke-width="1.7" stroke-linecap="round"/></svg>
    </button>

    <Transition name="bg-menu">
      <div v-if="open" class="bg-menu" @click.stop>
        <div class="bg-menu-title">Фон интерфейса</div>
        <button v-for="item in backgrounds" :key="item.id" type="button" class="bg-option" :class="{active:bgStore.type===item.id}" @click="select(item.id)">
          <span class="bg-option-preview">
            <span v-if="item.id==='galaxy'" class="preview-fill preview-galaxy-fill"><span class="preview-stars"></span></span>
            <span v-else-if="item.id==='gradient'" class="preview-fill preview-gradient-fill"></span>
            <span v-else-if="item.id==='particles'" class="preview-fill preview-particles-fill"><span class="preview-particles-dots"><i></i><i></i><i></i></span></span>
            <span v-else-if="item.id==='aurora'" class="preview-fill preview-aurora-fill"></span>
            <span v-else-if="item.id==='nebula3d'" class="preview-fill preview-nebula-fill"></span>
            <span v-else-if="item.id==='cybergrid'" class="preview-fill preview-grid-fill"></span>
            <span v-else class="preview-fill preview-black-fill"></span>
          </span>
          <span class="bg-option-main"><strong>{{item.name}}</strong><small>{{item.description}}</small></span>
          <svg v-if="bgStore.type===item.id" class="selected-mark" viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="m5 12 4 4L19 6" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/></svg>
        </button>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import {onBeforeUnmount,onMounted,ref} from 'vue'
import {useBackgroundStore} from '../stores/background'
const bgStore=useBackgroundStore(); const open=ref(false); const root=ref(null)
const backgrounds=[
 {id:'galaxy',name:'Галактика',description:'3D спиральная галактика'},
 {id:'nebula3d',name:'Туманность',description:'3D облако и звёзды'},
 {id:'aurora',name:'Aurora',description:'2D северное сияние'},
 {id:'gradient',name:'Aurora Gradient',description:'Современный mesh-градиент'},
 {id:'particles',name:'Частицы',description:'Круглые светящиеся частицы'},
 {id:'cybergrid',name:'Cyber Grid',description:'Неоновая 2.5D сетка'},
 {id:'black',name:'Чёрная',description:'Минимальная нагрузка'}
]
function toggle(){open.value=!open.value} function select(type){bgStore.setType(type);open.value=false} function onDocumentClick(e){if(!root.value?.contains(e.target))open.value=false}
onMounted(()=>document.addEventListener('click',onDocumentClick)); onBeforeUnmount(()=>document.removeEventListener('click',onDocumentClick))
</script>

<style scoped>
.bg-control{position:relative}.bg-button{display:inline-flex;align-items:center;justify-content:center;gap:6px;width:50px;height:38px;padding:0;border:1px solid rgba(255,255,255,.1);border-radius:12px;background:rgba(8,10,16,.42);backdrop-filter:blur(14px);-webkit-backdrop-filter:blur(14px);color:#94a3b8;cursor:pointer;transition:.18s;box-shadow:inset 0 1px rgba(255,255,255,.04)}.bg-button:hover{background:rgba(17,22,34,.62);color:#e2e8f0;border-color:rgba(129,140,248,.22)}.bg-button-icon{width:16px!important;height:16px!important;flex:none}.current-preview{position:relative;display:block;width:17px;height:17px;overflow:hidden;border-radius:5px;border:1px solid rgba(255,255,255,.12);background:#000;flex:none}.current-galaxy,.current-nebula3d,.current-aurora{background:radial-gradient(circle at 50% 44%,#c4b5fd 0 12%,#4c1d95 36%,#07101b 76%)}.current-stars{position:absolute;inset:0;background:transparent;box-shadow:3px 4px 0 #fff,11px 7px 0 #67e8f9,8px 2px 0 #f472b6}.current-gradient{position:absolute;inset:0;background:linear-gradient(135deg,#7c3aed,#06b6d4,#0f172a)}.current-particles{position:absolute;inset:0;display:flex;align-items:center;justify-content:center;gap:2px}.current-particles i{width:2px;height:2px;border-radius:50%;background:#bfe7ff;box-shadow:0 0 4px #67e8f9}.current-grid{position:absolute;inset:0;background:linear-gradient(rgba(56,189,248,.25) 1px,transparent 1px),linear-gradient(90deg,rgba(139,92,246,.25) 1px,transparent 1px);background-size:5px 5px;transform:perspective(10px) rotateX(45deg)}.current-black{position:absolute;inset:0;background:#000}.bg-menu{position:absolute;top:calc(100% + 8px);right:0;width:278px;padding:9px;border:1px solid rgba(255,255,255,.1);border-radius:17px;background:rgba(7,9,13,.84);backdrop-filter:blur(24px) saturate(130%);-webkit-backdrop-filter:blur(24px) saturate(130%);box-shadow:0 20px 60px rgba(0,0,0,.58),inset 0 1px rgba(255,255,255,.04)}.bg-menu-title{padding:5px 8px 8px;color:#64748b;font-size:10px;font-weight:700;text-transform:uppercase;letter-spacing:.12em}.bg-option{display:flex;align-items:center;gap:10px;width:100%;padding:8px;border:1px solid transparent;border-radius:12px;background:transparent;color:#cbd5e1;text-align:left;cursor:pointer;transition:.18s}.bg-option:hover{background:rgba(255,255,255,.045);border-color:rgba(255,255,255,.06)}.bg-option.active{background:rgba(99,102,241,.09);border-color:rgba(129,140,248,.18)}.bg-option-preview{position:relative;display:block;overflow:hidden;width:44px;height:32px;flex:none;border-radius:9px;border:1px solid rgba(255,255,255,.08);background:#000}.preview-fill{position:absolute;inset:0}.preview-galaxy-fill{background:radial-gradient(circle at 50% 50%,#c4b5fd 0 7%,#4c1d95 25%,#09101b 72%)}.preview-stars{position:absolute;width:3px;height:3px;top:13px;left:20px;border-radius:50%;background:#fff;box-shadow:-11px -5px 0 #67e8f9,10px 6px 0 #f472b6,-6px 8px 0 #fff}.preview-gradient-fill{background:radial-gradient(circle at 20% 20%,#8b5cf6,transparent 43%),radial-gradient(circle at 80% 20%,#22d3ee,transparent 42%),linear-gradient(135deg,#0f172a,#312e81)}.preview-particles-fill{background:radial-gradient(circle,#07101b,#03050a)}.preview-particles-dots{position:absolute;inset:0;display:flex;justify-content:center;align-items:center;gap:4px}.preview-particles-dots i{width:4px;height:4px;border-radius:50%;background:#bfe7ff;box-shadow:0 0 7px #67e8f9}.preview-aurora-fill{background:radial-gradient(ellipse at 20% 75%,rgba(16,185,129,.7),transparent 38%),radial-gradient(ellipse at 75% 35%,rgba(99,102,241,.75),transparent 45%),#050711}.preview-nebula-fill{background:radial-gradient(circle at 40% 45%,rgba(34,211,238,.55),transparent 18%),radial-gradient(circle at 65% 55%,rgba(168,85,247,.5),transparent 30%),#050712}.preview-grid-fill{background:linear-gradient(rgba(56,189,248,.28) 1px,transparent 1px),linear-gradient(90deg,rgba(139,92,246,.28) 1px,transparent 1px),#04070d;background-size:7px 7px;transform:skewY(-8deg) scale(1.25)}.preview-black-fill{background:#000}.bg-option-main{display:flex;min-width:0;flex:1;flex-direction:column;gap:2px}.bg-option-main strong{font-size:12px;color:#e2e8f0}.bg-option-main small{font-size:10px;color:#64748b}.selected-mark{width:16px;height:16px;color:#a5b4fc;flex:none}.bg-menu-enter-active,.bg-menu-leave-active{transition:opacity .14s ease,transform .14s ease}.bg-menu-enter-from,.bg-menu-leave-to{opacity:0;transform:translateY(-5px) scale(.98)}
</style>
