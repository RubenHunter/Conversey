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
        // components/admin
        'components_admin_sidebarNavigation': './Assets/components/admin/sidebarNavigation.ts',
        'components_admin_questionStepper': './Assets/components/admin/addquestion/questionStepper.ts',
        // components/aiAdmin
        'components_aiAdmin_indexPage': './Assets/components/aiAdmin/indexPage.ts',
        'components_aiAdmin_keywordsPage': './Assets/components/aiAdmin/keywordsPage.ts',
        'components_aiAdmin_pricingPage': './Assets/components/aiAdmin/pricingPage.ts',
        'components_aiAdmin_promptsPage': './Assets/components/aiAdmin/promptsPage.ts',
        'components_aiAdmin_providerEditPage': './Assets/components/aiAdmin/providerEditPage.ts',
        'components_aiAdmin_providerSetupModelsStep': './Assets/components/aiAdmin/providerSetupModelsStep.ts',
        'components_aiAdmin_providerSetupProviderStep': './Assets/components/aiAdmin/providerSetupProviderStep.ts',
        'components_aiAdmin_providerSetupState': './Assets/components/aiAdmin/providerSetupState.ts',
        'components_aiAdmin_providerSetupVerifyStep': './Assets/components/aiAdmin/providerSetupVerifyStep.ts',
        'components_aiAdmin_providersPage': './Assets/components/aiAdmin/providersPage.ts',
        // components/aiWorkspace
        'components_aiWorkspace_limitsPage': './Assets/components/aiWorkspace/limitsPage.ts',
        // components/analytics
        'components_analytics_converseyAnalyticsPage': './Assets/components/analytics/converseyAnalyticsPage.ts',
        'components_analytics_listPage': './Assets/components/analytics/listPage.ts',
        'components_analytics_moderationPage': './Assets/components/analytics/moderationPage.ts',
        'components_analytics_workspaceAnalyticsPage': './Assets/components/analytics/workspaceAnalyticsPage.ts',
        // components/ideas
        'components_ideas_ideasPage': './Assets/components/ideas/pages/ideasPage.ts',
        // components/survey
        'components_survey_surveyPage': './Assets/components/survey/pages/surveyPage.ts',
        // components/shared
        'components_shared_adminDeleteModal': './Assets/components/shared/adminDeleteModal.ts',
        'components_shared_adminHeader': './Assets/components/shared/adminHeader.ts',
        'components_shared_adminI18nBindings': './Assets/components/shared/adminI18nBindings.ts',
        'components_shared_adminManagementModals': './Assets/components/shared/adminManagementModals.ts',
        'components_shared_adminProfilePasswordModal': './Assets/components/shared/adminProfilePasswordModal.ts',
        'components_shared_adminProfileWorkspaceLogo': './Assets/components/shared/adminProfileWorkspaceLogo.ts',
        'components_shared_completedPage': './Assets/components/shared/completedPage.ts',
        'components_shared_costCharts': './Assets/components/shared/costCharts.ts',
        'components_shared_createProjectStepper': './Assets/components/shared/createProjectStepper.ts',
        'components_shared_forcePasswordChangeModal': './Assets/components/shared/forcePasswordChangeModal.ts',
        'components_shared_infoToggle': './Assets/components/shared/infoToggle.ts',
        'components_shared_langDropdown': './Assets/components/shared/langDropdown.ts',
        'components_shared_listSearch': './Assets/components/shared/listSearch.ts',
        'components_shared_passwordVisibilityToggle': './Assets/components/shared/passwordVisibilityToggle.ts',
        'components_shared_previewSurvey': './Assets/components/shared/previewSurvey.ts',
        'components_shared_projectArchiveModal': './Assets/components/shared/projectArchiveModal.ts',
        'components_shared_projectsPage': './Assets/components/shared/projectsPage.ts',
        'components_shared_topicManager': './Assets/components/shared/topicManager.ts',
        'components_shared_workspaceAdminManagementModals': './Assets/components/shared/workspaceAdminManagementModals.ts',
        'components_shared_workspaceModalForms': './Assets/components/shared/workspaceModalForms.ts',
        // modules
        'modules_addQuestionPage': './Assets/modules/addQuestionPage.ts',
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
