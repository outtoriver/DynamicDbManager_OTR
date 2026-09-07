<template>
  <div class="cyber-grid" aria-hidden="true">
    <div class="cyber-sky"></div>
    <div class="cyber-floor">
      <div class="grid-lines grid-lines-wide"></div>
      <div class="grid-lines grid-lines-fine"></div>
      <div class="grid-horizon"></div>
    </div>
    <div class="cyber-stars"></div>
    <div class="cyber-vignette"></div>
  </div>
</template>

<style scoped>
.cyber-grid{position:fixed;inset:0;overflow:hidden;pointer-events:none;background:linear-gradient(180deg,#03050a 0%,#07101d 44%,#020409 100%)}
.cyber-sky{position:absolute;inset:0;background:radial-gradient(ellipse 50% 34% at 50% 46%,rgba(34,211,238,.07),transparent 70%),radial-gradient(ellipse 32% 28% at 17% 28%,rgba(56,189,248,.055),transparent 74%),radial-gradient(ellipse 30% 28% at 83% 28%,rgba(139,92,246,.06),transparent 74%)}
.cyber-stars{position:absolute;inset:0;opacity:.28;background:radial-gradient(circle at 12% 20%,rgba(255,255,255,.8) 0 1px,transparent 1.6px),radial-gradient(circle at 31% 27%,rgba(103,232,249,.75) 0 1px,transparent 1.6px),radial-gradient(circle at 68% 22%,rgba(255,255,255,.7) 0 1px,transparent 1.6px),radial-gradient(circle at 86% 33%,rgba(167,139,250,.75) 0 1px,transparent 1.6px)}
.cyber-floor{position:absolute;left:-12%;right:-12%;top:48%;bottom:-5%;overflow:hidden;transform:perspective(900px) rotateX(58deg);transform-origin:50% 0%}
.grid-lines{position:absolute;inset:-15% -8%}
.grid-lines-wide{background-image:linear-gradient(to right,rgba(56,189,248,.24) 1px,transparent 1px),linear-gradient(to bottom,rgba(99,102,241,.18) 1px,transparent 1px);background-size:72px 72px;animation:gridMoveWide 9s linear infinite}
.grid-lines-fine{background-image:linear-gradient(to right,rgba(103,232,249,.11) 1px,transparent 1px),linear-gradient(to bottom,rgba(167,139,250,.10) 1px,transparent 1px);background-size:36px 36px;animation:gridMoveFine 5.5s linear infinite}
.grid-horizon{position:absolute;left:2%;right:2%;top:0;height:1px;background:linear-gradient(90deg,transparent,rgba(34,211,238,.7) 24%,#22d3ee 42%,#8b5cf6 50%,#22d3ee 58%,rgba(34,211,238,.7) 76%,transparent);box-shadow:0 0 6px rgba(34,211,238,.38)}
.cyber-vignette{position:absolute;inset:0;background:linear-gradient(to bottom,rgba(0,0,0,.08),transparent 20%,transparent 78%,rgba(0,0,0,.32)),radial-gradient(circle at center,transparent 42%,rgba(0,0,0,.20) 100%)}
@keyframes gridMoveWide{from{background-position:0 0,0 0}to{background-position:0 72px,72px 0}}
@keyframes gridMoveFine{from{background-position:0 0,0 0}to{background-position:0 36px,36px 0}}

html[data-theme="light"] .cyber-grid{background:linear-gradient(180deg,#ffffff 0%,#f6f9fd 47%,#eef3f8 100%)}
html[data-theme="light"] .cyber-sky{background:radial-gradient(ellipse 50% 34% at 50% 46%,rgba(14,165,233,.055),transparent 70%),radial-gradient(ellipse 32% 28% at 17% 28%,rgba(37,99,235,.035),transparent 74%),radial-gradient(ellipse 30% 28% at 83% 28%,rgba(79,70,229,.035),transparent 74%)}
html[data-theme="light"] .cyber-stars{opacity:.10}
html[data-theme="light"] .grid-lines-wide{background-image:linear-gradient(to right,rgba(14,116,144,.23) 1px,transparent 1px),linear-gradient(to bottom,rgba(79,70,229,.16) 1px,transparent 1px)}
html[data-theme="light"] .grid-lines-fine{background-image:linear-gradient(to right,rgba(8,145,178,.11) 1px,transparent 1px),linear-gradient(to bottom,rgba(99,102,241,.08) 1px,transparent 1px)}
html[data-theme="light"] .grid-horizon{background:linear-gradient(90deg,transparent,rgba(8,145,178,.48) 24%,#0891b2 42%,#6366f1 50%,#0891b2 58%,rgba(8,145,178,.48) 76%,transparent);box-shadow:0 0 5px rgba(8,145,178,.22)}
html[data-theme="light"] .cyber-vignette{background:linear-gradient(to bottom,rgba(255,255,255,.12),transparent 20%,transparent 78%,rgba(148,163,184,.10)),radial-gradient(circle at center,transparent 42%,rgba(148,163,184,.06) 100%)}

@media(max-width:900px){.cyber-floor{left:-30%;right:-30%;top:50%;transform:perspective(760px) rotateX(58deg)}.grid-lines-wide{background-size:58px 58px}.grid-lines-fine{background-size:29px 29px}}
@media(max-width:600px){.cyber-floor{left:-45%;right:-45%;top:53%}.grid-lines-wide{background-size:48px 48px}.grid-lines-fine{background-size:24px 24px}}
@media(prefers-reduced-motion:reduce){.grid-lines{animation:none}}
</style>
