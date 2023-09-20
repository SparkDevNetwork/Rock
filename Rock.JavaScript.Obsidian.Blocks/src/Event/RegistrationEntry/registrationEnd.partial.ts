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

import { computed, defineComponent, inject, ref, watch } from "vue";
import AttributeValuesContainer from "@Obsidian/Controls/attributeValuesContainer";
import RockForm from "@Obsidian/Controls/rockForm";
import RockButton from "@Obsidian/Controls/rockButton";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";
import { RegistrationEntryState } from "./types.partial";
// LPC CODE
import { useStore } from "@Obsidian/PageState";

const store = useStore();

/** Gets the lang parameter from the query string.
 * Returns "en" or "es". Defaults to "en" if invalid. */
function getLang(): string {
    var lang = typeof store.state.pageParameters["lang"] === 'string' ? store.state.pageParameters["lang"] : "";

    if (lang != "es") {
        lang = "en";
    }

    return lang;
}
// END LPC CODE

export default defineComponent({
    name: "Event.RegistrationEntry.RegistrationEnd",
    components: {
        RockButton,
        AttributeValuesContainer,
        RockForm
    },
    setup(props, { emit }) {
        const registrationEntryState = inject("registrationEntryState") as RegistrationEntryState;

        const attributeValues = ref<Record<string, string>>({});

        for (const a of registrationEntryState.viewModel.registrationAttributesEnd) {
            attributeValues.value[a.key ?? ""] = (registrationEntryState.registrationFieldValues[a.attributeGuid ?? ""] as string) || "";
        }

        const showPrevious = computed((): boolean => {
            return registrationEntryState.firstStep === registrationEntryState.steps.intro;
        });

        const attributes = computed((): Record<string, PublicAttributeBag> => {
            const attrs: Record<string, PublicAttributeBag> = {};

            for (const a of registrationEntryState.viewModel.registrationAttributesEnd) {
                attrs[a.key ?? ""] = a;
            }

            return attrs;
        });

        const onPrevious = (): void => {
            emit("previous");
        };

        const onNext = (): void => {
            emit("next");
        };

        watch(attributeValues, () => {
            for (const a of registrationEntryState.viewModel.registrationAttributesEnd) {
                registrationEntryState.registrationFieldValues[a.attributeGuid ?? ""] = attributeValues.value[a.key ?? ""];
            }
        });

        return {
            attributes,
            attributeValues,
            onNext,
            onPrevious,
            showPrevious
        };
    },
    // LPC CODE
    methods: {
        getLang
    },
    // END LPC CODE

    template: `
<div class="registrationentry-registration-attributes">
    <RockForm @submit="onNext">
        <AttributeValuesContainer v-model="attributeValues" :attributes="attributes" isEditMode :showCategoryLabel="false" />

        <div class="actions row">
            <!-- MODIFIED LPC CODE -->
            <div class="col-xs-6">
                <RockButton v-if="showPrevious" btnType="default" @click="onPrevious">
                    {{ getLang() == 'es' ? 'Anterior' : 'Previous' }}
                </RockButton>
            </div>
            <div class="col-xs-6 text-right">
                <RockButton btnType="primary" type="submit">
                    {{ getLang() == 'es' ? 'Siguiente' : 'Next' }}
                </RockButton>
            </div>
            <!-- END MODIFIED LPC CODE -->
        </div>
    </RockForm>
</div>`
});
