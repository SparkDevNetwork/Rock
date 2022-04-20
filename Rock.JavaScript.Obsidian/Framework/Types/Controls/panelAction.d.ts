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

/** The type to use for coloring and styling of the action. */
export type PanelActionType = "default" | "primary" | "success" | "info" | "warning" | "danger" | "link";

/** A function that will be called in response to an action. */
export type PanelActionCallback = (event: Event) => void | Promise<void>;

/** Defines a single action related to a Panel control. */
export type PanelAction = {
    /**
     * The title of the action, this should be a very short (one or two words)
     * description of the action that will be performed, such as "Delete". It
     * must be included even if the action doesn't currently show this value since
     * the display styles may change in the future.
     */
    title: string;

    /**
     * The CSS class for the icon used when displaying this action. It must be
     * included even when the current display style does not include the icon.
     */
    iconCssClass: string;

    /** The type of action for styling. */
    type: PanelActionType;

    /** The callback function that will handle the action. */
    handler?: PanelActionCallback;
};
