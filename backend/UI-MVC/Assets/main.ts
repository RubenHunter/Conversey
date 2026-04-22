import "./main.css";
import { initRouter, registerView, navigate, getInitialView } from './utils/router.ts'
import { renderLandingPage } from './components/landingPage.ts'
import { renderSurveyPage } from './components/survey/surveyPage.ts'
import { renderCompletedPage } from './components/completedPage.ts'
import { renderIdeasPage } from './components/ideas/ideasPage.ts'

function init(): void {
	initRouter()

	registerView('landing', renderLandingPage)
	registerView('survey', renderSurveyPage)
	registerView('completed', renderCompletedPage)
	registerView('ideas', renderIdeasPage)

	const initialView = getInitialView()
	void navigate(initialView, { replace: true })
}

init()
