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

import { defineComponent } from 'vue';
import { standardBlockSetup } from '../../Controls/RockBlock';
import Alert from '../../Elements/Alert';
import RockButton from '../../Elements/RockButton';
import PaneledBlockTemplate from '../../Templates/PaneledBlockTemplate';

/** An example block */
const StarkDetailOptions = defineComponent( {
    /** This is the name that will appear in the browser debug tools. This is mostly for organization and
     *  doesn't affect function. */
    name: 'Utility.StarkDetailOptions',

    /** This allows for standard block tools, such as invokeBlockAction, to be available to this block */
    setup: standardBlockSetup,

    /** These are the child components that are used by this block component */
    components: {
        PaneledBlockTemplate,
        Alert,
        RockButton
    },

    /** This method returns the zero-state of the component's local state object. All reactive data must
     *  be declared here (even if it only might be used at some point). */
    data()
    {
        return {
            configMessage: '',
            blockActionMessage: ''
        };
    },

    /** These are methods that can be invoked by button clicks or other methods. */
    methods: {

        /** Fetch a message from the C# block action named "GetMessage". */
        async loadBlockActionMessage()
        {
            const response = await this.invokeBlockAction<{ Message: string; }>( 'GetMessage', {
                paramFromClient: 'This is a value sent to the server from the client.'
            } );

            if ( response.data )
            {
                this.blockActionMessage = response.data.Message;
            }
            else
            {
                this.blockActionMessage = response.errorMessage || 'An error occurred';
            }
        }
    },

    /** This method is a lifecycle hook called when the component is created (initializing and not yet in
     *  the DOM) */
    created()
    {
        // Set the local state "configMessage" to the value sent by C#'s GetObsidianConfigurationValues
        this.configMessage = this.configurationValues.Message;
    },

    /** This method is another lifecycle hook called when the component is mounted (put in the DOM) */
    mounted()
    {
        // Do something when the component's elements are now in the DOM
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
            <RockButton btnType="primary" btnSize="sm" @click="loadBlockActionMessage">Invoke Block Action</RockButton>
            <pre>{{ blockActionMessage }}</pre>
        </div>
    </template>
</PaneledBlockTemplate>`
} );

export default StarkDetailOptions;