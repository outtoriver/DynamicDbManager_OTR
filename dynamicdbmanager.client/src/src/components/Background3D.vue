<template>
  <div ref="container" class="background-3d" aria-hidden="true"></div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'
import * as THREE from 'three'

const container = ref(null)

let scene = null
let camera = null
let renderer = null
let galaxyGroup = null
let corePoints = null
let starPoints = null
let fogPoints = null
let bgPoints = null
let glowTexture = null

let animationId = null
let lastFrameTime = 0
let isActive = true
let isDestroyed = false

let handleResize = null
let handleVisibilityChange = null

const reducedMotion =
  window.matchMedia?.('(prefers-reduced-motion: reduce)').matches ?? false

function getPerformanceProfile() {
  const width = window.innerWidth
  const height = window.innerHeight
  const area = width * height

  const dpr = Math.min(
    window.devicePixelRatio || 1,
    width <= 768 ? 1.25 : 1.75
  )

  const hardwareThreads = navigator.hardwareConcurrency || 4
  const deviceMemory = navigator.deviceMemory || 4

  const lowPower =
    width <= 768 ||
    hardwareThreads <= 4 ||
    deviceMemory <= 4 ||
    area < 700_000

  if (reducedMotion) {
    return {
      lowPower: true,
      dpr: Math.min(dpr, 1.15),
      fps: 24,
      coreCount: 800,
      starsPerArm: 450,
      fogCount: 70,
      bgStarCount: 220
    }
  }

  if (lowPower) {
    return {
      lowPower: true,
      dpr,
      fps: 30,
      coreCount: 1200,
      starsPerArm: 700,
      fogCount: 140,
      bgStarCount: 350
    }
  }

  return {
    lowPower: false,
    dpr,
    fps: 45,
    coreCount: 2000,
    starsPerArm: 1200,
    fogCount: 300,
    bgStarCount: 600
  }
}

function createGlowTexture(size = 64) {
  const canvas = document.createElement('canvas')
  canvas.width = size
  canvas.height = size

  const ctx = canvas.getContext('2d')
  if (!ctx) return null

  const center = size / 2
  const gradient = ctx.createRadialGradient(
    center,
    center,
    0,
    center,
    center,
    center
  )

  gradient.addColorStop(0, 'rgba(255,255,255,1)')
  gradient.addColorStop(0.3, 'rgba(255,255,255,0.8)')
  gradient.addColorStop(0.7, 'rgba(255,255,255,0.3)')
  gradient.addColorStop(1, 'rgba(255,255,255,0)')

  ctx.fillStyle = gradient
  ctx.fillRect(0, 0, size, size)

  const texture = new THREE.CanvasTexture(canvas)

  if ('colorSpace' in texture) {
    texture.colorSpace = THREE.SRGBColorSpace
  }

  texture.needsUpdate = true
  return texture
}

