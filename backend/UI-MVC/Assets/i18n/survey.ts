export type SurveyLocale = 'nl' | 'en' | 'fr'

export interface SurveyStrings {
    selectAbove: string
    typeHere: string
    typeBelow: string
    pleaseFill: string
    pleaseFillChoice: string
    answerHintSingleChoice: string
    answerHintMultipleChoice: string
    answerHintScale: string
    answerHintOpenText: string
    allDone: string
    submitSurvey: string
    submitting: string
    submittedTitle: string
    submittedSub: string
    somethingWrong: string
    offensiveLanguage: string
    readAloud: string
    close: string
    magicMode: string
    magicModeButton: string
    magicModeTitle: string
    voiceInput: string
    magicModeActivated: string
    magicModeAriaLabel: string
    micStartRecording: string
    micStopRecording: string
    micClickToSpeak: string
    micClickToStop: string
    magicModeEmptyState: string
    remove: string
    rejectedFeedbackPrefix: string
    rejectionWordCountTooLow: string
    rejectionWordCountExceeded: string
    rejectionDuplicateExact: string
    rejectionDuplicateSemantic: string
    rejectionSubsetOfExisting: string
    rejectionFillerContent: string
    rejectionTooGeneric: string
    loadMoreIdeas: string
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
    // Idea panel & modals
    selectTopicTitle: string
    ideaSingular: string
    editYourIdea: string
    cancel: string
    saveChanges: string
    addReaction: string
    useAsStarter: string
    editIdeaBeforePublish: string
    responses: string
    writeAComment: string
    post: string
    contentSafetyReview: string
    letsKeepSafe: string
    aiFlaggedText: string
    yourOriginalMessage: string
    editYourResponse: string
    aiSuggestion: string
    editAISuggestion: string
    acceptSuggestion: string
    postOriginalAnyway: string
    wantToStayInTouch: string
    stayInTouchCopy: string
    dontAskAgain: string
    noThanks: string
    leaveMyEmail: string
    stayInTouchTitle: string
    emailLeaveCopy: string
    privacyPolicyLink: string
    emailAddress: string
    agreeToContact: string
    deny: string
    allowContact: string
    deepenIdeaTitle: string
    deepenIdeaStatus: string
    yourAnswer: string
    answerAIsQuestion: string
    answerAndContinue: string
    confirmAndContinue: string
    switchTopic: string
    ideasList: string
    converseyBrand: string
}

