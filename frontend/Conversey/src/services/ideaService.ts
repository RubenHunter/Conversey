import type { ApiIdeaDto, ApiIdeaTopicDto } from '../api/dtos/ideaDto.ts'
import { mapApiIdeaToIdea, mapApiIdeaTopicToIdeaTopic, mapSubmitIdeaRequestToApiSubmitIdeaRequest } from '../mappers/ideaMapper.ts'
import type { Idea, IdeaTopic, SubmitIdeaRequest } from '../models/idea.ts'

const IDEAS_USER_KEY = 'conversey-ideas-user-id'

interface IdeasContext {
    topics: IdeaTopic[]
    ideas: Idea[]
}

const MOCK_TOPICS: Record<number, ApiIdeaTopicDto[]> = {
    1: [
        {
            Id: 1,
            ProjectId: 1,
            Title: 'Stress Triggers',
            Prompt: 'What practical change would reduce stress in your daily school or work routine?',
            Order: 1,
        },
        {
            Id: 2,
            ProjectId: 1,
            Title: 'Peer Support',
            Prompt: 'How can classmates or colleagues support each other better during difficult weeks?',
            Order: 2,
        },
        {
            Id: 3,
            ProjectId: 1,
            Title: 'Access to Help',
            Prompt: 'What would make mental-health resources easier to discover and actually use?',
            Order: 3,
        },
    ],
}

const MOCK_OTHER_IDEAS: Record<number, ApiIdeaDto[]> = {
    1: [
        { Id: 1, ProjectId: 1, TopicId: 1, Body: 'Add one no-meeting hour in the afternoon so people can reset.', AuthorType: 'other', CreatedAt: '2026-02-12T10:23:00.000Z' },
        { Id: 2, ProjectId: 1, TopicId: 1, Body: 'Share weekly workload snapshots so deadlines are visible early.', AuthorType: 'other', CreatedAt: '2026-02-16T09:10:00.000Z' },
        { Id: 3, ProjectId: 1, TopicId: 1, Body: 'Offer a weekly "focus half-day" with no chat notifications.', AuthorType: 'other', CreatedAt: '2026-02-18T11:44:00.000Z' },
        { Id: 4, ProjectId: 1, TopicId: 1, Body: 'Let teams pick two protected recovery slots each week.', AuthorType: 'other', CreatedAt: '2026-02-20T07:29:00.000Z' },
        { Id: 5, ProjectId: 1, TopicId: 1, Body: 'Show realistic effort estimates before tasks are accepted.', AuthorType: 'other', CreatedAt: '2026-02-21T16:58:00.000Z' },
        { Id: 6, ProjectId: 1, TopicId: 1, Body: 'Create a stress check-in pulse at the end of each sprint.', AuthorType: 'other', CreatedAt: '2026-02-23T19:04:00.000Z' },
        { Id: 7, ProjectId: 1, TopicId: 2, Body: 'Create buddy check-ins every Monday morning.', AuthorType: 'other', CreatedAt: '2026-02-19T14:08:00.000Z' },
        { Id: 8, ProjectId: 1, TopicId: 2, Body: 'Train mentors to recognize burnout signals quickly.', AuthorType: 'other', CreatedAt: '2026-02-22T18:50:00.000Z' },
        { Id: 9, ProjectId: 1, TopicId: 2, Body: 'Run short peer circles after exam or release weeks.', AuthorType: 'other', CreatedAt: '2026-02-24T08:42:00.000Z' },
        { Id: 10, ProjectId: 1, TopicId: 2, Body: 'Give each team a rotating wellbeing champion role.', AuthorType: 'other', CreatedAt: '2026-02-25T13:20:00.000Z' },
        { Id: 11, ProjectId: 1, TopicId: 2, Body: 'Add anonymous "need help" signals in team channels.', AuthorType: 'other', CreatedAt: '2026-02-27T09:35:00.000Z' },
        { Id: 12, ProjectId: 1, TopicId: 2, Body: 'Pair first-years with older students for practical support.', AuthorType: 'other', CreatedAt: '2026-03-01T12:18:00.000Z' },
        { Id: 13, ProjectId: 1, TopicId: 3, Body: 'Put all support options in a single QR code page.', AuthorType: 'other', CreatedAt: '2026-02-26T08:15:00.000Z' },
        { Id: 14, ProjectId: 1, TopicId: 3, Body: 'Add office-hours booking directly inside the school app.', AuthorType: 'other', CreatedAt: '2026-02-28T10:52:00.000Z' },
        { Id: 15, ProjectId: 1, TopicId: 3, Body: 'Use plain language cards: what it is, when to use it, how fast.', AuthorType: 'other', CreatedAt: '2026-03-02T16:07:00.000Z' },
        { Id: 16, ProjectId: 1, TopicId: 3, Body: 'Highlight one local support service each week in class news.', AuthorType: 'other', CreatedAt: '2026-03-03T11:26:00.000Z' },
        { Id: 17, ProjectId: 1, TopicId: 3, Body: 'Offer chat-first contact before requiring a formal appointment.', AuthorType: 'other', CreatedAt: '2026-03-05T09:54:00.000Z' },
        { Id: 18, ProjectId: 1, TopicId: 3, Body: 'Map services by urgency so students know where to start.', AuthorType: 'other', CreatedAt: '2026-03-06T18:30:00.000Z' },
    ],
}

