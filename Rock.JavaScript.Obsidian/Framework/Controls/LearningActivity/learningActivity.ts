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

//import { IActivityComponentField } from "./activityComponentField.partial";
import { Component, PropType, computed, defineComponent } from "vue";
// import { getLearningActivityProps } from "./utils.partial";
import { escapeHtml } from "@Obsidian/Utility/stringUtils";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";

/** Determines how the component should be rendered. */
export enum ComponentScreen {
    Configuration = "configuration",
    Completion = "completion",
    Scoring = "scoring",
    Summary = "summary"
}

export type ConfigurationValues = Record<string, string>;

export const enum ConfigurationValueKey {
    Options = "options"
}

export type VideoWatchLearningActivityValue = {
    completionThreshold: number;
    footerContent: string,
    headerContent: string,
    video: ListItemBag;
};

/**
 * The basic properties that all learning activity components must support.
 */
type LearningActivityComponentBaseProps = {
    modelValue: {
        type: PropType<string>,
        required: false
    };

    configurationValues: {
        type: PropType<ConfigurationValues>;
        default: () => ConfigurationValues;
    };

    completionJson: {
        type: PropType<string>;
        required: false
    };
    screenToShow: {
        type: PropType<ComponentScreen>,
        default: ComponentScreen.Summary
    };

    studentId: {
        type: PropType<number>;
        required: false;
    };
};

type LearningActivityComponentBaseEmits = {
    saveConfiguration(configurationValues: ConfigurationValues): void;
    saveCompletion(configurationValues: ConfigurationValues): void;
};

/**
 * Get the standard properties that all learning activity components must support.
 */
export function getLearningActivityProps(): LearningActivityComponentBaseProps {
    return {
        modelValue: {
            type: String as PropType<string>,
            required: false
        },

        screenToShow: {
            type: Object as PropType<ComponentScreen>,
            default: ComponentScreen.Summary
        },

        completionJson: {
            type: Object as PropType<string>,
            required: false
        },

        configurationValues: {
            type: Object as PropType<ConfigurationValues>,
            default: () => ({})
        },

        studentId: {
            type: Object as PropType<number>,
            required: false,
        }
    };
}

/**
 * Get the standard properties that all learning activity components must support.
 */
export function getLearningActivityEmits(): LearningActivityComponentBaseEmits {
    return {
        saveConfiguration(configurationValues: ConfigurationValues) {

        },
        saveCompletion(configurationValues: ConfigurationValues): void {

        }
    };
}

/**
 * Handles the rendering of a Learning Activity Component from JSON and based on it's display type.
 *
 * Note to plugins: Do not implement this interface directly or your implementation
 * may break if we add new required methods.
 */
export interface ILearningActivityType {
    activityTypeName: string;

    getConfigurationComponent(): Component;

    getCompletionComponent(): Component;

    getSummaryComponent(): Component;

    getScoringComponent(): Component;

    //saveConfiguration(): void;

    // The parent component will handle this.
    // saveProgress(completionJson: string, entityId: ): void;
}

/**
 * Basic learning activity type implementation that is suitable for implementations to
 * extend.
 */
export abstract class LearningActivityBase implements ILearningActivityType {

    activityTypeName: string = "Learning Activity Base";

    public getTextValue(value: string, _configurationValues: Record<string, string>): string {
        return value ?? "";
    }

    public getComponentHtmlContent(value: string, configurationValues: Record<string, string>, _isEscaped?: boolean): string {
        return `${escapeHtml(this.getTextValue(value, configurationValues))}`;
    }

    public getConfigurationComponent(): Component {
        return defineComponent({
            name: "LearningActivity.Configuration",
            props: { ...getLearningActivityProps(), isEscaped: Boolean },
            setup: (props) => {
                return {
                    content: computed(() => {
                        return this.getComponentHtmlContent(props.modelValue ?? "", props.configurationValues, props.isEscaped);
                    })
                };
            },

            template: `<span v-html="content"></span>`
        });
    }

    public getCompletionComponent(): Component {
        return defineComponent({
            name: "LearningActivity.Completion",
            props: { ...getLearningActivityProps(), isEscaped: Boolean },
            setup: (props) => {
                return {
                    content: computed(() => {
                        return this.getComponentHtmlContent(props.modelValue ?? "", props.configurationValues, props.isEscaped);
                    })
                };
            },

            template: `<span v-html="content"></span>`
        });
    }

    getSummaryComponent(): Component {
        return defineComponent({
            name: "LearningActivity.Summary",
            props: { ...getLearningActivityProps(), isEscaped: Boolean },
            setup: (props) => {
                return {
                    content: computed(() => {
                        return this.getComponentHtmlContent(props.modelValue ?? "", props.configurationValues, props.isEscaped);
                    })
                };
            },

            template: `<span v-html="content"></span>`
        });
    }

    getScoringComponent(): Component {
        return defineComponent({
            name: "LearningActivity.Scoring",
            props: { ...getLearningActivityProps(), isEscaped: Boolean },
            setup: (props) => {
                return {
                    content: computed(() => {
                        return this.getComponentHtmlContent(props.modelValue ?? "", props.configurationValues, props.isEscaped);
                    })
                };
            },

            template: `<span v-html="content"></span>`
        });
    }
}