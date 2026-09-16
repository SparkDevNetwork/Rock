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

import { Ref } from "vue";
import { WorkflowNavigationCategoryBag } from "@Obsidian/ViewModels/Blocks/Workflow/WorkflowNavigation/workflowNavigationCategoryBag";

export const enum NavigationUrlKey {
    EntryPage = "EntryPage",
    ManagePage = "ManagePage"
}

/** Single-open accordion state for a list of sibling category panels. */
export type CategoryAccordion = {
    /** The key of the sibling whose panel is expanded, or null when all are collapsed. */
    expandedKey: Ref<string | null>;

    /** Gets a stable key for a category, falling back to its position when no IdKey is set. */
    keyFor: (category: WorkflowNavigationCategoryBag, index: number) => string;

    /** Expands the given category and collapses its siblings, or clears the state when it is collapsed. */
    setExpanded: (key: string, isExpanded: boolean) => void;
};
