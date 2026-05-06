export type SurveyLocale = 'nl' | 'en' | 'fr'

export interface SurveyStrings {
    selectAbove: string
    typeHere: string
    typeBelow: string
    pleaseFill: string
    pleaseFillChoice: string
    allDone: string
    submitSurvey: string
    submitting: string
    submittedTitle: string
    submittedSub: string
    somethingWrong: string
    offensiveLanguage: string
    readAloud: string
    magicMode: string
    communityIdeas: string
    broadSelection: string
    similarIdeas: string
    differingIdeas: string
    allIdeas: string
    myIdeas: string
    topicLabel: string
    ideationIntro: string
    ideaShared: string
    shareIdea: string
    shareAnother: string
    selectTopicToShare: string
    noQuestions: string
    noIdeas: string
    resuming: string
    ideaCategories: string
    requiredLabel: string
    layoutPickerTitle: string
    layoutPickerClassic: string
    layoutPickerClassicDesc: string
    layoutPickerChat: string
    layoutPickerChatDesc: string
    layoutPickerSave: string
    exploreIdeas: string
    loadMoreIdeas: string
    loadingMoreIdeas: string
    selectTopic: string
    thoughtsOnTopic: string
    surveyCompleted: string
    noIdeasHere: string
    noIdeasYetBeFirst: string
    noSimilarIdeasFound: string
    noContrastingIdeasFound: string
    useAsStarter: string
    editIdeaBeforePublish: string
    submitIdea: string
    noIdeasMyIdeas: string
    noIdeasForView: string
    saveChanges: string
    loadingResponses: string
    couldNotLoadResponses: string
    submitFailed: string
    justNow: string
    minutesAgo: string
    hoursAgo: string
    daysAgo: string
    weeksAgo: string
    mostSimilar: string
    leastSimilar: string
    mostSimilarIdeas: string
    leastSimilarIdeas: string
    noResponsesYet: string
}

const nl: SurveyStrings = {
    selectAbove: 'Selecteer je antwoord hierboven...',
    typeHere: 'Typ je antwoord hier...',
    typeBelow: 'Typ je antwoord in de chatbalk hieronder',
    pleaseFill: 'Typ je antwoord in voor je verder gaat.',
    pleaseFillChoice: 'Selecteer een antwoord voor je verder gaat.',
    allDone: 'Je hebt alle vragen beantwoord — goed gedaan! Klaar om je antwoorden in te sturen?',
    submitSurvey: 'Antwoorden bevestigen',
    submitting: 'Bezig met versturen...',
    submittedTitle: 'Succesvol ingediend!',
    submittedSub: 'Overgaan naar ideeënfase...',
    somethingWrong: 'Er is iets misgegaan. Probeer het opnieuw.',
    offensiveLanguage: 'Aanstootgevend taalgebruik gedetecteerd',
    readAloud: 'Voorlezen',
    magicMode: 'Magic',
    communityIdeas: 'Ideeën van de community',
    broadSelection: 'Brede selectie',
    similarIdeas: 'Vergelijkbare ideeën',
    differingIdeas: 'Afwijkende ideeën',
    allIdeas: 'Alle ideeën',
    myIdeas: 'Mijn ideeën',
    topicLabel: 'Thema',
    ideationIntro: 'Je hebt de enquête afgerond — dank je wel! Nu is het tijd om ideeën te delen met de community.',
    ideaShared: 'Je idee is gedeeld met de community!',
    shareIdea: 'Deel een idee...',
    shareAnother: 'Deel nog een idee...',
    selectTopicToShare: 'Selecteer een thema hierboven om een idee te delen...',
    noQuestions: 'Er zijn nog geen vragen voor deze enquête.',
    noIdeas: 'Nog geen ideeën gedeeld. Wees de eerste!',
    resuming: 'We gaan verder waar je gebleven was...',
    ideaCategories: 'Ideeëncategorieën',
    requiredLabel: 'Verplicht',
    layoutPickerTitle: 'Hoe wil je de enquête invullen?',
    layoutPickerClassic: 'Klassieke weergave',
    layoutPickerClassicDesc: 'Scroll door de vragen op je eigen tempo.',
    layoutPickerChat: 'Chatweergave',
    layoutPickerChatDesc: 'Beantwoord vragen in een gespreksstijl.',
    layoutPickerSave: 'Mijn keuze onthouden',
    exploreIdeas: 'Ideeën verkennen',
    loadMoreIdeas: 'Klik of scroll naar beneden om 7 meer ideeën te laden',
    loadingMoreIdeas: '7 meer ideeën laden...',
    selectTopic: 'Selecteer een thema',
    thoughtsOnTopic: 'Wat zijn je gedachten over: "{topicTitle}"?',
    surveyCompleted: 'Bedankt voor het invullen van de enquête! Je antwoorden zijn opgeslagen.',
    noIdeasHere: 'Nog geen ideeën hier.',
    noIdeasYetBeFirst: 'Nog geen ideeën gedeeld. Wees de eerste!',
    noSimilarIdeasFound: 'Je idee lijkt super origineel — nog geen vergelijkbare ideeën gevonden.',
    noContrastingIdeasFound: 'Nog geen duidelijk afwijkende ideeën gevonden.',
    useAsStarter: 'Gebruik als startpunt',
    editIdeaBeforePublish: 'Bewerk idee voor publicatie',
    submitIdea: 'Deel een idee',
    noIdeasMyIdeas: 'Je hebt nog geen ideeën ingediend.',
    noIdeasForView: 'Nog geen ideeën voor deze weergave.',
    saveChanges: 'Wijzigingen opslaan',
    loadingResponses: 'Reacties laden...',
    couldNotLoadResponses: 'Kon reacties niet laden. Probeer dit idee opnieuw te openen.',
    submitFailed: 'Enquête versturen mislukt. Probeer opnieuw.',
    justNow: 'Zojuist',
    minutesAgo: '{n}m geleden',
    hoursAgo: '{n}u geleden',
    daysAgo: '{n}d geleden',
    weeksAgo: '{n}w geleden',
    mostSimilar: 'Meest gelijkend',
    leastSimilar: 'Minst gelijkend',
    mostSimilarIdeas: 'Meest vergelijkbare ideeën',
    leastSimilarIdeas: 'Minst vergelijkbare ideeën',
    noResponsesYet: 'Nog geen reacties. Wees de eerste!',
}

