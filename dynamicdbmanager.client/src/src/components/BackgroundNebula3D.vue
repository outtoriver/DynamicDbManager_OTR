<template>
  <div ref="container" class="background-nebula" aria-hidden="true"></div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'
import * as THREE from 'three'

const container = ref(null)
let scene, camera, renderer, group, animationId, resizeHandler, visibilityHandler
let texture, cloudA, cloudB, stars
let running = true
let last = 0

const reducedMotion = window.matchMedia?.('(prefers-reduced-motion: reduce)').matches ?? false

function glowTexture(size=96){
  const c=document.createElement('canvas'); c.width=c.height=size
  const x=c.getContext('2d'); const g=x.createRadialGradient(size/2,size/2,0,size/2,size/2,size/2)
  g.addColorStop(0,'rgba(255,255,255,1)'); g.addColorStop(.3,'rgba(190,220,255,.85)'); g.addColorStop(.7,'rgba(120,160,255,.24)'); g.addColorStop(1,'rgba(0,0,0,0)')
  x.fillStyle=g; x.fillRect(0,0,size,size)
  const t=new THREE.CanvasTexture(c); if('colorSpace' in t)t.colorSpace=THREE.SRGBColorSpace; return t
}

function makeCloud(count, radius, thickness, colors){
  const pos=new Float32Array(count*3), col=new Float32Array(count*3), tmp=new THREE.Color(), a=new THREE.Color(colors[0]), b=new THREE.Color(colors[1])
  for(let i=0;i<count;i++){
    const i3=i*3, r=Math.pow(Math.random(),.7)*radius, ang=Math.random()*Math.PI*2
    const spread=(Math.random()-.5)*thickness*(.3+r/radius)
    pos[i3]=Math.cos(ang)*r + spread
    pos[i3+1]=(Math.random()-.5)*thickness*(1-r/radius)
    pos[i3+2]=Math.sin(ang)*r + spread
    tmp.copy(a).lerp(b,Math.random()); const boost=.35+Math.random()*.65
    col[i3]=tmp.r*boost; col[i3+1]=tmp.g*boost; col[i3+2]=tmp.b*boost
  }
  const g=new THREE.BufferGeometry(); g.setAttribute('position',new THREE.BufferAttribute(pos,3)); g.setAttribute('color',new THREE.BufferAttribute(col,3))
  const m=new THREE.PointsMaterial({size:1.8,map:texture,transparent:true,opacity:.12,depthWrite:false,blending:THREE.AdditiveBlending,vertexColors:true,sizeAttenuation:true})
  return new THREE.Points(g,m)
}

function build(){
  const mobile=window.innerWidth<=768
  const count=mobile?700:1400
  group=new THREE.Group(); scene.add(group)
  cloudA=makeCloud(count,15,5,['0x6ee7f9','0x6366f1']); cloudA.rotation.z=.35; group.add(cloudA)
  cloudB=makeCloud(Math.round(count*.75),11,4,['0xa78bfa','0xec4899']); cloudB.rotation.z=-.6; group.add(cloudB)

  const starCount=mobile?350:700
  const p=new Float32Array(starCount*3), c=new Float32Array(starCount*3), temp=new THREE.Color()
  for(let i=0;i<starCount;i++){const i3=i*3,r=24+Math.random()*26,t=Math.random()*Math.PI*2,u=Math.acos(2*Math.random()-1);p[i3]=r*Math.sin(u)*Math.cos(t);p[i3+1]=r*Math.sin(u)*Math.sin(t)*.35;p[i3+2]=r*Math.cos(u);const br=.25+Math.random()*.65;temp.setHSL(.58+Math.random()*.1,.55,br);c[i3]=temp.r;c[i3+1]=temp.g;c[i3+2]=temp.b}
  const sg=new THREE.BufferGeometry(); sg.setAttribute('position',new THREE.BufferAttribute(p,3)); sg.setAttribute('color',new THREE.BufferAttribute(c,3))
  stars=new THREE.Points(sg,new THREE.PointsMaterial({size:.1,map:texture,transparent:true,opacity:.55,depthWrite:false,blending:THREE.AdditiveBlending,vertexColors:true,sizeAttenuation:true})); scene.add(stars)
}

function frame(now){
  if(!running){animationId=null;return}
  const interval=1000/(window.innerWidth<=768?30:45)
  if(now-last<interval){animationId=requestAnimationFrame(frame);return}
  const dt=Math.min(64,now-(last||now-16))/16.67; last=now
  if(!reducedMotion){group.rotation.y+=.00035*dt; group.rotation.x=Math.sin(now*.00008)*.035; cloudA.rotation.z+=.00011*dt; cloudB.rotation.z-=.00008*dt; stars.rotation.y+=.00007*dt}
  renderer.render(scene,camera); animationId=requestAnimationFrame(frame)
}
function start(){if(!animationId){last=performance.now();animationId=requestAnimationFrame(frame)}}
function stop(){if(animationId)cancelAnimationFrame(animationId);animationId=null}
function dispose(o){if(!o)return;o.geometry?.dispose();const m=o.material; if(m){(Array.isArray(m)?m:[m]).forEach(x=>{x.map?.dispose();x.dispose()})}}

onMounted(()=>{
  const w=window.innerWidth,h=window.innerHeight
  scene=new THREE.Scene(); camera=new THREE.PerspectiveCamera(52,w/h,.1,1000); camera.position.set(0,5,18); camera.lookAt(0,0,0)
  renderer=new THREE.WebGLRenderer({antialias:w>768,alpha:false,powerPreference:'high-performance',stencil:false}); renderer.setPixelRatio(Math.min(window.devicePixelRatio||1,w>768?1.5:1.2)); renderer.setSize(w,h,false); renderer.setClearColor(0x050712,1); if('outputColorSpace' in renderer)renderer.outputColorSpace=THREE.SRGBColorSpace; container.value.appendChild(renderer.domElement)
  texture=glowTexture(); build();
  resizeHandler=()=>{const w2=window.innerWidth,h2=window.innerHeight;camera.aspect=w2/h2;camera.updateProjectionMatrix();renderer.setPixelRatio(Math.min(window.devicePixelRatio||1,w2>768?1.5:1.2));renderer.setSize(w2,h2,false)}
  visibilityHandler=()=>{running=!document.hidden; if(running)start(); else stop()}
  window.addEventListener('resize',resizeHandler,{passive:true}); document.addEventListener('visibilitychange',visibilityHandler); start()
})

onBeforeUnmount(()=>{stop();window.removeEventListener('resize',resizeHandler);document.removeEventListener('visibilitychange',visibilityHandler);[cloudA,cloudB,stars].forEach(dispose);texture?.dispose();scene?.clear();renderer?.dispose();renderer?.forceContextLoss?.();if(renderer?.domElement?.parentNode===container.value)container.value.removeChild(renderer.domElement);scene=null;camera=null;renderer=null})
</script>

<style scoped>
.background-nebula{position:fixed;inset:0;z-index:-10;width:100%;height:100%;pointer-events:none;overflow:hidden;background:#050712}
.background-nebula :deep(canvas){display:block;width:100%;height:100%}
</style>
