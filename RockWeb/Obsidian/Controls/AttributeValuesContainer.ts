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
import { isNullOrWhitespace } from '../Services/String';
import { defineComponent, PropType } from 'vue';
import AttributeValue from '../ViewModels/CodeGenerated/AttributeValueViewModel';
import RockField from './RockField';

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
    <template v-if="a.Value">
        <label class="control-label">
            {{ getAttributeLabel(a) }}
        </label>
        <div class="control-wrapper">
            <div class="form-control-static">
                <RockField :fieldTypeGuid="a.Attribute.FieldTypeGuid" v-model="a.Value" :configurationValues="a.Attribute.QualifierValues" />
            </div>
        </div>
    </template>
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
