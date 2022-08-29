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
import NumberBox from "@Obsidian/Controls/numberBox";
import CheckBox from "@Obsidian/Controls/checkBox";
import TextBox from "@Obsidian/Controls/textBox";
import { watchPropertyChanges } from "@Obsidian/Utility/block";
import { propertyRef, updateRefValue } from "@Obsidian/Utility/component";
import { AssessmentTypeBag } from "@Obsidian/ViewModels/Blocks/Crm/AssessmentTypeDetail/assessmentTypeBag";
import { AssessmentTypeDetailOptionsBag } from "@Obsidian/ViewModels/Blocks/Crm/AssessmentTypeDetail/assessmentTypeDetailOptionsBag";

export default defineComponent({
    name: "Crm.AssessmentTypeDetail.EditPanel",

    props: {
        modelValue: {
            type: Object as PropType<AssessmentTypeBag>,
            required: true
        },

        options: {
            type: Object as PropType<AssessmentTypeDetailOptionsBag>,
            required: true
        }
    },

    components: {
        AttributeValuesContainer,
        CheckBox,
        TextBox,
        NumberBox
    },

    emits: {
        "update:modelValue": (_value: AssessmentTypeBag) => true,
        "propertyChanged": (_value: string) => true
    },

    setup(props, { emit }) {
        // #region Values
        const attributes = ref(props.modelValue.attributes ?? {});
        const attributeValues = ref(props.modelValue.attributeValues ?? {});
        const title = propertyRef(props.modelValue.title ?? "", "Title");
        const description = propertyRef(props.modelValue.description ?? "", "Description");
        const isActive = propertyRef(props.modelValue.isActive ?? false, "IsActive");
        const assessmentPath = propertyRef(props.modelValue.assessmentPath ?? "", "AssessmentPath");
        const assessmentResultsPath = propertyRef(props.modelValue.assessmentResultsPath ?? "", "AssessmentResultsPath");
        const minimumDaysToRetake = propertyRef(props.modelValue.minimumDaysToRetake ?? "", "MinimumDaysToRetake");
        const requiresRequest = propertyRef(props.modelValue.requiresRequest ?? false, "RequiresRequest");

        // The properties that are being edited. This should only contain
        // objects returned by propertyRef().
        const propRefs = [title, description, isActive, assessmentPath, assessmentResultsPath, minimumDaysToRetake, requiresRequest];

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
            updateRefValue(title, props.modelValue.title ?? "");
            updateRefValue(description, props.modelValue.description ?? "");
            updateRefValue(isActive, props.modelValue.isActive ?? false);
            updateRefValue(assessmentPath, props.modelValue.assessmentPath ?? "");
            updateRefValue(assessmentResultsPath, props.modelValue.assessmentResultsPath ?? "");
            updateRefValue(minimumDaysToRetake, props.modelValue.minimumDaysToRetake ?? "");
            updateRefValue(requiresRequest, props.modelValue.requiresRequest ?? false);
        });

        // Determines which values we want to track changes on (defined in the
        // array) and then emit a new object defined as newValue.
        watch([...propRefs], () => {
            const newValue: AssessmentTypeBag = {
                ...props.modelValue,
                attributeValues: attributeValues.value,
                title: title.value,
                description: description.value,
                isActive: isActive.value,
                assessmentPath: assessmentPath.value,
                assessmentResultsPath: assessmentResultsPath.value,
                minimumDaysToRetake: minimumDaysToRetake.value,
                requiresRequest: requiresRequest.value
            };

            emit("update:modelValue", newValue);
        });

        watchPropertyChanges(propRefs, emit);

        return {
            attributes,
            attributeValues,
            title,
            description,
            isActive,
            assessmentPath,
            assessmentResultsPath,
            minimumDaysToRetake,
            requiresRequest
        };
    },

    template: `
<fieldset>
    <div class="row">
        <div class="col-md-6">
            <TextBox v-model="title" label="Title" rules="required" />
        </div>
        <div class="col-md-6">
            <CheckBox v-model="isActive" label="Active" />
        </div>
    </div>

    <TextBox v-model="description" label="Description" textMode="multiline" />

    <TextBox v-model="assessmentPath" label="Assessment Path" rules="required" />

    <TextBox v-model="assessmentResultsPath" label="Assessment Results Path Path" rules="required" />

    <div class="row">
        <div class="col-md-6">
            <NumberBox v-model="minimumDaysToRetake" label="Minimum Days To Retake" help="The minimum number of days after the test has been taken before it can be taken again." />
        </div>
        <div class="col-md-6">
            <CheckBox v-model="requiresRequest" label="Requires Request" help="Is a person required to receive a request before this test can be taken?" />
        </div>
    </div>

    <AttributeValuesContainer v-model="attributeValues" :attributes="attributes" isEditMode :numberOfColumns="2" />
</fieldset>
`
});
