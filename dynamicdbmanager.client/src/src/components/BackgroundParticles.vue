<template>
  <div ref="container" class="background-particles" aria-hidden="true"></div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'
import * as THREE from 'three'

const container = ref(null)

let scene = null
let camera = null
let renderer = null
let particles = null
let geometry = null
let material = null

let animationId = null
let isActive = true
let isDestroyed = false
let lastFrameTime = 0

let handleResize = null
let handleVisibilityChange = null

const reducedMotion =
  window.matchMedia?.('(prefers-reduced-motion: reduce)').matches ?? false

function getProfile() {
  const width = window.innerWidth
  const threads = navigator.hardwareConcurrency || 4
  const memory = navigator.deviceMemory || 4

  let count = 2000

  if (width <= 768) {
    count = 700
  } else if (threads <= 4 || memory <= 4) {
    count = 1100
  } else if (threads <= 8) {
    count = 1600
  }

  if (reducedMotion) {
    count = Math.min(count, 450)
  }

  const dpr = Math.min(
    window.devicePixelRatio || 1,
    width <= 768 ? 1.25 : 1.5
  )

  const fps = reducedMotion ? 20 : width <= 768 ? 30 : 45

  return { count, dpr, fps }
}

function createParticles(count) {
  const positions = new Float32Array(count * 3)
  const colors = new Float32Array(count * 3)

  const colorBase = new THREE.Color(0x88aaff)
  const colorCyan = new THREE.Color(0x67e8f9)
  const tempColor = new THREE.Color()

  for (let i = 0; i < count; i++) {
    const i3 = i * 3

    positions[i3] = (Math.random() - 0.5) * 100
    positions[i3 + 1] = (Math.random() - 0.5) * 100
    positions[i3 + 2] = (Math.random() - 0.5) * 100

    tempColor.copy(colorBase).lerp(colorCyan, Math.random() * 0.35)

    const brightness = 0.55 + Math.random() * 0.45
    colors[i3] = tempColor.r * brightness
    colors[i3 + 1] = tempColor.g * brightness
    colors[i3 + 2] = tempColor.b * brightness
  }

  geometry = new THREE.BufferGeometry()
  geometry.setAttribute('position', new THREE.BufferAttribute(positions, 3))
  geometry.setAttribute('color', new THREE.BufferAttribute(colors, 3))

  material = new THREE.PointsMaterial({
    color: 0xffffff,
    size: window.innerWidth <= 768 ? 0.16 : 0.2,
    transparent: true,
    opacity: 0.78,
    depthWrite: false,
    depthTest: true,
    vertexColors: true,
    sizeAttenuation: true,
    blending: THREE.AdditiveBlending
  })

  particles = new THREE.Points(geometry, material)
  scene.add(particles)
}

function renderFrame(now) {
  if (isDestroyed || !isActive || !renderer || !scene || !camera || !particles) {
    return
  }

  const fps = window.innerWidth <= 768 ? 30 : 45
  const frameInterval = 1000 / (reducedMotion ? 20 : fps)

  if (now - lastFrameTime < frameInterval) {
    animationId = requestAnimationFrame(renderFrame)
    return
  }

  const delta = Math.min(now - lastFrameTime, 100) / 16.6667
  lastFrameTime = now

  if (!reducedMotion) {
    particles.rotation.y += 0.0005 * delta
    particles.rotation.x += 0.0002 * delta
  }

  renderer.render(scene, camera)
  animationId = requestAnimationFrame(renderFrame)
}

function startAnimation() {
  if (animationId || isDestroyed) return
  lastFrameTime = performance.now()
  animationId = requestAnimationFrame(renderFrame)
}

function stopAnimation() {
  if (!animationId) return
  cancelAnimationFrame(animationId)
  animationId = null
}

onMounted(() => {
  if (!container.value) return

  try {
    const profile = getProfile()
    const width = window.innerWidth
    const height = window.innerHeight

    scene = new THREE.Scene()

    camera = new THREE.PerspectiveCamera(
      75,
      width / height,
      0.1,
      1000
    )
    camera.position.z = 30

    renderer = new THREE.WebGLRenderer({
      antialias: width > 768,
      alpha: true,
      powerPreference: 'high-performance',
      stencil: false,
      depth: true
    })

    renderer.setPixelRatio(profile.dpr)
    renderer.setSize(width, height, false)
    renderer.setClearColor(0x000000, 0)

    if ('outputColorSpace' in renderer) {
      renderer.outputColorSpace = THREE.SRGBColorSpace
    }

    renderer.domElement.setAttribute('aria-hidden', 'true')
    container.value.appendChild(renderer.domElement)

    createParticles(profile.count)

    handleResize = () => {
      if (!camera || !renderer) return

      const w = window.innerWidth
      const h = window.innerHeight

      camera.aspect = w / h
      camera.updateProjectionMatrix()

      renderer.setPixelRatio(
        Math.min(window.devicePixelRatio || 1, w <= 768 ? 1.25 : 1.5)
      )
      renderer.setSize(w, h, false)

      if (material) {
        material.size = w <= 768 ? 0.16 : 0.2
        material.needsUpdate = true
      }
    }

    handleVisibilityChange = () => {
      isActive = !document.hidden
      if (!isActive) stopAnimation()
      else startAnimation()
    }

    window.addEventListener('resize', handleResize, { passive: true })
    document.addEventListener('visibilitychange', handleVisibilityChange)

    startAnimation()
  } catch (error) {
    console.error('[BackgroundParticles] initialization error:', error)
  }
})

onBeforeUnmount(() => {
  isDestroyed = true
  stopAnimation()

  if (handleResize) {
    window.removeEventListener('resize', handleResize)
    handleResize = null
  }

  if (handleVisibilityChange) {
    document.removeEventListener('visibilitychange', handleVisibilityChange)
    handleVisibilityChange = null
  }

  if (geometry) {
    geometry.dispose()
    geometry = null
  }

  if (material) {
    material.dispose()
    material = null
  }

  if (scene) {
    scene.clear()
  }

  particles = null
  scene = null
  camera = null

  if (renderer) {
    renderer.dispose()
    renderer.forceContextLoss?.()

    if (
      container.value &&
      renderer.domElement &&
      renderer.domElement.parentNode === container.value
    ) {
      container.value.removeChild(renderer.domElement)
    }
  }

  renderer = null
})
</script>

<style scoped>
.background-particles {
  position: fixed;
  inset: 0;
  z-index: -10;
  width: 100%;
  height: 100%;
  pointer-events: none;
  overflow: hidden;
  background: transparent;
}

.background-particles :deep(canvas) {
  display: block;
  width: 100%;
  height: 100%;
}
</style>
