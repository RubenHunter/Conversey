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
    brainstormMode: string
    brainstormModeButton: string
    brainstormModeTitle: string
    voiceInput: string
    brainstormModeActivated: string
    brainstormModeAriaLabel: string
    micStartRecording: string
    micStopRecording: string
    micClickToSpeak: string
    micClickToStop: string
    brainstormInstruction: string
    brainstormModeEmptyState: string
    remove: string
    rejectedFeedbackPrefix: string
    rejectionNone: string
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
    chooseTopic: string
    langNl: string
    langEn: string
    langFr: string
    switchedTopic: string
    thanksForIdea: string
    postIdeaNext: string
    topicQuestionLabel: string
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
    // Topic modal
    selectTopicTitle: string
    cancel: string
    // Idea panel
    ideaDetail: string
    originalIdea: string
    responses: string
    writeComment: string
    addReaction: string
    useAsStartingPoint: string
    editIdea: string
    post: string
    // Safety review
    safetyReviewTitle: string
    safetyReviewCopy: string
    yourOriginalMessage: string
    editYourResponse: string
    aiSuggestion: string
    editAiSuggestion: string
    acceptSuggestion: string
    postOriginalAnyway: string
    // Nudge dialog
    nudgeTitle: string
    nudgeStatus: string
    nudgeDraftLabel: string
    nudgeThinking: string
    nudgeApproved: string
    nudgeQuestionStatus: string
    yourAnswer: string
    answerContinue: string
    // First idea contact
    wantStayInTouch: string
    stayInTouchCopy: string
    dontAskAgain: string
    noThanks: string
    leaveMyEmail: string
    stayInTouchTitle: string
    leaveEmailCopy: string
    emailAddress: string
    agreeContact: string
    rememberChoice: string
    deny: string
    allowContact: string
    // Completed page
    thankYouSurvey: string
    helpShareIdeas: string
    continueToIdeas: string
    // Survey page
    surveyAlreadyCompleted: string
    redirectingToIdeas: string
    checkmarkConfirm: string
    chatSend: string
    // Admin delete modal
    deleteFailed: string
    deletedSuccessfully: string
    networkError: string
    // Idea panel & modals
    ideaSingular: string
    editYourIdea: string
    writeAComment: string
    contentSafetyReview: string
    letsKeepSafe: string
    aiFlaggedText: string
    editAISuggestion: string
    wantToStayInTouch: string
    emailLeaveCopy: string
    privacyPolicyLink: string
    agreeToContact: string
    deepenIdeaTitle: string
    deepenIdeaStatus: string
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
    brainstormMode: 'Brainstorm',
    brainstormModeButton: 'Brainstorm Mode',
    brainstormModeTitle: 'Beantwoord in Brainstorm Mode',
    voiceInput: 'Spraakinput',
    brainstormModeActivated: '<b>Brainstorm mode</b> geactiveerd',
    brainstormModeAriaLabel: 'Brainstorm Mode',
    micStartRecording: 'Start opname',
    micStopRecording: 'Stop opname',
    micClickToSpeak: 'Klik om te spreken',
    micClickToStop: 'Klik om te stoppen',
    brainstormInstruction: 'Spreek je gedachten uit en de AI haalt er de belangrijkste kernwoorden uit. Klik daarna op de microfoon om te beginnen.',
    brainstormModeEmptyState: 'Spreek om sleutelwoorden te genereren...',
    remove: 'Verwijder',
    rejectedFeedbackPrefix: '❌ ',
    rejectionNone: 'Geen',
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
    exploreIdeas: 'Ideeën verkennen',
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
    // Topic modal
    selectTopicTitle: 'Selecteer een onderwerp',
    cancel: 'Annuleren',
    // Idea panel
    ideaDetail: 'Idee',
    originalIdea: 'Origineel idee',
    responses: 'Reacties',
    writeComment: 'Schrijf een reactie...',
    addReaction: 'Reactie toevoegen',
    useAsStartingPoint: 'Gebruik als startpunt',
    editIdea: 'Bewerk idee',
    post: 'Plaatsen',
    // Safety review
    safetyReviewTitle: 'Laat dit een veilige plek zijn',
    safetyReviewCopy: 'Onze AI heeft je tekst gemarkeerd als mogelijk aanstootgevend. Je kunt de suggestie gebruiken, het bewerken, of je oorspronkelijke tekst behouden.',
    yourOriginalMessage: 'Je oorspronkelijke bericht',
    editYourResponse: 'Bewerk je reactie',
    aiSuggestion: 'AI suggestie',
    editAiSuggestion: 'Bewerk de AI suggestie',
    acceptSuggestion: 'Accepteer suggestie',
    postOriginalAnyway: 'Toch oorspronkelijke plaatsen',
    // Nudge dialog
    nudgeTitle: 'Laat je idee groeien',
    nudgeStatus: 'De AI stelt één vraag tegelijk. Sluit de dialoog om de huidige versie in te dienen als "wachtend op review".',
    nudgeDraftLabel: 'Jouw idee',
    nudgeThinking: 'Even denken...',
    nudgeApproved: 'Je idee ziet er goed uit!',
    nudgeQuestionStatus: 'Beantwoord de vraag:',
    yourAnswer: 'Je antwoord',
    answerContinue: 'Antwoord & ga door',
    // First idea contact
    wantStayInTouch: 'Wil je op de hoogte blijven?',
    stayInTouchCopy: 'We kunnen je laten weten wat er met je idee gebeurt.',
    dontAskAgain: 'Vraag me dit niet opnieuw',
    noThanks: 'Nee bedankt',
    leaveMyEmail: 'Laat mijn e-mail achter',
    stayInTouchTitle: 'Blijf in contact over je idee',
    leaveEmailCopy: 'Je kunt je e-mail achterlaten als je wilt dat we contact opnemen over je ideeën.',
    emailAddress: 'E-mailadres',
    agreeContact: 'Ik ga akkoord om gecontacteerd te worden over dit idee.',
    rememberChoice: 'Onthoud mijn keuze',
    deny: 'Weiger',
    allowContact: 'Sta contact toe',
    // Completed page
    thankYouSurvey: 'Bedankt voor het invullen van de enquête!',
    helpShareIdeas: 'Zou je ons ook kunnen helpen door je ideeën te delen?',
    continueToIdeas: 'Ga verder naar Ideeën',
    // Survey page
    surveyAlreadyCompleted: 'Enquête al ingevuld',
    redirectingToIdeas: 'Omleiden naar ideeën...',
    checkmarkConfirm: 'Bevestig antwoord en ga verder',
    chatSend: 'Verstuur',
    // Language selector
    chooseTopic: 'Kies onderwerp',
    langNl: 'Nederlands',
    langEn: 'English',
    langFr: 'Français',
    switchedTopic: 'Naar onderwerp "{topicTitle}" gewisseld —',
    thanksForIdea: 'Bedankt! Je idee is gedeeld.',
    postIdeaNext: 'Je kunt nog een idee delen voor dit onderwerp, of wissel van onderwerp hierboven.',
    topicQuestionLabel: 'Onderwerpsvraag: ',
    // Admin delete modal
    deleteFailed: 'Verwijderen mislukt',
    deletedSuccessfully: 'Succesvol verwijderd',
    networkError: 'Netwerkfout',
    // Idea panel & modals
    ideaSingular: 'Idee',
    editYourIdea: 'Bewerk je idee...',
    writeAComment: 'Schrijf een reactie...',
    contentSafetyReview: 'Inhoudsveiligheidscontrole',
    letsKeepSafe: 'Laten we deze ruimte veilig houden',
    aiFlaggedText: 'Onze AI heeft je tekst als mogelijk kwetsend gemarkeerd. Je kunt de suggestie gebruiken, deze bewerken of doorgaan met je oorspronkelijke tekst.',
    editAISuggestion: 'Bewerk de AI-suggestie',
    wantToStayInTouch: 'In contact blijven?',
    emailLeaveCopy: 'Je kunt je e-mailadres achterlaten als je wilt dat we contact met je opnemen over je ideeën.',
    privacyPolicyLink: 'Lees onze privacyverklaring hier.',
    agreeToContact: 'Ik ga akkoord met contact over dit idee.',
    deepenIdeaTitle: 'Laten we je idee verdiepen',
    deepenIdeaStatus: 'De AI stelt één vraag tegelijk. Sluit het dialoogvenster om de huidige versie als in afwachting van beoordeling te plaatsen.',
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
    brainstormMode: 'Brainstorm',
    brainstormModeButton: 'Brainstorm Mode',
    brainstormModeTitle: 'Answer in Brainstorm Mode',
    voiceInput: 'Voice input',
    brainstormModeActivated: '<b>Brainstorm mode</b> activated',
    brainstormModeAriaLabel: 'Brainstorm Mode',
    micStartRecording: 'Start recording',
    micStopRecording: 'Stop recording',
    micClickToSpeak: 'Click to speak',
    micClickToStop: 'Click to stop',
    brainstormInstruction: 'Speak your thoughts freely and the AI will extract the most important keywords. Press the microphone button to begin.',
    brainstormModeEmptyState: 'Speak to generate keywords...',
    remove: 'Remove',
    rejectedFeedbackPrefix: '❌ ',
    rejectionNone: 'None',
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
    exploreIdeas: 'Explore ideas',
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
    // Topic modal
    selectTopicTitle: 'Select a Topic',
    cancel: 'Cancel',
    // Idea panel
    ideaDetail: 'Idea',
    originalIdea: 'Original idea',
    responses: 'Responses',
    writeComment: 'Write a comment...',
    addReaction: 'Add reaction',
    useAsStartingPoint: 'Use as starter',
    editIdea: 'Edit idea before publish',
    post: 'Post',
    // Safety review
    safetyReviewTitle: 'Let\'s keep this space safe',
    safetyReviewCopy: 'Our AI flagged your text as potentially offensive. You can use the suggestion, edit it, or continue with your original text.',
    yourOriginalMessage: 'Your original message',
    editYourResponse: 'Edit your response',
    aiSuggestion: 'AI suggestion',
    editAiSuggestion: 'Edit the AI suggestion',
    acceptSuggestion: 'Accept suggestion',
    postOriginalAnyway: 'Post original anyway',
    // Nudge dialog
    nudgeTitle: 'Let\'s deepen your idea',
    nudgeStatus: 'The AI will ask one question at a time. Close the dialog to post the current version as pending review.',
    nudgeDraftLabel: 'Your idea',
    nudgeThinking: 'Thinking...',
    nudgeApproved: 'Your idea looks great!',
    nudgeQuestionStatus: 'Answer the question:',
    yourAnswer: 'Your answer',
    answerContinue: 'Answer & continue',
    // First idea contact
    wantStayInTouch: 'Want to stay in touch?',
    stayInTouchCopy: 'We can let you know if anything happens with your idea.',
    dontAskAgain: 'Don\'t ask me again',
    noThanks: 'No thanks',
    leaveMyEmail: 'Leave my email',
    stayInTouchTitle: 'Stay in touch about your idea',
    leaveEmailCopy: 'You can leave your email if you want us to contact you about your ideas.',
    emailAddress: 'Email address',
    agreeContact: 'I agree to be contacted about this idea.',
    rememberChoice: 'Remember my choice',
    deny: 'Deny',
    allowContact: 'Allow contact',
    // Completed page
    thankYouSurvey: 'Thank you for filling out this survey!',
    helpShareIdeas: 'Could you also help us by sharing your ideas?',
    continueToIdeas: 'Continue to Ideas',
    // Survey page
    surveyAlreadyCompleted: 'Survey already completed',
    redirectingToIdeas: 'Redirecting you to ideas...',
    checkmarkConfirm: 'Confirm answer and continue',
    chatSend: 'Send',
    // Language selector
    chooseTopic: 'Choose topic',
    langNl: 'Nederlands',
    langEn: 'English',
    langFr: 'Français',
    switchedTopic: 'Switched to topic "{topicTitle}" —',
    thanksForIdea: 'Thanks! Your idea has been shared.',
    postIdeaNext: 'You can share another idea on this topic, or switch to a different topic above.',
    topicQuestionLabel: 'Topic question: ',
    // Admin delete modal
    deleteFailed: 'Delete failed',
    deletedSuccessfully: 'Deleted successfully',
    networkError: 'Network error',
    ideaSingular: 'Idea',
    editYourIdea: 'Edit your idea...',
    writeAComment: 'Write a comment...',
    contentSafetyReview: 'Content safety review',
    letsKeepSafe: "Let's keep this space safe",
    aiFlaggedText: "Our AI has flagged your text as potentially offensive. You can use the suggestion, edit it, or proceed with your original text.",
    editAISuggestion: 'Edit the AI suggestion',
    wantToStayInTouch: 'Stay in touch?',
    emailLeaveCopy: 'You can leave your email address if you want us to contact you about your ideas.',
    privacyPolicyLink: 'Read our privacy policy here.',
    agreeToContact: 'I agree to contact about this idea',
    deepenIdeaTitle: "Let's deepen your idea",
    deepenIdeaStatus: 'The AI asks one question at a time. Close the dialog to place the current version as pending review.',
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
    brainstormMode: 'Brainstorm',
    brainstormModeButton: 'Brainstorm Mode',
    brainstormModeTitle: 'Répondre en Brainstorm Mode',
    voiceInput: 'Saisie vocale',
    brainstormModeActivated: '<b>Brainstorm mode</b> activé',
    brainstormModeAriaLabel: 'Brainstorm Mode',
    micStartRecording: 'Démarrer l\'enregistrement',
    micStopRecording: 'Arrêter l\'enregistrement',
    micClickToSpeak: 'Cliquez pour parler',
    micClickToStop: 'Cliquez pour arrêter',
    brainstormInstruction: 'Exprimez vos idées librement et l\'IA en extraira les mots-clés les plus importants. Appuyez sur le micro pour commencer.',
    brainstormModeEmptyState: 'Parlez pour générer des mots-clés...',
    remove: 'Supprimer',
    rejectedFeedbackPrefix: '❌ ',
    rejectionNone: 'Aucun',
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
    chooseTopic: 'Choisir un thème',
    langNl: 'Nederlands',
    langEn: 'English',
    langFr: 'Français',
    switchedTopic: 'Passé au thème "{topicTitle}" —',
    thanksForIdea: 'Merci ! Votre idée a été partagée.',
    postIdeaNext: 'Vous pouvez partager une autre idée sur ce thème, ou changer de thème ci-dessus.',
    topicQuestionLabel: 'Question du thème : ',
    exploreIdeas: 'Explorer les idées',
    loadingMoreIdeas: 'Chargement de 7 idées supplémentaires...',
    selectTopic: 'Sélectionner un thème',
    thoughtsOnTopic: 'Quelles sont vos pensées sur : "{topicTitle}" ?',
    surveyCompleted: 'Merci d\'avoir complété l\'enquête ! Vos réponses ont été enregistrées.',
    noIdeasHere: 'Pas d\'idées ici pour l\'instant.',
    noIdeasYetBeFirst: 'Aucune idée partagée pour l\'instant. Soyez le premier !',
    noSimilarIdeasFound: 'Votre idée semble super originale — aucune idée similaire trouvée.',
    noContrastingIdeasFound: 'Aucune idée contrastée claire trouvée pour l\'instant.',
    submitIdea: 'Soumettre l\'idée',
    noIdeasMyIdeas: 'Vous n\'avez pas encore soumis d\'idées.',
    noIdeasForView: 'Pas encore d\'idées pour cette vue.',
    loadingResponses: 'Chargement des réponses...',
    couldNotLoadResponses: 'Impossible de charger les réponses maintenant. Essayez de rouvrir cette idée.',
    submitFailed: 'Échec de l\'envoi de l\'enquête. Veuillez réessayer.',
    justNow: 'À l\'instant',
    minutesAgo: 'il y a {n} min',
    hoursAgo: 'il y a {n}h',
    daysAgo: 'il y a {n}j',
    weeksAgo: 'il y a {n} sem',
    mostSimilar: 'Plus similaire',
    leastSimilar: 'Moins similaire',
    mostSimilarIdeas: 'Idées les plus similaires',
    leastSimilarIdeas: 'Idées les moins similaires',
    noResponsesYet: 'Pas encore de réponses. Soyez le premier !',
    ideaDetail: 'Idée',
    originalIdea: 'Idée originale',
    writeComment: 'Écrire un commentaire...',
    useAsStartingPoint: 'Utiliser comme point de départ',
    editIdea: 'Modifier l\'idée avant publication',
    safetyReviewTitle: 'Gardons cet espace sûr',
    safetyReviewCopy: 'Notre IA a signalé votre texte comme potentiellement offensant. Vous pouvez utiliser la suggestion, la modifier, ou continuer avec votre texte original.',
    editAiSuggestion: 'Modifier la suggestion IA',
    nudgeTitle: 'Approfondissons votre idée',
    nudgeStatus: 'L\'IA posera une question à la fois. Fermez la boîte de dialogue pour soumettre la version actuelle en attente de révision.',
    nudgeDraftLabel: 'Votre idée',
    nudgeThinking: 'Je réfléchis...',
    nudgeApproved: 'Votre idée est super !',
    nudgeQuestionStatus: 'Répondez à la question :',
    answerContinue: 'Répondre et continuer',
    wantStayInTouch: 'Voulez-vous rester en contact ?',
    leaveEmailCopy: 'Vous pouvez laisser votre e-mail si vous souhaitez que nous vous contactons à propos de vos idées.',
    agreeContact: 'J\'accepte d\'être contacté à propos de cette idée.',
    rememberChoice: 'Se souvenir de mon choix',
    thankYouSurvey: 'Merci d\'avoir rempli l\'enquête !',
    helpShareIdeas: 'Pourriez-vous nous aider en partageant vos idées ?',
    continueToIdeas: 'Continuer vers les Idées',
    surveyAlreadyCompleted: 'Enquête déjà remplie',
    redirectingToIdeas: 'Redirection vers les idées...',
    checkmarkConfirm: 'Confirmer la réponse et continuer',
    chatSend: 'Envoyer',
    deleteFailed: 'La suppression a échoué',
    deletedSuccessfully: 'Supprimé avec succès',
    networkError: 'Erreur réseau',
}

