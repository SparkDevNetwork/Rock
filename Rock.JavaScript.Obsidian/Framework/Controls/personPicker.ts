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

import { computed, defineComponent, PropType, Ref, ref, watch } from "vue";
import { ListItem } from "../ViewModels";
import RockFormField from "../Elements/rockFormField";
import Panel from "./panel";
import TextBox from "../Elements/textBox";
import { nextTick } from "vue";
import { doApiCall } from "../Util/http";

const enum AgeClassification {
    Unknown = 0,

    Adult = 1,

    Child = 2
}

type PersonSearchResult = {
    guid?: string | null;

    name?: string | null;

    isActive?: boolean;

    isDeceased?: boolean;

    isBusiness?: boolean;

    imageUrl?: string | null;

    age?: number | null;

    formattedAge?: string | null;

    ageClassification?: AgeClassification;

    gender?: string | null;

    connectionStatus?: string | null;

    recordStatus?: string | null;

    email?: string | null;

    spouseName?: string | null;

    spouseNickName?: string | null;

    address?: string | null;

    phoneNumbers?: PersonSearchPhoneNumber[] | null;
};

type PersonSearchPhoneNumber = {
    type?: string | null;

    number?: string | null;

    isUnlisted?: boolean;
};

const sleep = (ms: number): Promise<void> => {
    return new Promise<void>(resolve => {
        setTimeout(resolve, ms);
    });
};

