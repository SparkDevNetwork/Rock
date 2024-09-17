// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

import { Guid } from "@Obsidian/Types";
import { LearningComponentBaseDefaults } from "./learningActivity";
import { newGuid } from "@Obsidian/Utility/guid";

/** Determines how the component should be rendered. */
export enum AssessmentItemType {
    MultipleChoice = "Multiple Choice",
    Section = "Section",
    ShortAnswer = "Short Answer"
}

/**
 * The abstract class to inherit from for Learning Activity Component Assessment Items.
 * These are specific to the Assessment Component.
 */
export abstract class AssessmentItem {

    /** An indicator of whether an assessment item has been graded. */
    hasBeenGraded?: boolean;

    /** The order in which this assessment item should be shown. */
    order!: number;

    /** The number of points earned for the assessment item. */
    pointsEarned!: number;

    /** The participant's response to the assessment item (e.g. the selected answer). */
    response?: string;

    /** The type of assessment item. */
    typeName!: string;

    /**
     * A unique identifier for looking up a student response.
     * This ensures that changing the order or name of an assessment item
     * doesn't cause issues with grading.
     */
    uniqueId!: Guid;
}

/**
 * The base class for an assessment item.
 * This class handles initialization of AssessmentItem type properties.
 */
export class AssessmentItemBase extends AssessmentItem {
    constructor(typeName: string, order = 0) {
        super();
        this.order = order;
        this.pointsEarned = 0;
        this.response = "";
        this.typeName = typeName;
        this.uniqueId = newGuid();
    }
}

export class MultipleChoiceItem extends AssessmentItemBase {
    answers: string[] = [];
    correctAnswer?: string;
    helpText?: string;
    question!: string;

    constructor(order = 0) {
        super(AssessmentItemType.MultipleChoice, order);
        this.answers = ["", "", ""];
        this.helpText = "";
        this.question = "";
    }
}

export class SectionItem extends AssessmentItemBase {
    summary?: string;
    title!: string;

    constructor(order = 0) {
        super(AssessmentItemType.Section, order);
        this.title = "";
    }
}

export class ShortAnswerItem extends AssessmentItemBase {
    answerBoxRows!: number;
    helpText?: string;
    maxCharacterCount?: number;
    pointsPossible?: number;
    question!: string;
    questionWeight!: number;

    constructor(order = 0) {
        super(AssessmentItemType.ShortAnswer, order);
        this.answerBoxRows = 4;
        this.pointsPossible = 0;
        this.question = "";
        this.questionWeight = 0;
    }
}

export type AssessmentActivityConfiguration = {
    assessmentTerm: string;
    header: string;
    items: Array<AssessmentItem | MultipleChoiceItem | ShortAnswerItem | SectionItem>;
    multipleChoiceWeight: number;
    showMissedQuestionsOnResults: boolean;
    showResultsOnCompletion: boolean;
};

export type AssessmentActivityCompletion = {
    completedItems: Array<AssessmentItem | MultipleChoiceItem | SectionItem | ShortAnswerItem>;
    multipleChoiceWeightAtCompletion: number;
};

export class AssessmentActivityDefaults
    extends LearningComponentBaseDefaults<AssessmentActivityConfiguration, AssessmentActivityCompletion> {
    constructor() {
        super();
        this.defaultConfig = {
            assessmentTerm: "Test",
            header: "",
            items: [],
            multipleChoiceWeight: 0,
            showMissedQuestionsOnResults: false,
            showResultsOnCompletion: false,
        };
        this.defaultCompletion = {
            completedItems: [],
            multipleChoiceWeightAtCompletion: 0,
        };
    }
}