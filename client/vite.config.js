import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import path from 'path';
// https://vite.dev/config/
export default defineConfig({
    plugins: [react(),
        tailwindcss()
    ],
    resolve: {
        alias: {
            '@core': path.resolve(__dirname, './src/core'),
            '@utilities': path.resolve(__dirname, './src/utilities'),
            '@components': path.resolve(__dirname, './src/components')
        }
    }
});
