import { submitIdea, updateIdeaAfterSafetyReview } from '../../services/ideaService'
import type { Idea } from '../../models/idea'
import type { ActiveView } from './types'
import type { PostSafetyDecision } from './safetyReviewDialog'
import type { IdeaNudgingContext } from '../../services/ideaService'

interface IdeaNudgingResult {
    proceed: boolean
    finalText: string
    bypassQualityNudging: boolean
}

interface CreateIdeasSubmitHandlerParams {
    organizationSlug: string
    projectSlug: string
    projectId: string | number
    reviewBeforePost: (text: string) => Promise<PostSafetyDecision>
    reviewWithSuggestion: (original: string, suggestion: string) => Promise<PostSafetyDecision>
    getNudgingContext?: (activeView: ActiveView) => IdeaNudgingContext | null
    runNudgingFlow?: (input: string, activeView: ActiveView, context: IdeaNudgingContext) => Promise<IdeaNudgingResult>
    onIdeaSubmitted: (idea: Idea, flagged: boolean) => void
}

interface IdeasSubmitHandler {
    submit(body: string, activeView: ActiveView): Promise<boolean>
}

export function createIdeasSubmitHandler({
    organizationSlug,
    projectSlug,
    projectId,
    reviewBeforePost,
    reviewWithSuggestion,
    getNudgingContext,
    runNudgingFlow,
    onIdeaSubmitted,
}: CreateIdeasSubmitHandlerParams): IdeasSubmitHandler {
    async function submit(body: string, activeView: ActiveView): Promise<boolean> {
        if (activeView.type !== 'topic') return false

        const trimmedBody = body.trim()
        if (trimmedBody.length === 0) return false

        const decision = await reviewBeforePost(trimmedBody)
        if (!decision.proceed) {
            return false
        }

        let finalBody = trimmedBody
        let bypassQualityNudging = false

        const nudgingContext = getNudgingContext?.(activeView) ?? null
        if (nudgingContext && runNudgingFlow) {
            try {
                const nudgeResult = await runNudgingFlow(trimmedBody, activeView, nudgingContext)
                if (!nudgeResult.proceed) {
                    return false
                }

                finalBody = nudgeResult.finalText.trim()
                bypassQualityNudging = nudgeResult.bypassQualityNudging
            } catch (error) {
                console.error('[ideas] nudging flow failed, continuing without nudging', error)
            }
        }

        if (finalBody.length === 0) return false

        console.log('[ideas] submitting idea, waiting for AI moderation...')

        try {
            // First submit — backend runs Mistral moderation
            const result = await submitIdea(organizationSlug, projectSlug, {
                projectId,
                topicId: activeView.topicId,
                body: finalBody,
                authorType: 'self',
                qualityNudgeBypassed: bypassQualityNudging,
            })

            if (result.requiresSafetyReview) {
                // Backend flagged it — show dialog with real AI suggestion
                console.log('[ideas] showing safety review dialog to user')
                const suggestion = result.aiSuggestion ?? 'Please rephrase your idea in a respectful and constructive way.'
                const reviewDecision = await reviewWithSuggestion(finalBody, suggestion)

                if (!reviewDecision.proceed) {
                    console.log('[ideas] user dismissed safety dialog — idea stays Pending in DB')
                    return false
                }

                if (reviewDecision.useOriginal) {
                    // Keep original idea pending; if edited, persist the edited text as pending.
                    if (reviewDecision.edited) {
                        await updateIdeaAfterSafetyReview(
                            organizationSlug,
                            projectSlug,
                            activeView.topicId,
                            result.idea.id,
                            reviewDecision.text,
                            true,
                        )
                        result.idea.body = reviewDecision.text
                    }

                    console.log('[ideas] user chose original text — idea stays Pending in DB')
                    onIdeaSubmitted({ ...result.idea, authorType: 'self' } as Idea, true)
                    return true
                }

                // User accepted AI suggestion. If edited, keep it pending for review.
                if (reviewDecision.edited) {
                    await updateIdeaAfterSafetyReview(
                        organizationSlug,
                        projectSlug,
                        activeView.topicId,
                        result.idea.id,
                        reviewDecision.text,
                        true,
                    )

                    console.log('[ideas] edited AI suggestion saved as Pending for review')
                    onIdeaSubmitted({ ...result.idea, body: reviewDecision.text, authorType: 'self' } as Idea, true)
                } else {
                    // Unedited AI suggestion can be approved immediately.
                    await updateIdeaAfterSafetyReview(
                        organizationSlug,
                        projectSlug,
                        activeView.topicId,
                        result.idea.id,
                        reviewDecision.text,
                        false,
                    )

                    console.log('[ideas] unedited AI suggestion accepted and approved')
                    onIdeaSubmitted({ ...result.idea, body: reviewDecision.text, authorType: 'self' } as Idea, false)
                }
            } else {
                // Approved — add directly
                console.log('[ideas] idea approved and added to list')
                onIdeaSubmitted({ ...result.idea, authorType: 'self' } as Idea, false)
            }

            return true
        } catch (err) {
            console.error('[ideas] submit failed', err)
            return false
        }
    }

    return { submit }
}
