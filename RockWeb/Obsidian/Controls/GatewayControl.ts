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
        ComponentFromUrl
    },
    props: {
        gatewayControlModel: {
            type: Object as PropType<GatewayControlModel>,
            required: true
        }
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
<ComponentFromUrl :url="url" :settings="settings" @validationRaw="transformValidation" />`
});
