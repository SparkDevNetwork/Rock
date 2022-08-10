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

import { defineComponent, PropType, ref, watch } from "vue";
import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
import TextBox from "@Obsidian/Controls/textBox";
import { watchPropertyChanges } from "@Obsidian/Utility/block";
import { propertyRef, updateRefValue } from "@Obsidian/Utility/component";
import { PersonalLinkSectionBag } from "@Obsidian/ViewModels/Blocks/Cms/PersonalLinkSectionDetail/personalLinkSectionBag";
import { PersonalLinkSectionDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Cms/PersonalLinkSectionDetail/personalLinkSectionDetailOptionsBag";

export default defineComponent({
    name: "Cms.PersonalLinkSectionDetail.EditPanel",

    props: {
        modelValue: {
            type: Object as PropType<PersonalLinkSectionBag>,
            required: true
        },

        options: {
            type: Object as PropType<PersonalLinkSectionDetailOptionsBag>,
            required: true
        }
    },

    components: {
        AttributeValuesContainer,
        TextBox
    },

    emits: {
        "update:modelValue": (_value: PersonalLinkSectionBag) => true,
        "propertyChanged": (_value: string) => true
    },

    setup(props, { emit }) {
        // #region Values

        const attributes = ref(props.modelValue.attributes ?? {});
        const attributeValues = ref(props.modelValue.attributeValues ?? {});
        const name = propertyRef(props.modelValue.name ?? "", "Name");

        // The properties that are being edited. This should only contain
        // objects returned by propertyRef().
        const propRefs = [name];

        // #endregion

        // #region Computed Values

        // #endregion

        // #region Functions

        // #endregion

        // #region Event Handlers

        // #endregion

        // Watch for parental changes in our model value and update all our values.
        watch(() => props.modelValue, () => {
            updateRefValue(attributes, props.modelValue.attributes ?? {});
            updateRefValue(attributeValues, props.modelValue.attributeValues ?? {});
            updateRefValue(name, props.modelValue.name ?? "");
        });

        // Determines which values we want to track changes on (defined in the
        // array) and then emit a new object defined as newValue.
        watch([attributeValues, ...propRefs], () => {
            const newValue: PersonalLinkSectionBag = {
                ...props.modelValue,
                attributeValues: attributeValues.value,
                name: name.value
            };

            emit("update:modelValue", newValue);
        });

        // Watch for any changes to props that represent properties and then
        // automatically emit which property changed.
        watchPropertyChanges(propRefs, emit);

        return {
            attributes,
            attributeValues,
            name
        };
    },

    template: `
<fieldset>
    <div class="row">
        <div class="col-md-6">
            <TextBox v-model="name"
                label="Name"
                rules="required" />
        </div>

    </div>

    <AttributeValuesContainer v-model="attributeValues" :attributes="attributes" isEditMode :numberOfColumns="2" />
</fieldset>
`
});
