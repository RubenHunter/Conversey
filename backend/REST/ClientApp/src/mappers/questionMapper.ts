import type { ApiAnswerOptionDto, ApiQuestionDto, ApiQuestionTypeDto } from '../api/dtos/questionDto.ts'
import { QuestionType, type AnswerOption, type Question } from '../models/question.ts'

function pickNumber(...values: Array<number | undefined>): number | undefined {
    return values.find((value) => typeof value === 'number' && Number.isFinite(value))
}

function pickString(...values: Array<string | undefined>): string | undefined {
    return values.find((value) => typeof value === 'string' && value.length > 0)
}

function toStableNumericId(value: string): number {
    let hash = 17
    for (const ch of value) {
        hash = (hash * 31 + ch.charCodeAt(0)) | 0
    }
    return Math.abs(hash === -2147483648 ? 2147483647 : hash)
}

function mapQuestionType(rawType: ApiQuestionTypeDto | undefined): Question['type'] {
    if (rawType === undefined) return QuestionType.OpenText

    if (typeof rawType === 'number') {
        // Keep numeric mapping permissive until backend API contract is fixed.
        if (rawType === 0) return QuestionType.SingleChoice
        if (rawType === 1) return QuestionType.OpenText
        if (rawType === 2) return QuestionType.Scale
        if (rawType === 3) return QuestionType.MultipleChoice
        return QuestionType.OpenText
    }

    const normalized = rawType.replace(/[\s-]/g, '_').toUpperCase()

    if (normalized.includes('MULTIPLE')) {
        return QuestionType.MultipleChoice
    }

    if (normalized.includes('CHOICE') || normalized === QuestionType.SingleChoice) {
        return QuestionType.SingleChoice
    }

    if (normalized.includes('SCALE')) {
        return QuestionType.Scale
    }

    return QuestionType.OpenText
}

function mapAnswerOption(dto: ApiAnswerOptionDto, questionId: number): AnswerOption {
    return {
        id: pickNumber(dto.id, dto.Id) ?? 0,
        questionId: pickNumber(dto.questionId, dto.QuestionId) ?? questionId,
        text: pickString(dto.text, dto.Text) ?? '',
    }
}

export function mapApiQuestionToQuestion(dto: ApiQuestionDto): Question {
    const id = pickNumber(dto.id, dto.Id) ?? 0
    const projectSlug = pickString(dto.projectSlug, dto.ProjectSlug)
    const projectId = pickNumber(dto.projectId, dto.ProjectId) ?? (projectSlug ? toStableNumericId(projectSlug) : 0)
    const rawType = pickString(
        dto.type as string | undefined,
        dto.Type as string | undefined,
        dto.questionType as string | undefined,
        dto.QuestionType as string | undefined,
    )

    const options = (dto.options ?? dto.Options)?.map((option) => mapAnswerOption(option, id))

    return {
        id,
        projectId,
        text: pickString(dto.text, dto.Text) ?? `Question ${id}`,
        type: mapQuestionType(rawType),
        isRequired: dto.isRequired ?? dto.IsRequired ?? false,
        order: pickNumber(dto.order, dto.Order),
        backendType: rawType,
        options: options && options.length > 0 ? options : undefined,
    }
}

export function mapApiQuestionsToQuestions(questionDtos: ApiQuestionDto[]): Question[] {
    return questionDtos
        .map(mapApiQuestionToQuestion)
        .sort((a, b) => {
            if (a.order !== undefined && b.order !== undefined) {
                return a.order - b.order
            }
            return a.id - b.id
        })
}

