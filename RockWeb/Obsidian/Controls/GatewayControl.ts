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
import JavaScriptAnchor from '../Elements/JavaScriptAnchor';
import ComponentFromUrl from './ComponentFromUrl';

export interface GatewayControlModel {
    FileUrl: string;
    Settings: Record<string, unknown>;
}

export enum ValidationField {
    CardNumber,
    Expiry,
    SecurityCode
}

export default defineComponent({
    name: 'GatewayControl',
    components: {
        ComponentFromUrl,
        JavaScriptAnchor
    },
    props: {
        gatewayControlModel: {
            type: Object as PropType<GatewayControlModel>,
            required: true
        }
    },
    data() {
        return {
            isSuccess: false
        };
    },
    computed: {
        url(): string {
            return this.gatewayControlModel.FileUrl;
        },
        settings(): Record<string, unknown> {
            return this.gatewayControlModel.Settings;
        }
    },
    methods: {
        /** Reset the component */
        reset ()
        {
            // Remove the component from the DOM
            this.isSuccess = true;

            // Add the component back to the DOM on the next DOM update cycle
            this.$nextTick( () =>
            {
                this.isSuccess = false;
                this.$emit( 'reset' );
            } );
        },

        /**
         * Intercept the success event, so that local state can reflect it.
         * @param token
         */
        async onSuccess ( token: string )
        {
            this.isSuccess = true;
            this.$emit( 'success', token );
        },

        /**
         * This method transforms the enum values into human friendly validation messages.
         * @param validationFields
         */
        transformValidation(validationFields: ValidationField[]) {
            const errors = {} as Record<string, string>;
            let foundError = false;

            if (validationFields?.includes(ValidationField.CardNumber)) {
                errors['Card Number'] = 'is not valid.';
                foundError = true;
            }

            if (validationFields?.includes(ValidationField.Expiry)) {
                errors['Expiration Date'] = 'is not valid.';
                foundError = true;
            }

            if (validationFields?.includes(ValidationField.SecurityCode)) {
                errors['Security Code'] = 'is not valid.';
                foundError = true;
            }

            if (!foundError) {
                errors['Payment Info'] = 'is not valid.';
            }

            this.$emit('validation', errors);
            return;
        }
    },
    template: `
<ComponentFromUrl v-if="!isSuccess" :url="url" :settings="settings" @validationRaw="transformValidation" @successRaw="onSuccess" />
<div v-else class="text-center">
    Your payment is ready.
    <small>
        <JavaScriptAnchor @click="reset">
            Reset Payment
        </JavaScriptAnchor>
    </small>
</div>`
});
