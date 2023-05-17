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

import { Guid } from "@Obsidian/Types";
import { computed, defineComponent, ref } from "vue";
import Block from "@Obsidian/Templates/block";
import Loading from "@Obsidian/Controls/loading";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import { useStore } from "@Obsidian/PageState";
import { useConfigurationValues, useInvokeBlockAction } from "@Obsidian/Utility/block";
import JavaScriptAnchor from "@Obsidian/Controls/javaScriptAnchor";
import RockForm from "@Obsidian/Controls/rockForm";
import TextBox from "@Obsidian/Controls/textBox";
import RockButton from "@Obsidian/Controls/rockButton";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";
import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
import { List } from "@Obsidian/Utility/linq";

const store = useStore();

type ConfigurationValues = {
    blockIconCssClass: string;

    blockTitle: string;

    showCategoryNamesAsSeparators: boolean;

    useAbbreviatedNames: boolean;

    categoryGuids: Guid[];

    attributes: Record<string, PublicAttributeBag>;

    values: Record<string, string>;
};

function sortedAttributeValues(attributeValues: PublicAttributeBag[]): PublicAttributeBag[] {
    return new List(attributeValues)
        .orderBy(v => v.order)
        .thenBy(v => v.name)
        .toArray();
}

export default defineComponent({
    name: "Crm.AttributeValues",
    components: {
        NotificationBox,
        Block,
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
        const attributes = ref(configurationValues.attributes);
        const attributeValues = ref(configurationValues.values);
        const personKey = computed(() => store.personContext?.idKey || null);
        const isLoading = ref(false);
        const isEditMode = ref(false);
        const errorMessage = ref("");

        const goToViewMode = (): void => {
            isEditMode.value = false;
        };

        const goToEditMode = async (): Promise<void> => {
            const result = await invokeBlockAction<PublicAttributeBag[]>("GetAttributeValuesForEdit");
            if (result.isSuccess) {
                //attributeValues.value = sortedAttributeValues(result.data ?? []);
                isEditMode.value = true;
            }
        };

        const doSave = async (): Promise<void> => {
            isLoading.value = true;

            const keyValueMap: Record<string, string | null> = {};

            //for (const a of attributeValues.value) {
            //    keyValueMap[(a as PublicEditableAttributeValue).key] = a.value || "";
            //}

            //const result = await invokeBlockAction<PublicAttributeValue[]>("SaveAttributeValues", {
            //    personKey: personKey.value,
            //    keyValueMap
            //});

            //if (result.isSuccess) {
            //    attributeValues.value = sortedAttributeValues(result.data ?? []);
            //    goToViewMode();
            //}
            //else {
            //    errorMessage.value = "Failed to save values.";
            //}

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
            attributes,
            attributeValues
        };
    },
    template: `
<Block :title="blockTitle">
    <template #headerActions>
        <JavaScriptAnchor title="Order Attributes" class="action btn-link edit">
            <i class="fa fa-bars"></i>
        </JavaScriptAnchor>
        <JavaScriptAnchor title="Edit Attributes" class="action btn-link edit" @click="goToEditMode">
            <i class="fa fa-pencil"></i>
        </JavaScriptAnchor>
    </template>

    <template #default>
        <Loading :isLoading="isLoading">
            <NotificationBox v-if="errorMessage" alertType="warning">{{ errorMessage }}</NotificationBox>
            <AttributeValuesContainer v-if="!isEditMode" :attributeValues="attributeValues" :showEmptyValues="false" :showCategoryLabel="false" />
            <RockForm v-else @submit="doSave">
                <AttributeValuesContainer v-model="attributeValues" :attributes="attributes" isEditMode :showAbbreviatedName="useAbbreviatedNames" :showCategoryLabel="false" />
                <div class="actions">
                    <RockButton btnType="primary" btnSize="xs" type="submit">Save</RockButton>
                    <RockButton btnType="link" btnSize="xs" @click="goToViewMode">Cancel</RockButton>
                </div>
            </RockForm>
        </Loading>
    </template>
</Block>`
});
