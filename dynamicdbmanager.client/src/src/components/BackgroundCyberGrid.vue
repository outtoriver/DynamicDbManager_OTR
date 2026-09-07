<template>
  <div class="cyber-grid" aria-hidden="true">
    <div class="cyber-sky"></div>
    <div class="cyber-floor">
      <div class="grid grid-back"></div>
      <div class="grid grid-main"></div>
      <div class="grid grid-fine"></div>
      <div class="perspective-lines"></div>
    </div>
    <div class="cyber-horizon"></div>
    <div class="cyber-scan"></div>
    <div class="cyber-vignette"></div>
  </div>
</template>
<style scoped>
.cyber-grid{position:fixed;inset:0;overflow:hidden;pointer-events:none;background:#02050b}
.cyber-sky{position:absolute;inset:0;background:radial-gradient(ellipse 55% 30% at 50% 46%,rgba(34,211,238,.08),transparent 72%),linear-gradient(180deg,#02040a 0%,#06101d 48%,#02050b 100%)}
.cyber-floor{position:absolute;left:-22%;right:-22%;top:46%;bottom:-4%;overflow:hidden;perspective:620px;transform:translateZ(0)}
.grid{position:absolute;inset:-10% -4%;transform-origin:50% 0;transform:rotateX(62deg) scaleY(1.12);background-repeat:repeat;will-change:background-position}
.grid-back{background-image:linear-gradient(rgba(56,189,248,.18) 1px,transparent 1px),linear-gradient(90deg,rgba(139,92,246,.14) 1px,transparent 1px);background-size:92px 92px;opacity:.9;animation:moveBack 14s linear infinite}
.grid-main{background-image:linear-gradient(rgba(56,189,248,.28) 1px,transparent 1px),linear-gradient(90deg,rgba(99,102,241,.22) 1px,transparent 1px);background-size:46px 46px;opacity:.9;animation:moveMain 7s linear infinite}
.grid-fine{background-image:linear-gradient(rgba(103,232,249,.16) 1px,transparent 1px),linear-gradient(90deg,rgba(167,139,250,.12) 1px,transparent 1px);background-size:23px 23px;opacity:.5;animation:moveFine 4s linear infinite}
.perspective-lines{position:absolute;left:0;right:0;top:0;bottom:0;transform:perspective(700px);background:repeating-linear-gradient(90deg,transparent 0 11.5%,rgba(56,189,248,.16) 11.65% 11.8%,transparent 11.95% 24%);transform-origin:50% 0;opacity:.35;mix-blend-mode:screen}
.cyber-horizon{position:absolute;left:-5%;right:-5%;top:46%;height:1px;background:linear-gradient(90deg,transparent,rgba(34,211,238,.55) 25%,rgba(103,232,249,.95) 45%,rgba(167,139,250,.95) 55%,rgba(34,211,238,.55) 75%,transparent);box-shadow:0 0 10px rgba(34,211,238,.22);z-index:5}
.cyber-scan{position:absolute;inset:0;background:repeating-linear-gradient(180deg,transparent 0 5px,rgba(255,255,255,.012) 6px,transparent 7px);opacity:.25;z-index:7}
.cyber-vignette{position:absolute;inset:0;background:linear-gradient(180deg,rgba(0,0,0,.18),transparent 30%,transparent 75%,rgba(0,0,0,.24));z-index:8}
@keyframes moveBack{to{background-position:0 92px,92px 0}}@keyframes moveMain{to{background-position:0 46px,46px 0}}@keyframes moveFine{to{background-position:0 23px,23px 0}}
html[data-theme="light"] .cyber-grid{background:#eef3f9}
html[data-theme="light"] .cyber-sky{background:radial-gradient(ellipse 52% 26% at 50% 45%,rgba(14,165,233,.10),transparent 70%),linear-gradient(180deg,#f8fafc 0%,#eaf2fb 48%,#dfe9f4 100%)}
html[data-theme="light"] .grid-back{background-image:linear-gradient(rgba(14,116,144,.16) 1px,transparent 1px),linear-gradient(90deg,rgba(67,56,202,.10) 1px,transparent 1px)}
html[data-theme="light"] .grid-main{background-image:linear-gradient(rgba(8,145,178,.27) 1px,transparent 1px),linear-gradient(90deg,rgba(79,70,229,.18) 1px,transparent 1px)}
html[data-theme="light"] .grid-fine{background-image:linear-gradient(rgba(14,116,144,.12) 1px,transparent 1px),linear-gradient(90deg,rgba(99,102,241,.09) 1px,transparent 1px)}
html[data-theme="light"] .perspective-lines{opacity:.22;mix-blend-mode:multiply}
html[data-theme="light"] .cyber-horizon{background:linear-gradient(90deg,transparent,rgba(8,145,178,.5) 25%,rgba(14,165,233,.82) 45%,rgba(99,102,241,.72) 55%,rgba(8,145,178,.5) 75%,transparent);box-shadow:none}
html[data-theme="light"] .cyber-scan{opacity:.1}
html[data-theme="light"] .cyber-vignette{background:linear-gradient(180deg,rgba(255,255,255,.08),transparent 30%,transparent 75%,rgba(100,116,139,.10))}
@media(max-width:700px){.cyber-floor{left:-60%;right:-60%}.grid-back{background-size:72px 72px}.grid-main{background-size:36px 36px}.grid-fine{background-size:18px 18px}.cyber-horizon{top:47%}}
@media(prefers-reduced-motion:reduce){.grid{animation:none}}
</style>
