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

import { PropType } from "vue";
import { LearningActivityParticipantBag } from "@Obsidian/ViewModels/Blocks/Lms/LearningActivityComponent/learningActivityParticipantBag";
import { LearningActivityBag } from "@Obsidian/ViewModels/Blocks/Lms/LearningActivityDetail/learningActivityBag";
import { LearningActivityCompletionBag } from "@Obsidian/ViewModels/Blocks/Lms/LearningActivityCompletionDetail/learningActivityCompletionBag";

/** Determines how the component should be rendered. */
export enum ComponentScreen {
    Configuration = "configuration",
    Completion = "completion",
    Scoring = "scoring",
    Summary = "summary"
}

/**
 * The basic properties that all learning activity components must support.
 */
type LearningActivityComponentBaseProps = {
    activityBag: {
        type: PropType<LearningActivityBag>;
        required: true;
    };

    completionBag: {
        type: PropType<LearningActivityCompletionBag>;
        required: false;
    };

    currentPerson: {
        type: PropType<LearningActivityParticipantBag>;
        required: false;
    };

    screenToShow: {
        type: PropType<ComponentScreen>,
        default: ComponentScreen.Summary
    };
};


/**
 * The emits that all Learning Activity Components are expected to emit.
 * Failure to implement all emits could result in unexpected behavior.
 */
type LearningActivityComponentBaseEmits = {
    /** The model change event for the LearningActivityBag of the component. */
    ["update:activityBag"]: (activityBag: LearningActivityBag) => void;

    /** The model change event for the LearningActivityCompletionBag of the component. */
    ["update:completionBag"]: (completionBag: LearningActivityCompletionBag) => void;
    //(e: "update:completionBag", completionBag: LearningActivityCompletionBag): void;

    /**
     * @description Emitted when the screen's complete or cancel button has been clicked.
     * @param isSuccess True if the button click was for a completion; false if cancelled.
     */
    completed(isSuccess: boolean): void;
};

/**
 * Get the standard properties that all learning activity components must support.
 */
export const learningActivityProps: LearningActivityComponentBaseProps = {
    activityBag: {
        type: Object as PropType<LearningActivityBag>,
        required: true,
    },

    completionBag: {
        type: Object as PropType<LearningActivityCompletionBag>,
        required: false,
    },

    currentPerson: {
        type: Object as PropType<LearningActivityParticipantBag>,
        required: false,
    },

    screenToShow: {
        type: Object as PropType<ComponentScreen>,
        default: ComponentScreen.Summary
    }
};

/**
 * Get the standard emits that all learning activity components must support.
 */
export const learningActivityEmits: LearningActivityComponentBaseEmits = {
    completed(): void { },
    ["update:activityBag"](_bag: LearningActivityBag): void { },
    ["update:completionBag"](_bag: LearningActivityCompletionBag): void { }
};