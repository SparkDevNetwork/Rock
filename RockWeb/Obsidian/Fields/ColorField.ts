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
import DropDownList, { DropDownListOption } from '../Elements/DropDownList';
import ColorPicker from '../Elements/ColorPicker';
import { Guid } from '../Util/Guid';
import { getConfigurationValue, getFieldTypeProps, registerFieldType } from './Index';

const fieldTypeGuid: Guid = 'D747E6AE-C383-4E22-8846-71518E3DD06F';

enum ColorControlType {
    ColorPicker,
    NamedColor
}

enum ConfigurationValueKey {
    ColorControlType = 'selectiontype',
    ColorPicker = 'Color Picker',
    NamedColor = 'Named Color'
}

export default registerFieldType(fieldTypeGuid, defineComponent({
    name: 'ColorField',
    components: {
        DropDownList,
        ColorPicker
    },
    props: getFieldTypeProps(),
    data() {
        return {
            internalBooleanValue: false,
            internalValue: ''
        };
    },
    computed: {
        colorControlType(): ColorControlType {
            const controlType = getConfigurationValue(ConfigurationValueKey.ColorControlType, this.configurationValues);

            switch (controlType) {
                case ConfigurationValueKey.ColorPicker:
                    return ColorControlType.ColorPicker;

                case ConfigurationValueKey.NamedColor:
                default:
                    return ColorControlType.NamedColor;
            }
        },
        isColorPicker(): boolean {
            return this.colorControlType === ColorControlType.ColorPicker;
        },
        isNamedPicker(): boolean {
            return this.colorControlType === ColorControlType.NamedColor;
        },
        displayValue(): string {
            return this.internalValue;
        },
        dropDownListOptions(): DropDownListOption[] {
            return [
                { key: 'Black', text: 'Black', value: 'Black' },
                { key: 'White', text: 'White', value: 'White' }
            ] as DropDownListOption[];
        }
    },
    watch: {
        internalValue() {
            this.$emit('update:modelValue', this.internalValue);
        },
        modelValue: {
            immediate: true,
            handler() {
                this.internalValue = this.modelValue || '';
            }
        }
    },
    template: `
<DropDownList v-if="isEditMode && isNamedPicker" v-model="internalValue" v-bind="dropDownListOptions" />
<ColorPicker v-else-if="isEditMode && isColorPicker" v-model="internalValue" />
<span v-else>{{ displayValue }}</span>`
}));
