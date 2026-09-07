<template><div ref="container" class="background-nebula" aria-hidden="true"></div></template>

<script setup>
import { onBeforeUnmount, onMounted, ref } from 'vue'
import * as THREE from 'three'

const container = ref(null)
let scene
let camera
let renderer
let cloud
let stars
let cloudGeometry
let starGeometry
let cloudMaterial
let starMaterial
let raf
let themeListener
let resizeObserver
let running = true
let last = 0

const reducedMotion = window.matchMedia?.('(prefers-reduced-motion: reduce)').matches ?? false
const lightTheme = () => document.documentElement.dataset.theme === 'light'

function rebuildMaterials() {
  if (!cloudGeometry || !starGeometry) return
  cloudMaterial?.dispose()
  starMaterial?.dispose()
  const light = lightTheme()

  const cloudColors = light ? ['#38bdf8', '#6366f1', '#a78bfa'] : ['#22d3ee', '#8b5cf6', '#ec4899']
  const starColor = light ? '#334155' : '#e2e8f0'

  const cloudColorsAttribute = new Float32Array(cloudGeometry.attributes.position.count * 3)
  for (let i = 0; i < cloudGeometry.attributes.position.count; i += 1) {
    const c = new THREE.Color(cloudColors[i % cloudColors.length])
    cloudColorsAttribute[i * 3] = c.r
    cloudColorsAttribute[i * 3 + 1] = c.g
    cloudColorsAttribute[i * 3 + 2] = c.b
  }
  cloudGeometry.setAttribute('color', new THREE.Float32BufferAttribute(cloudColorsAttribute, 3))

  cloudMaterial = new THREE.PointsMaterial({ size: light ? 0.085 : 0.11, transparent: true, opacity: light ? 0.12 : 0.18, vertexColors: true, depthWrite: false, blending: THREE.AdditiveBlending })
  starMaterial = new THREE.PointsMaterial({ size: light ? 0.025 : 0.035, color: starColor, transparent: true, opacity: light ? 0.28 : 0.55, depthWrite: false })

  cloud.material = cloudMaterial
  stars.material = starMaterial
}

function build() {
  scene = new THREE.Scene()
  camera = new THREE.PerspectiveCamera(45, window.innerWidth / window.innerHeight, 0.1, 100)
  camera.position.z = 9
  renderer = new THREE.WebGLRenderer({ alpha: true, antialias: false, powerPreference: 'high-performance' })
  renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 1.5))
  renderer.setSize(window.innerWidth, window.innerHeight)
  renderer.setClearColor(0x000000, 0)
  renderer.outputColorSpace = THREE.SRGBColorSpace
  container.value.appendChild(renderer.domElement)

  const count = reducedMotion ? 1000 : 2300
  const positions = new Float32Array(count * 3)
  for (let i = 0; i < count; i += 1) {
    const angle = Math.random() * Math.PI * 2
    const radius = Math.pow(Math.random(), 0.8) * 7
    positions[i * 3] = Math.cos(angle) * radius + (Math.random() - 0.5) * 1.2
    positions[i * 3 + 1] = (Math.random() - 0.5) * 4.5
    positions[i * 3 + 2] = Math.sin(angle) * radius * 0.45
  }
  cloudGeometry = new THREE.BufferGeometry()
  cloudGeometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3))
  cloud = new THREE.Points(cloudGeometry, new THREE.PointsMaterial())
  scene.add(cloud)

  const starCount = reducedMotion ? 500 : 1000
  const starPositions = new Float32Array(starCount * 3)
  for (let i = 0; i < starCount; i += 1) {
    starPositions[i * 3] = (Math.random() - 0.5) * 26
    starPositions[i * 3 + 1] = (Math.random() - 0.5) * 15
    starPositions[i * 3 + 2] = (Math.random() - 0.5) * 18
  }
  starGeometry = new THREE.BufferGeometry()
  starGeometry.setAttribute('position', new THREE.Float32BufferAttribute(starPositions, 3))
  stars = new THREE.Points(starGeometry, new THREE.PointsMaterial())
  scene.add(stars)
  rebuildMaterials()
}

function resize() {
  if (!camera || !renderer) return
  camera.aspect = window.innerWidth / window.innerHeight
  camera.updateProjectionMatrix()
  renderer.setSize(window.innerWidth, window.innerHeight)
}

function animate(now) {
  if (!running) { raf = requestAnimationFrame(animate); return }
  raf = requestAnimationFrame(animate)
  if (now - last < 40) return
  last = now
  if (!reducedMotion) {
    cloud.rotation.y += 0.00022
    cloud.rotation.x += 0.00006
  }
  renderer.render(scene, camera)
}

onMounted(() => {
  build()
  resizeObserver = new ResizeObserver(resize)
  resizeObserver.observe(container.value)
  window.addEventListener('resize', resize, { passive: true })
  themeListener = () => rebuildMaterials()
  window.addEventListener('themechange', themeListener)
  raf = requestAnimationFrame(animate)
})

onBeforeUnmount(() => {
  cancelAnimationFrame(raf)
  resizeObserver?.disconnect()
  window.removeEventListener('resize', resize)
  window.removeEventListener('themechange', themeListener)
  cloudGeometry?.dispose(); starGeometry?.dispose(); cloudMaterial?.dispose(); starMaterial?.dispose(); renderer?.dispose(); renderer?.forceContextLoss?.(); renderer?.domElement?.remove()
})
</script>

<style scoped>.background-nebula{position:fixed;inset:0;z-index:-10;pointer-events:none;overflow:hidden}.background-nebula :deep(canvas){width:100%;height:100%;display:block;background:transparent!important}</style>
