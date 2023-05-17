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

import { ITopic } from "@Obsidian/Utility/realTime";
import { ActionRenderConfigurationBag } from "@Obsidian/ViewModels/Event/InteractiveExperiences/actionRenderConfigurationBag";
import { PropType } from "vue";
import { IParticipantTopic } from "../types.partial";

type ActionPropsType = {
    /** The identifier of the event. This should be used when sending responses. */
    eventId: {
        type: PropType<string>,
        required: true
    },

    /** The identifier of the action. This should be used when sending responses. */
    actionId: {
        type: PropType<string>,
        required: true
    },

    /** The configuration to use when rendering this action. */
    renderConfiguration: {
        type: PropType<ActionRenderConfigurationBag>,
        required: true
    },

    /** The RealTime topic proxy to use when communicating with the server. */
    realTimeTopic: {
        type: PropType<ITopic<IParticipantTopic>>,
        required: true
    }
};

/** The properties that will be passed to every action component. */
export const actionProps: ActionPropsType = {
    eventId: {
        type: String as PropType<string>,
        required: true
    },

    actionId: {
        type: String as PropType<string>,
        required: true
    },

    renderConfiguration: {
        type: Object as PropType<ActionRenderConfigurationBag>,
        required: true
    },

    realTimeTopic: {
        type: Object as PropType<ITopic<IParticipantTopic>>,
        required: true
    }
};
