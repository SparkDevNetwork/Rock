import { isNullOrWhitespace } from '../Filters/String.js';
import { defineComponent, PropType } from '../Vendor/Vue/vue.js';
import AttributeValue from '../ViewModels/CodeGenerated/AttributeValueViewModel.js';
import RockField from './RockField.js';

export default defineComponent({
    name: 'AttributeValuesContainer',
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
            if (this.showAbbreviatedName && attributeValue.Attribute?.AbbreviatedName) {
                return attributeValue.Attribute.AbbreviatedName;
            }

            return attributeValue.Attribute?.Name || '';
        }
    },
    computed: {
        validAttributeValues(): AttributeValue[] {
            return this.attributeValues.filter(av => av.Attribute);
        },
        valuesToShow(): AttributeValue[] {
            if (this.showEmptyValues) {
                return this.validAttributeValues;
            }

            return this.validAttributeValues.filter(av => !isNullOrWhitespace(av.Value));
        }
    },
    template: `
<div v-if="!isEditMode" v-for="a in valuesToShow" class="form-group static-control">
    <label class="control-label">
        {{ getAttributeLabel(a) }}
    </label>
    <div class="control-wrapper">
        <div class="form-control-static">
            <RockField :fieldTypeGuid="a.Attribute.FieldTypeGuid" v-model="a.Value" />
        </div>
    </div>
</div>
<template v-else>
    <template v-for="a in validAttributeValues">
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
