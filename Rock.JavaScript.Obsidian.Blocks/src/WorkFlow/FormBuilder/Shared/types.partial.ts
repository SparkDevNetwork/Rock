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
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { FieldFilterGroupBag } from "@Obsidian/ViewModels/Reporting/fieldFilterGroupBag";

// #region Enums

/**
 * The state of the field being shown on the page and if it should be required.
 */
export const enum FormFieldVisibility {
    /** Don't show the control. */
    Hidden = 0,

    /** Control is visible, but a value is not required. */
    Optional = 1,

    /** Control is visible, and a value is required. */
    Required = 2
}

/**
 * The state of the field being shown on the page. Use this if a "required" option does not make sense.
 */
export const enum FormFieldShowHide {
    /** Don't show the control */
    Hide = 0,

    /** Control is visible */
    Show = 1
}

/**
 * The possible sources that can be used when generating an e-mail in the
 * FormBuilder system.
 */
export const enum FormEmailSourceType {
    /**
     * A template will be used that contains all the information required to
     * generate the e-mail contents.
     */
    UseTemplate = 0,

    /** Custom properties will be used to generate the e-mail contents. */
    Custom = 1
}

/**
 * The possible destination options for a form notification e-mail.
 */
export const enum FormNotificationEmailDestination {
    /** A specific individual in the database will be sent the notification e-mail. */
    SpecificIndividual = 0,

    /** One or more raw e-mail adresses will be sent the notification e-mail. */
    EmailAddress = 1,

    /**
     * A secondary lookup will be performed using CampusTopic to determine
     * the final recipient of the notification e-mail.
     */
    CampusTopic = 2
}

/**
 * The possible actions that can be performed after the form has been submitted.
 */
export const enum FormCompletionActionType {
    /** A message is displayed to the user after the form has been submitted. */
    DisplayMessage = 0,

    /** The individual will be redirected to a specific URL. */
    Redirect = 1
}

/**
 * Defines the way the campus context is set when a form is processed.
 */
export const enum CampusSetFrom {
    /** Use the campus of the current person who is logged in while filling out the form. */
    CurrentPerson = 0,

    /** Use the campus of the person in the Person attribute. */
    WorkflowPerson = 1,

    /** Use the campus from the "Campus" query string which could be an Id or Guid. */
    QueryString = 2
}

// #endregion

// #region Types

/**
 * All the settings related to a single section on the form.
 */
export type FormSection = {
    /** The unique identifier of this section. */
    guid: Guid;

    /** The title that will be displayed above this section. */
    title?: string | null;

    /** The additional descriptive text that will be displayed under the title. */
    description?: string | null;

    /**
     * Determines if the heading separator will be visible when the form is
     * displayed.
     */
    showHeadingSeparator?: boolean;

    /**
     * The unique identifier of the type that controls how the section is
     * rendered.
     */
    type?: string | null;

    /** The list of fields that are contained within this section. */
    fields?: FormField[] | null;

    /** The rule that controls when this section is visible. */
    visibilityRule?: FieldFilterGroupBag | null;
};

/**
 * Identifies a single form field that has been placed on the form.
 */
export type FormField = {
    /** The unique identifier for this form field. */
    guid: Guid;

    /**
     * The unique identifier of the field type used to identify this field.
     */
    fieldTypeGuid: Guid;

    /**
     * The unique identifier of the field type used to render the edit control
     * of this field if it is a universal type.
     */
    universalFieldTypeGuid?: Guid | null;

    /** The display name of this field. */
    name: string;

    /** The descriptive help text that will be rendered along with the name. */
    description?: string | null;

    /** The unique key used to identify this field in Lava operations. */
    key: string;

    /**
     * The width of this field in display columns. This should be a value
     * between 1 and 12 inclusive.
     */
    size: number;

    /**
     * Determines if this field will be considered required in order to submit
     * the form.
     */
    isRequired?: boolean;

    /** Determines if the label (name) should be hidden when this field is displayed. */
    isHideLabel?: boolean;

    /**
     * Determines if this field will be included in the results grid displayed
     * to staff when examining the submissions.
     */
    isShowOnGrid?: boolean;

    /** The configuration values that have been set for this field. */
    configurationValues?: Record<string, string> | null;

    /** The configuration values that have been set for this field when in edit mode. */
    editConfigurationValues?: Record<string, string> | null;

    /** The rule that controls when this field is visible. */
    visibilityRule?: FieldFilterGroupBag | null;

    /** The default value that will be used when the field is initially displayed. */
    defaultValue?: string | null;
};

