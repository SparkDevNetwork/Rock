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

import { defineComponent, inject } from 'vue';
import { InvokeBlockActionFunc } from '../../Controls/RockBlock';
import Alert from '../../Elements/Alert';
import RockButton from '../../Elements/RockButton';
import PaneledBlockTemplate from '../../Templates/PaneledBlockTemplate';

/** An example block that uses the Vue Options API */
const StarkDetailOptions = defineComponent( {
    name: 'Utility.StarkDetailOptions',

    /** These are the child components that are used by this block component */
    components: {
        PaneledBlockTemplate,
        Alert,
        RockButton
    },

    /** Even in the Options API, we use the setup method to inject values (logic or values provided by a parent component - in this
     *  case the Rock Block wrapper) to allow TypeScript typing to occur. All blocks are wrapped by RockBlock and have access to
     *  these tools. */
    setup()
    {
        return {
            configurationValues: inject( 'configurationValues' ) as { Message: string; },
            invokeBlockAction: inject( 'invokeBlockAction' ) as InvokeBlockActionFunc
        };
    },

    /** This method returns the zero-state of the component. All reactive data must be declared here. */
    data()
    {
        return {
            configMessage: '',
            blockActionMessage: ''
        };
    },

    /** These are methods that can be invoked by button clicks or other methods. */
    methods: {
        /** Load the message from a C# block action. */
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

    /** This method is a lifecycle hook called when the component is created (before mounting to the DOM) */
    created()
    {
        this.configMessage = this.configurationValues.Message;
    },

    /** The template is the markup of the component */
    template: `
<PaneledBlockTemplate>
    <template #title>
        <i class="fa fa-star"></i>
        Blank Detail Block
    </template>
    <template #titleAside>
        <div class="panel-labels">
            <span class="label label-info">Vue Options API</span>
        </div>
    </template>
    <template #drawer>
        An example block that uses the Vue Options API
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