const translations: Record<SurveyLocale, SurveyStrings> = { nl, en, fr }

const LOCALE_STORAGE_KEY = 'conversey-locale'

export function detectLocale(): SurveyLocale {
    try {
        const stored = localStorage.getItem(LOCALE_STORAGE_KEY) as SurveyLocale | null
        if (stored && translations[stored]) return stored
    } catch { /* ignore storage errors */ }
    const lang = navigator.language.toLowerCase()
    if (lang.startsWith('en')) return 'en'
    if (lang.startsWith('fr')) return 'fr'
    return 'nl'
}

type LocaleChangeCallback = (locale: SurveyLocale) => void
const localeChangeListeners: LocaleChangeCallback[] = []

export function onLocaleChange(callback: LocaleChangeCallback): () => void {
    localeChangeListeners.push(callback)
    return () => {
        const idx = localeChangeListeners.indexOf(callback)
        if (idx >= 0) localeChangeListeners.splice(idx, 1)
    }
}

let currentLocale: SurveyLocale = detectLocale()

export function setLocale(locale: SurveyLocale): void {
    try {
        localStorage.setItem(LOCALE_STORAGE_KEY, locale)
    } catch { /* ignore storage errors */ }
    if (locale === currentLocale) return
    currentLocale = locale
    localeChangeListeners.forEach(fn => fn(locale))
}

export function getLocale(): SurveyLocale {
    return currentLocale
}

export function getSurveyStrings(): SurveyStrings {
    return translations[currentLocale]
}
