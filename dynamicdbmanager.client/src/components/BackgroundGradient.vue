<template>
  <div class="background-gradient" aria-hidden="true"></div>
</template>

<style scoped>
.background-gradient {
  position: fixed;
  inset: 0;
  z-index: -10;
  width: 100%;
  height: 100%;
  pointer-events: none;
  overflow: hidden;
  background:
    radial-gradient(circle at 15% 20%, var(--gradient-a), transparent 34%),
    radial-gradient(circle at 82% 15%, var(--gradient-b), transparent 32%),
    radial-gradient(circle at 55% 85%, var(--gradient-c), transparent 38%),
    linear-gradient(135deg, var(--gradient-base-a) 0%, var(--gradient-base-b) 48%, var(--gradient-base-c) 100%);
  background-size: 140% 140%, 140% 140%, 140% 140%, 100% 100%;
  background-position: 0% 50%, 100% 0%, 50% 100%, center;
  animation: gradient-shift 18s ease-in-out infinite;
  transform: translateZ(0);
  will-change: background-position;
  --gradient-a: rgba(168,85,247,.34);
  --gradient-b: rgba(59,130,246,.28);
  --gradient-c: rgba(99,102,241,.30);
  --gradient-base-a: #090618;
  --gradient-base-b: #111342;
  --gradient-base-c: #0b1028;
}

html[data-theme="light"] .background-gradient{
  --gradient-a: rgba(99,102,241,.24);
  --gradient-b: rgba(14,165,233,.18);
  --gradient-c: rgba(59,130,246,.18);
  --gradient-base-a: #eef3fa;
  --gradient-base-b: #dfe9f7;
  --gradient-base-c: #f4f7fb;
}


@keyframes gradient-shift {
  0% {
    background-position: 0% 50%, 100% 0%, 50% 100%, center;
  }
  50% {
    background-position: 55% 20%, 45% 70%, 80% 35%, center;
  }
  100% {
    background-position: 0% 50%, 100% 0%, 50% 100%, center;
  }
}

@media (prefers-reduced-motion: reduce) {
  .background-gradient {
    animation: none;
    background-position: 0% 50%, 100% 0%, 50% 100%, center;
  }
}
</style>
