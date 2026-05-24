export type {Question, FixedQuestion, RangeQuestion, OpenQuestion, Answer};
export {QuestionType}


enum QuestionType {
    Open = 'Open',
    MultipleChoice = 'MultipleChoice',
    SingleChoice = 'SingleChoice',
    scale = 'Scale',
}

type Question = {
    id?: number;
    type: QuestionType;
    text: string;
    required: boolean;
    hint?: string;
};

type FixedQuestion = Question & {
    type: QuestionType.MultipleChoice | QuestionType.SingleChoice;
    possibleAnswers: readonly Answer[];
};

type RangeQuestion = Question & {
    type: QuestionType.scale;
    min: number;
    max: number;
};

type OpenQuestion = Question & {
    type: QuestionType.Open;
};

type Answer = {
    id?: number;
    text: string;
}