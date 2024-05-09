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

import { ComputedRef, PropType, Ref, computed, ref, watch } from "vue";
import { LearningActivityParticipantBag } from "@Obsidian/ViewModels/Blocks/Lms/LearningActivityComponent/learningActivityParticipantBag";
import { LearningActivityBag } from "@Obsidian/ViewModels/Blocks/Lms/LearningActivityDetail/learningActivityBag";
import { LearningActivityCompletionBag } from "@Obsidian/ViewModels/Blocks/Lms/LearningActivityCompletionDetail/learningActivityCompletionBag";
import { AssignTo, AssignToDescription } from "@Obsidian/Enums/Lms/assignTo";
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
    activityBag: {
        type: PropType<LearningActivityBag>;
        required: true;
    };

    completionBag: {
        type: PropType<LearningActivityCompletionBag>;
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

type LearningComponent<TConfig extends object, TCompletion extends object> = {
    //activityName: Ref<string>;
    assignee: Ref<LearningActivityParticipantBag>;
    assignTo: Ref<AssignTo>;
    binaryFile: Ref<ListItemBag | null>;
    completion: TCompletion;
    configuration: TConfig;
    containerClasses: ComputedRef<string[]>;
    currentPerson: LearningActivityParticipantBag;
    defaultAssigneeDescription: ComputedRef<string>;
    fileUrl: ComputedRef<string>;
    panelTitle: ComputedRef<string>;
    student: Ref<LearningActivityParticipantBag>;
};

/**
 * Initializes the base functionality and properties common to all learning activity components.
 *
 * @returns The properties common to all learning activity components.
 */
export function useLearningComponent<TConfig extends object, TCompletion extends object>(
    activityBag: Ref<LearningActivityBag>,
    completionBag: Ref<LearningActivityCompletionBag>,
    screenToShow: Ref<ComponentScreen>
): LearningComponent<TConfig, TCompletion> {

    let configuration = { } as TConfig;
    try {
        configuration = JSON.parse(activityBag.value?.activityComponentSettingsJson ?? "{}") as TConfig;
    }
    catch (error) {
        // Intentionally ignored error.
    }

    let completion = { } as TCompletion;
    try {
        completion = JSON.parse(completionBag.value?.activityComponentCompletionJson ?? "{}") as TCompletion;
    }
    catch (error) {
        // Intentionally ignored error.
    }

    const assignTo = ref(activityBag.value?.assignTo ?? AssignTo.Student);
    const defaultAssigneeDescription = computed(() => `The ${AssignToDescription[assignTo.value]}`);
    const currentPerson = activityBag.value?.currentPerson ?? {} as LearningActivityParticipantBag;
    const student = ref(completionBag?.value?.student ?? {} as LearningActivityParticipantBag);
    //const activityName = ref(activityBag?.value?.name ?? "");

    const assignee = computed((): LearningActivityParticipantBag => {
        if (assignTo.value === AssignTo.Facilitator && activityBag.value.currentPerson?.isFacilitator === true) {
            return activityBag.value.currentPerson;
        }
        else {
            return student.value;
        }
    });

    const binaryFile = ref<ListItemBag | null>(completionBag?.value?.binaryFile ?? null);

    const panelTitle = computed((): string => {
        console.warn("computing panelTitle", activityBag.value?.name);
        const activityName = activityBag.value?.name ?? "";
        switch (screenToShow.value) {
            case ComponentScreen.Completion:
                return activityName;
            case ComponentScreen.Configuration:
                return `Configure ${activityName}`;
            case ComponentScreen.Scoring:
                return `Score ${activityName}`;
            case ComponentScreen.Summary:
                return `${activityName} - Summary`;
            default:
                return activityName ?? "";
        }
    });

    const fileUrl = computed((): string => {
        if (completionBag?.value?.binaryFile?.value) {
            return `/GetFile.ashx?guid=${completionBag?.value?.binaryFile?.value}`;
        }

        return "";
    });

    /** CSS classes for the panel. */
    const containerClasses = computed((): string[] => {
        return [
            `lms-${screenToShow.value}-container`,
            `lms-file-upload-${screenToShow.value}-container`
        ];
    });

    watch(() => [activityBag, completionBag], () => {
        console.warn("update to bag", {activityBag:activityBag});
    });

    return {
        //activityName,
        assignee,
        assignTo,
        binaryFile,
        completion,
        configuration,
        containerClasses,
        currentPerson,
        defaultAssigneeDescription,
        fileUrl,
        panelTitle,
        student
    };
}
