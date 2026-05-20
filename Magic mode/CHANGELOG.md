# Magic Mode — Changelog

All notable changes to the Magic Mode feature, consolidated from all documentation files.

---

## 2026

### May

| Date | Change | File | By |
|---|---|---|---|
| 2026-05-15 | Created SignalR implementation plan | SignalR-Implementation-Plan.md | Mistral Vibe |
| 2026-05-11 | Rewrote Magic mode README.md | Magic mode README.md | Matéo Rohr |
| 2026-05-08 | Created STT optimization plan | STT-Optimalisatie-Plan.md | - |
| 2026-05-03 | Rewrote: RTF → Markdown, architecture corrected | Magic mode README.md, Magic mode extra README.md | Claude |
| 2026-05-03 | Rewritten: correct use IAiManager, no separate IAiService | phase3-ai.md | Claude |
| 2026-05-03 | Extended: context-aware prompt, existingPhrases + rejectedPhrases in interface, DTO and controller | phase3-ai.md | Claude |
| 2026-05-03 | Herschreven: correct gebruik van STTManager client-side | phase2-ai.md | Claude |
| 2026-05-03 | Herschreven: TypeScript modules, Tailwind/DaisyUI, Heroicons | phase4-ai.md | Claude |
| 2026-05-03 | Uitgebreid: deduplicatie + rejected set in bubbleList, getRejectedPhrases(), fetchKeyPhrases met context | phase4-ai.md | Claude |
| 2026-05-03 | Herschreven: client-side TypeScript state only | phase5-ai.md | Claude |
| 2026-05-03 | Uitgebreid: twee state-structuren (activeBubbles + rejectedPhrases), levenscyclus bijgewerkt | phase5-ai.md | Claude |
| 2026-05-03 | Herschreven: bestaande knop wiren, geen inline JS/CSS | phase6-ai.md | Claude |
| 2026-05-03 | Herschreven: xUnit, pnpm build, handmatige browser test | phase7-ai.md | Claude |
| 2026-05-03 | Aangemaakt: correct gebruik IAiManager, geen aparte IAiService | phase3-ai.md | Claude |
| 2026-05-03 | Uitgebreid: context-aware prompt, existingPhrases + rejectedPhrases in interface, DTO en controller | phase3-ai.md | Claude |
| 2026-05-03 | Aangemaakt | phase1-setup.md | Claude |
| 2026-05-03 | Aangemaakt | phase2-ai.md | Claude |

### April

| Date | Change | File | By |
|---|---|---|---|
| 2026-04-26 | Initial documentation structure | phase1-setup.md, phase2-ai.md, phase3-ai.md, phase4-ai.md, phase5-ai.md, phase6-ai.md, phase7-ai.md | Matéo Rohr |
| 2026-04-26 | Initiële documentatiestructuur | Magic mode README.md | Matéo Rohr |
| 2026-04-26 | Initiële checklist | end-check.md | Matéo Rohr |
| 2026-04-26 | Initiële UI mockup, workflow, stack | Magic mode extra README.md | Matéo Rohr |
| 2026-04-26 | Sans phase3-ai.md created (alternative version) | Sans phase3-ai.md | Matéo Rohr |
| 2026-04-26 | Created STT & Mistral call fix plan | STT&MitsralCall-Fix.md | - |

---

## Implementation Milestones

### Phase 1: Foundation (2026-04-26)
- Project structure defined
- DTOs created
- BL interface extended
- Initial documentation written

### Phase 2-7: Complete Implementation (2026-05-03)
- All phases rewritten with correct patterns
- Client-side only architecture confirmed
- No Blazor/React/jQuery
- Tailwind + DaisyUI styling
- STTManager integration (not Voxtrall)
- IAiManager extension (not separate service)

### Optimizations (2026-05-06+)
- STT dual buffer system planned
- AI-validated bubbles
- Caching implementation
- Sequential processing

---

## Key Architectural Decisions

| Date | Decision | Rationale |
|---|---|---|
| 2026-05-03 | Client-side state only | Bubbles are session-specific, server-side causes race conditions |
| 2026-05-03 | Extend IAiManager, not new service | Maintains SOLID principles, DRY |
| 2026-05-03 | Use existing STTManager | Already tested, no Voxtrall dependency |
| 2026-05-03 | TypeScript, not Blazor | Better developer experience, faster iteration |
| 2026-05-03 | Tailwind + DaisyUI | Consistent with project styling |
| 2026-05-03 | No server-side state | Simpler, more scalable |

---

## AI Model Changes

| Date | Model | Change |
|---|---|---|
| 2026-05-03 | mistral-small-latest | Configured as default KeyPhraseModel |

---

## Performance Improvements

| Date | Change | Impact |
|---|---|---|
| 2026-05-06 | Dual buffer system | ~65-70% API call reduction |
| 2026-05-06 | AI validation | ~75-85% fewer bubbles, better quality |
| 2026-05-06 | Caching | 30-50% cache hit rate |
| 2026-05-06 | Sequential processing | No rate limiting issues |

---

## See Also

- [README.md](./README.md) — Main overview
- [PHASES.md](./PHASES.md) — Detailed implementation phases
- [IMPLEMENTATION.md](./IMPLEMENTATION.md) — AI implementation details
