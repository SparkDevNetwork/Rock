import { defineComponent } from '../Vendor/Vue/vue.js';
import { Guid } from '../Util/Guid.js';
import { registerFieldType, getFieldTypeProps } from './Index.js';
import TextBox from '../Elements/TextBox.js';
import { asBooleanOrNull } from '../Filters/Boolean.js';

const fieldTypeGuid: Guid = '9C204CD0-1233-41C5-818A-C5DA439445AA';

enum ConfigurationValueKey {
    IsPassword = 'ispassword',
    MaxCharacters = 'maxcharacters',
    ShowCountDown = 'showcountdown'
}

export default registerFieldType(fieldTypeGuid, defineComponent({
    name: 'TextField',
    components: {
        TextBox
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
        configAttributes(): Record<string, number | boolean> {
            const attributes: Record<string, number | boolean> = {};

            const maxCharsConfig = this.configurationValues[ConfigurationValueKey.MaxCharacters];
            if (maxCharsConfig && maxCharsConfig.Value) {
                const maxCharsValue = Number(maxCharsConfig.Value);

                if (maxCharsValue) {
                    attributes.maxLength = maxCharsValue;
                }
            }

            const showCountDownConfig = this.configurationValues[ConfigurationValueKey.ShowCountDown];
            if (showCountDownConfig && showCountDownConfig.Value) {
                const showCountDownValue = asBooleanOrNull(showCountDownConfig.Value) || false;

                if (showCountDownValue) {
                    attributes.showCountDown = showCountDownValue;
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
<TextBox v-if="edit" v-model="internalValue" v-bind="configAttributes" />
<span v-else>{{ safeValue }}</span>`
}));