const nl: SurveyStrings = {
    selectAbove: 'Selecteer je antwoord hierboven...',
    typeHere: 'Typ je antwoord hier...',
    typeBelow: 'Typ je antwoord in de chatbalk hieronder',
    pleaseFill: 'Typ je antwoord in voor je verder gaat.',
    pleaseFillChoice: 'Selecteer een antwoord voor je verder gaat.',
    answerHintSingleChoice: 'Kies één optie.',
    answerHintMultipleChoice: 'Kies één of meer opties.',
    answerHintScale: 'Voer een numerieke waarde in.',
    answerHintOpenText: 'Schrijf je antwoord in je eigen woorden.',
    allDone: 'Je hebt alle vragen beantwoord — goed gedaan! Klaar om je antwoorden in te sturen?',
    submitSurvey: 'Antwoorden bevestigen',
    submitting: 'Bezig met versturen...',
    submittedTitle: 'Succesvol ingediend!',
    submittedSub: 'Overgaan naar ideeënfase...',
    somethingWrong: 'Er is iets misgegaan. Probeer het opnieuw.',
    offensiveLanguage: 'Aanstootgevend taalgebruik gedetecteerd',
    readAloud: 'Voorlezen',
    close: 'Sluiten',
    magicMode: 'Magic',
    magicModeButton: 'Magic Mode',
    magicModeTitle: 'Beantwoord in Magic Mode',
    voiceInput: 'Spraakinput',
    magicModeActivated: '<b>Magic mode</b> geactiveerd',
    magicModeAriaLabel: 'Magic Mode',
    micStartRecording: 'Start opname',
    micStopRecording: 'Stop opname',
    micClickToSpeak: 'Klik om te spreken',
    micClickToStop: 'Klik om te stoppen',
    magicModeEmptyState: 'Spreek om sleutelwoorden te genereren...',
    remove: 'Verwijder',
    rejectedFeedbackPrefix: '❌ ',
    rejectionWordCountTooLow: 'Te kort',
    rejectionWordCountExceeded: 'Te lang',
    rejectionDuplicateExact: 'Duplicaat',
    rejectionDuplicateSemantic: 'Lijkt op bestaand',
    rejectionSubsetOfExisting: 'Deel van bestaand',
    rejectionFillerContent: 'Vulwoord',
    rejectionTooGeneric: 'Te generiek',
    loadMoreIdeas: 'Klik of scroll om 7 meer ideeën te laden',
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
    // Idea panel & modals
    selectTopicTitle: 'Selecteer een thema',
    ideaSingular: 'Idee',
    editYourIdea: 'Bewerk je idee...',
    cancel: 'Annuleren',
    saveChanges: 'Wijzigingen opslaan',
    addReaction: 'Voeg reactie toe',
    useAsStarter: 'Gebruik als startpunt',
    editIdeaBeforePublish: 'Bewerk idee voor publicatie',
    responses: 'Reacties',
    writeAComment: 'Schrijf een reactie...',
    post: 'Plaatsen',
    contentSafetyReview: 'Inhoudsveiligheidscontrole',
    letsKeepSafe: 'Laten we deze ruimte veilig houden',
    aiFlaggedText: 'Onze AI heeft je tekst als mogelijk kwetsend gemarkeerd. Je kunt de suggestie gebruiken, deze bewerken of doorgaan met je oorspronkelijke tekst.',
    yourOriginalMessage: 'Je oorspronkelijke bericht',
    editYourResponse: 'Bewerk je antwoord',
    aiSuggestion: 'AI-suggestie',
    editAISuggestion: 'Bewerk de AI-suggestie',
    acceptSuggestion: 'Suggestie accepteren',
    postOriginalAnyway: 'Toch oorspronkelijke bericht plaatsen',
    wantToStayInTouch: 'In contact blijven?',
    stayInTouchCopy: 'We kunnen je op de hoogte houden als er iets gebeurt met je idee.',
    dontAskAgain: 'Vraag het niet meer',
    noThanks: 'Nee, bedankt',
    leaveMyEmail: 'Laat je e-mail achter',
    stayInTouchTitle: 'Blijf op de hoogte over je idee',
    emailLeaveCopy: 'Je kunt je e-mailadres achterlaten als je wilt dat we contact met je opnemen over je ideeën.',
    privacyPolicyLink: 'Lees onze privacyverklaring hier.',
    emailAddress: 'E-mailadres',
    agreeToContact: 'Ik ga akkoord met contact over dit idee.',
    deny: 'Weigeren',
    allowContact: 'Contact toestaan',
    deepenIdeaTitle: 'Laten we je idee verdiepen',
    deepenIdeaStatus: 'De AI stelt één vraag tegelijk. Sluit het dialoogvenster om de huidige versie als in afwachting van beoordeling te plaatsen.',
    yourAnswer: 'Jouw antwoord',
    answerAIsQuestion: 'Beantwoord de vraag van de AI...',
    answerAndContinue: 'Beantwoorden & doorgaan',
    confirmAndContinue: 'Bevestigen en doorgaan',
    switchTopic: 'Wissel van thema',
    ideasList: 'Ideeënlijst',
    converseyBrand: 'Conversey',
}

