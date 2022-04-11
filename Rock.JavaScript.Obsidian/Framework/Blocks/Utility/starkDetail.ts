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

import { useConfigurationValues, useInvokeBlockAction } from "../../Util/block";
import { defineComponent, ref } from "vue";
import Alert from "../../Elements/alert";
import RockButton from "../../Elements/rockButton";
import PaneledBlockTemplate from "../../Templates/paneledBlockTemplate";

/**
 * Stark Detail
 * Domain: Utility
 *
 * This is an example block that provides developers with a good starting point in
 * creating blocks.
 *
 */
export default defineComponent({
    /**
     * This is the name that will appear in the browser debug tools. This is mostly for organization and
     * doesn't affect function.
     */
    name: "Utility.StarkDetailOptions",

    /** These are the child components that are used by this block component */
    components: {
        PaneledBlockTemplate,
        Alert,
        RockButton
    },

    /** This allows for standard block tools, such as invokeBlockAction, to be available to this block */
    setup() {
        // #region Variables

        const invokeBlockAction = useInvokeBlockAction();
        const configurationValues = useConfigurationValues<Record<string, unknown>>();

        // Set the state "configMessage" to the value sent by C#'s GetObsidianConfigurationValues
        const configMessage = ref((configurationValues.message as string) ?? "");

        // Set the initial state of the block action message to an empty string.
        const blockActionMessage = ref("");

        // #endregion

        // #region Computed Values

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        /**
         * Event handler for the Invoke Block Action button. Fetches a message from the C# block
         * action named "GetMessage".
         */
        const invokeBlockActionClick = async (): Promise<void> => {
            const response = await invokeBlockAction<{ message: string; }>("GetMessage", {
                paramFromClient: "This is a value sent to the server from the client."
            });

            if (response.data) {
                blockActionMessage.value = response.data.message;
            }
            else {
                blockActionMessage.value = response.errorMessage || "An error occurred";
            }
        };

        // #endregion

        /**
         * This returns the variables and functions that make up the component's local state object.
         * It is available during the lifetime of the component.
         */
        return {
            blockActionMessage,
            configMessage,
            invokeBlockActionClick
        };
    },

    /** The template is the markup of the component. Any custom components used within this template,
     *  like <Alert> and <PaneledBlockTemplate> must be included in the "components" option above. */
    template: `
<PaneledBlockTemplate>
    <template #title>
        <i class="fa fa-star"></i>
        Blank Detail Block
    </template>

    <template #titleAside>
        <div class="panel-labels">
            <span class="label label-info">Vue</span>
        </div>
    </template>

    <template #drawer>
        An example block that uses Vue
    </template>

    <template #default>
        <Alert alertType="info">
            <h4>Stark Template Block</h4>
            <p>This block serves as a starting point for creating new blocks. After copy/pasting it and renaming the resulting file be sure to make the following changes:</p>

            <strong>Changes to the Codebehind (.cs) File</strong>
            <ul>
                <li>Update the namespace to match your directory</li>
                <li>Update the class name</li>
                <li>Fill in the DisplayName, Category and Description attributes</li>
            </ul>

            <strong>Changes to the Vue component (.ts/.js) File</strong>
            <ul>
                <li>Remove this text... unless you really like it...</li>
            </ul>
        </Alert>

        <div>
            <h4>Value from Configuration</h4>
            <p>
                This value came from the C# file and was provided to the JavaScript before the Vue component was even mounted:
            </p>

            <pre>{{ configMessage }}</pre>

            <h4>Value from Block Action</h4>
            <p>
                This value will come from the C# file using a "Block Action". Block Actions allow the Vue Component to communicate with the
                C# code behind (much like a Web Forms Postback):
            </p>

            <pre>{{ blockActionMessage }}</pre>

            <div class="actions">
                <RockButton btnType="primary" btnSize="sm" @click="invokeBlockActionClick">Invoke Block Action</RockButton>
            </div>
        </div>
    </template>
</PaneledBlockTemplate>`
});
