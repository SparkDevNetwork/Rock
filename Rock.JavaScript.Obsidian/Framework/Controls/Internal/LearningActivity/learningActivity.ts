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

/** Determines what screen should be shown. */
export enum ComponentScreen {
    /** The screen for configuring the activity. */
    Configuration = "configuration",

    /** The screen for the student or facilitator to complete the activity. */
    Completion = "completion",

    /** The screen for the facilitator to score the activity. */
    Scoring = "scoring",

    /** The screen for the facilitator to view the results of the completed activity. */
    Summary = "summary"
}

/**
 * The basic properties that all learning activity components must support.
 */
type LearningActivityComponentBaseProps = {
    /** The LearningActivityBag for saving any activity configuration data. */
    activityBag: {
        type: PropType<LearningActivityBag>;
        required: true;
    };

    /** The LearningActivityCompletionBag for saving any completion data. */
    completionBag: {
        type: PropType<LearningActivityCompletionBag>;
        required: false;
    };

    /** Whether or not configuration changes that affect existing responses should be disabled. */
    disabled: {
        type: PropType<boolean>,
        default: false
    };

    /** The screen that should be presented to the current user (e.g. 'Completion' screen for a student). */
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

    disabled: {
        type: Boolean as PropType<boolean>,
        default: false
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

/**
 * A generic type destructured to a list of refs.
 */
type ToRef<T> = {
    [Property in keyof T]: Ref<T[Property]>;
};

type LearningComponentBaseProps = {
    /** The name of the activity. */
    activityName: ComputedRef<string>;

    /** The person responsible for completing the activity.  */
    assignee: ComputedRef<LearningActivityParticipantBag>;

    /** Whether the assignee is a student or a facilitator. */
    assignTo: ComputedRef<AssignTo>;

    /** The binary file related to the activity (e.g. an uploaded assignment).  */
    binaryFile: Ref<ListItemBag | null>;

    /** The computed CSS classes to aplpy to the container element. */
    containerClasses: ComputedRef<string[]>;

    /** The person currently viewing the activity. */
    currentPerson: ComputedRef<LearningActivityParticipantBag>;

    /** The text to use for the assignee when no assignee property is available. */
    defaultAssigneeDescription: ComputedRef<string>;

    /** The URL for the binaryFile if one was provided. */
    fileUrl: ComputedRef<string>;

    /** The title of the panel to display in the template. */
    panelTitle: ComputedRef<string>;

    /** The student the activity is related to. */
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

/**
 * The class containing the default configuration and default completion classes.
 * These classes define the list of ref properties that will be dynamically
 * returned by the useLearningComponent composable.
 */
export abstract class LearningComponentBaseDefaults<TConfig, TCompletion> {
    /** The shape of the data necessary for managing the activity component. */
    defaultConfig!: TConfig;

    /** The shape of the data necessary for handling the student completion logic. */
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
 * @returns The properties common to all learning activity components, the parsed settings
 *  and completion objects.
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
        configuration = defaults.defaultConfig;
    }

    let completion = defaults.defaultCompletion;
    try {
        completion = JSON.parse(toValue(completionBag)?.activityComponentCompletionJson ?? "") as TCompletion;
    }
    catch (error) {
        completion = defaults.defaultCompletion;
    }

    const assignTo = ref(toValue(activityBag)?.assignTo ?? AssignTo.Student);
    const defaultAssigneeDescription = computed(() => `The ${AssignToDescription[toValue(activityBag)?.assignTo ?? AssignTo.Student]}`);
    const currentPerson = computed(() => toValue(activityBag)?.currentPerson ?? {} as LearningActivityParticipantBag);
    const student = ref(toValue(completionBag)?.student ?? {} as LearningActivityParticipantBag);
    const activityName = ref(toValue(activityBag)?.name ?? "");

    /**
     * If the assignee is the faciliator and the facilitator is currently viewing
     * then return the current person. Otherwise, return the student.
     * Note - because multiple facilitators may be configured we can't just return
     * the faciliator if based on the assignTo value alone since we wouldn't know which facilitator.
     */
    const assignee = computed((): LearningActivityParticipantBag => {
        if (assignTo.value === AssignTo.Facilitator && currentPerson.value.isFacilitator === true) {
            return currentPerson.value;
        }
        else {
            return student.value;
        }
    });

    /**
     * The binary file related to the activity (e.g. an uploaded assignment).
     */
    const binaryFile = ref<ListItemBag | null>(toValue(completionBag)?.binaryFile ?? null);

    /**
     * The default title of the panel to display.
     * This function is provided for consistency,
     * but can be overridden by the component implementation.
     */
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

    /**
     * The file URL to the binary file if one was provided.
     */
    const fileUrl = computed((): string => {
        if (toValue(completionBag)?.binaryFile?.value) {
            return `/GetFile.ashx?guid=${toValue(completionBag)?.binaryFile?.value}`;
        }

        return "";
    });

    /** The CSS classes for the containing panel. */
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

        This code is what destructures the TConfig and TCompletion property values.
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
