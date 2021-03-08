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
import { Guid } from '../Util/Guid.js';
import { registerFieldType, getFieldTypeProps } from './Index.js';
import DefinedValuePicker from '../Controls/DefinedValuePicker.js';
import { toNumberOrNull } from '../Services/Number.js';
import DefinedType from '../ViewModels/CodeGenerated/DefinedTypeViewModel.js';

const fieldTypeGuid: Guid = '59D5A94C-94A0-4630-B80A-BB25697D74C7';

enum ConfigurationValueKey {
    DefinedType = 'definedtype'
}

export default registerFieldType(fieldTypeGuid, defineComponent({
    name: 'DefinedValueField',
    components: {
        DefinedValuePicker
    },
    props: getFieldTypeProps(),
    data() {
        return {
            internalValue: this.modelValue
        };
    },
    computed: {
        safeValue(): string {
            return (this.modelValue || '').trim();
        },
        configAttributes(): Record<string, unknown> {
            const attributes: Record<string, unknown> = {};

            const definedTypeConfig = this.configurationValues[ConfigurationValueKey.DefinedType];
            if (definedTypeConfig && definedTypeConfig.Value) {
                const definedTypeId = toNumberOrNull(definedTypeConfig.Value);

                if (definedTypeId) {
                    const definedType = this.$store.getters['definedTypes/getById'](definedTypeId) as DefinedType | null;
                    attributes.definedTypeGuid = definedType?.Guid || '';
                }
            }

            return attributes;
        }
    },
    watch: {
        internalValue() {
            this.$emit('update:modelValue', this.internalValue);
        }
    },
    template: `
<DefinedValuePicker v-if="isEditMode" v-model="internalValue" v-bind="configAttributes" />
<span v-else>{{ safeValue }}</span>`
}));
