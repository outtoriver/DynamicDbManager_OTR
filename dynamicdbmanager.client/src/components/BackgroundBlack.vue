<template><div class="background-black" aria-hidden="true"></div></template>

<style scoped>
.background-black{position:fixed;inset:0;z-index:-10;pointer-events:none;background:var(--bg-black)}
:global(:root){--bg-black:#02040a}
:global(html[data-theme="light"]){--bg-black:#ffffff}
</style>