function getStoredUserId(): string {
    const existing = localStorage.getItem(IDEAS_USER_KEY)
    if (existing) return existing

    const userId = `anon-${Math.random().toString(36).slice(2, 10)}`
    localStorage.setItem(IDEAS_USER_KEY, userId)
    return userId
}

function getMyIdeasKey(projectId: number, userId: string): string {
    return `ideas-self-${projectId}-${userId}`
}

function getStoredMyIdeas(projectId: number, userId: string): Idea[] {
    const raw = localStorage.getItem(getMyIdeasKey(projectId, userId))
    if (!raw) return []

    try {
        const parsed = JSON.parse(raw) as Idea[]
        return Array.isArray(parsed) ? parsed : []
    } catch {
        return []
    }
}

function setStoredMyIdeas(projectId: number, userId: string, ideas: Idea[]): void {
    localStorage.setItem(getMyIdeasKey(projectId, userId), JSON.stringify(ideas))
}

export function getIdeasUserId(): string {
    return getStoredUserId()
}

export async function getIdeasContext(projectId: number): Promise<IdeasContext> {
    const topics = (MOCK_TOPICS[projectId] ?? []).map(mapApiIdeaTopicToIdeaTopic).sort((a, b) => (a.order ?? a.id) - (b.order ?? b.id))
    const otherIdeas = (MOCK_OTHER_IDEAS[projectId] ?? []).map(mapApiIdeaToIdea)
    const myIdeas = getStoredMyIdeas(projectId, getStoredUserId())

    return {
        topics,
        ideas: [...otherIdeas, ...myIdeas].sort((a, b) => Date.parse(b.createdAt) - Date.parse(a.createdAt)),
    }
}

export async function submitIdea(request: SubmitIdeaRequest): Promise<Idea> {
    const requestDto = mapSubmitIdeaRequestToApiSubmitIdeaRequest(request)
    const userId = getStoredUserId()

    const idea: Idea = {
        id: Date.now(),
        projectId: requestDto.projectId,
        topicId: requestDto.topicId,
        body: requestDto.body.trim(),
        authorType: requestDto.authorType,
        createdAt: new Date().toISOString(),
    }

    if (idea.authorType === 'self') {
        const current = getStoredMyIdeas(idea.projectId, userId)
        setStoredMyIdeas(idea.projectId, userId, [idea, ...current])
    }

    return idea
}
