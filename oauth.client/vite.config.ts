import { fileURLToPath, URL } from 'node:url';

import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import fs from 'fs';
import path from 'path';
import child_process from 'child_process';
import { env } from 'process';

const baseFolder =
    env.APPDATA !== undefined && env.APPDATA !== ''
        ? `${env.APPDATA}/ASP.NET/https`
        : `${env.HOME}/.aspnet/https`;

const certificateName = "oauth.client";
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
    if (0 !== child_process.spawnSync('dotnet', [
        'dev-certs',
        'https',
        '--export-path',
        certFilePath,
        '--format',
        'Pem',
        '--no-password',
    ], { stdio: 'inherit', }).status) {
        throw new Error("Could not create certificate.");
    }
}

const target = 'https://localhost:7079';
    

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url))
        }
    },
    server: {
        proxy: {
            '/login-github': {
                target: 'https://localhost:7079',
                changeOrigin: true,
                secure: false
            },
            '/login-google': {
                target: 'https://localhost:7079',
                changeOrigin: true,
                secure: false
            },
            '/login-twitter': {
                target: 'https://localhost:7079',
                changeOrigin: true,
                secure: false
            },
            '/login-check': {
                target: 'https://localhost:7079',
                changeOrigin: true,
                secure: false
            },
            '/signin': {
                target: 'https://localhost:7079',
                changeOrigin: true,
                secure: false
            },
            '/register': {
                target: 'https://localhost:7079',
                changeOrigin: true,
                secure: false
            },
            '/logout': {
                target: 'https://localhost:7079',
                changeOrigin: true,
                secure: false
            }
        },
        port: 5173,
        https: {
            key: fs.readFileSync(keyFilePath),
            cert: fs.readFileSync(certFilePath),
        }
    }
})
