<template>
  <div class="bg-control" ref="root">
    <button type="button" class="bg-button" title="Сменить фон" aria-label="Сменить фон" @click.stop="toggle">
      <span class="current-preview" :class="`current-${bgStore.type}`" aria-hidden="true">
        <span v-if="bgStore.type==='galaxy'" class="current-art current-galaxy-art"><i></i><i></i><i></i><i></i></span>
        <span v-else-if="bgStore.type==='nebula3d'" class="current-art current-nebula-art"><i></i><i></i><i></i></span>
        <span v-else-if="bgStore.type==='aurora'" class="current-art current-aurora-art"></span>
        <span v-else-if="bgStore.type==='gradient'" class="current-art current-gradient-art"></span>
        <span v-else-if="bgStore.type==='particles'" class="current-art current-particles-art"><i></i><i></i><i></i><i></i><i></i></span>
        <span v-else-if="bgStore.type==='cybergrid'" class="current-art current-cyber-art"></span>
        <span v-else class="current-art current-black-art"></span>
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
import { onBeforeUnmount,onMounted,ref } from 'vue'
import { useBackgroundStore } from '../stores/background'
const bgStore=useBackgroundStore();const open=ref(false);const root=ref(null)
const backgrounds=[
{id:'galaxy',name:'Галактика',description:'3D спиральная галактика'},
{id:'nebula3d',name:'Туманность',description:'3D облако и звёзды'},
{id:'aurora',name:'Aurora',description:'2D северное сияние'},
{id:'gradient',name:'Aurora Gradient',description:'Современный mesh-градиент'},
{id:'particles',name:'Частицы',description:'Круглые светящиеся частицы'},
{id:'cybergrid',name:'Cyber Grid',description:'Чистая неоновая перспектива'},
{id:'black',name:'Чёрная',description:'Минимальная нагрузка'}]
function toggle(){open.value=!open.value} function select(type){bgStore.setType(type);open.value=false} function onDocumentClick(e){if(!root.value?.contains(e.target))open.value=false}
onMounted(()=>document.addEventListener('click',onDocumentClick));onBeforeUnmount(()=>document.removeEventListener('click',onDocumentClick))
</script>
<style scoped>
.bg-control{position:relative}.bg-button{display:inline-flex;align-items:center;justify-content:center;gap:6px;width:58px;height:38px;padding:0;border:1px solid var(--app-border);border-radius:12px;background:var(--app-control);backdrop-filter:blur(14px);-webkit-backdrop-filter:blur(14px);color:var(--app-muted);cursor:pointer;transition:.18s;box-shadow:inset 0 1px var(--app-highlight)}.bg-button:hover{background:var(--app-control-hover);color:var(--app-text);border-color:var(--app-border-strong)}.bg-button-icon{width:16px!important;height:16px!important;flex:none}.current-preview{position:relative;display:block;width:21px;height:21px;overflow:hidden;border-radius:6px;border:1px solid var(--app-border-strong);background:#02040a;flex:none;box-shadow:inset 0 0 8px rgba(255,255,255,.04)}.current-art{position:absolute;inset:0;display:block;overflow:hidden}.current-galaxy-art{background:radial-gradient(circle at 50% 48%,#fff0b2 0 10%,#a78bfa 20%,#4c1d95 42%,#020817 78%)}.current-galaxy-art:before{content:"";position:absolute;inset:-20%;background:conic-gradient(from 15deg,transparent 0 10%,rgba(103,232,249,.5) 16%,transparent 24% 34%,rgba(244,114,182,.38) 41%,transparent 49% 62%,rgba(139,92,246,.38) 69%,transparent 78%);border-radius:50%;transform:rotate(-18deg) scale(.9)}.current-galaxy-art i,.current-nebula-art i{position:absolute;width:2px;height:2px;border-radius:50%;background:#fff;box-shadow:0 0 3px rgba(255,255,255,.9)}.current-galaxy-art i:nth-child(1){left:4px;top:5px}.current-galaxy-art i:nth-child(2){right:4px;top:7px;background:#67e8f9}.current-galaxy-art i:nth-child(3){left:8px;bottom:4px;background:#f472b6}.current-galaxy-art i:nth-child(4){right:7px;bottom:5px}.current-nebula-art{background:radial-gradient(circle at 35% 45%,rgba(34,211,238,.5),transparent 22%),radial-gradient(circle at 68% 58%,rgba(168,85,247,.6),transparent 34%),#050817}.current-nebula-art i:nth-child(1){left:5px;top:4px}.current-nebula-art i:nth-child(2){right:5px;top:8px;background:#67e8f9}.current-nebula-art i:nth-child(3){left:10px;bottom:4px;background:#c4b5fd}.current-aurora-art{background:radial-gradient(ellipse at 22% 82%,rgba(16,185,129,.9),transparent 42%),radial-gradient(ellipse at 72% 20%,rgba(99,102,241,.8),transparent 45%),linear-gradient(160deg,#03101a,#081126 62%,#050812)}.current-gradient-art{background:radial-gradient(circle at 20% 20%,#8b5cf6 0 10%,transparent 47%),radial-gradient(circle at 78% 26%,#06b6d4 0 8%,transparent 50%),linear-gradient(135deg,#0b1020,#24104f 58%,#071827)}.current-particles-art{background:#03060c}.current-particles-art i{position:absolute;width:3px;height:3px;border-radius:50%;background:#dff6ff;box-shadow:0 0 5px #67e8f9}.current-particles-art i:nth-child(1){left:3px;top:5px}.current-particles-art i:nth-child(2){left:9px;top:11px}.current-particles-art i:nth-child(3){right:4px;top:6px}.current-particles-art i:nth-child(4){right:7px;bottom:4px}.current-particles-art i:nth-child(5){left:6px;bottom:4px}.current-cyber-art{background:#03050a}.current-cyber-art:before{content:"";position:absolute;inset:-5%;background:repeating-linear-gradient(to bottom,rgba(56,189,248,.55) 0 1px,transparent 1px 5px),repeating-linear-gradient(to right,rgba(139,92,246,.48) 0 1px,transparent 1px 5px);transform:perspective(26px) rotateX(58deg);transform-origin:50% 100%;}.current-cyber-art:after{content:"";position:absolute;left:6%;right:6%;top:54%;height:1px;background:linear-gradient(90deg,transparent,#22d3ee,#a78bfa,#22d3ee,transparent)}.current-black-art{background:#000}.bg-menu{position:absolute;top:calc(100% + 8px);right:0;width:278px;padding:9px;border:1px solid var(--app-border);border-radius:17px;background:var(--app-menu);backdrop-filter:blur(24px) saturate(130%);-webkit-backdrop-filter:blur(24px) saturate(130%);box-shadow:var(--app-shadow-lg),inset 0 1px var(--app-highlight);z-index:100}.bg-menu-title{padding:5px 8px 8px;color:var(--app-muted);font-size:10px;font-weight:700;text-transform:uppercase;letter-spacing:.12em}.bg-option{display:flex;align-items:center;gap:10px;width:100%;padding:8px;border:1px solid transparent;border-radius:12px;background:transparent;color:var(--app-text-soft);text-align:left;cursor:pointer;transition:.18s}.bg-option:hover{background:var(--app-hover);border-color:var(--app-border)}.bg-option.active{background:var(--app-accent-soft);border-color:var(--app-accent-border)}.bg-option-preview{position:relative;display:block;overflow:hidden;width:44px;height:32px;flex:none;border-radius:9px;border:1px solid var(--app-border);background:#000}.preview-fill{position:absolute;inset:0}.preview-galaxy-fill{background:radial-gradient(circle at 50% 50%,#fef3c7 0 7%,#a78bfa 20%,#4c1d95 36%,#09101b 75%)}.preview-stars{position:absolute;width:3px;height:3px;top:13px;left:20px;border-radius:50%;background:#fff;box-shadow:-11px -5px 0 #67e8f9,10px 6px 0 #f472b6,-6px 8px 0 #fff}.preview-gradient-fill{background:radial-gradient(circle at 20% 20%,#8b5cf6 0 11%,transparent 42%),radial-gradient(circle at 80% 20%,#22d3ee 0 9%,transparent 43%),linear-gradient(135deg,#0f172a,#312e81)}.preview-particles-fill{background:radial-gradient(circle,#07101b,#03050a)}.preview-particles-dots{position:absolute;inset:0;display:flex;justify-content:center;align-items:center;gap:5px}.preview-particles-dots i{width:4px;height:4px;border-radius:50%;background:#dff6ff;box-shadow:0 0 7px #67e8f9}.preview-aurora-fill{background:radial-gradient(ellipse at 20% 78%,rgba(16,185,129,.75),transparent 39%),radial-gradient(ellipse at 75% 25%,rgba(99,102,241,.8),transparent 45%),linear-gradient(160deg,#07111b,#060914)}.preview-nebula-fill{background:radial-gradient(circle at 36% 45%,rgba(34,211,238,.65),transparent 19%),radial-gradient(circle at 68% 54%,rgba(168,85,247,.62),transparent 31%),#050712}.preview-grid-fill{background-image:linear-gradient(rgba(56,189,248,.62) 1px,transparent 1px),linear-gradient(90deg,rgba(139,92,246,.54) 1px,transparent 1px);background-size:7px 7px;transform:perspective(90px) rotateX(48deg) scale(1.35);transform-origin:center bottom}.preview-black-fill{background:#000}.bg-option-main{display:flex;min-width:0;flex:1;flex-direction:column;gap:2px}.bg-option-main strong{font-size:12px;color:var(--app-text)}.bg-option-main small{font-size:10px;color:var(--app-muted)}.selected-mark{width:16px;height:16px;color:var(--app-primary);flex:none}.bg-menu-enter-active,.bg-menu-leave-active{transition:opacity .14s ease,transform .14s ease}.bg-menu-enter-from,.bg-menu-leave-to{opacity:0;transform:translateY(-5px) scale(.98)}
html[data-theme="light"] .current-preview,html[data-theme="light"] .bg-option-preview{border-color:rgba(51,65,85,.14)}

html[data-theme="light"] .current-galaxy-art{background:radial-gradient(circle at 50% 48%,#ffffff 0 10%,#c7d2fe 22%,#818cf8 40%,#eaf0ff 77%)}
html[data-theme="light"] .current-nebula-art{background:radial-gradient(circle at 35% 45%,rgba(14,165,233,.36),transparent 22%),radial-gradient(circle at 68% 58%,rgba(124,58,237,.30),transparent 34%),#f5f8fc}
html[data-theme="light"] .current-aurora-art{background:radial-gradient(ellipse at 22% 82%,rgba(16,185,129,.55),transparent 42%),radial-gradient(ellipse at 72% 20%,rgba(79,70,229,.40),transparent 45%),linear-gradient(160deg,#ffffff,#eef4fb 62%,#f8fafc)}
html[data-theme="light"] .current-gradient-art{background:radial-gradient(circle at 20% 20%,rgba(124,58,237,.72) 0 10%,transparent 47%),radial-gradient(circle at 78% 26%,rgba(8,145,178,.62) 0 8%,transparent 50%),linear-gradient(135deg,#f7f9fc,#eef2ff 58%,#f2fbff)}
html[data-theme="light"] .current-particles-art{background:#f7f9fc}
html[data-theme="light"] .current-particles-art i{background:#334155;box-shadow:0 0 4px rgba(8,145,178,.35)}
html[data-theme="light"] .current-cyber-art{background:#f7f9fc}
html[data-theme="light"] .current-cyber-art:before{background:repeating-linear-gradient(to bottom,rgba(14,116,144,.30) 0 1px,transparent 1px 5px),repeating-linear-gradient(to right,rgba(79,70,229,.26) 0 1px,transparent 1px 5px)}
html[data-theme="light"] .current-cyber-art:after{background:linear-gradient(90deg,transparent,#0891b2,#6366f1,#0891b2,transparent)}
html[data-theme="light"] .preview-galaxy-fill{background:radial-gradient(circle at 50% 50%,#ffffff 0 8%,#c7d2fe 21%,#818cf8 38%,#eef2ff 75%)}
html[data-theme="light"] .preview-gradient-fill{background:radial-gradient(circle at 20% 20%,rgba(124,58,237,.60) 0 11%,transparent 42%),radial-gradient(circle at 80% 20%,rgba(8,145,178,.55) 0 9%,transparent 43%),linear-gradient(135deg,#f8fafc,#eef2ff)}
html[data-theme="light"] .preview-particles-fill{background:#f7f9fc}
html[data-theme="light"] .preview-particles-dots i{background:#334155;box-shadow:0 0 4px rgba(8,145,178,.30)}
html[data-theme="light"] .preview-aurora-fill{background:radial-gradient(ellipse at 20% 78%,rgba(16,185,129,.48),transparent 39%),radial-gradient(ellipse at 75% 25%,rgba(79,70,229,.46),transparent 45%),linear-gradient(160deg,#ffffff,#eef4fb)}
html[data-theme="light"] .preview-nebula-fill{background:radial-gradient(circle at 36% 45%,rgba(8,145,178,.46),transparent 19%),radial-gradient(circle at 68% 54%,rgba(124,58,237,.40),transparent 31%),#f7f9fc}
html[data-theme="light"] .preview-grid-fill{background-image:linear-gradient(rgba(14,116,144,.40) 1px,transparent 1px),linear-gradient(90deg,rgba(79,70,229,.30) 1px,transparent 1px);background-color:#f7f9fc}
html[data-theme="light"] .preview-black-fill{background:#fff}

</style>
