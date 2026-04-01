# BL Method Review Matrix

Generated on: 2026-03-30

This file maps BL methods to integration tests so each method can be reviewed for:
- positive path behavior
- negative path behavior
- whether the method still makes sense to keep

## WorkspaceManager (`WorkspaceManager`)

| Method | Positive coverage | Negative coverage | Notes |
|---|---|---|---|
| `GetAllWorkspaces` | `GetAllWorkspaces_ShouldContainSeededWorkspace` | N/A (returns collection) | Keep |
| `GetAllWorkspacesWithProjects` | `GetAllWorkspacesWithProjects_ShouldContainSeededWorkspaceAndProject` | N/A (returns collection) | Keep |
| `CreateWorkspace` | `CreateWorkspace_WhenSlugIsUnique_ShouldPersistWorkspace` | `CreateWorkspace_WhenSlugAlreadyExists_ShouldThrowValidationException` | Keep |
| `GetWorkspaceBySlug` | `GetWorkspaceBySlug_ShouldReturnWorkspace` | `GetWorkspaceBySlug_WhenWorkspaceDoesNotExist_ShouldThrowWorkspaceNotFoundException` | Keep |
| `GetWorkspaceBySlugWithProjects` | `GetWorkspaceBySlugWithProjects_ShouldReturnWorkspaceWithItsProjects` | Covered via same manager not-found behavior (`GetWorkspaceBySlug_WhenWorkspaceDoesNotExist_ShouldThrowWorkspaceNotFoundException`) | Keep |
| `GetWorkspaceById` | `GetWorkspaceById_ShouldReturnWorkspace` | `GetWorkspaceById_WhenWorkspaceDoesNotExist_ShouldThrowWorkspaceNotFoundException` | Keep |
| `GetWorkspaceByIdWithProjects` | `GetWorkspaceByIdWithProjects_ShouldReturnWorkspaceWithProjects` | `GetWorkspaceByIdWithProjects_WhenWorkspaceDoesNotExist_ShouldThrowWorkspaceNotFoundException` | Keep |

## ProjectManager (`ProjectManager`)

