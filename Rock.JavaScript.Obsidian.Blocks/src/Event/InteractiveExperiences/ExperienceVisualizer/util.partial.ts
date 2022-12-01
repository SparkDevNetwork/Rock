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

import { VisualizerRenderConfigurationBag } from "@Obsidian/ViewModels/Event/InteractiveExperiences/visualizerRenderConfigurationBag";
import { ExperienceAnswerBag } from "@Obsidian/ViewModels/Event/InteractiveExperiences/experienceAnswerBag";
import { PropType } from "vue";

type VisualizerPropsType = {
    /** The configuration to use when rendering this visualizer. */
    renderConfiguration: {
        type: PropType<VisualizerRenderConfigurationBag>,
        required: true
    },

    /** The responses that should be displayed. */
    responses: {
        type: PropType<ExperienceAnswerBag[]>,
        required: true
    }
};

/** The properties that will be passed to every visualizer component. */
export const visualizerProps: VisualizerPropsType = {
    renderConfiguration: {
        type: Object as PropType<VisualizerRenderConfigurationBag>,
        required: true
    },

    responses: {
        type: Array as PropType<ExperienceAnswerBag[]>,
        required: true
    }
};
