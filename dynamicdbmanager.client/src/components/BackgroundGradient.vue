<template><div class="background-gradient" aria-hidden="true"></div></template>

<style scoped>
.background-gradient{position:fixed;inset:0;z-index:-10;pointer-events:none;overflow:hidden;background:radial-gradient(circle at 15% 20%,var(--gradient-a),transparent 34%),radial-gradient(circle at 82% 15%,var(--gradient-b),transparent 32%),radial-gradient(circle at 55% 85%,var(--gradient-c),transparent 38%),linear-gradient(135deg,var(--gradient-base-a),var(--gradient-base-b) 48%,var(--gradient-base-c));background-size:140% 140%,140% 140%,140% 140%,100% 100%;background-position:0% 50%,100% 0%,50% 100%,center;animation:gradient-shift 18s ease-in-out infinite;transform:translateZ(0)}
:global(:root){--gradient-a:rgba(168,85,247,.34);--gradient-b:rgba(59,130,246,.28);--gradient-c:rgba(99,102,241,.30);--gradient-base-a:#090618;--gradient-base-b:#111b3d;--gradient-base-c:#061522}
:global(html[data-theme="light"]){--gradient-a:rgba(99,102,241,.16);--gradient-b:rgba(14,165,233,.13);--gradient-c:rgba(124,58,237,.11);--gradient-base-a:#f9fbff;--gradient-base-b:#eef4ff;--gradient-base-c:#f7fbff}
@keyframes gradient-shift{0%,100%{background-position:0% 50%,100% 0%,50% 100%,center}50%{background-position:10% 42%,90% 12%,55% 92%,center}}
@media(prefers-reduced-motion:reduce){.background-gradient{animation:none}}
</style>
