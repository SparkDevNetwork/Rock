import { isNullOrWhitespace } from '../Filters/String.js';
import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import Attribute from '../ViewModels/CodeGenerated/AttributeViewModel.js';
import RockField from './RockField.js';

export type AttributeWithValue = {
    Attribute: Attribute,
    Value: string
};

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
        attributeWithValues: {
            type: Array as PropType<AttributeWithValue[]>,
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
        getAttributeLabel(attributeWithValue: AttributeWithValue) {
            if (this.showAbbreviatedName) {
                return attributeWithValue.Attribute.AbbreviatedName || attributeWithValue.Attribute.Name;
            }

            return attributeWithValue.Attribute.Name;
        }
    },
    computed: {
        valuesToShow(): AttributeWithValue[] {
            if (this.showEmptyValues) {
                return this.attributeWithValues;
            }

            return this.attributeWithValues.filter(av => !isNullOrWhitespace(av.Value));
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
            :fieldTypeGuid="a.Attribute.FieldTypeGuid"
            v-model="a.Value"
            :label="getAttributeLabel(a)"
            :help="a.Attribute.Description"
            :rules="a.Attribute.IsRequired ? 'required' : ''"
            :configurationValues="a.Attribute.QualifierValues"  />
    </template>
</template>`
});
