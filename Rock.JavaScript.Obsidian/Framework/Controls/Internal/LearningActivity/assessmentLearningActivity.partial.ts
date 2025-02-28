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

export enum AssessmentItemType {
    MultipleChoice = 0,
    Section = 1,
    ShortAnswer = 2
}

export type AssessmentItem = {
    type: AssessmentItemType;

    uniqueId: Guid;

    hasBeenGraded?: boolean | null;

    order: number;

    pointsEarned?: number | null;

    response?: string | null;

    answers?: string[] | null;

    correctAnswer?: string | null;

    helpText?: string | null;

    question?: string | null;

    title?: string | null;

    summary?: string | null;

    answerBoxRows?: number | null;

    maxCharacterCount?: number | null;

    pointsPossible?: number | null;

    questionWeight?: number | null;
};

/**
 * A function used to validate that the sum of all weights for an assessment is 100 (percent).
 * This function is shared between the AssessmentLearningActivity (parent) component
 * and zero or more assessmentItemShortAnswer (children) components.
 *
 * @returns An error message if the sum of weights is not 100, or true if the sum is 100.
 */
export type WeightsValidationFunction = () => string | boolean;
