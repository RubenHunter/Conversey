import './style.css'
import { initRouter, registerView, navigate, getInitialView } from './utils/router.ts'
import { renderLandingPage } from './components/landingPage.ts'
import { renderSurveyPage } from './components/survey/surveyPage.ts'
import { renderCompletedPage } from './components/completedPage.ts'

function syncViewportHeightVar(): void {
    const vh = window.innerHeight * 0.01
    document.documentElement.style.setProperty('--app-vh', `${vh}px`)
}

function init(): void {
    syncViewportHeightVar()
    window.addEventListener('resize', syncViewportHeightVar)
    window.addEventListener('orientationchange', syncViewportHeightVar)

    initRouter()

    registerView('landing', renderLandingPage)
    registerView('survey', renderSurveyPage)
    registerView('completed', renderCompletedPage)

    const initialView = getInitialView()
    void navigate(initialView, { replace: true })
}

init()
