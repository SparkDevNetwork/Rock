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

import { ComputedRef, MaybeRefOrGetter, PropType, Ref, computed, ref, toValue } from "vue";
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
    completed(_isSuccess: boolean): void { },
    ["update:activityBag"](_bag: LearningActivityBag): void { },
    ["update:completionBag"](_bag: LearningActivityCompletionBag): void { }
};

type ToRef<T> = {
    [Property in keyof T]: Ref<T[Property]>;
};

type LearningComponentBaseProps = {
    activityName: ComputedRef<string>;
    assignee: ComputedRef<LearningActivityParticipantBag>;
    assignTo: ComputedRef<AssignTo>;
    binaryFile: Ref<ListItemBag | null>;
    containerClasses: ComputedRef<string[]>;
    currentPerson: ComputedRef<LearningActivityParticipantBag>;
    defaultAssigneeDescription: ComputedRef<string>;
    fileUrl: ComputedRef<string>;
    panelTitle: ComputedRef<string>;
    student: Ref<LearningActivityParticipantBag>;
};

/**
 * Default implementation of the learning component state for internal use.
 *
 * @private The is an internal class that should not be used by plugins as it may change at any time.
 */
type LearningComponent<TConfig, TCompletion> =
    LearningComponentBaseProps &
    ToRef<Exclude<TConfig, LearningComponentBaseProps>> &
    ToRef<Exclude<TCompletion, LearningComponentBaseProps>>;

export abstract class LearningComponentBaseDefaults<TConfig, TCompletion> {
    defaultConfig!: TConfig;
    defaultCompletion!: TCompletion;
}

/**
 * Initializes the base functionality and properties common to all learning activity components.
 * And parses the component Settings and Completion JSON into the specified types.
 *
 * @param activityBag The LearningActivityBag for the activity.
 * @param completionBag The LearningActivityCompletionBag for the activty.
 * @param screenToShow The screen that should be shown to the current user.
 * @param defaults A class extending the LearningComponentBaseDefaults which initializes default values for the control.
 *      NOTE: Failure to provide this class with all fields initialized
 *      will result in no TConfig or TCompletion ref's to be returned by the composable.
 * @returns The properties common to all learning activity components
 *  and the parsed settings and completion objects.
 */
export function useLearningComponent<TConfig extends object, TCompletion extends object>(
    activityBag: MaybeRefOrGetter<LearningActivityBag>,
    completionBag: MaybeRefOrGetter<LearningActivityCompletionBag>,
    screenToShow: MaybeRefOrGetter<ComponentScreen>,
    defaults: LearningComponentBaseDefaults<TConfig, TCompletion>
): LearningComponent<TConfig, TCompletion> {

    let configuration = defaults.defaultConfig;
    try {
        configuration = JSON.parse(toValue(activityBag)?.activityComponentSettingsJson ?? "") as TConfig;
    }
    catch (error) {
        // It's expected that this will happen with new instances so don't log it as an error.
        console.debug(error, toValue(activityBag)?.activityComponentSettingsJson);
        configuration = defaults.defaultConfig;
    }

    let completion = defaults.defaultCompletion;
    try {
        completion = JSON.parse(toValue(completionBag)?.activityComponentCompletionJson ?? "") as TCompletion;
    }
    catch (error) {
        // It's expected that this will happen with new instances so don't log it as an error.
        console.debug(error, toValue(completionBag)?.activityComponentCompletionJson);
        completion = defaults.defaultCompletion;
    }

    const assignTo = ref(toValue(activityBag)?.assignTo ?? AssignTo.Student);
    const defaultAssigneeDescription = computed(() => `The ${AssignToDescription[toValue(activityBag)?.assignTo ?? AssignTo.Student]}`);
    const currentPerson = computed(() => toValue(activityBag)?.currentPerson ?? {} as LearningActivityParticipantBag);
    const student = ref(toValue(completionBag)?.student ?? {} as LearningActivityParticipantBag);
    const activityName = ref(toValue(activityBag)?.name ?? "");

    const assignee = computed((): LearningActivityParticipantBag => {
        if (assignTo.value === AssignTo.Facilitator && currentPerson.value.isFacilitator === true) {
            return currentPerson.value;
        }
        else {
            return student.value;
        }
    });

    const binaryFile = ref<ListItemBag | null>(toValue(completionBag)?.binaryFile ?? null);

    const panelTitle = computed(() => {
        const activityName = toValue(activityBag).name ?? "";
        switch (toValue(screenToShow)) {
            case ComponentScreen.Configuration:
                return `Configure ${activityName}`;
            case ComponentScreen.Scoring:
                return `Score ${activityName}`;
            case ComponentScreen.Summary:
                return `${activityName} - Summary`;
            default:
                return activityName;
        }
    });

    const fileUrl = computed((): string => {
        if (toValue(completionBag)?.binaryFile?.value) {
            return `/GetFile.ashx?guid=${toValue(completionBag)?.binaryFile?.value}`;
        }

        return "";
    });

    /** CSS classes for the panel. */
    const containerClasses = computed((): string[] => {
        const screenName = toValue(screenToShow);
        const componentName = toValue(activityBag)?.activityComponent?.name ?? "";
        return [
            `lms-${screenName.toLowerCase()}-container`,
            `lms-${componentName.toLowerCase()}-container`
        ];
    });

    /*
        Get the properties of the generic types - TConfig and TCompletion.
        Assuming the LearningComponentBaseDefaults was given with proper values
        all fields should be initialized and therefore returned as ref's.
     */
    const dynamicProps: { [key: string]: unknown } = {};
    for (const key in configuration) {
        if (Object.prototype.hasOwnProperty.call(configuration, key)) {
            const value = configuration[key];
            dynamicProps[key] = ref(value);
        }
    }

    for (const key in completion) {
        if (Object.prototype.hasOwnProperty.call(completion, key)) {
            const value = completion[key];
            dynamicProps[key] = ref(value);
        }
    }

    return {
        activityName,
        assignee,
        assignTo,
        binaryFile,
        containerClasses,
        currentPerson,
        defaultAssigneeDescription,
        ...dynamicProps,
        fileUrl,
        panelTitle,
        student
    } as LearningComponent<TConfig, TCompletion>;
}
