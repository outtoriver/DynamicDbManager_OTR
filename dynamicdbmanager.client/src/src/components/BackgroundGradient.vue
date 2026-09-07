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
    radial-gradient(circle at 15% 20%, rgba(168,85,247,.34), transparent 34%),
    radial-gradient(circle at 82% 15%, rgba(59,130,246,.28), transparent 32%),
    radial-gradient(circle at 55% 85%, rgba(99,102,241,.30), transparent 38%),
    linear-gradient(135deg, #090618 0%, #111342 48%, #0b1028 100%);
  background-size: 140% 140%, 140% 140%, 140% 140%, 100% 100%;
  background-position: 0% 50%, 100% 0%, 50% 100%, center;
  animation: gradient-shift 18s ease-in-out infinite;
  transform: translateZ(0);
  will-change: background-position;
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
