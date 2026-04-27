import { UserConfig, defineConfig } from 'vite';
import fs from 'fs';
import path from 'path';
import tailwindcss from "@tailwindcss/vite";


export default defineConfig(async () => {
    const files = fs.readdirSync('./Assets');
    const inputEntries = files
        .filter(file => file.endsWith('.ts') )
        .reduce((acc, file) => {
            const fileName = path.parse(file).name;
            acc[fileName] = path.join('./Assets', file);
            return acc;
        }, {} as Record<string, string>);

    const config: UserConfig = {
        appType: 'custom',
        root: 'Assets',
        publicDir: 'public',
        plugins: [
            tailwindcss(),
        ],
        build: {
            emptyOutDir: true,
            manifest: true,
            outDir: '../wwwroot',
            assetsDir: '',
            rollupOptions: {
                input: inputEntries
            },
        },
        server: {
            strictPort: true,
            // host: true,
            // cors: {
            //     origin: [
            //         'http://localhost:4180',
            //         'https://localhost:7093',
            //         'http://hogeschool-nova.localhost:4180',
            //         'https://hogeschool-nova.localhost:7093'
            //     ],
            //     credentials: true
            // }
        },
        optimizeDeps: {
            include: []
        }
    }
    return config;
});
