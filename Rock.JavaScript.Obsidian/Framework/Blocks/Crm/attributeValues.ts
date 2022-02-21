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
import { computed, defineComponent, ref } from "vue";
import PaneledBlockTemplate from "../../Templates/paneledBlockTemplate";
import Loading from "../../Controls/loading";
import Alert from "../../Elements/alert";
import { useStore } from "../../Store/index";
import { Guid } from "../../Util/guid";
import { useConfigurationValues, useInvokeBlockAction } from "../../Util/block";
import JavaScriptAnchor from "../../Elements/javaScriptAnchor";
import RockForm from "../../Controls/rockForm";
import TextBox from "../../Elements/textBox";
import RockButton from "../../Elements/rockButton";
import { PublicAttributeValue, PublicEditableAttributeValue } from "../../ViewModels";
import AttributeValuesContainer from "../../Controls/attributeValuesContainer";
import { List } from "../../Util/linq";

const store = useStore();

type ConfigurationValues = {
    blockIconCssClass: string;

    blockTitle: string;

    showCategoryNamesAsSeparators: boolean;

    useAbbreviatedNames: boolean;

    categoryGuids: Guid[];

    attributes: PublicAttributeValue[];
};

function sortedAttributeValues(attributeValues: PublicAttributeValue[]): PublicAttributeValue[] {
    return new List(attributeValues)
        .orderBy(v => v.order)
        .thenBy(v => v.name)
        .toArray();
}

export default defineComponent({
    name: "Crm.AttributeValues",
    components: {
        PaneledBlockTemplate,
        Alert,
        Loading,
        JavaScriptAnchor,
        RockForm,
        TextBox,
        RockButton,
        AttributeValuesContainer
    },
    setup() {
        const configurationValues = useConfigurationValues<ConfigurationValues>();
        const invokeBlockAction = useInvokeBlockAction();
        const attributeValues = ref(sortedAttributeValues(configurationValues.attributes));
        const personGuid = computed(() => store.personContext?.guid || null);
        const isLoading = ref(false);
        const isEditMode = ref(false);
        const errorMessage = ref("");

        const goToViewMode = (): void => {
            isEditMode.value = false;
        };

        const goToEditMode = async (): Promise<void> => {
            const result = await invokeBlockAction<PublicEditableAttributeValue[]>("GetAttributeValuesForEdit");
            if (result.isSuccess) {
                attributeValues.value = sortedAttributeValues(result.data ?? []);
                isEditMode.value = true;
            }
        };

        const doSave = async (): Promise<void> => {
            isLoading.value = true;

            const keyValueMap: Record<string, string | null> = {};

            for (const a of attributeValues.value) {
                keyValueMap[(a as PublicEditableAttributeValue).key] = a.value || "";
            }

            const result = await invokeBlockAction<PublicAttributeValue[]>("SaveAttributeValues", {
                personGuid: personGuid.value,
                keyValueMap
            });

            if (result.isSuccess) {
                attributeValues.value = sortedAttributeValues(result.data ?? []);
                goToViewMode();
            }
            else {
                errorMessage.value = "Failed to save values.";
            }

            isLoading.value = false;
        };

        return {
            blockTitle: computed(() => configurationValues.blockTitle),
            blockIconCssClass: computed(() => configurationValues.blockIconCssClass),
            isLoading,
            isEditMode,
            errorMessage,
            goToViewMode,
            goToEditMode,
            doSave,
            useAbbreviatedNames: configurationValues.useAbbreviatedNames,
            attributeValues
        };
    },
    template: `
<PaneledBlockTemplate class="panel-persondetails">
    <template v-slot:title>
        <i :class="blockIconCssClass"></i>
        {{ blockTitle }}
    </template>
    <template v-slot:titleAside>
        <div class="actions rollover-item pull-right">
            <JavaScriptAnchor title="Order Attributes" class="btn-link edit">
                <i class="fa fa-bars"></i>
            </JavaScriptAnchor>
            <JavaScriptAnchor title="Edit Attributes" class="btn-link edit" @click="goToEditMode">
                <i class="fa fa-pencil"></i>
            </JavaScriptAnchor>
        </div>
    </template>
    <template v-slot:default>
        <Loading :isLoading="isLoading">
            <Alert v-if="errorMessage" alertType="warning">{{ errorMessage }}</Alert>
            <AttributeValuesContainer v-if="!isEditMode" :attributeValues="attributeValues" :showEmptyValues="false" />
            <RockForm v-else @submit="doSave">
                <AttributeValuesContainer :attributeValues="attributeValues" isEditMode :showAbbreviatedName="useAbbreviatedNames" />
                <div class="actions">
                    <RockButton btnType="primary" btnSize="xs" type="submit">Save</RockButton>
                    <RockButton btnType="link" btnSize="xs" @click="goToViewMode">Cancel</RockButton>
                </div>
            </RockForm>
        </Loading>
    </template>
</PaneledBlockTemplate>`
});
