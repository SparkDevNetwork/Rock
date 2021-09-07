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
import { defineComponent, PropType } from 'vue';
import LoadingIndicator from '../Elements/LoadingIndicator';
import TextBox from '../Elements/TextBox';
import { newGuid } from '../Util/Guid';
import { ValidationField } from './GatewayControl';

type Settings = {
};

export default defineComponent( {
    name: 'TestGatewayControl',
    components: {
        LoadingIndicator,
        TextBox
    },
    props: {
        settings: {
            type: Object as PropType<Settings>,
            required: true
        },
        submit: {
            type: Boolean as PropType<boolean>,
            required: true
        }
    },
    data ()
    {
        return {
            loading: false,
            cardNumber: ''
        };
    },
    watch: {
        async submit ()
        {
            if ( !this.submit || this.loading )
            {
                return;
            }

            this.loading = true;

            // Simulate an AJAX call delay
            await new Promise( resolve => setTimeout( resolve, 500 ) );

            // Throw an error for a '0000'
            if ( this.cardNumber === '0000' )
            {
                this.$emit( 'error', 'This is a serious problem with the gateway.' );
                this.loading = false;
                return;
            }

            // Validate the card number is greater than 10 digits
            if ( this.cardNumber.length <= 10 )
            {
                const validationFields: ValidationField[] = [ ValidationField.CardNumber ];
                this.$emit( 'validationRaw', validationFields );
                this.loading = false;
                return;
            }

            const token = newGuid().replace( /-/g, '' );
            this.$emit( 'successRaw', token );
            this.loading = false;
        }
    },
    template: `
<div>
    <div v-if="loading" class="text-center">
        <LoadingIndicator />
    </div>
    <div v-else>
        <TextBox label="Credit Card" v-model="cardNumber" />
    </div>
</div>`
} );
