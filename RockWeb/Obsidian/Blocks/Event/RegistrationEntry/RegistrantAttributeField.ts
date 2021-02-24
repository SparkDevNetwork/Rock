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

import { defineComponent, inject, PropType } from 'vue';
import { ConfigurationValues } from '../../..';
import RockField from '../../../Controls/RockField';
import Alert from '../../../Elements/Alert';
import Attribute from '../../../ViewModels/CodeGenerated/AttributeViewModel';
import RegistrationTemplateFormField from '../../../ViewModels/CodeGenerated/RegistrationTemplateFormFieldViewModel';

export default defineComponent({
    name: 'Event.RegistrationEntry.RegistrantAttributeField',
    components: {
        Alert,
        RockField
    },
    setup() {
        return {
            configurationValues: inject('configurationValues') as ConfigurationValues
        };
    },
    props: {
        field: {
            type: Object as PropType<RegistrationTemplateFormField>,
            required: true
        }
    },
    data() {
        return {
            fieldControlComponent: null as unknown,
            fieldControlComponentProps: {},
            value: '',
            attributes: (this.configurationValues['fieldAttributes'] || []) as Attribute[]
        };
    },
    computed: {
        attribute(): Attribute | null {
            return this.attributes.find(a => a.Id === this.field.AttributeId) || null;
        },
        props(): Record<string, unknown> {
            if (!this.attribute) {
                return {};
            }

            return {
                fieldTypeGuid: this.attribute.FieldTypeGuid,
                isEditMode: true,
                label: this.attribute.Name,
                help: this.attribute.Description,
                rules: this.field.IsRequired ? 'required' : ''
            };
        }
    },
    template: `
<RockField v-if="attribute" v-model="value" v-bind="props" />
<Alert v-else alertType="danger">Could not resolve attribute field</Alert>`
});