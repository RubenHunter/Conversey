import { UserConfig, defineConfig } from 'vite';
import fs from 'fs';
import path from 'path';
import tailwindcss from "@tailwindcss/vite";


export default defineConfig(async () => {
    const getFiles = (dir: string): string[] => {
        const subdirs = fs.readdirSync(dir);
        const files = subdirs.map((subdir) => {
            const res = path.resolve(dir, subdir);
            return fs.statSync(res).isDirectory() ? getFiles(res) : res;
        });
        return Array.prototype.concat(...files);
    };

    const allFiles = getFiles('./Assets');
    const inputEntries = allFiles
        .filter(file => 
            file.endsWith('main.ts') || 
            file.endsWith('Page.ts')
        )
        .reduce((acc, file) => {
            const relativePath = path.relative('./Assets', file);
            const fileName = relativePath.replace(/\\/g, '/').replace(/\.ts$/, '');
            acc[fileName] = path.join('./Assets', relativePath);
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
