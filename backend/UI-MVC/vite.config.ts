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
            file.endsWith('Page.ts') ||
            file.endsWith('adminDeleteModal.ts')
        )
        .reduce((acc, file) => {
            const relativePath = path.relative('./Assets', file);
            const entryName = relativePath.replace(/\\/g, '/');
            acc[entryName] = path.join('./Assets', relativePath);
            return acc;
        }, {} as Record<string, string>);

    return {
        appType: 'custom',
        root: 'Assets',
        publicDir: 'public',
        plugins: [
            tailwindcss(),
        ],
        build: {
            emptyOutDir: true,
            manifest: 'manifest.json',
            outDir: '../wwwroot',
            assetsDir: '',
            rollupOptions: {
                input: inputEntries
            },
        }
    };
});
