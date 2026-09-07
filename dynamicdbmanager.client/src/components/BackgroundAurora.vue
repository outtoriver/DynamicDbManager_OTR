<template>
  <canvas ref="canvas" class="background-aurora" aria-hidden="true"></canvas>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'

const canvas = ref(null)
let ctx = null
let raf = 0
let running = true
let width = 0
let height = 0
let dpr = 1
let resizeObserver = null
let last = 0
let phase = 0

const reducedMotion = window.matchMedia?.('(prefers-reduced-motion: reduce)').matches ?? false

function resize() {
  if (!canvas.value) return
  const rect = canvas.value.getBoundingClientRect()
  width = Math.max(1, Math.floor(rect.width))
  height = Math.max(1, Math.floor(rect.height))
  dpr = Math.min(window.devicePixelRatio || 1, width <= 768 ? 1.25 : 1.5)
  canvas.value.width = Math.floor(width * dpr)
  canvas.value.height = Math.floor(height * dpr)
  ctx = canvas.value.getContext('2d', { alpha: true })
  ctx?.setTransform(dpr, 0, 0, dpr, 0, 0)
}

function draw(now) {
  if (!running || !ctx) return
  const dt = Math.min(64, now - last || 16)
  last = now
  phase += reducedMotion ? 0 : dt * 0.00022

  ctx.clearRect(0, 0, width, height)

  const light = document.documentElement.dataset.theme === 'light'
  const gradient = ctx.createLinearGradient(0, 0, width, height)
  gradient.addColorStop(0, light ? '#eef4fb' : '#050711')
  gradient.addColorStop(0.45, light ? '#dce8f6' : '#0b1230')
  gradient.addColorStop(1, light ? '#f6f8fc' : '#04060d')
  ctx.fillStyle = gradient
  ctx.fillRect(0, 0, width, height)

  const blobs = [
    { x: 0.18, y: 0.20, r: 0.42, c: light ? 'rgba(99,102,241,.14)' : 'rgba(99,102,241,.26)', s: 1.0 },
    { x: 0.78, y: 0.18, r: 0.34, c: light ? 'rgba(14,165,233,.12)' : 'rgba(34,211,238,.20)', s: .9 },
    { x: 0.62, y: 0.82, r: 0.45, c: light ? 'rgba(168,85,247,.11)' : 'rgba(168,85,247,.22)', s: 1.1 },
    { x: 0.28, y: 0.72, r: 0.30, c: light ? 'rgba(16,185,129,.09)' : 'rgba(16,185,129,.13)', s: .8 }
  ]

  for (const b of blobs) {
    const dx = Math.sin(phase * b.s + b.y * 3.1) * width * 0.08
    const dy = Math.cos(phase * 0.9 * b.s + b.x * 4.7) * height * 0.06
    const g = ctx.createRadialGradient(
      b.x * width + dx,
      b.y * height + dy,
      0,
      b.x * width + dx,
      b.y * height + dy,
      Math.max(width, height) * b.r
    )
    g.addColorStop(0, b.c)
    g.addColorStop(1, 'rgba(0,0,0,0)')
    ctx.fillStyle = g
    ctx.fillRect(0, 0, width, height)
  }

  const band = Math.min(width, height) * 0.18
  ctx.save()
  ctx.globalCompositeOperation = 'screen'
  for (let layer = 0; layer < 3; layer++) {
    ctx.beginPath()
    const yBase = height * (0.38 + layer * 0.11)
    for (let x = -40; x <= width + 40; x += 18) {
      const nx = x / Math.max(width, 1)
      const y = yBase
        + Math.sin(nx * 8 + phase * (1.15 + layer * .18)) * band * (0.22 + layer * .05)
        + Math.sin(nx * 17 - phase * .7 + layer) * band * .08
      if (x === -40) ctx.moveTo(x, y)
      else ctx.lineTo(x, y)
    }
    ctx.lineTo(width + 40, height + 40)
    ctx.lineTo(-40, height + 40)
    ctx.closePath()
    const hue = layer === 0 ? '99,102,241' : layer === 1 ? '34,211,238' : '168,85,247'
    const fill = ctx.createLinearGradient(0, yBase - band, 0, yBase + band * 2)
    fill.addColorStop(0, `rgba(${hue},0)`)
    fill.addColorStop(.45, `rgba(${hue},${0.10 - layer * 0.015})`)
    fill.addColorStop(1, `rgba(${hue},0)`)
    ctx.fillStyle = fill
    ctx.fill()
  }
  ctx.restore()

  if (!reducedMotion) raf = requestAnimationFrame(draw)
}

function start() {
  if (reducedMotion) {
    draw(performance.now())
    return
  }
  if (!raf) raf = requestAnimationFrame(draw)
}

function stop() {
  if (raf) cancelAnimationFrame(raf)
  raf = 0
}

onMounted(() => {
  resize()
  resizeObserver = new ResizeObserver(resize)
  resizeObserver.observe(canvas.value)
  document.addEventListener('visibilitychange', () => {
    running = !document.hidden
    if (running) start()
    else stop()
  })
  start()
})

onBeforeUnmount(() => {
  stop()
  resizeObserver?.disconnect()
  resizeObserver = null
  ctx = null
})
</script>

<style scoped>
.background-aurora{
  position:fixed;
  inset:0;
  z-index:-10;
  width:100%;
  height:100%;
  display:block;
  pointer-events:none;
  background:#050711;
}
</style>