/**
 * Identifies a single field type that can be used when designing the form.
 */
export type FormFieldType = {
    /** The unique identifier of the field type. */
    guid: Guid;

    /** The text that represents the display name of the field type. */
    text: string;

    /**
     * The SVG content that is used to provide an iconic representation of this
     * field type.
     */
    svg: string;

    /**
     * Determines if this field type is considered common and should be made
     * readily accessible.
     */
    isCommon: boolean;
};

/**
 * Contains details about a confirmation e-mail for a Form Builder form. This
 * specifies if one should be sent, who receives it and the content it will
 * contain.
 */
export type FormConfirmationEmail = {
    /** Specifies if the confirmation e-mail has been enabled and should be sent. */
    enabled?: boolean;

    /**
     * Specifies which workflow attribute will be used to determine the
     * recipient of the confirmation e-mail.
     */
    recipientAttributeGuid?: string | null;

    /** Determines how the content of the e-mail will be generated. */
    source?: FormEmailSource | null;
};

/**
 * Specifies how an e-mail used by the FormBuilder system will be generated.
 */
export type FormEmailSource = {
    /** The source type that will be used to generate the contents of the e-mail. */
    type?: FormEmailSourceType;

    /**
     * The template unique identifier that should be used to generate the
     * e-mail contents.
     */
    template?: string | null;

    /** The plain text to use for the custom subject of the e-mail. */
    subject?: string | null;

    /** The e-mail address to be used as the reply-to address for the custom e-mail. */
    replyTo?: string | null;

    /** The HTML content to use for the custom e-mail body. */
    body?: string | null;

    /**
     * Determines if the standard organization header and footer should be
     * prepended and appended to the custom body.
     */
    appendOrgHeaderAndFooter?: boolean;
};

/**
 * Contains details about a notification e-mail for a Form Builder form.
 * This specifies if one should be sent, who receives it and the content
 * it will contain.
 */
export type FormNotificationEmail = {
    /** Specifies if the notification e-mail has been enabled and should be sent. */
    enabled?: boolean;

    /**
     * Determines the destination recipient type for this notification e-mail.
     * This also determines which other properties are valid.
     */
    destination?: FormNotificationEmailDestination;

    /**
     * The ListItemBag that identifies the current person that will receive
     * the e-mail.
     */
    recipient?: ListItemBag | null;

    /**
     * Contains the e-mail address that will receive the notification e-mail.
     * Multiple addresses may be separated with a comma.
     */
    emailAddress?: string | null;

    /**
     * Contains the campus topic DefinedValue unique identifier that will
     * determine who receives the e-mail. This is used in conjunction with
     * the campus specified on the workflow to find the specific recipient.
     */
    campusTopicGuid?: Guid | null;

    /** Determines how the content of the e-mail will be generated. */
    source?: FormEmailSource;
};

/**
 * Contains the general settings about this form. These loosely correlate
 * to the UI of the General tab when viewing the form.
 */
export type FormGeneral = {
    /**
     * The name of the form. This is used internally to identify the form
     * and not normally displayed to the user filling out the form.
     */
    name?: string | null;

    /**
     * A description of the purpose this form fills and the reason it exists.
     * This is primarily for internal use by staff.
     */
    description?: string | null;

    /**
     * The unique identifier of the template that is being used by this form
     * to provided a set of overrides.
     */
    template?: Guid | null;

    /** The category that this form belongs to for organization purposes. */
    category?: ListItemBag | null;

    /** The date and time this form will begin to allow entries. */
    entryStarts?: string | null;

    /** The date and time at which point this form will no longer accept new entries. */
    entryEnds?: string | null;

    /**
     * Determines if this form requires the person to be logged in before they
     * can begin filling it out.
     */
    isLoginRequired?: boolean;
};

/**
 * Identifies the action that should be taken after the form has been submitted
 * by the individual.
 */
export type FormCompletionAction = {
    /**
     * The type of action that should be performed after the form has been
     * submitted.
     */
    type?: FormCompletionActionType;

    /**
     * contains the HTML message content that should be displayed to the
     * individual after the form has been submitted.
     */
    message?: string | null;

    /**
     * Contains the URL to redirect the individual to after the form has been
     * submitted.
     */
    redirectUrl?: string | null;
};

