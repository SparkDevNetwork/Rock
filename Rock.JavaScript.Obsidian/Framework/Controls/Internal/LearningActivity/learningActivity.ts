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
import { LearningClassActivityBag } from "@Obsidian/ViewModels/Blocks/Lms/LearningClassActivityDetail/learningClassActivityBag";
import { LearningClassActivityCompletionBag } from "@Obsidian/ViewModels/Blocks/Lms/LearningClassActivityCompletionDetail/learningClassActivityCompletionBag";
import { AssignTo, AssignToDescription } from "@Obsidian/Enums/Lms/assignTo";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { isValidGuid } from "@Obsidian/Utility/guid";

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
        type: PropType<LearningClassActivityBag>;
        required: true;
    };

    /** The LearningActivityCompletionBag for saving any completion data. */
    completionBag: {
        type: PropType<LearningClassActivityCompletionBag>;
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
 * Extra values that can be passed to the completionValuesChanged emit.
 */
export type CompletionExtraValues = {
    /**
     * If not `undefined` then this will update the points earned for the
     * activity.
     */
    pointsEarned?: number;

    /**
     * If not `undefined` then this will update the binary file tracked by
     * the activity. If `null` then the binary file will be removed.
     */
    binaryFile?: ListItemBag | null;
};

/**
 * The emits that all Learning Activity Components are expected to emit.
 * Failure to implement all emits could result in unexpected behavior.
 */
type LearningActivityComponentBaseEmits = {
    /**
     * Emitted when the screen's complete or cancel button has been clicked.
     *
     * @param isSuccess True if the button click was for a completion; false if cancelled.
     */
    completed(isSuccess: boolean): void;

    /**
     * Emitted when the activity is done and should be closed
     * or the next activity opened.
     */
    closed(): void;

    /**
     * Emitted when the student comment has been changed.
     *
     * @param comment The student comment text.
     */
    commentChanged(comment: string): void;

    /**
     * Emitted when the settings for an activity has been changed while on the
     * configuration screen.
     *
     * @param settings The updated settings for the activity.
     */
    activitySettingsChanged(settings: Record<string, string>): void;

    /**
     * Emitted when the completion values for an activity have been changed.
     *
     * @param values The updated completion values.
     * @param extra Additional values related to the completion of an activity.
     */
    completionValuesChanged(values: Record<string, string>, extra?: CompletionExtraValues): void;
};

/**
 * Get the standard properties that all learning activity components must support.
 */
export const learningActivityProps: LearningActivityComponentBaseProps = {
    activityBag: {
        type: Object as PropType<LearningClassActivityBag>,
        required: true,
    },

    completionBag: {
        type: Object as PropType<LearningClassActivityCompletionBag>,
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
    closed(): void { },
    activitySettingsChanged(_settings: Record<string, string>): void { },
    completionValuesChanged(_values: Record<string, string>, _extra?: CompletionExtraValues): void { },
    commentChanged(_comment: string): void { }
};

/**
 * Default implementation of the learning component state for internal use.
 */
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

    /** Determines if the actiivty has been graded by a facilitator. */
    hasBeenGraded: ComputedRef<boolean>;

    /** Whether the student or facilitator has completed the activity. */
    isCompleted: ComputedRef<boolean>;

    /** The title of the panel to display in the template. */
    panelTitle: ComputedRef<string>;

    /** The student the activity is related to. */
    student: Ref<LearningActivityParticipantBag>;
};

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
 * @param classActivityBag The LearningActivityBag for the activity.
 * @param completionBag The LearningActivityCompletionBag for the activty.
 * @param screenToShow The screen that should be shown to the current user.
 * @param defaults A class extending the LearningComponentBaseDefaults which initializes default values for the control.
 *      NOTE: Failure to provide this class with all fields initialized
 *      will result in no TConfig or TCompletion ref's to be returned by the composable.
 * @returns The properties common to all learning activity components, the parsed settings
 *  and completion objects.
 */
export function useLearningComponent(
    classActivityBag: MaybeRefOrGetter<LearningClassActivityBag>,
    completionBag: MaybeRefOrGetter<LearningClassActivityCompletionBag>,
    screenToShow: MaybeRefOrGetter<ComponentScreen>
): LearningComponentBaseProps {
    const assignTo = ref(toValue(classActivityBag)?.assignTo ?? AssignTo.Student);
    const defaultAssigneeDescription = computed(() => `The ${AssignToDescription[toValue(classActivityBag)?.assignTo ?? AssignTo.Student]}`);
    const currentPerson = computed(() => toValue(classActivityBag)?.currentPerson ?? {} as LearningActivityParticipantBag);
    const student = ref(toValue(completionBag)?.student ?? {} as LearningActivityParticipantBag);
    const activityName = ref(toValue(classActivityBag)?.name ?? "");

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

    /** The binary file related to the activity (e.g. an uploaded assignment). */
    const binaryFile = ref<ListItemBag | null>(toValue(completionBag)?.binaryFile ?? null);

    /** Determines if the actiivty has been graded by a facilitator. */
    const hasBeenGraded = computed(() => isValidGuid(toValue(completionBag)?.gradedByPersonAlias?.value ?? ""));

    /** Whether the student or facilitator has completed the activity. */
    const isCompleted = computed(() => {
        const completion = toValue(completionBag);
        return completion?.isStudentCompleted || completion?.isFacilitatorCompleted;
    });

    /**
     * The default title of the panel to display.
     * This function is provided for consistency,
     * but can be overridden by the component implementation.
     */
    const panelTitle = computed(() => {
        const activityName = toValue(classActivityBag).name ?? "";
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
        const securityGrantToken = toValue(completionBag).binaryFileSecurityGrant ?? "";
        const securityGrantQueryParam = securityGrantToken.length > 0 ? `&securitygrant=${securityGrantToken}` : "";

        if (toValue(completionBag)?.binaryFile?.value) {
            return `/GetFile.ashx?guid=${toValue(completionBag)?.binaryFile?.value}${securityGrantQueryParam}`;
        }

        return "";
    });

    /** The CSS classes for the containing panel. */
    const containerClasses = computed((): string[] => {
        const screenName = toValue(screenToShow);
        const componentName = toValue(classActivityBag)?.activityComponent?.name ?? "";
        return [
            `lms-${screenName.toLowerCase()}-container`,
            `lms-${componentName.toLowerCase()}-container`
        ];
    });

    return {
        activityName,
        assignee,
        assignTo,
        binaryFile,
        containerClasses,
        currentPerson,
        defaultAssigneeDescription,
        fileUrl,
        hasBeenGraded,
        isCompleted,
        panelTitle,
        student
    } as LearningComponentBaseProps;
}
