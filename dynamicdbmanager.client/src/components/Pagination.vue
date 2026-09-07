<template>
  <div class="pagination">
    <div class="pagination-summary">
      <span v-if="total">Показано {{ start }}–{{ end }} из {{ total }}</span>
      <span v-else>Нет записей</span>
    </div>

    <div class="pagination-pages">
      <button type="button" class="page-btn arrow" :disabled="current <= 1" @click="emit('change', current - 1)">‹</button>

      <template v-for="(page, index) in pages" :key="`${page}-${index}`">
        <span v-if="page === 'ellipsis'" class="ellipsis">…</span>
        <button v-else type="button" class="page-btn" :class="{ active: page === current }" @click="emit('change', page)">{{ page }}</button>
      </template>

      <button type="button" class="page-btn arrow" :disabled="current >= totalPages" @click="emit('change', current + 1)">›</button>
    </div>

    <label class="page-size">
      <span>Показывать</span>
      <select :value="pageSize" @change="emit('update:pageSize', Number($event.target.value))">
        <option :value="100">100</option>
        <option :value="500">500</option>
        <option :value="1000">1000</option>
      </select>
    </label>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  current: { type: Number, required: true },
  total: { type: Number, required: true },
  pageSize: { type: Number, required: true }
})

const emit = defineEmits(['change', 'update:pageSize'])

const totalPages = computed(() => Math.max(1, Math.ceil(Math.max(0, props.total) / Math.max(1, props.pageSize))))
const start = computed(() => props.total ? ((props.current - 1) * props.pageSize) + 1 : 0)
const end = computed(() => Math.min(props.current * props.pageSize, props.total))

const pages = computed(() => {
  const total = totalPages.value
  const current = props.current

  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1)

  const result = [1]
  if (current > 3) result.push('ellipsis')

  for (let page = Math.max(2, current - 1); page <= Math.min(total - 1, current + 1); page += 1) {
    result.push(page)
  }

  if (current < total - 2) result.push('ellipsis')
  result.push(total)
  return result
})
</script>

<style scoped>
.pagination{display:flex;align-items:center;justify-content:space-between;gap:12px;width:100%;padding:10px 2px 2px;color:var(--table-footer-text)}
.pagination-summary{font-size:11px;white-space:nowrap}
.pagination-pages{display:flex;align-items:center;gap:4px}
.page-btn{min-width:32px;height:32px;padding:0 8px;border:1px solid var(--app-border);border-radius:10px;background:var(--app-control);color:var(--app-text-soft);cursor:pointer;font-size:11px;font-weight:700;transition:.16s ease}
.page-btn:hover:not(:disabled){background:var(--app-control-hover);border-color:var(--app-border-strong);color:var(--app-text)}
.page-btn.active{background:var(--app-primary);border-color:var(--app-primary);color:#fff;box-shadow:0 7px 18px color-mix(in srgb,var(--app-primary) 18%,transparent)}
.page-btn.arrow{font-size:18px;line-height:1}
.page-btn:disabled{opacity:.35;cursor:not-allowed}
.ellipsis{width:24px;text-align:center;color:var(--app-muted)}
.page-size{display:flex;align-items:center;gap:7px;font-size:11px;white-space:nowrap}
.page-size select{height:32px;padding:0 10px;border:1px solid var(--app-border);border-radius:10px;background:var(--app-control);color:var(--app-text);outline:none}
.page-size select:focus{border-color:var(--app-primary-border);box-shadow:0 0 0 3px color-mix(in srgb,var(--app-primary) 10%,transparent)}
@media(max-width:760px){.pagination{flex-wrap:wrap}.pagination-summary{order:3;width:100%}.page-size{margin-left:auto}}
</style>