/**
 * The settings that describe a single form.
 */
export type FormSettings = {
    /**
     * The HTML content that will be displayed before all the sections of
     * the form.
     */
    headerContent?: string | null;

    /**
     * The HTML content that will be displayed after all the sections of the
     * form.
     */
    footerContent?: string | null;

    /** The list of sections that exist in this form, including all of the fields. */
    sections?: FormSection[] | null;

    /** The general settings about this form. */
    general?: FormGeneral | null;

    /**
     * The settings that describe the confirmation e-mail to be sent when
     * this form is submitted.
     */
    confirmationEmail?: FormConfirmationEmail | null;

    /**
     * The settings that describe the notification e-mail to be sent when
     * this form is submitted.
     */
    notificationEmail?: FormNotificationEmail | null;

    /** The action to perform after this form is submitted. */
    completion?: FormCompletionAction | null;

    /** Determines how the form's campus context will be set when it first runs. */
    campusSetFrom?: CampusSetFrom;

    /**
     * Determines if the person entry section should be displayed at the top
     * of the form.
     */
    allowPersonEntry?: boolean;

    /** The settings that describe how the person entry section will be displayed. */
    personEntry?: FormPersonEntry | null;
};

/**
 * Identifies all the settings related to configuring hte Person Entry
 * section of a FormBuilder form.
 */
export type FormPersonEntry = {
    /**
     * Indicates if the form should auto-fill values from the Person that is
     * currently logged in.
     */
    autofillCurrentPerson?: boolean;

    /**
     * Indicates if the form should be hidden when a Person is already
     * logged in and known.
     */
    hideIfCurrentPersonKnown?: boolean;

    /**
     * The DefinedValue unique identifier that specifies the value used
     * for Person.RecordStatusValue when a new Person is created.
     */
    recordStatus?: Guid | null;

    /**
     * The DefinedValue unique identifier that specifies the value used for
     * Person.ConnectionStatusValue when a new Person is created.
     */
    connectionStatus?: Guid | null;

    /**
     * Indicates if the campus picker should be shown on the person entry form.
     * The campus picker will always be required if it is visible.
     */
    showCampus?: boolean;

    /**
     * The DefinedValue unique identifier for the campus type used to filter
     * Campuses when displaying the campus picker.
     */
    campusType?: Guid | null;

    /**
     * The DefinedValue unique identifier for the campus status used to filter
     * Campuses when displaying the campus picker.
     */
    campusStatus?: Guid | null;

    /**
     * Determines if the gender control should be hidden, optional or required
     * when displaying the person entry form.
     */
    gender?: FormFieldVisibility;

    /**
     * Determines if the e-mail control should be hidden, optional or required
     * when displaying the person entry form.
     */
    email?: FormFieldVisibility;

    /**
     * Determines if the mobile phone control should be hidden, optional or required
     * when displaying the person entry form.
     */
    mobilePhone?: FormFieldVisibility;

    /**
     * Determines if the SmsOptIn control should be hidden, or shown when displaying on the person entry form.
     */
    smsOptIn?: FormFieldShowHide;

    /**
     * Determines if the birthdate control should be hidden, optional or required
     * when displaying the person entry form.
     */
    birthdate?: FormFieldVisibility;

    /**
     * Determines if the address control should be hidden, optional or required
     * when displaying the person entry form.
     */
    address?: FormFieldVisibility;

    /**
     * The DefinedValue unique identifier that specifies which address type
     * will be used on the person entry form.
     */
    addressType?: Guid | null;

    /**
     * Determines if the marital status control should be hidden, optional or required
     * when displaying the person entry form.
     */
    maritalStatus?: FormFieldVisibility;

    /**
     * Determines if the spouse controls should be hidden, optional or required
     * when displaying the person entry form.
     */
    spouseEntry?: FormFieldVisibility;

    /**
     * The text string that is used above the spouse entry controls to indicate
     * that the following controls are for the spouse.
     */
    spouseLabel?: string | null;

    /**
    * Determines if the race picker should be hidden, optional or required
    * when displaying the person entry form.
    */
    raceEntry?: FormFieldVisibility;

    /**
    * Determines if ethnicity picker should be hidden, optional or required
    * when displaying the person entry form.
    */
    ethnicityEntry?: FormFieldVisibility;
};

// #endregion
