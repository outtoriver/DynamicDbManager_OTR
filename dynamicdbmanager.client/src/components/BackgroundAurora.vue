<template><canvas ref="canvas" class="background-aurora" aria-hidden="true"></canvas></template>

<script setup>
import { onBeforeUnmount, onMounted, ref } from 'vue'

const canvas = ref(null)
let ctx
let raf = 0
let resizeObserver
let themeListener
let running = true
let width = 0
let height = 0
let dpr = 1
let phase = 0
let last = 0

const reducedMotion = window.matchMedia?.('(prefers-reduced-motion: reduce)').matches ?? false

function lightTheme() {
  return document.documentElement.dataset.theme === 'light'
}

function resize() {
  if (!canvas.value) return
  const rect = canvas.value.getBoundingClientRect()
  width = Math.max(1, Math.floor(rect.width))
  height = Math.max(1, Math.floor(rect.height))
  dpr = Math.min(window.devicePixelRatio || 1, width <= 768 ? 1.2 : 1.5)
  canvas.value.width = Math.floor(width * dpr)
  canvas.value.height = Math.floor(height * dpr)
  ctx = canvas.value.getContext('2d', { alpha: true })
  ctx?.setTransform(dpr, 0, 0, dpr, 0, 0)
}

function draw(now) {
  if (!running || !ctx) return
  const dt = Math.min(64, now - last || 16)
  last = now
  phase += reducedMotion ? 0 : dt * 0.00018
  ctx.clearRect(0, 0, width, height)

  const light = lightTheme()
  const baseAlpha = light ? 0.16 : 0.26
  const colors = light
    ? ['56,189,248', '99,102,241', '16,185,129']
    : ['34,211,238', '99,102,241', '52,211,153']

  colors.forEach((rgb, index) => {
    const x = width * (0.16 + index * 0.34 + Math.sin(phase + index) * 0.035)
    const y = height * (0.30 + Math.cos(phase * 0.9 + index) * 0.08)
    const radius = Math.max(width, height) * (0.34 - index * 0.025)
    const gradient = ctx.createRadialGradient(x, y, 0, x, y, radius)
    gradient.addColorStop(0, `rgba(${rgb},${baseAlpha})`)
    gradient.addColorStop(1, `rgba(${rgb},0)`)
    ctx.fillStyle = gradient
    ctx.beginPath()
    ctx.arc(x, y, radius, 0, Math.PI * 2)
    ctx.fill()
  })

  raf = requestAnimationFrame(draw)
}

onMounted(() => {
  resize()
  resizeObserver = new ResizeObserver(resize)
  resizeObserver.observe(canvas.value)
  document.addEventListener('visibilitychange', () => {})
  themeListener = () => resize()
  window.addEventListener('themechange', themeListener)
  raf = requestAnimationFrame(draw)
})

onBeforeUnmount(() => {
  cancelAnimationFrame(raf)
  resizeObserver?.disconnect()
  window.removeEventListener('themechange', themeListener)
  ctx = null
})
</script>

<style scoped>.background-aurora{position:fixed;inset:0;z-index:-10;width:100%;height:100%;pointer-events:none;display:block}</style>