export default defineComponent({
    name: "PersonPicker",

    components: {
        RockFormField,
        Panel,
        TextBox
    },

    props: {
        modelValue: {
            type: Object as PropType<ListItem>
        }
    },

    setup(props, { emit }) {
        const internalValue = ref(props.modelValue);

        /** Determines if the clear button should be shown. */
        const showClear = computed(() => props.modelValue?.value);

        /** True if the popup person picker should be visible. */
        const showPopup = ref(false);

        /** The current text typed in the search box. */
        const searchText = ref("");

        /** A reference to the container element for the search box. */
        const searchTextBox = ref<HTMLElement | null>(null);

        /** The currently displayed search results. */
        const searchResults = ref<PersonSearchResult[]>([]);

        /** The currently selected search result. */
        const selectedSearchResult = ref("");

        /** The cancellation token used to cancel a previous search API call. */
        let searchCancelToken: Ref<boolean> | null = null;

        /** The currently selected name to display in the picker. */
        const selectedName = computed((): string => internalValue.value?.text ?? "");

        /**
         * Updates the search results. This is called as soon as the search text
         * value changes.
         * 
         * @param text The text to be searched for.
         * @param cancellationToken The token that indicates if we should abort our search.
         */
        const updateSearchResults = async (text: string, cancellationToken: Ref<boolean>): Promise<void> => {
            // Only search if we have 3 or more characters.
            if (text.length < 3) {
                return;
            }

            // Wait 200ms to see if the user has continued to type.
            await sleep(200);

            // This is set if the user kept typing.
            if (cancellationToken.value) {
                return;
            }

            const params = {
                name: text,
                includeDetails: true
            };

            // Make the API call to get the search results.
            const result = await doApiCall<PersonSearchResult[]>("POST", "/api/v2/Controls/PersonPicker/Search", undefined, params);

            // Check again if we have been cancelled before we do the update.
            if (cancellationToken.value) {
                return;
            }

            // Update the search results if we didn't get back an error.
            if (result.isSuccess && result.data) {
                searchResults.value = result.data;
                selectedSearchResult.value = "";
            }
            else {
                console.warn(result.errorMessage);
            }
        };

        /**
         * Gets the additional text to display next to the name.
         * 
         * @param result The details of the person.
         */
        const getNameAdditionalText = (result: PersonSearchResult): string => {
            if (result.spouseNickName && result.formattedAge) {
                return `Age: ${result.formattedAge}; Spouse: ${result.spouseNickName}`;
            }
            else if (result.formattedAge) {
                return `Age: ${result.formattedAge}`;
            }
            else if (result.spouseNickName) {
                return `Spouse: ${result.spouseNickName}`;
            }
            else {
                return "";
            }
        };

        /**
         * Gets the style attribute values for the person image tag.
         * 
         * @param result The details of the person.
         */
        const getPersonImageStyle = (result: PersonSearchResult): Record<string, string> => {
            if (result.imageUrl) {
                return {
                    backgroundImage: `url(${result.imageUrl})`,
                    width: "70px",
                    height: "70px",
                    backgroundSize: "cover",
                    marginRight: "8px",
                    border: "1px solid #dfe0e1"
                };
            }
            else {
                return {};
            }
        };

        /**
         * Gets the card container style attribute values.
         * 
         * @param result The details of the person.
         */
        const getCardStyle = (result: PersonSearchResult): Record<string, string> => {
            const styles: Record<string, string> = {
                margin: "0px 20px 20px 0px"
            };

            if (result.guid === selectedSearchResult.value) {
                styles["border"] = "2px solid var(--brand-color)";
            }
            else {
                styles["border"] = "2px solid transparent";
            }

            return styles;
        };

        /**
         * Event handler for when the clear button is clicked by the user.
         */
        const onClear = (): void => {
            emit("update:modelValue", undefined);
        };

        /**
         * Event handler for when the user clicks on the picker. Show/hide the
         * popup.
         */
        const onPickerClick = (): void => {
            showPopup.value = !showPopup.value;

            if (showPopup.value) {
                searchText.value = "";
                selectedSearchResult.value = "";
                searchResults.value = [];

                nextTick(() => {
                    if (searchTextBox.value) {
                        const input = searchTextBox.value.querySelector("input");

                        input?.focus();
                    }
                });
            }
        };

        /**
         * Event handler for when the user clicks the cancel button. Hide the
         * popup.
         */
        const onCancel = (): void => {
            showPopup.value = false;
        };

        /**
         * Event handler for when the user presses a key anywhere inside the
         * popup body. If it is the escape key then close the popup.
         * 
         * @param ev The event details about the key press.
         */
        const onPopupKeyDown = (ev: KeyboardEvent): void => {
            if (ev.keyCode === 27 && showPopup.value) {
                ev.stopImmediatePropagation();
                onCancel();
            }
        };

        /**
         * Event handler for when a card is clicked. If the card is not selected
         * them mark it selected. If it is already selected then close the
         * popup and emit the new selection.
         * 
         * @param result The result object that contains the details about the person.
         */
        const onCardClick = (result: PersonSearchResult): void => {
            if (!result.guid || !result.name) {
                return;
            }

            internalValue.value = {
                value: selectedSearchResult.value,
                text: result.name
            };

            // Emit the new value and close the popup.
            emit("update:modelValue", internalValue.value);
            showPopup.value = false;
        };

        /**
         * Event handler for when a card gains focus. This allows keyboard
         * navigation through the cards.
         * 
         * @param result The result object that contains the details about the person.
         */
        const onCardFocus = (result: PersonSearchResult): void => {
            if (!result.guid || !result.name) {
                return;
            }

            selectedSearchResult.value = result.guid;
        };

        /**
         * Event handler for when a card loses focus. This allows keyboard
         * navigation through the cards.
         * 
         * @param result The result object that contains the details about the person.
         */
        const onCardBlur = (result: PersonSearchResult): void => {
            if (!result.guid || !result.name) {
                return;
            }

            if (selectedSearchResult.value === result.guid) {
                selectedSearchResult.value = "";
            }
        };

        /**
         * Event handler for when a key is pressed while a card has focus. If
         * it is the enter key and the card is selected then emit the new value
         * and close the popup.
         * 
         * @param result The result object that contains the details about the person.
         */
        const onCardKeyPress = (result: PersonSearchResult, ev: KeyboardEvent): void => {
            if (!result.guid || !result.name) {
                return;
            }

            const isEnterKey = ev.keyCode === 10 || ev.keyCode === 13;

            if (selectedSearchResult.value === result.guid && isEnterKey) {
                internalValue.value = {
                    value: selectedSearchResult.value,
                    text: result.name
                };

                // Emit the new value and close the popup.
                emit("update:modelValue", internalValue.value);
                showPopup.value = false;
            }
        };

        // Watch for changes to what the user has typed and update the search
        // results.
        watch(searchText, () => {
            // If a search is in progress, cancel it.
            if (searchCancelToken) {
                searchCancelToken.value = true;
            }

            // Create a new cancellation token that we can use if the user
            // continues to type in the search box.
            searchCancelToken = ref(false);

            updateSearchResults(searchText.value, searchCancelToken);
        });

        // Watch for changes in our provided value and update the UI.
        watch(() => props.modelValue, () => internalValue.value = props.modelValue);

        return {
            getCardStyle,
            getNameAdditionalText,
            getPersonImageStyle,
            internalValue,
            onCardBlur,
            onCardClick,
            onCardFocus,
            onCardKeyPress,
            onClear,
            onPickerClick,
            onCancel,
            onPopupKeyDown,
            searchResults,
            searchText,
            searchTextBox,
            selectedName,
            selectedSearchResult,
            showClear,
            showPopup
        };
    },

    template: `
<RockFormField
    :modelValue="internalValue"
    formGroupClasses="person-picker"
    name="personpicker">
    <template #default="{uniqueId, field}">
        <div class="control-wrapper">
            <div class="picker picker-select person-picker">
                <a class="picker-label" href="#" @click.prevent.stop="onPickerClick">
                    <i class="fa fa-user fa-fw"></i>
                    <span class="selected-name" v-text="selectedName"></span>
                    <i class="fa fa-caret-down pull-right"></i>
                </a>

                <a v-if="showClear" class="picker-select-none" @click.prevent.stop="onClear">
                    <i class="fa fa-times"></i>
                </a>

                <Panel v-if="showPopup" isFullscreen title="Person Search">
                    <template #actionAside>
                        <span class="panel-action" @click.prevent.stop="onCancel">
                            <i class="fa fa-times"></i>
                        </span>
                    </template>

                    <div @keydown="onPopupKeyDown" tabindex="0">
                        <div ref="searchTextBox">
                            <TextBox v-model="searchText" label="Search" />
                        </div>

                        <div style="display: flex;">
                            <div v-for="result in searchResults" :key="result.guid" class="well clickable" :style="getCardStyle(result)" tabindex="0" @click="onCardClick(result)" @focus="onCardFocus(result)" @blur="onCardBlur(result)" @keypress="onCardKeyPress(result, $event)">
                                <div style="display: flex; min-width: 250px;">
                                    <div class="person-image" :style="getPersonImageStyle(result)"></div>
                                    <div>
                                        <div>{{ result.name }}</div>
                                        <div v-if="getNameAdditionalText(result)" class="text-muted"><small>{{ getNameAdditionalText(result) }}</small></div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </Panel>
            </div>
        </div>
    </template>
</RockFormField>
`
});
