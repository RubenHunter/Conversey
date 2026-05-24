export type {ChoiceDto, QuestionDto};

interface ChoiceDto {
    id: number
    text: string
}

interface QuestionDto {
    id: number
    text: string
    required: boolean
    type: string
    possibleAnswers?: ChoiceDto[],
    lowerBound?: number,
    upperBound?: number,
}

