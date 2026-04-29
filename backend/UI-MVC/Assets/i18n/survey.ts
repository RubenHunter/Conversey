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
}

const fr: SurveyStrings = {
    selectAbove: 'Sélectionnez votre réponse ci-dessus...',
    typeHere: 'Tapez votre réponse ici...',
    typeBelow: 'Tapez votre réponse dans la barre de chat ci-dessous',
    pleaseFill: 'Veuillez saisir votre réponse avant de continuer.',
    pleaseFillChoice: 'Veuillez sélectionner une réponse avant de continuer.',
    allDone: 'Vous avez répondu à toutes les questions — bravo ! Prêt(e) à envoyer vos réponses ?',
    submitSurvey: 'Envoyer les réponses',
    submitting: 'Envoi en cours...',
    submittedTitle: 'Envoyé avec succès !',
    submittedSub: "Passage à la phase d'idéation...",
    somethingWrong: "Une erreur s'est produite. Veuillez réessayer.",
    offensiveLanguage: 'Langage offensant détecté',
    readAloud: 'Lire à voix haute',
    magicMode: 'Magic',
    communityIdeas: 'Idées de la communauté',
    broadSelection: 'Large sélection',
    similarIdeas: 'Idées similaires',
    differingIdeas: 'Idées divergentes',
    allIdeas: 'Toutes les idées',
    myIdeas: 'Mes idées',
    topicLabel: 'Thème',
    ideationIntro: "Vous avez terminé l'enquête — merci ! Il est maintenant temps de partager vos idées avec la communauté.",
    ideaShared: 'Votre idée a été partagée avec la communauté !',
    shareIdea: 'Partagez une idée...',
    shareAnother: 'Partagez une autre idée...',
    selectTopicToShare: 'Sélectionnez un thème ci-dessus pour partager votre idée...',
    noQuestions: "Il n'y a pas encore de questions pour cette enquête.",
    noIdeas: 'Aucune idée partagée pour l\'instant. Soyez le premier !',
    resuming: 'Bienvenue à nouveau — reprenons là où vous en étiez.',
    ideaCategories: "Catégories d'idées",
    requiredLabel: 'Obligatoire',
    layoutPickerTitle: 'Comment souhaitez-vous remplir l\'enquête ?',
    layoutPickerClassic: 'Mise en page classique',
    layoutPickerClassicDesc: 'Faites défiler les questions à votre propre rythme.',
    layoutPickerChat: 'Mise en page chat',
    layoutPickerChatDesc: 'Répondez aux questions dans un style conversationnel.',
    layoutPickerSave: 'Retenir mon choix',
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
