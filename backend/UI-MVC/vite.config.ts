import { UserConfig, defineConfig } from 'vite';
import fs from 'fs';
import path from 'path';
import tailwindcss from "@tailwindcss/vite";


export default defineConfig(async () => {
    // Entry points: files directly in Assets/ plus files referenced in views via vite-src
    const files = fs.readdirSync('./Assets');
    const inputEntries = files
        .filter(file => file.endsWith('.ts') )
        .reduce((acc, file) => {
            const fileName = path.parse(file).name;
            acc[fileName] = path.join('./Assets', file);
            return acc;
        }, {} as Record<string, string>);
    
    // Add entry points from view files (vite-src attributes)
    // These are referenced in _Layout.cshtml and other views
    const additionalEntries = {
        'components_adminDeleteModal': './Assets/components/adminDeleteModal.ts',
        'components_completedPage': './Assets/components/completedPage.ts',
        'components_ideas_ideasPage': './Assets/components/ideas/ideasPage.ts',
        'components_survey_surveyPage': './Assets/components/survey/surveyPage.ts',
    };
    
    Object.assign(inputEntries, additionalEntries);

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
