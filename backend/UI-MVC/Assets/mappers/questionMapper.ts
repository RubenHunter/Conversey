import {QuestionType, type Answer, type Question, FixedQuestion, RangeQuestion} from '../models/Question'
import {ChoiceDto, QuestionDto} from "../api/dtos/questionDto.ts";

export {mapQuestionDtosToQuestions};

function mapChoiceToAnswer(dto: ChoiceDto): Answer {
    return {
        id: dto.id,
        text: dto.text,
    }
}

function mapQuestionDtoToQuestion(dto: QuestionDto): Question {
    const id = dto.id
    
    switch (dto.type) {
        case QuestionType.SingleChoice:
        case QuestionType.MultipleChoice:
            return {
                id: id,
                text: dto.text,
                type: dto.type,
                required: dto.required,
                possibleAnswers: dto.possibleAnswers!.map((option) => mapChoiceToAnswer(option)),
            } as FixedQuestion;
        case QuestionType.scale:
            return {
                id: id,
                text: dto.text,
                type: dto.type,
                required: dto.required,
                min: dto.lowerBound!,
                max: dto.upperBound!,
            } as RangeQuestion;
        case QuestionType.Open:
            return {
                id: id,
                text: dto.text,
                type: dto.type,
                required: dto.required,
            };
        default:
            throw new Error(`Unknown question type: ${dto.type}`);
    }
}

function mapQuestionDtosToQuestions(questionDtos: QuestionDto[]): Question[] {
    return questionDtos
        .map(mapQuestionDtoToQuestion);
}

