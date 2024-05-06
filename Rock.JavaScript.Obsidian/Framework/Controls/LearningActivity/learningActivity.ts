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
import { AssignTo } from "@Obsidian/Enums/Lms/assignTo";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

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

    activityName: {
        type: PropType<string>;
        default:() => "";
    };

    assignTo: {
        type: PropType<AssignTo>;
        default: () => 0;
    };

    binaryFile: {
        type: PropType<ListItemBag>,
        required: false
    },

    componentSettingsJson: {
        type: PropType<string>;
        default: () => "{}";
    };

    componentCompletionJson: {
        type: PropType<string>;
        default: () => "{}";
    };

    currentPerson: {
        type: PropType<LearningActivityParticipantBag>;
        required: false;
    };

    pointsEarned: {
        type: PropType<number>;
        required: false
    };

    pointsPossible: {
        type: PropType<number>;
        required: false
    };

    screenToShow: {
        type: PropType<ComponentScreen>,
        default: ComponentScreen.Summary
    };

    student: {
        type: PropType<LearningActivityParticipantBag>;
        required: false;
    };
};

/**
 * The emits that all Learning Activity Components are expected to emit.
 * Failure to implement all emits could result in unexpected behavior.
 */
type LearningActivityComponentBaseEmits = {
    /** Emitted when there are changes to the configuration of a component. */
    componentSettingsChanged(configurationValues: string): void;

    /** Emitted when the completion of a component has been modified. */
    componentCompletionChanged(_componentCompletionAsJson: string): void;

    /** Emitted when the completion of a component has been finalized. */
    componentCompleted(_componentCompletionAsJson: string): void;

    /** Emitted when the completion of a component has been cancelled.
      *  It's expected the parent of the component will discard any changes. */
    componentCompletionCancelled(): void;
};

/**
 * Get the standard properties that all learning activity components must support.
 */
export function getLearningActivityProps(): LearningActivityComponentBaseProps {
    return {

        activityName: {
            type: String as PropType<string>,
            default:() => ""
        },

        assignTo: {
            type: Object as PropType<AssignTo>,
            default: () => AssignTo.Student
        },

        binaryFile: {
            type: Object as PropType<ListItemBag>,
            required: false
        },

        componentCompletionJson: {
            type: String as PropType<string>,
            default: () => "{}"
        },

        componentSettingsJson: {
            type: String as PropType<string>,
            default: () => "{}"
        },

        currentPerson: {
            type: Object as PropType<LearningActivityParticipantBag>,
            required: false,
        },

        pointsEarned: {
            type: Number as PropType<number>,
            required: false
        },

        pointsPossible: {
            type: Number as PropType<number>,
            required: false
        },

        screenToShow: {
            type: Object as PropType<ComponentScreen>,
            default: ComponentScreen.Summary
        },

        student: {
            type: Object as PropType<LearningActivityParticipantBag>,
            required: false,
        },
    };
}

/**
 * Get the standard properties that all learning activity components must support.
 */
export function getLearningActivityEmits(): LearningActivityComponentBaseEmits {
    return {
        componentSettingsChanged(_componentSettingsAsJson: string): void {

        },
        componentCompletionChanged(_componentCompletionAsJson: string): void {

        },
        componentCompleted(_componentCompletionAsJson: string): void {

        },
        componentCompletionCancelled(): void {

        }
    };
}

/**
 * Handles the rendering of a Learning Activity Component from JSON and based on it's display type.
 *
 * Note to plugins: Do not implement this interface directly or your implementation
 * may break if we add new required methods.
 */
// export interface ILearningActivityType {
//     activityTypeName: string;

//     getConfigurationComponent(): Component;

//     getCompletionComponent(): Component;

//     getSummaryComponent(): Component;

//     getScoringComponent(): Component;

//     //saveConfiguration(): void;

//     // The parent component will handle this.
//     // saveProgress(completionJson: string, entityId: ): void;
// }

/**
 * Basic learning activity type implementation that is suitable for implementations to
 * extend.
 */
// export abstract class LearningActivityBase implements ILearningActivityType {

//     activityTypeName: string = "Learning Activity Base";

//     public getTextValue(value: string, _configurationValues: Record<string, unknown>): string {
//         return value ?? "";
//     }

//     public getComponentHtmlContent(value: string, configurationValues: Record<string, unknown>, _isEscaped?: boolean): string {
//         return `${escapeHtml(this.getTextValue(value, configurationValues))}`;
//     }

//     public getConfigurationComponent(): Component {
//         return defineComponent({
//             name: "LearningActivity.Configuration",
//             props: { ...getLearningActivityProps(), isEscaped: Boolean },
//             setup: (props) => {
//                 return {
//                     content: computed(() => {
//                         return this.getComponentHtmlContent(props.modelValue ?? "", props.configurationValues, props.isEscaped);
//                     })
//                 };
//             },

//             template: `<span v-html="content"></span>`
//         });
//     }

//     public getCompletionComponent(): Component {
//         return defineComponent({
//             name: "LearningActivity.Completion",
//             props: { ...getLearningActivityProps(), isEscaped: Boolean },
//             setup: (props) => {
//                 return {
//                     content: computed(() => {
//                         return this.getComponentHtmlContent(props.modelValue ?? "", props.configurationValues, props.isEscaped);
//                     })
//                 };
//             },

//             template: `<span v-html="content"></span>`
//         });
//     }

//     getSummaryComponent(): Component {
//         return defineComponent({
//             name: "LearningActivity.Summary",
//             props: { ...getLearningActivityProps(), isEscaped: Boolean },
//             setup: (props) => {
//                 return {
//                     content: computed(() => {
//                         return this.getComponentHtmlContent(props.modelValue ?? "", props.configurationValues, props.isEscaped);
//                     })
//                 };
//             },

//             template: `<span v-html="content"></span>`
//         });
//     }

//     getScoringComponent(): Component {
//         return defineComponent({
//             name: "LearningActivity.Scoring",
//             props: { ...getLearningActivityProps(), isEscaped: Boolean },
//             setup: (props) => {
//                 return {
//                     content: computed(() => {
//                         return this.getComponentHtmlContent(props.modelValue ?? "", props.configurationValues, props.isEscaped);
//                     })
//                 };
//             },

//             template: `<span v-html="content"></span>`
//         });
//     }
// }