| Method | Positive coverage | Negative coverage | Notes |
|---|---|---|---|
| `GetProjectById` | `ProjectByIdReaders_WhenProjectExists_ShouldReturnProject` (`ProjectByIdReaders`) | `ProjectByIdReaders_WhenProjectDoesNotExist_ShouldThrowProjectNotFoundException` | Keep |
| `GetProjectByIdWithTopics` | Same as above (`ProjectByIdReaders`) | Same as above | Keep |
| `GetProjectByIdWithQuestions` | Same as above (`ProjectByIdReaders`) | Same as above | Keep |
| `GetProjectByIdWithTopicsAndQuestions` | Same as above (`ProjectByIdReaders`) | Same as above | Keep |
| `GetProjectByIdWithWorkspaceAndQuestions` | Same as above (`ProjectByIdReaders`) | Same as above | Keep |
| `GetProjectByIdWithWorkspaceTopicsYouthsAndQuestions` | Same as above (`ProjectByIdReaders`) | Same as above | Keep |
| `GetProjectBySlug` | `ProjectBySlugReaders_WhenProjectExists_ShouldReturnProject` (`ProjectBySlugReaders`) | `ProjectBySlugReaders_WhenProjectDoesNotExist_ShouldThrowProjectNotFoundException` | Keep |
| `GetProjectBySlugWithTopics` | Same as above (`ProjectBySlugReaders`) | Same as above | Keep |
| `GetProjectBySlugWithQuestions` | Same as above (`ProjectBySlugReaders`) | Same as above | Keep |
| `GetProjectBySlugWithTopicsAndQuestions` | Same as above (`ProjectBySlugReaders`) | Same as above | Keep |
| `GetProjectBySlugWithWorkspaceAndQuestions` | Same as above (`ProjectBySlugReaders`) | Same as above | Keep |
| `GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions` | Same as above (`ProjectBySlugReaders`) | Same as above | Keep |
| `GetAllProjects` | `AllProjectReaders_WhenSeededDataExists_ShouldReturnProjects` (`AllProjectReaders`) | N/A (returns collection) | Keep |
| `GetAllProjectsWithTopics` | Same as above (`AllProjectReaders`) | N/A | Keep |
| `GetAllProjectsWithQuestions` | Same as above (`AllProjectReaders`) | N/A | Keep |
| `GetAllProjectsWithTopicsAndQuestions` | Same as above (`AllProjectReaders`) | N/A | Keep |
| `GetProjectsFromWorkspaceByWorkspaceId` | `WorkspaceProjectReaders_WhenWorkspaceExists_ShouldReturnProjects` (`WorkspaceProjectReaders`) | `WorkspaceProjectReaders_WhenWorkspaceDoesNotExist_ShouldReturnEmpty` | Keep |
| `GetProjectsFromWorkspaceByWorkspaceIdWithTopics` | Same as above (`WorkspaceProjectReaders`) | Same as above | Keep |
| `GetProjectsFromWorkspaceByWorkspaceIdWithQuestions` | Same as above (`WorkspaceProjectReaders`) | Same as above | Keep |
| `GetProjectsFromWorkspaceByWorkspaceIdWithTopicsAndQuestions` | Same as above (`WorkspaceProjectReaders`) | Same as above | Keep |
| `AddProject` | `AddProject_WhenTitleIsUnique_ShouldCreateProject` | `AddProject_WhenSlugAlreadyExists_ShouldThrowValidationException` | Keep |
| `ChangeProject` | `ChangeProject_WhenProjectIsValid_ShouldPersistChanges` | `ChangeProject_WhenProjectIsInvalid_ShouldThrowValidationException` | Keep |
| `RemoveProject` | `RemoveProject_WhenProjectExists_ShouldDeleteProject` | `RemoveProject_WhenProjectDoesNotExist_ShouldThrowProjectNotFoundException` | Keep |
| `GetTopicById` | `GetTopicById_WhenTopicExists_ShouldReturnTopic` | `GetTopicById_WhenTopicDoesNotExist_ShouldThrowTopicNotFoundException` | Keep |
| `GetTopicsFromProjectByProjectId` | `GetTopicsFromProjectByProjectId_ShouldReturnTopicsForProject` | `GetTopicsFromProjectByProjectId_WhenProjectDoesNotExist_ShouldReturnEmpty` | Keep |
| `AddTopic` | `AddTopic_WhenProjectExists_ShouldCreateTopic` | `AddTopic_WhenProjectDoesNotExist_ShouldThrowProjectNotFoundException` | Keep |
| `ChangeTopic` | `ChangeTopic_WhenTopicIsValid_ShouldPersistChanges` | Validation path is indirectly covered by manager-level validation tests; no dedicated invalid-topic test yet | Review: optional extra negative test |
| `RemoveTopic` | (Created via `AddTopic_WhenProjectExists_ShouldCreateTopic`) | `RemoveTopic_WhenTopicDoesNotExist_ShouldThrowTopicNotFoundException` | Review: add explicit happy delete test |
| `GetYouthByToken` | `GetYouthByToken_WhenYouthExists_ShouldReturnYouth` | `GetYouthByToken_WhenYouthDoesNotExist_ShouldThrowYouthNotFoundException` | Keep |
| `GetYouthsFromProjectByProjectId` | `GetYouthsFromProjectByProjectId_ShouldReturnYouthsForProject` | `GetYouthsFromProjectByProjectId_WhenProjectDoesNotExist_ShouldReturnEmpty` | Keep |
| `AddYouth` | `AddYouth_WhenProjectExists_ShouldCreateYouth` | `AddYouth_WhenProjectDoesNotExist_ShouldThrowProjectNotFoundException` | Keep |
| `ChangeYouth` | `ChangeYouth_WhenYouthIsValid_ShouldPersistChanges` | `ChangeYouth_WhenYouthIsInvalid_ShouldThrowValidationException` | Keep |
| `RemoveYouth` | `RemoveYouth_WhenYouthExists_ShouldDeleteYouth` | `RemoveYouth_WhenYouthDoesNotExist_ShouldThrowYouthNotFoundException` | Keep |

## QuestionManager (`QuestionManager`)

