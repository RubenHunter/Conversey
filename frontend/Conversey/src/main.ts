import './style.css'
import { initRouter, registerView, navigate, getInitialView } from './utils/router.ts'
import { renderLandingPage } from './components/landingPage.ts'
import { renderSurveyPage } from './components/survey/surveyPage.ts'
import { renderCompletedPage } from './components/completedPage.ts'
import { renderIdeasPage } from './components/ideasPage.ts'
import { renderWorkspaceTestPage } from './components/workspaceTestPage.ts'

function init(): void {
    initRouter()

    registerView('landing', renderLandingPage)
    registerView('survey', renderSurveyPage)
    registerView('completed', renderCompletedPage)
    registerView('ideas', renderIdeasPage)
    registerView('workspace-test', renderWorkspaceTestPage)

    const initialView = getInitialView()
    void navigate(initialView, { replace: true })
}

init()
