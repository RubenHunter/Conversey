export type {Question, FixedQuestion, RangeQuestion, OpenQuestion};
export {QuestionType}


enum QuestionType {
    Open = 'open',
    MultipleChoice = 'multiple choice',
    SingleChoice = 'single choice',
    Range = 'range',
}

type Question = {
    type: QuestionType;
    text: string;
    startDrag?: () => void;
    endDrag?: () => void;
};

type FixedQuestion = Question & {
    type: QuestionType.MultipleChoice | QuestionType.SingleChoice;
    possibleAnswers: readonly string[];
};

type RangeQuestion = Question & {
    type: QuestionType.Range;
    min: number;
    max: number;
};

type OpenQuestion = Question & {
    type: QuestionType.Open;
};