| Method | Positive coverage | Negative coverage | Notes |
|---|---|---|---|
| `GetQuestionById` | `QuestionByIdReaders_WhenQuestionExists_ShouldReturnQuestion` (`QuestionByIdReaders`) | `QuestionByIdReaders_WhenQuestionDoesNotExist_ShouldThrowQuestionNotFoundException` | Keep |
| `GetQuestionByIdWithProject` | Same as above (`QuestionByIdReaders`) | Same as above | Keep |
| `GetAllQuestions` | `QuestionCollectionReaders_WhenSeededDataExists_ShouldReturnQuestions` (`QuestionCollectionReaders`) | N/A (returns collection) | Keep |
| `GetAllQuestionsWithProject` | Same as above (`QuestionCollectionReaders`) | N/A | Keep |
| `GetQuestionsByProjectId` | `ProjectQuestionReaders_WhenProjectExists_ShouldReturnQuestions` (`ProjectQuestionReaders`) | `ProjectQuestionReaders_WhenProjectDoesNotExist_ShouldReturnEmpty` | Keep |
| `GetQuestionsByProjectIdWithProject` | Same as above (`ProjectQuestionReaders`) | Same as above | Keep |
| `AddQuestion` | `AddQuestion_WhenValidQuestion_ShouldPersistQuestion` | `AddQuestion_WhenQuestionIsInvalid_ShouldThrowValidationException` | Keep |
| `ChangeQuestion` | `ChangeQuestion_WhenQuestionIsValid_ShouldPersistChanges` | `ChangeQuestion_WhenQuestionIsInvalid_ShouldThrowValidationException` | Keep |
| `RemoveQuestion` | `RemoveQuestion_WhenQuestionExists_ShouldDeleteQuestion` | `RemoveQuestion_WhenQuestionDoesNotExist_ShouldThrowQuestionNotFoundException` | Keep |
| `GetTextAnswerById` | `TextAnswerByIdReaders_WhenAnswerExists_ShouldReturnAnswer` (`TextAnswerByIdReaders`) | `TextAnswerByIdReaders_WhenAnswerDoesNotExist_ShouldThrowTextAnswerNotFoundException` | Keep |
| `GetTextAnswerByIdWithYouth` | Same as above (`TextAnswerByIdReaders`) | Same as above | Keep |
| `GetTextAnswerByIdWithQuestion` | Same as above (`TextAnswerByIdReaders`) | Same as above | Keep |
| `GetTextAnswerByIdWithYouthAndQuestion` | Same as above (`TextAnswerByIdReaders`) | Same as above | Keep |
| `GetTextAnswersByQuestionId` | `TextAnswerCollectionReaders_WhenQuestionExists_ShouldReturnAnswers` (`TextAnswerCollectionReaders`) | `TextAnswerCollectionReaders_WhenQuestionDoesNotExist_ShouldReturnEmpty` | Keep |
| `GetTextAnswersByQuestionIdWithYouth` | Same as above (`TextAnswerCollectionReaders`) | Same as above | Keep |
| `GetTextAnswersByQuestionIdWithQuestion` | Same as above (`TextAnswerCollectionReaders`) | Same as above | Keep |
| `GetTextAnswersByQuestionIdWithYouthAndQuestion` | Same as above (`TextAnswerCollectionReaders`) | Same as above | Keep |
| `AddTextAnswer` | `AddTextAnswer_WhenValidAnswer_ShouldPersistAndBeRetrievable` | Manager validation + delete/not-found paths cover invalid lifecycle (`ChangeTextAnswer_WhenAnswerIsInvalid_ShouldThrowValidationException`, `RemoveTextAnswer_WhenAnswerDoesNotExist_ShouldThrowTextAnswerNotFoundException`) | Keep |
| `ChangeTextAnswer` | `ChangeTextAnswer_WhenAnswerIsValid_ShouldPersistChanges` | `ChangeTextAnswer_WhenAnswerIsInvalid_ShouldThrowValidationException` | Keep |
| `RemoveTextAnswer` | `RemoveTextAnswer_WhenAnswerExists_ShouldDeleteAnswer` | `RemoveTextAnswer_WhenAnswerDoesNotExist_ShouldThrowTextAnswerNotFoundException` | Keep |
| `GetIntegerAnswerById` | `IntegerAnswerByIdReaders_WhenAnswerExists_ShouldReturnAnswer` (`IntegerAnswerByIdReaders`) | `IntegerAnswerByIdReaders_WhenAnswerDoesNotExist_ShouldThrowIntegerAnswerNotFoundException` | Keep |
| `GetIntegerAnswerByIdWithYouth` | Same as above (`IntegerAnswerByIdReaders`) | Same as above | Keep |
| `GetIntegerAnswerByIdWithQuestion` | Same as above (`IntegerAnswerByIdReaders`) | Same as above | Keep |
| `GetIntegerAnswerByIdWithYouthAndQuestion` | Same as above (`IntegerAnswerByIdReaders`) | Same as above | Keep |
| `GetIntegerAnswersByQuestionId` | `IntegerAnswerCollectionReaders_WhenQuestionExists_ShouldReturnAnswers` (`IntegerAnswerCollectionReaders`) | `IntegerAnswerCollectionReaders_WhenQuestionDoesNotExist_ShouldReturnEmpty` | Keep |
| `GetIntegerAnswersByQuestionIdWithYouth` | Same as above (`IntegerAnswerCollectionReaders`) | Same as above | Keep |
| `GetIntegerAnswersByQuestionIdWithQuestion` | Same as above (`IntegerAnswerCollectionReaders`) | Same as above | Keep |
| `GetIntegerAnswersByQuestionIdWithYouthAndQuestion` | Same as above (`IntegerAnswerCollectionReaders`) | Same as above | Keep |
| `AddIntegerAnswer` | `AddIntegerAnswer_WhenValidAnswer_ShouldPersistAndBeRetrievable` | `AddIntegerAnswer_WhenAnswerIsInvalid_ShouldThrowValidationException` | Keep |
| `ChangeIntegerAnswer` | `ChangeIntegerAnswer_WhenAnswerIsValid_ShouldPersistChanges` | `ChangeIntegerAnswer_WhenAnswerIsInvalid_ShouldThrowValidationException` | Keep |
| `RemoveIntegerAnswer` | `RemoveIntegerAnswer_WhenAnswerExists_ShouldDeleteAnswer` | `RemoveIntegerAnswer_WhenAnswerDoesNotExist_ShouldThrowIntegerAnswerNotFoundException` | Keep |