function buildGalaxy(profile) {
  const { coreCount, starsPerArm, fogCount, bgStarCount } = profile

  galaxyGroup = new THREE.Group()
  scene.add(galaxyGroup)

  glowTexture = createGlowTexture(64)

  if (!glowTexture) {
    throw new Error('Не удалось создать glow texture')
  }

  const tempColor = new THREE.Color()

  // ------------------------------------------------------------
  // 1. ЯДРО
  // ------------------------------------------------------------

  const corePositions = new Float32Array(coreCount * 3)
  const coreColors = new Float32Array(coreCount * 3)

  const coreColor1 = new THREE.Color(0xffdd77)
  const coreColor2 = new THREE.Color(0xffaa44)

  for (let i = 0; i < coreCount; i++) {
    const i3 = i * 3
    const r = Math.pow(Math.random(), 1.5) * 2.5
    const theta = Math.random() * Math.PI * 2
    const phi = Math.acos(2 * Math.random() - 1)

    corePositions[i3] = r * Math.sin(phi) * Math.cos(theta)
    corePositions[i3 + 1] = r * Math.sin(phi) * Math.sin(theta) * 0.6
    corePositions[i3 + 2] = r * Math.cos(phi)

    tempColor.copy(coreColor1).lerp(coreColor2, Math.random())
    coreColors[i3] = tempColor.r
    coreColors[i3 + 1] = tempColor.g
    coreColors[i3 + 2] = tempColor.b
  }

  const coreGeometry = new THREE.BufferGeometry()
  coreGeometry.setAttribute(
    'position',
    new THREE.BufferAttribute(corePositions, 3)
  )
  coreGeometry.setAttribute(
    'color',
    new THREE.BufferAttribute(coreColors, 3)
  )

  corePoints = new THREE.Points(
    coreGeometry,
    new THREE.PointsMaterial({
      size: profile.lowPower ? 0.23 : 0.25,
      map: glowTexture,
      blending: THREE.AdditiveBlending,
      depthWrite: false,
      depthTest: true,
      transparent: true,
      opacity: 0.95,
      vertexColors: true,
      sizeAttenuation: true
    })
  )

  galaxyGroup.add(corePoints)

  // ------------------------------------------------------------
  // 2. СПИРАЛЬНЫЕ РУКАВА
  // ------------------------------------------------------------

  const armCount = 3
  const totalStars = armCount * starsPerArm
  const starPositions = new Float32Array(totalStars * 3)
  const starColors = new Float32Array(totalStars * 3)

  const colorBlue = new THREE.Color(0x6db3f2)
  const colorPurple = new THREE.Color(0xa78bfa)
  const colorPink = new THREE.Color(0xf472b6)
  const colorCyan = new THREE.Color(0x67e8f9)

  let index = 0

  for (let arm = 0; arm < armCount; arm++) {
    const armAngleOffset = (arm / armCount) * Math.PI * 2

    for (let i = 0; i < starsPerArm; i++) {
      const i3 = index * 3
      const t = i / starsPerArm
      const radius = 2.5 + t * 11.5
      const spiralAngle = t * 8 + armAngleOffset
      const spread = 0.3 + t * 0.6

      const offsetX = (Math.random() - 0.5) * spread * 1.2
      const offsetZ = (Math.random() - 0.5) * spread * 1.2
      const offsetY = (Math.random() - 0.5) * 0.5 * (1 - t * 0.5)

      starPositions[i3] = Math.cos(spiralAngle) * radius + offsetX
      starPositions[i3 + 1] = offsetY
      starPositions[i3 + 2] = Math.sin(spiralAngle) * radius + offsetZ

      const rand = Math.random()

      if (radius < 5) {
        tempColor.copy(colorBlue).lerp(colorPurple, rand)
      } else if (radius < 9) {
        tempColor.copy(colorPurple).lerp(colorPink, rand)
      } else {
        tempColor.copy(colorPink).lerp(colorCyan, rand)
      }

      tempColor.multiplyScalar(0.7 + Math.random() * 0.3)

      starColors[i3] = tempColor.r
      starColors[i3 + 1] = tempColor.g
      starColors[i3 + 2] = tempColor.b

      index++
    }
  }

  const starGeometry = new THREE.BufferGeometry()
  starGeometry.setAttribute(
    'position',
    new THREE.BufferAttribute(starPositions, 3)
  )
  starGeometry.setAttribute(
    'color',
    new THREE.BufferAttribute(starColors, 3)
  )

  starPoints = new THREE.Points(
    starGeometry,
    new THREE.PointsMaterial({
      size: profile.lowPower ? 0.25 : 0.3,
      map: glowTexture,
      blending: THREE.AdditiveBlending,
      depthWrite: false,
      depthTest: true,
      transparent: true,
      opacity: 0.9,
      vertexColors: true,
      sizeAttenuation: true
    })
  )

  galaxyGroup.add(starPoints)

  // ------------------------------------------------------------
  // 3. ТУМАННОСТЬ
  // ------------------------------------------------------------

  const fogPositions = new Float32Array(fogCount * 3)
  const fogColors = new Float32Array(fogCount * 3)

  const fogColor1 = new THREE.Color(0x6ee7b7)
  const fogColor2 = new THREE.Color(0xa78bfa)

  for (let i = 0; i < fogCount; i++) {
    const i3 = i * 3
    const radius = 2 + Math.random() * 12
    const angle = Math.random() * Math.PI * 2

    fogPositions[i3] = Math.cos(angle) * radius
    fogPositions[i3 + 1] = (Math.random() - 0.5) * 1.2
    fogPositions[i3 + 2] = Math.sin(angle) * radius

    tempColor.copy(fogColor1).lerp(fogColor2, Math.random())

    fogColors[i3] = tempColor.r * 0.4
    fogColors[i3 + 1] = tempColor.g * 0.4
    fogColors[i3 + 2] = tempColor.b * 0.4
  }

  const fogGeometry = new THREE.BufferGeometry()
  fogGeometry.setAttribute(
    'position',
    new THREE.BufferAttribute(fogPositions, 3)
  )
  fogGeometry.setAttribute(
    'color',
    new THREE.BufferAttribute(fogColors, 3)
  )

  fogPoints = new THREE.Points(
    fogGeometry,
    new THREE.PointsMaterial({
      size: profile.lowPower ? 2.1 : 2.5,
      map: glowTexture,
      blending: THREE.AdditiveBlending,
      depthWrite: false,
      depthTest: true,
      transparent: true,
      opacity: 0.15,
      vertexColors: true,
      sizeAttenuation: true
    })
  )

  galaxyGroup.add(fogPoints)

  // ------------------------------------------------------------
  // 4. ДАЛЁКИЕ ЗВЁЗДЫ
  // ------------------------------------------------------------

  const bgPositions = new Float32Array(bgStarCount * 3)
  const bgColors = new Float32Array(bgStarCount * 3)

  for (let i = 0; i < bgStarCount; i++) {
    const i3 = i * 3
    const radius = 25 + Math.random() * 30
    const theta = Math.random() * Math.PI * 2
    const phi = Math.acos(2 * Math.random() - 1)

    bgPositions[i3] =
      radius * Math.sin(phi) * Math.cos(theta)
    bgPositions[i3 + 1] =
      radius * Math.sin(phi) * Math.sin(theta) * 0.3
    bgPositions[i3 + 2] = radius * Math.cos(phi)

    const brightness = 0.3 + Math.random() * 0.7

    bgColors[i3] = brightness * 0.8
    bgColors[i3 + 1] = brightness * 0.8
    bgColors[i3 + 2] = brightness
  }

  const bgGeometry = new THREE.BufferGeometry()
  bgGeometry.setAttribute(
    'position',
    new THREE.BufferAttribute(bgPositions, 3)
  )
  bgGeometry.setAttribute(
    'color',
    new THREE.BufferAttribute(bgColors, 3)
  )

  bgPoints = new THREE.Points(
    bgGeometry,
    new THREE.PointsMaterial({
      size: profile.lowPower ? 0.1 : 0.12,
      map: glowTexture,
      blending: THREE.AdditiveBlending,
      depthWrite: false,
      depthTest: true,
      transparent: true,
      opacity: 0.6,
      vertexColors: true,
      sizeAttenuation: true
    })
  )

  scene.add(bgPoints)
}