const en: SurveyStrings = {
    selectAbove: 'Select your answer above...',
    typeHere: 'Type your answer here...',
    typeBelow: 'Type your answer in the chat bar below',
    pleaseFill: 'Please type your answer before continuing.',
    pleaseFillChoice: 'Please select an answer before continuing.',
    answerHintSingleChoice: 'Choose one option.',
    answerHintMultipleChoice: 'Choose one or more options.',
    answerHintScale: 'Enter a numeric value.',
    answerHintOpenText: 'Write your answer in your own words.',
    allDone: "You've answered all the questions — well done! Ready to submit your responses?",
    submitSurvey: 'Submit Survey',
    submitting: 'Submitting...',
    submittedTitle: 'Submitted successfully!',
    submittedSub: 'Moving to ideation phase...',
    somethingWrong: 'Sorry, something went wrong. Please try again.',
    offensiveLanguage: 'Offensive language detected',
    readAloud: 'Read aloud',
    close: 'Close',
    magicMode: 'Magic',
    magicModeButton: 'Magic Mode',
    magicModeTitle: 'Answer in Magic Mode',
    voiceInput: 'Voice input',
    magicModeActivated: '<b>Magic mode</b> activated',
    magicModeAriaLabel: 'Magic Mode',
    micStartRecording: 'Start recording',
    micStopRecording: 'Stop recording',
    micClickToSpeak: 'Click to speak',
    micClickToStop: 'Click to stop',
    magicModeEmptyState: 'Speak to generate keywords...',
    remove: 'Remove',
    rejectedFeedbackPrefix: '❌ ',
    rejectionWordCountTooLow: 'Too short',
    rejectionWordCountExceeded: 'Too long',
    rejectionDuplicateExact: 'Duplicate',
    rejectionDuplicateSemantic: 'Similar to existing',
    rejectionSubsetOfExisting: 'Part of existing',
    rejectionFillerContent: 'Filler word',
    rejectionTooGeneric: 'Too generic',
    loadMoreIdeas: 'Click or scroll down to load 7 more ideas',
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
    selectTopicTitle: 'Select a topic',
    ideaSingular: 'Idea',
    editYourIdea: 'Edit your idea...',
    cancel: 'Cancel',
    saveChanges: 'Save changes',
    addReaction: 'Add reaction',
    useAsStarter: 'Use as starter',
    editIdeaBeforePublish: 'Edit idea before publish',
    responses: 'Responses',
    writeAComment: 'Write a comment...',
    post: 'Post',
    contentSafetyReview: 'Content safety review',
    letsKeepSafe: "Let's keep this space safe",
    aiFlaggedText: "Our AI has flagged your text as potentially offensive. You can use the suggestion, edit it, or proceed with your original text.",
    yourOriginalMessage: 'Your original message',
    editYourResponse: 'Edit your response',
    aiSuggestion: 'AI suggestion',
    editAISuggestion: 'Edit the AI suggestion',
    acceptSuggestion: 'Accept suggestion',
    postOriginalAnyway: 'Post original anyway',
    wantToStayInTouch: 'Stay in touch?',
    stayInTouchCopy: 'We can keep you updated if something happens with your idea.',
    dontAskAgain: 'Don’t ask again',
    noThanks: 'No, thanks',
    leaveMyEmail: 'Leave my email',
    stayInTouchTitle: 'Stay updated on your idea',
    emailLeaveCopy: 'You can leave your email address if you want us to contact you about your ideas.',
    privacyPolicyLink: 'Read our privacy policy here.',
    emailAddress: 'Email address',
    agreeToContact: 'I agree to contact about this idea',
    deny: 'Deny',
    allowContact: 'Allow contact',
    deepenIdeaTitle: "Let's deepen your idea",
    deepenIdeaStatus: 'The AI asks one question at a time. Close the dialog to place the current version as pending review.',
    yourAnswer: 'Your answer',
    answerAIsQuestion: 'Answer the AI’s question...',
    answerAndContinue: 'Answer & continue',
    confirmAndContinue: 'Confirm and continue',
    switchTopic: 'Switch topic',
    ideasList: 'Ideas list',
    converseyBrand: 'Conversey',
}

