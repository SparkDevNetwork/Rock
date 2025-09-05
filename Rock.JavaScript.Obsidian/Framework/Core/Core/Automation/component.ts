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

import { StandardDynamicComponentProps, standardDynamicComponentProps } from "@Obsidian/Utility/component";
import { AutomationValueDefinitionBag } from "@Obsidian/ViewModels/Core/Automation/automationValueDefinitionBag";
import { PropType } from "vue";

type AutomationEventComponentProps = StandardDynamicComponentProps & {
    mergeFields: {
        type: PropType<AutomationValueDefinitionBag[]>,
        required: true
    }
};

/**
 * The standard props that are available to an instantiated component by the
 * dynamicComponent.obs component.
 */
export const automationEventComponentProps: AutomationEventComponentProps = {
    ...standardDynamicComponentProps,

    mergeFields: {
        type: Array as PropType<AutomationValueDefinitionBag[]>,
        required: true
    }
};
