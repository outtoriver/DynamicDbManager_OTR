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
.cyber-grid{position:fixed;inset:0;overflow:hidden;pointer-events:none;background:var(--cyber-bg)}
.cyber-sky{position:absolute;inset:0;background:radial-gradient(ellipse 50% 32% at 50% 44%,var(--cyber-sky-glow),transparent 72%),linear-gradient(180deg,var(--cyber-top),var(--cyber-bottom))}
.cyber-floor{position:absolute;left:-15%;right:-15%;top:43%;bottom:-8%;overflow:hidden;perspective:850px}
.grid-lines{position:absolute;inset:-20% -10%;transform:perspective(700px) rotateX(60deg) scaleY(1.28);transform-origin:50% 0%;background-repeat:repeat;}
.grid-lines-wide{background-image:linear-gradient(var(--cyber-line-wide) 1px,transparent 1px),linear-gradient(90deg,var(--cyber-line-wide-2) 1px,transparent 1px);background-size:72px 72px;animation:grid-wide 9s linear infinite}
.grid-lines-fine{background-image:linear-gradient(var(--cyber-line-fine) 1px,transparent 1px),linear-gradient(90deg,var(--cyber-line-fine-2) 1px,transparent 1px);background-size:36px 36px;animation:grid-fine 5.5s linear infinite}
.grid-horizon{position:absolute;left:4%;right:4%;top:2%;height:1px;background:linear-gradient(90deg,transparent,var(--cyber-horizon) 20%,var(--cyber-horizon-2) 50%,var(--cyber-horizon) 80%,transparent);box-shadow:0 0 10px color-mix(in srgb,var(--cyber-horizon) 45%,transparent)}
.cyber-stars{position:absolute;inset:0;opacity:.36;background:radial-gradient(circle at 12% 20%,var(--cyber-star) 0 1px,transparent 1.6px),radial-gradient(circle at 31% 27%,var(--cyber-star-2) 0 1px,transparent 1.6px),radial-gradient(circle at 68% 22%,var(--cyber-star) 0 1px,transparent 1.6px),radial-gradient(circle at 86% 33%,var(--cyber-star-2) 0 1px,transparent 1.6px)}
.cyber-vignette{position:absolute;inset:0;background:linear-gradient(180deg,rgba(0,0,0,.10),transparent 35%,transparent 70%,var(--cyber-vignette))}
:global(:root){--cyber-bg:#02040a;--cyber-top:#03050b;--cyber-bottom:#02050d;--cyber-sky-glow:rgba(34,211,238,.07);--cyber-line-wide:rgba(56,189,248,.24);--cyber-line-wide-2:rgba(139,92,246,.22);--cyber-line-fine:rgba(103,232,249,.12);--cyber-line-fine-2:rgba(167,139,250,.10);--cyber-horizon:rgba(34,211,238,.85);--cyber-horizon-2:rgba(139,92,246,.90);--cyber-star:rgba(255,255,255,.65);--cyber-star-2:rgba(103,232,249,.55);--cyber-vignette:rgba(0,0,0,.48)}
:global(html[data-theme="light"]){--cyber-bg:#f9fbff;--cyber-top:#eef5ff;--cyber-bottom:#f9fbff;--cyber-sky-glow:rgba(14,165,233,.10);--cyber-line-wide:rgba(14,165,233,.22);--cyber-line-wide-2:rgba(79,70,229,.17);--cyber-line-fine:rgba(14,165,233,.10);--cyber-line-fine-2:rgba(79,70,229,.08);--cyber-horizon:rgba(8,145,178,.65);--cyber-horizon-2:rgba(79,70,229,.65);--cyber-star:rgba(71,85,105,.34);--cyber-star-2:rgba(14,116,144,.34);--cyber-vignette:rgba(148,163,184,.12)}
@keyframes grid-wide{from{background-position:0 0,0 0}to{background-position:0 72px,72px 0}}
@keyframes grid-fine{from{background-position:0 0,0 0}to{background-position:0 36px,36px 0}}
@media(prefers-reduced-motion:reduce){.grid-lines{animation:none}}
</style>
