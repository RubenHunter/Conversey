# SignalR Implementation Plan - Magic Mode

## Overview
Integrate SignalR for real-time server-to-client communication in Magic Mode, enabling streaming AI responses, live transcription updates, and future multi-user collaboration.

---

## Phase 1: Foundation (Priority: High)
**Goal**: Basic SignalR infrastructure with AI status push

### Tasks
| ID | Task | File | Effort | Dependencies |
|---|---|---|---|---|
| 1.1 | Add SignalR NuGet package | `backend/UI-MVC/` | 5 min | None |
| 1.2 | Create `MagicModeHub.cs` | `backend/UI-MVC/Hubs/MagicModeHub.cs` | 15 min | 1.1 |
| 1.3 | Configure SignalR in `Program.cs` | `backend/UI-MVC/Program.cs` | 10 min | 1.1 |
| 1.4 | Add CORS for SignalR | `Program.cs` | 5 min | 1.3 |
| 1.5 | Add TypeScript SignalR client | `backend/UI-MVC/Assets/lib/@microsoft/signalr/` | 5 min | None |

### Deliverables
- Hub with methods: `JoinSurveyGroup(surveyId)`, `LeaveSurveyGroup(surveyId)`
- Hub events: `AIStatusUpdate`, `TranscriptComplete`
- Basic connection management

---

## Phase 2: AI Status Streaming (Priority: High)
**Goal**: Replace polling with real-time AI state updates

### Tasks
| ID | Task | File | Effort | Dependencies |
|---|---|---|---|---|
| 2.1 | Server: Broadcast AI state from controller | `MagicModeController.cs` | 20 min | Phase 1 |
| 2.2 | Client: Listen for AI status in modal | `magicModeModal.ts` | 30 min | Phase 1, 2.1 |
| 2.3 | Remove polling logic | `magicModeModal.ts` | 15 min | 2.2 |
| 2.4 | Add connection state UI | `magicModeModal.ts` + CSS | 20 min | 2.2 |

### Client Events
```typescript
// Receive AI state updates
connection.on("AIStatusUpdate", (data: { state: 'thinking' | 'transcribing' | 'complete', surveyId: string }) => {
    updateRingStates(data.state);
});
```

---

## Phase 3: STT Streaming (Priority: Medium)
**Goal**: Stream transcription chunks as they arrive

### Tasks
| ID | Task | File | Effort | Dependencies |
|---|---|---|---|---|
| 3.1 | Modify `SpeechService.cs` to emit chunks | `speechService.ts` | 30 min | Phase 1 |
| 3.2 | Server: Forward chunks via SignalR | `MagicModeController.cs` | 20 min | Phase 1, 3.1 |
| 3.3 | Client: Append chunks to bubbles | `bubbleList.ts` | 30 min | 3.2 |

### Considerations
- Verify Mistral STT API supports streaming
- Fallback: Keep client-side STT as backup
- Chunk throttling to avoid UI flooding

---

## Phase 4: Multi-User Collaboration (Priority: Low - Future)
**Goal**: Sync Magic Mode across multiple clients

### Tasks
| ID | Task | File | Effort | Dependencies |
|---|---|---|---|---|
| 4.1 | Broadcast bubble additions | `MagicModeHub.cs` | 30 min | Phase 2 |
| 4.2 | Broadcast bubble edits/removals | `MagicModeHub.cs` | 30 min | 4.1 |
| 4.3 | Client: Sync bubble state | `bubbleList.ts` | 45 min | 4.2 |
| 4.4 | Conflict resolution strategy | Design doc | 60 min | 4.1 |

### Conflict Resolution Options
1. **Last-write-wins** (Simple, good for ephemeral bubbles)
2. **Operational Transform** (Complex, like Google Docs)
3. **Server authoritative** (Server merges changes)

---

## Architecture Decisions

### 1. Group Strategy
- **Approach**: Groups by `surveyId`
- **Reason**: Isolates traffic per survey, scales better
- **Implementation**: `await Groups.AddToGroupAsync(Context.ConnectionId, surveyId)`

### 2. Message Format
```csharp
public record MagicModeMessage(
    string SurveyId,
    MagicModeEventType EventType,
    string? Text = null,
    MagicModeState? State = null,
    string? BubbleId = null
);
```

### 3. Client Connection Lifecycle
```
1. Modal opens -> connect()
2. Join group -> JoinSurveyGroup(surveyId)
3. Modal closes -> LeaveSurveyGroup(surveyId) + disconnect()
```

---

## File Changes Summary

| File | Change Type | Description |
|---|---|---|
| `backend/UI-MVC/Hubs/MagicModeHub.cs` | New | SignalR hub with group management |
| `backend/UI-MVC/Program.cs` | Modify | Add SignalR services + CORS |
| `backend/UI-MVC/appsettings.json` | Modify | Add SignalR config (optional) |
| `backend/UI-MVC/Assets/lib/@microsoft/signalr/` | New | SignalR client library |
| `backend/UI-MVC/Assets/components/survey/magicMode/magicModeModal.ts` | Modify | Add SignalR client + event handlers |
| `backend/UI-MVC/Assets/components/survey/magicMode/bubbleList.ts` | Modify | Add real-time bubble updates (Phase 3/4) |
| `backend/UI-MVC/Controllers/MagicModeController.cs` | Modify | Broadcast via hub instead of returning direct |

---

## Risk Mitigation

| Risk | Mitigation |
|---|---|
| SignalR downtime | Fallback to polling for AI status |
| Connection drops | Auto-reconnect + queue messages client-side |
| Scaling limits | Document Redis backplane requirement at 1000+ users |
| STT not streaming | Keep existing client-side STT as fallback |

---

## Testing Checklist

- [ ] Single user: AI status updates work
- [ ] Single user: STT chunks stream correctly
- [ ] Connection loss: Auto-reconnect works
- [ ] Connection loss: UI shows reconnecting state
- [ ] Multiple tabs: Same user sees consistent state
- [ ] Multiple users: Changes sync (Phase 4)
- [ ] Mobile: Works on iOS/Android browsers

---

## Rollout Strategy

1. **Phase 1+2**: Deploy to staging, test AI status push
2. **Phase 3**: Deploy STT streaming, monitor performance
3. **Phase 4**: Separate feature flag, opt-in beta

---

## Questions for User

1. Should SignalR be behind a feature flag initially?
2. Do you want to support self-hosted SignalR or plan for Azure SignalR Service?
3. For STT streaming: Does Mistral API support chunked results?
4. Multi-user: Is this needed for v1 or can it wait?
