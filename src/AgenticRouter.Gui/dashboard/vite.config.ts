import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  optimizeDeps: {
    exclude: ['lucide-react'],
  },
  // Photino loads the built app from the local filesystem (Load("wwwroot/index.html")), so asset
  // references must be relative rather than absolute ("/assets/...") for them to resolve correctly.
  base: './',
  build: {
    // Emit straight into the Photino host's wwwroot so `npm run build` here is the single step that
    // updates what AgenticRouter.Gui serves. This output is generated - see wwwroot/.gitignore.
    outDir: '../wwwroot',
    emptyOutDir: true,
  },
});
