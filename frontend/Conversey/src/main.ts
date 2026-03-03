import './style.css'
import { initRouter, registerView, navigate } from './utils/router.ts'
import { renderLandingPage } from './components/landingPage.ts'
import { renderSurveyPage } from './components/survey/surveyPage.ts'
import { renderCompletedPage } from './components/completedPage.ts'

function init(): void {
    initRouter()

    registerView('landing', renderLandingPage)
    registerView('survey', renderSurveyPage)
    registerView('completed', renderCompletedPage)

    navigate('landing')
}

init()
