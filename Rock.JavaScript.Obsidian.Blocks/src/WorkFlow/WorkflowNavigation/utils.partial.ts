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

import { ref } from "vue";
import { WorkflowNavigationCategoryBag } from "@Obsidian/ViewModels/Blocks/Workflow/WorkflowNavigation/workflowNavigationCategoryBag";
import { CategoryAccordion } from "./types.partial";

/**
 * Provides single-open accordion state for a list of sibling category panels.
 * Each level (the top-level categories and each category's children) calls this
 * separately so it gets its own independent state, matching the WebForms block
 * where expanding a section collapsed only its siblings, not child sections.
 */
export function useCategoryAccordion(): CategoryAccordion {
    const expandedKey = ref<string | null>(null);

    function keyFor(category: WorkflowNavigationCategoryBag, index: number): string {
        return category.idKey ?? String(index);
    }

    function setExpanded(key: string, isExpanded: boolean): void {
        expandedKey.value = isExpanded ? key : null;
    }

    return { expandedKey, keyFor, setExpanded };
}
