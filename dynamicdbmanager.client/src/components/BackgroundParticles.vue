<template>
  <div ref="container" class="background-particles" aria-hidden="true"></div>
</template>

<script setup>
import { onBeforeUnmount, onMounted, ref } from 'vue'
import * as THREE from 'three'

const container = ref(null)
let scene
let camera
let renderer
let particles
let geometry
let material
let animationId
let resizeObserver
let themeListener
let visibilityListener
let running = true
let lastFrame = 0
let frameMs = 33

const reducedMotion = window.matchMedia?.('(prefers-reduced-motion: reduce)').matches ?? false

function themeIsLight() {
  return document.documentElement.dataset.theme === 'light'
}

function createMaterial() {
  const count = geometry?.attributes?.position?.count || 0
  const colors = new Float32Array(count * 3)
  const palette = themeIsLight()
    ? [
        new THREE.Color('#2563eb'),
        new THREE.Color('#0891b2'),
        new THREE.Color('#6366f1'),
        new THREE.Color('#475569')
      ]
    : [
        new THREE.Color('#67e8f9'),
        new THREE.Color('#a78bfa'),
        new THREE.Color('#60a5fa'),
        new THREE.Color('#ffffff')
      ]

  for (let i = 0; i < count; i += 1) {
    const color = palette[i % palette.length]
    colors[i * 3] = color.r
    colors[i * 3 + 1] = color.g
    colors[i * 3 + 2] = color.b
  }

  geometry.setAttribute('color', new THREE.Float32BufferAttribute(colors, 3))

  return new THREE.PointsMaterial({
    size: themeIsLight() ? 0.035 : 0.045,
    transparent: true,
    opacity: themeIsLight() ? 0.34 : 0.52,
    vertexColors: true,
    depthWrite: false,
    blending: THREE.AdditiveBlending
  })
}

function buildScene() {
  if (!container.value) return

  scene = new THREE.Scene()
  camera = new THREE.PerspectiveCamera(50, window.innerWidth / window.innerHeight, 0.1, 100)
  camera.position.z = 10

  renderer = new THREE.WebGLRenderer({ alpha: true, antialias: false, powerPreference: 'high-performance' })
  renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, window.innerWidth <= 768 ? 1.2 : 1.5))
  renderer.setSize(window.innerWidth, window.innerHeight)
  renderer.setClearColor(0x000000, 0)
  renderer.outputColorSpace = THREE.SRGBColorSpace
  container.value.appendChild(renderer.domElement)

  const count = reducedMotion ? 450 : window.innerWidth <= 768 ? 700 : 1400
  geometry = new THREE.BufferGeometry()
  const positions = new Float32Array(count * 3)

  for (let i = 0; i < count; i += 1) {
    positions[i * 3] = (Math.random() - 0.5) * 22
    positions[i * 3 + 1] = (Math.random() - 0.5) * 14
    positions[i * 3 + 2] = (Math.random() - 0.5) * 12
  }

  geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3))
  material = createMaterial()
  particles = new THREE.Points(geometry, material)
  scene.add(particles)
}

function handleResize() {
  if (!camera || !renderer) return
  camera.aspect = window.innerWidth / window.innerHeight
  camera.updateProjectionMatrix()
  renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, window.innerWidth <= 768 ? 1.2 : 1.5))
  renderer.setSize(window.innerWidth, window.innerHeight)
}

function handleThemeChange() {
  if (!geometry || !particles) return
  material?.dispose()
  material = createMaterial()
  particles.material = material
}

function animate(now) {
  if (!running || !renderer || !scene || !camera) return
  animationId = requestAnimationFrame(animate)

  if (now - lastFrame < frameMs) return
  lastFrame = now

  if (!reducedMotion && particles) {
    particles.rotation.y += 0.00035
    particles.rotation.x += 0.00008
  }

  renderer.render(scene, camera)
}

onMounted(() => {
  buildScene()
  resizeObserver = new ResizeObserver(handleResize)
  resizeObserver.observe(container.value)
  window.addEventListener('resize', handleResize, { passive: true })
  visibilityListener = () => { running = document.visibilityState === 'visible' }
  document.addEventListener('visibilitychange', visibilityListener)
  themeListener = () => handleThemeChange()
  window.addEventListener('themechange', themeListener)
  animationId = requestAnimationFrame(animate)
})

onBeforeUnmount(() => {
  cancelAnimationFrame(animationId)
  resizeObserver?.disconnect()
  window.removeEventListener('resize', handleResize)
  document.removeEventListener('visibilitychange', visibilityListener)
  window.removeEventListener('themechange', themeListener)
  geometry?.dispose()
  material?.dispose()
  renderer?.dispose()
  renderer?.forceContextLoss?.()
  renderer?.domElement?.remove()
})
</script>

<style scoped>
.background-particles{position:fixed;inset:0;z-index:-10;pointer-events:none;overflow:hidden}
.background-particles :deep(canvas){display:block;width:100%;height:100%;background:transparent!important}
</style>
