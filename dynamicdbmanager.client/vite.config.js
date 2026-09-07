import { fileURLToPath, URL } from 'node:url';
import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import tailwindcss from '@tailwindcss/vite';

export default defineConfig({
  plugins: [
    vue(),
    tailwindcss(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5242', // или 
        changeOrigin: true,
        secure: false,                    // отключаем проверку SSL для самоподписанного сертификата
        // rewrite: (path) => path.replace(/^\/api/, ''), // если нужно убрать префикс (обычно не надо)
      }
    }
  }
});
