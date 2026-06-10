import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
    plugins: [react()],
    server: {
        port: parseInt(process.env.PORT || '5173'),
        strictPort: true,
        proxy: {
            '/agent': {
                target: process.env.services__hrassistant__https__0
                    || process.env.services__hrassistant__http__0
                    || 'https://localhost:7297',
                changeOrigin: true,
                secure: false,
            }
        }
    }
})