const en: SurveyStrings = {
    selectAbove: 'Select your answer above...',
    typeHere: 'Type your answer here...',
    typeBelow: 'Type your answer in the chat bar below',
    pleaseFill: 'Please type your answer before continuing.',
    pleaseFillChoice: 'Please select an answer before continuing.',
    allDone: "You've answered all the questions — well done! Ready to submit your responses?",
    submitSurvey: 'Submit Survey',
    submitting: 'Submitting...',
    submittedTitle: 'Submitted successfully!',
    submittedSub: 'Moving to ideation phase...',
    somethingWrong: 'Sorry, something went wrong. Please try again.',
    offensiveLanguage: 'Offensive language detected',
    readAloud: 'Read aloud',
    magicMode: 'Magic',
    communityIdeas: 'Community ideas',
    broadSelection: 'Broad selection',
    similarIdeas: 'Similar ideas',
    differingIdeas: 'Differing ideas',
    allIdeas: 'All ideas',
    myIdeas: 'My ideas',
    topicLabel: 'Topic',
    ideationIntro: "You've completed the survey — thank you! Now let's share ideas with the community.",
    ideaShared: 'Your idea has been shared with the community!',
    shareIdea: 'Share your idea...',
    shareAnother: 'Share another idea...',
    selectTopicToShare: 'Select a topic above to share your idea...',
    noQuestions: 'There are no questions for this survey yet.',
    noIdeas: 'No ideas shared yet. Be the first!',
    resuming: "Welcome back — let's pick up where you left off.",
    ideaCategories: 'Idea categories',
    requiredLabel: 'Required',
    layoutPickerTitle: 'How would you like to fill in the survey?',
    layoutPickerClassic: 'Classic layout',
    layoutPickerClassicDesc: 'Scroll through the questions at your own pace.',
    layoutPickerChat: 'Chat layout',
    layoutPickerChatDesc: 'Answer questions in a conversational style.',
    layoutPickerSave: 'Remember my choice',
    exploreIdeas: 'Explore ideas',
    loadMoreIdeas: 'Click or scroll down to load 7 more ideas',
    loadingMoreIdeas: 'Loading 7 more ideas...',
    selectTopic: 'Select a topic',
    thoughtsOnTopic: 'What are your thoughts on: "{topicTitle}"?',
    surveyCompleted: 'Thank you for completing the survey! Your responses have been recorded.',
    noIdeasHere: 'No ideas here yet.',
    noIdeasYetBeFirst: 'No ideas have been shared yet. Be the first!',
    noSimilarIdeasFound: 'Your idea seems super original — no similar ideas found yet.',
    noContrastingIdeasFound: 'No clearly contrasting ideas found yet.',
    useAsStarter: 'Use as starter',
    editIdeaBeforePublish: 'Edit idea before publish',
    submitIdea: 'Submit Idea',
    noIdeasMyIdeas: 'You have not submitted any ideas yet.',
    noIdeasForView: 'No ideas yet for this view.',
    saveChanges: 'Save changes',
    loadingResponses: 'Loading responses...',
    couldNotLoadResponses: 'Could not load responses right now. Try reopening this idea.',
    submitFailed: 'Failed to submit survey. Please try again.',
    justNow: 'Just now',
    minutesAgo: '{n}m ago',
    hoursAgo: '{n}h ago',
    daysAgo: '{n}d ago',
    weeksAgo: '{n}w ago',
    mostSimilar: 'Most similar',
    leastSimilar: 'Least similar',
    mostSimilarIdeas: 'Most similar ideas',
    leastSimilarIdeas: 'Least similar ideas',
    noResponsesYet: 'No responses yet. Be the first!',
}

