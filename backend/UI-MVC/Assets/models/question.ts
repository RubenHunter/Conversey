export type {Question, FixedQuestion, RangeQuestion, OpenQuestion, Answer};
export {QuestionType}


enum QuestionType {
    Open = 'Open',
    MultipleChoice = 'MultipleChoice',
    SingleChoice = 'SingleChoice',
    Scale = 'Scale',
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
    possibleAnswers: Answer[];
};

type RangeQuestion = Question & {
    type: QuestionType.Scale;
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