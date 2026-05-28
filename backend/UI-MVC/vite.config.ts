import { UserConfig, defineConfig } from 'vite';
import fs from 'fs';
import path from 'path';
import tailwindcss from "@tailwindcss/vite";


export default defineConfig(async () => {
    // Entry points: files directly in Assets/ plus files referenced in views via vite-src
    const files = fs.readdirSync('./Assets');
    const inputEntries = files
        .filter(file => file.endsWith('.ts'))
        .reduce((acc, file) => {
            const fileName = path.parse(file).name;
            acc[fileName] = path.join('./Assets', file);
            return acc;
        }, {} as Record<string, string>);

    // Add entry points from view files (vite-src attributes)
    // These are referenced in _Layout.cshtml and other views
    const additionalEntries = {
        'components_adminDeleteModal': './Assets/components/shared/adminDeleteModal.ts',
        'components_completedPage': './Assets/components/shared/completedPage.ts',
        'components_ideas_ideasPage': './Assets/components/ideas/pages/ideasPage.ts',
        'components_survey_surveyPage': './Assets/components/survey/pages/surveyPage.ts',
        'components_projectsPage': './Assets/components/shared/projectsPage.ts',
        'components_questionStepper': './Assets/components/admin/addquestion/questionStepper.ts',
        'components_topicManager': './Assets/components/shared/topicManager.ts',
        'components_limitsPage': './Assets/components/aiWorkspace/limitsPage.ts',
        'components_listPage': './Assets/components/analytics/listPage.ts',
        'components_shared_previewSurvey': './Assets/components/shared/previewSurvey.ts',
        'components_shared_adminHeader': './Assets/components/shared/adminHeader.ts',
        'components_shared_langDropdown': './Assets/components/shared/langDropdown.ts',
        'components_shared_adminI18nBindings': './Assets/components/shared/adminI18nBindings.ts',
        'components_shared_projectArchiveModal': './Assets/components/shared/projectArchiveModal.ts',
        'components_shared_listSearch': './Assets/components/shared/listSearch.ts',
        'components_shared_createProjectStepper': './Assets/components/shared/createProjectStepper.ts',
        'components_shared_passwordVisibilityToggle': './Assets/components/shared/passwordVisibilityToggle.ts',
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