function disposeObject(object) {
  if (!object) return

  if (object.geometry) {
    object.geometry.dispose()
  }

  if (object.material) {
    const materials = Array.isArray(object.material)
      ? object.material
      : [object.material]

    for (const material of materials) {
      material.dispose()
    }
  }
}

function disposeScene() {
  disposeObject(corePoints)
  disposeObject(starPoints)
  disposeObject(fogPoints)
  disposeObject(bgPoints)

  corePoints = null
  starPoints = null
  fogPoints = null
  bgPoints = null

  if (glowTexture) {
    glowTexture.dispose()
    glowTexture = null
  }

  if (scene) {
    scene.clear()
  }

  galaxyGroup = null
}

function renderFrame(now) {
  if (isDestroyed || !isActive || !renderer || !scene || !camera) {
    return
  }

  const width = window.innerWidth
  const targetFps = reducedMotion ? 24 : width <= 768 ? 30 : 45
  const frameInterval = 1000 / targetFps

  if (now - lastFrameTime < frameInterval) {
    animationId = requestAnimationFrame(renderFrame)
    return
  }

  const delta = Math.min(now - lastFrameTime, 100) / 16.6667
  lastFrameTime = now

  if (!reducedMotion && galaxyGroup) {
    galaxyGroup.rotation.y += 0.0008 * delta
    galaxyGroup.rotation.x +=
      (Math.sin(now * 0.0001) * 0.01 - galaxyGroup.rotation.x) * 0.015
  }

  if (!reducedMotion && bgPoints) {
    bgPoints.rotation.y += 0.0001 * delta
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
    const profile = getPerformanceProfile()
    const width = window.innerWidth
    const height = window.innerHeight

    scene = new THREE.Scene()
    scene.background = new THREE.Color(0x050510)

    camera = new THREE.PerspectiveCamera(
      50,
      width / height,
      0.1,
      1000
    )
    camera.position.set(0, 6, 16)
    camera.lookAt(0, 0, 0)

    renderer = new THREE.WebGLRenderer({
      antialias: width > 768,
      alpha: false,
      powerPreference: 'high-performance',
      stencil: false,
      depth: true
    })

    renderer.setPixelRatio(profile.dpr)
    renderer.setSize(width, height, false)

    if ('outputColorSpace' in renderer) {
      renderer.outputColorSpace = THREE.SRGBColorSpace
    }

    if ('toneMapping' in renderer) {
      renderer.toneMapping = THREE.ACESFilmicToneMapping
      renderer.toneMappingExposure = 1
    }

    renderer.domElement.setAttribute('aria-hidden', 'true')
    container.value.appendChild(renderer.domElement)

    buildGalaxy(profile)

    handleResize = () => {
      if (!camera || !renderer) return

      const w = window.innerWidth
      const h = window.innerHeight

      camera.aspect = w / h
      camera.updateProjectionMatrix()

      const nextDpr = Math.min(
        window.devicePixelRatio || 1,
        w <= 768 ? 1.25 : 1.75
      )

      renderer.setPixelRatio(nextDpr)
      renderer.setSize(w, h, false)
    }

    handleVisibilityChange = () => {
      isActive = !document.hidden

      if (!isActive) {
        stopAnimation()
      } else {
        startAnimation()
      }
    }

    window.addEventListener('resize', handleResize, { passive: true })
    document.addEventListener('visibilitychange', handleVisibilityChange)

    startAnimation()
  } catch (error) {
    console.error('[Background3D] initialization error:', error)
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

  disposeScene()

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
  camera = null
  scene = null
})
</script>

<style scoped>
.background-3d {
  position: fixed;
  inset: 0;
  z-index: -10;
  width: 100%;
  height: 100%;
  pointer-events: none;
  overflow: hidden;
  background: #050510;
}

.background-3d :deep(canvas) {
  display: block;
  width: 100%;
  height: 100%;
}
:global(html[data-theme="light"]) .background-3d{background:radial-gradient(circle at 50% 48%,rgba(255,255,255,.94),rgba(236,244,252,.98) 58%,#e8eff7 100%)}
:global(html[data-theme="dark"]) .background-3d{background:#050712}
</style>
