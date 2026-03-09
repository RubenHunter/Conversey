```startuml
actor "Young person" as Youth
boundary "Web UI" as UI
control "ReactionController" as Controller
control "ReactionService" as Service
entity "IdeaRepository" as IdeaRepo
entity "ReactionRepository" as ReactionRepo
database "Relational DB" as DB

Youth -> UI: Type reaction + click Submit
UI -> Controller: POST /ideas/{ideaId}/reactions\n(body: text, optional emoji/mediaRef)

Controller -> Service: addReaction(ideaId, reactionText, userContext)

Service -> IdeaRepo: existsById(ideaId)
IdeaRepo -> DB: SELECT 1 FROM Ideas WHERE id = ideaId
DB --> IdeaRepo: exists / not exists
IdeaRepo --> Service: exists?

alt idea does not exist
  Service --> Controller: error(NotFound)
  Controller --> UI: 404 + message "Idea not found"
else idea exists
  Service -> Service: validateInput(reactionText)\n(length, required, etc.)
  Service -> ReactionRepo: save(new Reaction(ideaId, text, createdAt))
  ReactionRepo -> DB: INSERT INTO Reactions(...)
  DB --> ReactionRepo: reactionId
  ReactionRepo --> Service: savedReaction

  Service --> Controller: success(savedReactionDTO)
  Controller --> UI: 201 Created + reaction data
  UI --> Youth: Show reaction under the idea
```

@enduml