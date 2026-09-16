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

export const enum NavigationUrlKey {
    ParentPage = "ParentPage",
    RegistrationInstancePage = "RegistrationInstancePage",
    RegistrationTemplatePage = "RegistrationTemplatePage"
}

/** A single step in the registrant detail wizard breadcrumb. */
export type RegistrantWizardItem = {
    /** The icon CSS class to display in the wizard step icon area. */
    iconCssClass: string;

    /** The display label for this wizard step. */
    label: string;

    /**
     * The navigation URL for this step. When present on a non-active item
     * the step renders as a clickable anchor. Omit or set null for the
     * active (current) step.
     */
    url?: string | null;
};