const fr: SurveyStrings = {
    selectAbove: `Sélectionnez votre réponse ci-dessus...`,
    typeHere: `Tapez votre réponse ici...`,
    typeBelow: `Tapez votre réponse dans la barre de chat ci-dessous`,
    pleaseFill: `Veuillez saisir votre réponse avant de continuer.`,
    pleaseFillChoice: `Veuillez sélectionner une réponse avant de continuer.`,
    allDone: `Vous avez répondu à toutes les questions — bravo ! Prêt(e) à envoyer vos réponses ?`,
    submitSurvey: `Envoyer les réponses`,
    submitting: `Envoi en cours...`,
    submittedTitle: `Envoyé avec succès !`,
    submittedSub: `Passage à la phase d'idéation...`,
    somethingWrong: `Une erreur s'est produite. Veuillez réessayer.`,
    offensiveLanguage: `Langage offensant détecté`,
    readAloud: `Lire à voix haute`,
    magicMode: `Magic`,
    communityIdeas: `Idées de la communauté`,
    broadSelection: `Large sélection`,
    similarIdeas: `Idées similaires`,
    differingIdeas: `Idées divergentes`,
    allIdeas: `Toutes les idées`,
    myIdeas: `Mes idées`,
    topicLabel: `Thème`,
    ideationIntro: `Vous avez terminé l'enquête — merci ! Il est maintenant temps de partager vos idées avec la communauté.`,
    ideaShared: `Votre idée a été partagée avec la communauté !`,
    shareIdea: `Partagez une idée...`,
    shareAnother: `Partagez une autre idée...`,
    selectTopicToShare: `Sélectionnez un thème ci-dessus pour partager votre idée...`,
    noQuestions: `Il n'y a pas encore de questions pour cette enquête.`,
    noIdeas: `Aucune idée partagée pour l'instant. Soyez le premier !`,
    resuming: `Bienvenue à nouveau — reprenons là où vous en étiez.`,
    ideaCategories: `Catégories d'idées`,
    requiredLabel: `Obligatoire`,
    layoutPickerTitle: `Comment souhaitez-vous remplir l'enquête ?`,
    layoutPickerClassic: `Mise en page classique`,
    layoutPickerClassicDesc: `Faites défiler les questions à votre propre rythme.`,
    layoutPickerChat: `Mise en page chat`,
    layoutPickerChatDesc: `Répondez aux questions dans un style conversationnel.`,
    layoutPickerSave: `Retenir mon choix`,
    exploreIdeas: `Explorer les idées`,
    loadMoreIdeas: `Cliquez ou faites défiler vers le bas pour charger 7 idées supplémentaires`,
    loadingMoreIdeas: `Chargement de 7 idées supplémentaires...`,
    selectTopic: `Sélectionner un thème`,
    thoughtsOnTopic: `Quelles sont vos pensées sur : "{topicTitle}" ?`,
    surveyCompleted: `Merci d'avoir complété l'enquête ! Vos réponses ont été enregistrées.`,
    noIdeasHere: `Pas d'idées ici pour l'instant.`,
    noIdeasYetBeFirst: `Aucune idée partagée pour l'instant. Soyez le premier !`,
    noSimilarIdeasFound: `Votre idée semble super originale — aucune idée similaire trouvée.`,
    noContrastingIdeasFound: `Aucune idée contrastée claire trouvée pour l'instant.`,
    useAsStarter: `Utiliser comme point de départ`,
    editIdeaBeforePublish: `Modifier l'idée avant publication`,
    submitIdea: `Soumettre l'idée`,
    noIdeasMyIdeas: `Vous n'avez pas encore soumis d'idées.`,
    noIdeasForView: `Pas encore d'idées pour cette vue.`,
    saveChanges: `Enregistrer les modifications`,
    loadingResponses: `Chargement des réponses...`,
    couldNotLoadResponses: `Impossible de charger les réponses maintenant. Essayez de rouvrir cette idée.`,
    submitFailed: `Échec de l'envoi de l'enquête. Veuillez réessayer.`,
    justNow: `À l'instant`,
    minutesAgo: `il y a {n} min`,
    hoursAgo: `il y a {n}h`,
    daysAgo: `il y a {n}j`,
    weeksAgo: `il y a {n} sem`,
    mostSimilar: `Plus similaire`,
    leastSimilar: `Moins similaire`,
    mostSimilarIdeas: `Idées les plus similaires`,
    leastSimilarIdeas: `Idées les moins similaires`,
    noResponsesYet: `Pas encore de réponses. Soyez le premier !`,
}

const translations: Record<SurveyLocale, SurveyStrings> = { nl, en, fr }

export function detectLocale(): SurveyLocale {
    const lang = navigator.language.toLowerCase()
    if (lang.startsWith('en')) return 'en'
    if (lang.startsWith('fr')) return 'fr'
    return 'nl'
}

export function getSurveyStrings(): SurveyStrings {
    return translations[detectLocale()]
}
