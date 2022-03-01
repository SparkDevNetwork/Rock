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

import { Guid } from "../../../../Util/guid";
import { ListItem } from "../../../../ViewModels";
import { FormConfirmationEmail, FormFieldType, FormNotificationEmail, FormPersonEntry, FormSection, FormSettings } from "../Shared/types";

// #region Interfaces

/**
 * Identifies an aside that provides a set of supported actions. */
export interface IAsideProvider {
    /**
     * Determines if the aside is safe to close. If an aside has invalid settings
     * then it should not be closed.
     *
     * @returns true if the aside can be closed; otherwise false to prevent the action.
     */
    isSafeToClose: () => boolean;
}

// #endregion

// #region Types

/**
 * The settings that are handled by the General aside panel.
 */
export type GeneralAsideSettings = {
    /** Determines what method will be used to set the campus context of the submission. */
    campusSetFrom?: number; // TODO: Enum

    /** Determines if the form will display a person entry section. */
    hasPersonEntry?: boolean;
};

/**
 * The settings that are available to edit inside the Section aside panel.
 */
export type SectionAsideSettings = {
    /** The unique identifier of the section being edited. */
    guid: Guid;

    /** The title that will be displayed above this section. */
    title: string;

    /** The additional descriptive text that will be displayed under the title. */
    description: string;

    /** Determines if the heading separator will be visible when the form is displayed. */
    showHeadingSeparator: boolean;

    /** The unique identifier of the type that controls how the section is rendered. */
    type: Guid | null;
};

/**
 * The configuration settings that are passed to the Communication tab to be
 * displayed and configured.
 */
export type FormCommunication = {
    /** The confirmation e-mail settings for this form. */
    confirmationEmail?: FormConfirmationEmail;

    /** The notification e-mail settings for this form. */
    notificationEmail?: FormNotificationEmail;
};

/**
 * The settings that are used and modified on the Form Builder tab.
 */
export type FormBuilderSettings = {
    /**
     * The HTML content that will be displayed before all the sections of
     * the form.
     */
    headerContent?: string | null;

    /**
     * The HTML content that will be displayed after all the sections of
     * the form.
     */
    footerContent?: string | null;

    /** The list of sections that exist in this form, including all the fields. */
    sections?: FormSection[] | null;

    /** Determines how the form's campus context will be set when it first runs. */
    campusSetFrom?: number;

    /**
     * Determines if the person entry section should be displayed at the top
     * of the form.
     */
    allowPersonEntry?: boolean;

    /** The settings that describe how the person entry section will be displayed. */
    personEntry?: FormPersonEntry | null;
};

/**
 * Custom ListItem that extends the item to incldue details about what forced
 * settings the template has.
 */
export type FormTemplateListItem = ListItem & {
    /** The form header content that will be displayed above the form. */
    formHeader?: string | null;

    /** The form footer content that will be displayed below the form. */
    formFooter?: string | null;

    /** True if the template forces the login required setting; otherwise false. */
    isLoginRequiredConfigured?: boolean;

    /** True if the template forces the person entry settings; otherwise false. */
    isPersonEntryConfigured?: boolean;

    /** True if the template forces the confirmation email settings; otherwise false. */
    isConfirmationEmailConfigured?: boolean;

    /** True if the template forces the completion action settings; otherwise false. */
    isCompletionActionConfigured?: boolean;
};

/**
 * Represents the sources of truth for various pickers and lists of entities
 * that will be used by the JavaScript code.
 */
export type FormValueSources = {
    /** The list of campus topic options that are available to pick from. */
    campusTopicOptions?: ListItem[] | null;

    /** The list of campus type options that are available to pick from. */
    campusTypeOptions?: ListItem[] | null;

    /** The list of campus status options that are available to pick from. */
    campusStatusOptions?: ListItem[] | null;

    /** The list of record status options that are available to pick from. */
    recordStatusOptions?: ListItem[] | null;

    /** The list of connection status options that are available to pick from. */
    connectionStatusOptions?: ListItem[] | null;

    /** The list of address type options that are available to pick from. */
    addressTypeOptions?: ListItem[] | null;

    /** The list of e-mail template options that are available to pick from. */
    emailTemplateOptions?: ListItem[] | null;

    /** The list of section type options that are available to pick from. */
    sectionTypeOptions?: ListItem[] | null;

    /** The list of field types that are available to pick from. */
    fieldTypes?: FormFieldType[] | null;

    /** The form templates that are available to pick from. */
    formTemplateOptions?: FormTemplateListItem[] | null;
};

/**
 * Represents a single attribute that came from outside the form that can be
 * used when the form processes.
 */
export type FormOtherAttribute = {
    /** The unique identifier of the attribute. */
    guid?: Guid | null;

    /** The unique identifier of the field type. */
    fieldTypeGuid?: Guid | null;

    /** The name of the attribute. */
    name?: string | null;
};

/**
 * The primary view model that contains all the runtime information needed
 * by the FormBuilder block.
 */
export type FormBuilderDetailConfiguration = {
    /** The URL to redirect the individual to when the Submissions tab is clicked. */
    submissionsPageUrl?: string | null;

    /** The URL to redirect the individual to when the Analytics tab is clicked. */
    analyticsPageUrl?: string | null;

    /** The source of information for various pickers and controls. */
    sources?: FormValueSources | null;

    /** The unique identifier of the form being edited. */
    formGuid: Guid;

    /** The details about the form that is to be edited. */
    form?: FormSettings | null;

    /** Other attributes that are available for use in the form. */
    otherAttributes?: FormOtherAttribute[] | null;
};

// #endregion