## IdeaManager (`IdeaManager`)

| Method | Positive coverage | Negative coverage | Notes |
|---|---|---|---|
| `SubmitIdea` | `SubmitIdea_WhenPayloadIsValid_ShouldReturnApprovedSubmission` | `SubmitIdea_WhenProjectDoesNotExist_ShouldThrowProjectNotFoundException`, `SubmitIdea_WhenTopicDoesNotExistInProject_ShouldThrowTopicNotFoundException`, `SubmitIdea_WhenYouthDoesNotExist_ShouldThrowYouthNotFoundException`, `SubmitIdea_WhenContentIsWhitespace_ShouldThrowValidationException` | Keep |
| `GetIdeaById` | `IdeaReaders_WhenIdeaExists_ShouldReturnIdea` (`IdeaByIdReaders`) | `IdeaReaders_WhenIdeaDoesNotExist_ShouldThrowIdeaNotFoundException` | Keep |
| `GetIdeaByIdWithProject` | Same as above + `GetIdeaByIdWithProject_WhenIdeaExists_ShouldReturnIdeaWithProject` | Same as above | Keep |
| `GetIdeaByIdWithResponses` | `IdeaReaders_WhenIdeaExists_ShouldReturnIdea` (`IdeaByIdReaders`) | `IdeaReaders_WhenIdeaDoesNotExist_ShouldThrowIdeaNotFoundException` | Keep |
| `GetIdeaByIdWithProjectAndResponses` | `IdeaReaders_WhenIdeaExists_ShouldReturnIdea` (`IdeaByIdReaders`) | `IdeaReaders_WhenIdeaDoesNotExist_ShouldThrowIdeaNotFoundException` | Keep |
| `GetAllIdeas` | `IdeaCollectionReaders_WhenSeededDataExists_ShouldReturnIdeas` (`IdeaCollectionReaders`) | N/A (returns collection) | Keep |
| `GetAllIdeasWithProject` | Same as above (`IdeaCollectionReaders`) | N/A | Keep |
| `GetAllIdeasWithResponses` | Same as above (`IdeaCollectionReaders`) | N/A | Keep |
| `GetAllIdeasWithProjectAndResponses` | Same as above (`IdeaCollectionReaders`) | N/A | Keep |
| `GetIdeasFromProjectByProjectId` | covered by project-scoped tests | `GetIdeasFromProjectByProjectId_WhenProjectDoesNotExist_ShouldReturnEmptyCollection` | Keep |
| `GetIdeasFromProjectByProjectIdWithResponses` | covered by project-scoped tests | `GetIdeasFromProjectByProjectIdWithResponses_WhenProjectDoesNotExist_ShouldReturnEmptyCollection` | Keep |
| `GetIdeasFromProjectByYouthToken` | `GetIdeasFromProjectByYouthToken_ShouldReturnOnlyIdeasFromThatYouthOrderedDescending` | Indirectly covered by empty-result semantics for unknown project/topic readers | Review: optional explicit invalid youth token case |
| `GetIdeasFromTopicByProjectSlugAndTopicId` | `GetIdeasFromTopicByProjectSlugAndTopicId_ShouldReturnApprovedIdeasForThatTopic` | `GetIdeasFromTopicByProjectSlugAndTopicId_WhenTopicDoesNotExist_ShouldReturnEmptyCollection` | Keep |
| `ChangeIdea` | `ChangeIdea_WhenIdeaIsValid_ShouldPersistChanges` | `ChangeIdea_WhenIdeaIsInvalid_ShouldThrowValidationException` | Keep |
| `RemoveIdea` | `RemoveIdea_WhenIdeaExists_ShouldDeleteIdea` | `RemoveIdea_WhenIdeaDoesNotExist_ShouldThrowIdeaNotFoundException` | Keep |
| `AddResponse` | `AddResponse_WhenPayloadIsValid_ShouldReturnApprovedResponse` | `AddResponse_WhenIdeaDoesNotExist_ShouldThrowIdeaNotFoundException`, `AddResponse_WhenYouthDoesNotBelongToProject_ShouldThrowValidationException` | Keep |
| `GetResponseById` | `ResponseReaders_WhenResponseExists_ShouldReturnResponse` (`ResponseByIdReaders`) | `ResponseReaders_WhenResponseDoesNotExist_ShouldThrowResponseNotFoundException` | Keep |
| `GetResponseByIdWithIdea` | Same as above (`ResponseByIdReaders`) | Same as above | Keep |
| `GetResponsesFromIdeaByIdeaId` | `ResponseCollectionReaders_WhenIdeaExists_ShouldReturnResponses` (`ResponseCollectionReaders`) | `ResponseCollectionReaders_WhenIdeaDoesNotExist_ShouldReturnEmpty` | Keep |
| `GetResponsesFromIdeaByIdeaIdWithIdea` | Same as above (`ResponseCollectionReaders`) | Same as above | Keep |
| `ChangeResponse` | `ChangeResponse_WhenResponseIsValid_ShouldPersistChanges` | `ChangeResponse_WhenResponseIsInvalid_ShouldThrowValidationException` | Keep |
| `RemoveResponse` | `RemoveResponse_WhenResponseExists_ShouldDeleteResponse` | `RemoveResponse_WhenResponseDoesNotExist_ShouldThrowResponseNotFoundException` | Keep |
| `AddIdeaReaction` | `AddIdeaReaction_WhenReactionAlreadyExists_ShouldReturnExistingReaction` | `AddIdeaReaction_WhenIdeaDoesNotExist_ShouldThrowIdeaNotFoundException`, `AddIdeaReaction_WhenYouthDoesNotExist_ShouldThrowYouthNotFoundException` | Keep |
| `GetIdeaReactionsFromIdeaByIdeaId` | `GetIdeaReactionsFromIdeaByIdeaId_WhenIdeaExists_ShouldReturnReactions` | `GetIdeaReactionsFromIdeaByIdeaId_WhenIdeaDoesNotExist_ShouldThrowIdeaNotFoundException` | Keep |
| `RemoveIdeaReaction` | `RemoveIdeaReaction_WhenReactionExists_ShouldDeleteReaction` | `RemoveIdeaReaction_WhenReactionDoesNotExist_ShouldThrowIdeaReactionNotFoundException` | Keep |
| `AddResponseReaction` | `AddResponseReaction_WhenResponseExists_ShouldCreateReaction` | `AddResponseReaction_WhenResponseDoesNotExist_ShouldThrowResponseNotFoundException` | Keep |
| `GetResponseReactionsFromResponseByResponseId` | `GetResponseReactionsFromResponseByResponseId_WhenResponseExists_ShouldReturnReactions` | `GetResponseReactionsFromResponseByResponseId_WhenResponseDoesNotExist_ShouldThrowResponseNotFoundException` | Keep |
| `RemoveResponseReaction` | `RemoveResponseReaction_WhenReactionExists_ShouldDeleteReaction` | `RemoveResponseReaction_WhenReactionDoesNotExist_ShouldThrowResponseReactionNotFoundException` | Keep |

## MistralAiManager (`MistralAiManager`)

| Method | Positive coverage | Negative coverage | Notes |
|---|---|---|---|
| `GenerateAiAlternative` | Not covered in integration tests | Not covered in integration tests | Recommended: dedicated unit tests with mocked `HttpMessageHandler` |
| `ModerateContent` | Not covered in integration tests | Not covered in integration tests | Recommended: dedicated unit tests with mocked `HttpMessageHandler` |

## Quick follow-up list

- Add explicit positive+negative tests for `ProjectManager.ChangeTopic` invalid path and `ProjectManager.RemoveTopic` happy path.
- Add explicit negative test for `IdeaManager.GetIdeasFromProjectByYouthToken` with unknown youth token (expected empty collection).
- Add unit tests for `MistralAiManager` to close BL-method coverage gap outside manager integration tests.

