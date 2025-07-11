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
import { FormCompletionAction, FormConfirmationEmail, FormPersonEntry } from "../Shared/types.partial";

/**
 * Represents the sources of truth for various pickers and lists of entities
 * that will be used by the JavaScript code.
 */
export type ValueSources = {
    /** The list of campus type options that are available to pick from. */
    campusTypeOptions?: ListItemBag[] | null;

    /** The list of campus status options that are available to pick from. */
    campusStatusOptions?: ListItemBag[] | null;

    /** The list of record status options that are available to pick from. */
    recordStatusOptions?: ListItemBag[] | null;

    /** The list of record source options that are available to pick from. */
    recordSourceOptions?: ListItemBag[] | null;

    /** The list of connection status options that are available to pick from. */
    connectionStatusOptions?: ListItemBag[] | null;

    /** The list of address type options that are available to pick from. */
    addressTypeOptions?: ListItemBag[] | null;

    /** The list of e-mail template options that are available to pick from. */
    emailTemplateOptions?: ListItemBag[] | null;
};

/**
 * Representation of the form template that provides the required information
 * to display the read-only view on the template detail block.
 */
export type TemplateDetail = {
    /** The name of the form template. */
    name?: string | null;

    /** The descriptive purpose for the form template. */
    description?: string | null;

    /** The list of workflow types that use this form template. */
    usedBy?: ListItemBag[] | null;

    /**
     * True if this form template is active and should show up as a selection
     * item in lists; otherwise false.
     */
    isActive?: boolean;
};

/**
 * Representation of the form template that provides the required information
 * to make edits to an existing form template or to create a new one.
 */
export type TemplateEditDetail = {
    /** The name of the form template. */
    name?: string | null;

    /** The descriptive purpose for the form template. */
    description?: string | null;

    /**
     * True if this form template is active and should show up as a selection
     * item in lists; otherwise false.
     */
    isActive?: boolean;

    /**
     * True if the individual mus tbe logged in before they can fill out the
     * form; false if the setting on the form should be used instead.
     */
    isLoginRequired?: boolean;

    /** The HTML content that will be displayed before the form. */
    formHeader?: string | null;

    /** The HTML content that will be displayed after the form. */
    formFooter?: string | null;

    /** True if all forms using this template will have a person entry; otherwise false. */
    allowPersonEntry?: boolean;

    /** The configuration options for the person entry. */
    personEntry?: FormPersonEntry | null;

    /** The configuration options for sending a confirmation e-mail. */
    confirmationEmail?: FormConfirmationEmail | null;

    /** The configuration options for the completion action. */
    completionAction?: FormCompletionAction | null;
};


/**
 * The primary view model that contains all the runtime information needed
 * by the form template detail block.
 */
export type FormTemplateDetailConfiguration = {
    /** The source of information for various pickers and controls. */
    sources?: ValueSources | null;

    /** The URL of the parent page. */
    parentUrl?: string | null;

    /** True if the template can be edited. */
    isEditable?: boolean;

    /** The unique identifier of the template being edited. */
    templateGuid?: Guid | null;

    /** The details about the template that is to be viewed. */
    template?: TemplateDetail | null;
};

