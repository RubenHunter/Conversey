export type {AddQuestion, FixedQuestion, RangeQuestion, OpenQuestion};
export {createOpenQuestion, QuestionType, createFixedQuestion, createRangeQuestion}


enum QuestionType {
    Open = 'open',
    MultipleChoice = 'multiple choice',
    SingleChoice = 'single choice',
    Range = 'range',
}

type AddQuestion = {
    text: string;
    startDrag?: () => void;
    endDrag?: () => void;
};

type FixedQuestion = AddQuestion & {
    single: boolean;
    possibleAnswers: readonly string[];
};

type RangeQuestion = AddQuestion & {
    min: number;
    max: number;
};

type OpenQuestion = AddQuestion;

function createQuestion(text: string): AddQuestion {
    return {
        text: text,
    }
}

function createOpenQuestion(text: string): OpenQuestion {
    return createQuestion(text) as OpenQuestion;
}

function createFixedQuestion(text: string, single: boolean, answers: string[]): FixedQuestion {
    const question = createQuestion(text) as FixedQuestion;
    question.single = single;
    question.possibleAnswers = answers;
    return question;
}

function createRangeQuestion(text: string, min: number, max: number): RangeQuestion {
    const question = createQuestion(text) as RangeQuestion;
    question.min = min;
    question.max = max;
    return question;
}