const fr: SurveyStrings = {
    selectAbove: 'Sélectionnez votre réponse ci-dessus...',
    typeHere: 'Tapez votre réponse ici...',
    typeBelow: 'Tapez votre réponse dans la barre de chat ci-dessous',
    pleaseFill: 'Veuillez saisir votre réponse avant de continuer.',
    pleaseFillChoice: 'Veuillez sélectionner une réponse avant de continuer.',
    answerHintSingleChoice: 'Choisissez une option.',
    answerHintMultipleChoice: 'Choisissez une ou plusieurs options.',
    answerHintScale: 'Entrez une valeur numérique.',
    answerHintOpenText: 'Écrivez votre réponse avec vos propres mots.',
    allDone: 'Vous avez répondu à toutes les questions — bravo ! Prêt(e) à envoyer vos réponses ?',
    submitSurvey: 'Envoyer les réponses',
    submitting: 'Envoi en cours...',
    submittedTitle: 'Envoyé avec succès !',
    submittedSub: "Passage à la phase d'idéation...",
    somethingWrong: "Une erreur s'est produite. Veuillez réessayer.",
    offensiveLanguage: 'Langage offensant détecté',
    readAloud: 'Lire à voix haute',
    close: 'Fermer',
    magicMode: 'Magic',
    magicModeButton: 'Magic Mode',
    magicModeTitle: 'Répondre en Magic Mode',
    voiceInput: 'Saisie vocale',
    magicModeActivated: '<b>Magic mode</b> activé',
    magicModeAriaLabel: 'Magic Mode',
    micStartRecording: 'Démarrer l\'enregistrement',
    micStopRecording: 'Arrêter l\'enregistrement',
    micClickToSpeak: 'Cliquez pour parler',
    micClickToStop: 'Cliquez pour arrêter',
    magicModeEmptyState: 'Parlez pour générer des mots-clés...',
    remove: 'Supprimer',
    rejectedFeedbackPrefix: '❌ ',
    rejectionWordCountTooLow: 'Trop court',
    rejectionWordCountExceeded: 'Trop long',
    rejectionDuplicateExact: 'Dupliqué',
    rejectionDuplicateSemantic: 'Similaire à existant',
    rejectionSubsetOfExisting: 'Partie de existant',
    rejectionFillerContent: 'Mot de remplissage',
    rejectionTooGeneric: 'Trop générique',
    loadMoreIdeas: 'Cliquez ou faites défiler vers le bas pour charger 7 idées supplémentaires',
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
    selectTopicTitle: 'Sélectionnez un thème',
    ideaSingular: 'Idée',
    editYourIdea: 'Modifiez votre idée...',
    cancel: 'Annuler',
    saveChanges: 'Enregistrer les modifications',
    addReaction: 'Ajouter une réaction',
    useAsStarter: 'Utiliser comme point de départ',
    editIdeaBeforePublish: 'Modifier l\'idée avant publication',
    responses: 'Réponses',
    writeAComment: 'Écrivez un commentaire...',
    post: 'Publier',
    contentSafetyReview: 'Contrôle de sécurité du contenu',
    letsKeepSafe: 'Gardons cet espace sécurisé',
    aiFlaggedText: "Notre IA a marqué votre texte comme potentiellement offensant. Vous pouvez utiliser la suggestion, la modifier ou continuer avec votre texte original.",
    yourOriginalMessage: 'Votre message original',
    editYourResponse: 'Modifier votre réponse',
    aiSuggestion: 'Suggestion IA',
    editAISuggestion: 'Modifier la suggestion IA',
    acceptSuggestion: 'Accepter la suggestion',
    postOriginalAnyway: 'Publier l\'original quand même',
    wantToStayInTouch: 'Rester en contact ?',
    stayInTouchCopy: 'Nous pouvons vous tenir informé si quelque chose se passe avec votre idée.',
    dontAskAgain: 'Ne plus demander',
    noThanks: 'Non, merci',
    leaveMyEmail: 'Laisser mon email',
    stayInTouchTitle: 'Restez informé sur votre idée',
    emailLeaveCopy: 'Vous pouvez laisser votre adresse email si vous souhaitez que nous vous contactions concernant vos idées.',
    privacyPolicyLink: 'Lisez notre politique de confidentialité ici.',
    emailAddress: 'Adresse email',
    agreeToContact: 'J\'accepte le contact concernant cette idée',
    deny: 'Refuser',
    allowContact: 'Autoriser le contact',
    deepenIdeaTitle: 'Approfondissons votre idée',
    deepenIdeaStatus: "L'IA pose une question à la fois. Fermez la boîte de dialogue pour placer la version actuelle en attente d'examen.",
    yourAnswer: 'Votre réponse',
    answerAIsQuestion: 'Répondez à la question de l\'IA...',
    answerAndContinue: 'Répondre et continuer',
    confirmAndContinue: 'Confirmer et continuer',
    switchTopic: 'Changer de thème',
    ideasList: 'Liste d\'idées',
    converseyBrand: 'Conversey',
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
