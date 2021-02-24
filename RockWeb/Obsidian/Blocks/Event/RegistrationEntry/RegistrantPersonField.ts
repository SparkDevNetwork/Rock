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

import { defineComponent, markRaw, PropType } from 'vue';
import Alert from '../../../Elements/Alert';
import RegistrationTemplateFormField from '../../../ViewModels/CodeGenerated/RegistrationTemplateFormFieldViewModel';
import { RegistrationPersonFieldType } from '../RegistrationEntry';

export default defineComponent({
    name: 'Event.RegistrationEntry.RegistrantPersonField',
    components: {
        Alert
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
            loading: true,
            value: ''
        };
    },
    watch: {
        field: {
            immediate: true,
            async handler() {
                this.loading = true;
                let componentPath = '';
                const props: Record<string, unknown> = {
                    rules: this.field.IsRequired ? 'required' : ''
                };

                switch (this.field.PersonFieldType) {
                    case RegistrationPersonFieldType.FirstName:
                        componentPath = 'Elements/TextBox';
                        props.label = 'First Name';
                        break;
                    case RegistrationPersonFieldType.LastName:
                        componentPath = 'Elements/TextBox';
                        props.label = 'Last Name';
                        break;
                    case RegistrationPersonFieldType.MiddleName:
                        componentPath = 'Elements/TextBox';
                        props.label = 'Middle Name';
                        break;
                    case RegistrationPersonFieldType.Campus:
                        componentPath = 'Components/CampusPicker';
                        props.label = 'Campus';
                        break;
                    case RegistrationPersonFieldType.Email:
                        componentPath = 'Elements/EmailBox';
                        props.label = 'Email';
                        break;
                    case RegistrationPersonFieldType.Gender:
                        componentPath = 'Elements/GenderDropDownList';
                        break;
                }

                const componentModule = componentPath ? (await import(`../../../${componentPath}`)) : null;
                const component = componentModule ? (componentModule.default || componentModule) : null;

                if (component) {
                    this.fieldControlComponentProps = props;
                    this.fieldControlComponent = markRaw(component);
                }
                else {
                    this.fieldControlComponentProps = {};
                    this.fieldControlComponent = null;
                }

                this.loading = false;
            }
        }
    },
    template: `
<component v-if="fieldControlComponent" :is="fieldControlComponent" v-bind="fieldControlComponentProps" v-model="value" />
<Alert v-else-if="!loading" alertType="danger">Could not resolve person field</Alert>`
});