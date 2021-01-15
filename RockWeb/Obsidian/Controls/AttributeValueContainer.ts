import { isNullOrWhitespace } from '../Filters/String.js';
import AttributeValue from '../Types/Models/AttributeValue.js';
import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import RockField from './RockField.js';

export default defineComponent({
    name: 'AttributeValueContainer',
    components: {
        RockField
    },
    props: {
        isEditMode: {
            type: Boolean as PropType<boolean>,
            default: false
        },
        attributeValues: {
            type: Array as PropType<AttributeValue[]>,
            required: true
        },
        showEmptyValues: {
            type: Boolean as PropType<boolean>,
            default: true
        },
        showAbbreviatedName: {
            type: Boolean as PropType<boolean>,
            default: false
        }
    },
    methods: {
        getAttributeLabel(attributeValue: AttributeValue) {
            if (this.showAbbreviatedName) {
                return attributeValue.AttributeAbbreviatedName || attributeValue.AttributeName;
            }

            return attributeValue.AttributeName;
        }
    },
    computed: {
        valuesToShow(): AttributeValue[] {
            if (this.showEmptyValues) {
                return this.attributeValues;
            }

            return this.attributeValues.filter(av => !isNullOrWhitespace(av.Value));
        }
    },
    template: `
<div v-if="!isEditMode" v-for="a in valuesToShow" class="form-group static-control">
    <label class="control-label">
        {{ getAttributeLabel(a) }}
    </label>
    <div class="control-wrapper">
        <div class="form-control-static">
            <RockField :fieldTypeGuid="a.AttributeFieldTypeGuid" v-model="a.Value" />
        </div>
    </div>
</div>
<template v-else>
    <template v-for="a in attributeValues">
        <RockField
            isEditMode
            :fieldTypeGuid="a.AttributeFieldTypeGuid"
            v-model="a.Value"
            :label="getAttributeLabel(a)"
            :help="a.AttributeDescription"
            :rules="a.AttributeIsRequired ? 'required' : ''"
            :configurationValues="a.AttributeQualifierValues"  />
    </template>
</template>`
});
