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
import { computed, defineComponent, inject, PropType, ref } from "vue";
import DropDownList from "@Obsidian/Controls/dropDownList";
import ElectronicSignature from "@Obsidian/Controls/electronicSignature";
import RadioButtonList from "@Obsidian/Controls/radioButtonList";
import { getRegistrantBasicInfo } from "./utils.partial";
import StringFilter from "@Obsidian/Utility/stringUtils";
import RockButton from "@Obsidian/Controls/rockButton";
import RegistrantPersonField from "./registrantPersonField.partial";
import RegistrantAttributeField from "./registrantAttributeField.partial";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import { RegistrantInfo, RegistrantsSameFamily, RegistrationEntryBlockFamilyMemberViewModel, RegistrationEntryBlockFormFieldViewModel, RegistrationEntryBlockFormViewModel, RegistrationEntryBlockViewModel, RegistrationFieldSource, RegistrationEntryState, RegistrationEntryBlockArgs } from "./types.partial";
import { areEqual, newGuid } from "@Obsidian/Utility/guid";
import RockForm from "@Obsidian/Controls/rockForm";
import FeeField from "./feeField.partial";
import ItemsWithPreAndPostHtml, { ItemWithPreAndPostHtml } from "@Obsidian/Controls/itemsWithPreAndPostHtml";
import { useStore } from "@Obsidian/PageState";
import { PersonBag } from "@Obsidian/ViewModels/Entities/personBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { useInvokeBlockAction } from "@Obsidian/Utility/block";
import { ElectronicSignatureValue } from "@Obsidian/ViewModels/Controls/electronicSignatureValue";
// LPC CODE
import { FilterExpressionType } from "@Obsidian/Core/Reporting/filterExpressionType";
import { RegistrationEntryBlockFormFieldRuleViewModel, RegistrationPersonFieldType } from "./types.partial";
import { getFieldType } from "@Obsidian/Utility/fieldTypes";
import { getDefaultAddressControlModel } from "@Obsidian/Utility/address";
import { getDefaultDatePartsPickerModel } from "@Obsidian/Controls/datePartsPicker";
// END LPC CODE

const store = useStore();
// LPC CODE
/** Gets the lang parameter from the query string.
 * Returns "en" or "es". Defaults to "en" if invalid. */
function getLang(): string {
    var lang = typeof store.state.pageParameters["lang"] === 'string' ? store.state.pageParameters["lang"] : "";

    if (lang != "es") {
        lang = "en";
    }

    return lang;
}

function isRuleMet(rule: RegistrationEntryBlockFormFieldRuleViewModel, fieldValues: Record<Guid, unknown>, formFields: RegistrationEntryBlockFormFieldViewModel[]): boolean {
    const value = fieldValues[rule.comparedToRegistrationTemplateFormFieldGuid] || "";

    if (typeof value !== "string") {
        return false;
    }

    const comparedToFormField = formFields.find(ff => areEqual(ff.guid, rule.comparedToRegistrationTemplateFormFieldGuid));
    if (!comparedToFormField?.attribute?.fieldTypeGuid) {
        return false;
    }

    const fieldType = getFieldType(comparedToFormField.attribute.fieldTypeGuid);

    if (!fieldType) {
        return false;
    }

    return fieldType.doesValueMatchFilter(value, rule.comparisonValue, comparedToFormField.attribute.configurationValues ?? {});
}
// END LPC CODE

export default defineComponent({
    name: "Event.RegistrationEntry.Registrant",
    components: {
        ElectronicSignature,
        RadioButtonList,
        RockButton,
        RegistrantPersonField,
        RegistrantAttributeField,
        NotificationBox,
        RockForm,
        FeeField,
        DropDownList,
        ItemsWithPreAndPostHtml
    },
    props: {
        currentRegistrant: {
            type: Object as PropType<RegistrantInfo>,
            required: true
        },
        isWaitList: {
            type: Boolean as PropType<boolean>,
            required: true
        }
    },
    setup() {
        const invokeBlockAction = useInvokeBlockAction();
        const registrationEntryState = inject("registrationEntryState") as RegistrationEntryState;
        const getRegistrationEntryBlockArgs = inject("getRegistrationEntryBlockArgs") as () => RegistrationEntryBlockArgs;

        const signatureData = ref<ElectronicSignatureValue | null>(null);
        const signatureSource = ref("");
        const signatureToken = ref("");
        const formResetKey = ref("");

        const isNextDisabled = ref(false);

        const isSignatureDrawn = computed((): boolean => registrationEntryState.viewModel.isSignatureDrawn);

        return {
            formResetKey,
            getRegistrationEntryBlockArgs,
            invokeBlockAction,
            isNextDisabled,
            isSignatureDrawn,
            registrationEntryState,
            signatureData,
            signatureSource,
            signatureToken
        };
    },
    data() {
        return {
            fieldSources: {
                personField: RegistrationFieldSource.PersonField,
                personAttribute: RegistrationFieldSource.PersonAttribute,
                groupMemberAttribute: RegistrationFieldSource.GroupMemberAttribute,
                registrantAttribute: RegistrationFieldSource.RegistrantAttribute
            }
        };
    },
    computed: {
        showPrevious(): boolean {
            return this.registrationEntryState.firstStep !== this.registrationEntryState.steps.perRegistrantForms;
        },
        viewModel(): RegistrationEntryBlockViewModel {
            return this.registrationEntryState.viewModel;
        },
        currentFormIndex(): number {
            return this.registrationEntryState.currentRegistrantFormIndex;
        },
        currentForm(): RegistrationEntryBlockFormViewModel | null {
            return this.formsToShow[this.currentFormIndex] || null;
        },
        isLastForm(): boolean {
            return (this.currentFormIndex + 1) === this.formsToShow.length;
        },
        isDataForm(): boolean {
            return this.currentFormIndex < this.formsToShow.length;
        },
        isSignatureForm(): boolean {
            return this.viewModel.isInlineSignatureRequired && this.currentFormIndex === this.formsToShow.length;
        },

        isNextVisible(): boolean {
            return !this.isSignatureForm;
        },

        /** The filtered list of forms that will be shown */
        formsToShow(): RegistrationEntryBlockFormViewModel[] {
            if (!this.isWaitList) {
                return this.viewModel.registrantForms;
            }

            return this.viewModel.registrantForms.filter(form => form.fields.some(field => field.showOnWaitList));
        },

        /** The filtered fields to show on the current form */
        // MODIFIED LPC CODE
        currentFormFields(): RegistrationEntryBlockFormFieldViewModel[] {
            var formFields = (this.currentForm?.fields || []).filter(f => !this.isWaitList || f.showOnWaitList);
            // END MODIFIED LPC CODE
            // LPC CODE
            if (getLang() == 'es') {
                // Get Translations
                var optionTranslations = new Map();
                var allFields = this.currentForm?.fields || [];

                // Search all fields' Pre-HTML for elements with the class "SpanishOption"
                // For each element found, add its "option" attribute and its text content to the optionTranslations dictionary
                for (let i = 0; i < allFields.length; i++) {
                    var el = document.createElement('div');
                    if (allFields[i] != null && allFields[i].preHtml != null) {
                        el.innerHTML = allFields[i].preHtml;
                        let options = el.getElementsByClassName("SpanishOption");
                        for (var j = 0; j < options.length; j++) {
                            optionTranslations.set(options[j].getAttribute("option"), options[j].textContent);
                        }
                    }
                }

                // Translate Options
                for (var x = 0; x < formFields.length; x++) {
                    if (formFields[x] != null && formFields[x].attribute != null && formFields[x].attribute!.configurationValues != null) {
                        var configValues = formFields[x].attribute!.configurationValues ?? {};
                        var values = configValues["values"];
                        var trueText = configValues["truetext"];
                        var falseText = configValues["falsetext"];
                        var output = "";

                        // Handle Booleans
                        if (trueText != null && trueText != "") {
                            if (optionTranslations.has(trueText)) {
                                formFields[x].attribute!.configurationValues!["truetext"] = optionTranslations.get(trueText);
                            }
                        }
                        if (falseText != null && falseText != "") {
                            if (optionTranslations.has(falseText)) {
                                formFields[x].attribute!.configurationValues!["falsetext"] = optionTranslations.get(falseText);
                            }
                        }

                        // Expected values examples:
                        // [{"value":"1","text":"One"},{"value":"2","text":"Two"},{"value":"3","text":"Three"}]
                        // [{"value":"1","text":"One","description":"The first value"},{"value":"2","text":"Two","description":"The second value"},{"value":"3","text":"Three","description":"The third value"}]
                        if (values != null && values != "" && values.includes("value") && values.includes("text")) {
                            let valuesObjects = JSON.parse(values);

                            for (var y = 0; y < valuesObjects.length; y++) {
                                if (valuesObjects[y].hasOwnProperty('text')) {
                                    let originalText = valuesObjects[y].text;

                                    if (optionTranslations.has(originalText)) {
                                        valuesObjects[y].text = optionTranslations.get(originalText);

                                        if (valuesObjects[y].hasOwnProperty('description')) {
                                            valuesObjects[y].description = optionTranslations.get(originalText);
                                        }
                                    }
                                }
                            }

                            // Replace values property with the results
                            formFields[x].attribute!.configurationValues!["values"] = JSON.stringify(valuesObjects);
                        }
                    }
                }
            }

            return formFields;
        },

        /** The filtered fields to show on the current form augmented to remove pre/post HTML from non-visible fields */
        currentFormFieldsAugmented (): RegistrationEntryBlockFormFieldViewModel[] {
            var fields = JSON.parse(JSON.stringify(this.currentFormFields));

            fields.forEach((value, index) => {
                if (value.fieldSource != this.fieldSources.personField) {
                    let isVisible = true;
                    switch (value.visibilityRuleType) {
                        case FilterExpressionType.GroupAll:
                            isVisible = value.visibilityRules.every(vr => isRuleMet(vr, this.currentRegistrant.fieldValues, fields));
                            break;

                        case FilterExpressionType.GroupAllFalse:
                            isVisible = value.visibilityRules.every(vr => !isRuleMet(vr, this.currentRegistrant.fieldValues, fields));
                            break;

                        case FilterExpressionType.GroupAny:
                            isVisible = value.visibilityRules.some(vr => isRuleMet(vr, this.currentRegistrant.fieldValues, fields));
                            break;

                        case FilterExpressionType.GroupAnyFalse:
                            isVisible = value.visibilityRules.some(vr => !isRuleMet(vr, this.currentRegistrant.fieldValues, fields));
                            break;

                        default:
                            isVisible = true;
                            break;
                    }

                    if (isVisible == false) {
                        value.preHtml = "";
                        value.postHtml = "";
                    }
                }
            })

            return fields;
        },
        // END LPC CODE

        /** The current fields as pre-post items to allow pre-post HTML to be rendered */
        // MODIFIED LPC CODE
        prePostHtmlItems(): ItemWithPreAndPostHtml[] {
            return this.currentFormFieldsAugmented
                .map(f => ({
                    preHtml: f.preHtml,
                    postHtml: f.postHtml,
                    slotName: f.guid,
                    isRequired: f.isRequired
                }));
        },
        // END MODIFIED LPC CODE
        currentPerson(): PersonBag | null {
            return store.state.currentPerson;
        },
        pluralFeeTerm(): string {
            return StringFilter.toTitleCase(this.viewModel.pluralFeeTerm || "fees");
        },

        /** The radio options that are displayed to allow the user to pick another person that this
         *  registrant is part of a family. */
        familyOptions(): ListItemBag[] {
            const options: ListItemBag[] = [];
            const usedFamilyGuids: Record<Guid, boolean> = {};

            if (this.viewModel.registrantsSameFamily !== RegistrantsSameFamily.Ask) {
                return options;
            }

            // Add previous registrants as options
            for (let i = 0; i < this.registrationEntryState.currentRegistrantIndex; i++) {
                const registrant = this.registrationEntryState.registrants[i];
                const info = getRegistrantBasicInfo(registrant, this.viewModel.registrantForms);

                if (!usedFamilyGuids[registrant.familyGuid] && info?.firstName && info?.lastName) {
                    options.push({
                        text: `${info.firstName} ${info.lastName}`,
                        value: registrant.familyGuid
                    });

                    usedFamilyGuids[registrant.familyGuid] = true;
                }
            }

            // Add the current person (registrant) if not already added
            if (this.currentPerson?.primaryFamilyGuid && this.currentPerson.fullName && !usedFamilyGuids[this.currentPerson.primaryFamilyGuid]) {
                usedFamilyGuids[this.currentPerson.primaryFamilyGuid] = true;
                options.push({
                    text: this.currentPerson.fullName,
                    value: this.currentPerson.primaryFamilyGuid
                });
            }

            // Add the current person (registrant) if not already added
            const familyGuid = usedFamilyGuids[this.currentRegistrant.familyGuid] == true
                ? newGuid()
                : this.currentRegistrant.familyGuid;
            options.push({
                // MODIFIED LPC CODE
                text: getLang() == 'es' ? 'Ninguno de los anteriores' : 'None of the above',
                // END MODIFIED LPC CODE
                value: familyGuid
            });

            return options;
        },

        /** The people that can be picked from because they are members of the same family. */
        familyMemberOptions(): ListItemBag[] {

            const usedFamilyMemberGuids = this.registrationEntryState.registrants
                .filter(r => r.personGuid && r.personGuid !== this.currentRegistrant.personGuid)
                .map(r => r.personGuid);

            return this.viewModel.familyMembers
                .filter(fm => !usedFamilyMemberGuids.includes(fm.guid))
                .map(fm => ({
                    text: fm.fullName,
                    value: fm.guid
                }));
        },
        uppercaseRegistrantTerm(): string {
            // MODIFIED LPC CODE
            const defaultTerm = (this.viewModel.registrantTerm).toLowerCase();
            return StringFilter.toTitleCase(getLang() == 'es' ? 'registrante' : defaultTerm);
            // END MODIFIED LPC CODE
        },
        firstName(): string {
            return getRegistrantBasicInfo(this.currentRegistrant, this.viewModel.registrantForms).firstName;
        },
        familyMember(): RegistrationEntryBlockFamilyMemberViewModel | null {
            const personGuid = this.currentRegistrant.personGuid;

            if (!personGuid) {
                return null;
            }

            return this.viewModel.familyMembers.find(fm => areEqual(fm.guid, personGuid)) || null;
        },
        signatureDocumentTerm(): string {
            return StringFilter.toTitleCase(this.viewModel.signatureDocumentTerm || "Release");
        }
    },
    methods: {
        // LPC CODE
        getLang,
        // END LPC CODE
        onPrevious(): void {
            this.clearFormErrors();
            if (this.currentFormIndex <= 0) {
                this.$emit("previous");
                return;
            }

            this.registrationEntryState.currentRegistrantFormIndex--;
        },
        async onNext(): Promise<void> {
            this.clearFormErrors();
            let lastFormIndex = this.formsToShow.length - 1;

            // If we have an inline signature then there is an additional form
            // screen that we need to show. Get the document to be signed from
            // the server and then display the form.
            if (this.viewModel.isInlineSignatureRequired) {
                this.isNextDisabled = true;

                try {
                    const result = await this.invokeBlockAction("GetSignatureDocumentData", {
                        args: this.getRegistrationEntryBlockArgs(),
                        registrantGuid: this.currentRegistrant.guid
                    });

                    if (result.isSuccess && result.data) {
                        this.signatureSource = (result.data as Record<string, string>).documentHtml;
                        this.signatureToken = (result.data as Record<string, string>).securityToken;

                        lastFormIndex += 1;
                    }
                    else {
                        console.error(result.data);
                        return;
                    }
                }
                finally {
                    this.isNextDisabled = false;
                }
            }

            if (this.currentFormIndex >= lastFormIndex) {
                this.$emit("next");
                return;
            }

            this.registrationEntryState.currentRegistrantFormIndex++;
        },

        /**
         * Clears any existing errors from the RockForm component. This is
         * needed since we display multiple registrant forms inside the single
         * RockForm instance. Otherwise when moving from the first form to
         * the second form it would immediately validate and display errors.
         */
        clearFormErrors(): void {
            this.formResetKey = newGuid();
        },

        async onSigned(): Promise<void> {
            // Send all the signed document information to the server. This will
            // prepare the final signed document data that will be later sent
            // when we complete the registration.
            const result = await this.invokeBlockAction<string>("SignDocument", {
                args: this.getRegistrationEntryBlockArgs(),
                registrantGuid: this.currentRegistrant.guid,
                documentHtml: this.signatureSource,
                securityToken: this.signatureToken,
                signature: this.signatureData
            });

            if (result.isSuccess && result.data) {
                // Store the signed document data on the registrant.
                this.currentRegistrant.signatureData = result.data;
                this.$emit("next");
            }
            else {
                console.error(result.data);
            }
        },

        /** Copy the values that are to have current values used */
        copyValuesFromFamilyMember(): void {
            if (!this.familyMember || this.registrationEntryState.navBack) {
                // Nothing to copy
                return;
            }

            // If the family member selection is made then set all form fields where use existing value is enabled
            for (const form of this.viewModel.registrantForms) {
                for (const field of form.fields) {
                    if (field.guid in this.familyMember.fieldValues) {
                        const familyMemberValue = this.familyMember.fieldValues[field.guid];

                        if (!familyMemberValue) {
                            delete this.currentRegistrant.fieldValues[field.guid];
                        }
                        else if (typeof familyMemberValue === "object") {
                            this.currentRegistrant.fieldValues[field.guid] = { ...familyMemberValue };
                        }
                        else {
                            this.currentRegistrant.fieldValues[field.guid] = familyMemberValue;
                        }
                    }
                    else {
                        delete this.currentRegistrant.fieldValues[field.guid];
                    }
                }
            }
        }
    },
    watch: {
        "currentRegistrant.familyGuid"(): void {
            // Clear the person guid if the family changes
            this.currentRegistrant.personGuid = null;
        },
        familyMember: {
            handler(): void {
                if (!this.familyMember) {
                    // If the family member selection is cleared then clear all form fields
                    for (const form of this.viewModel.registrantForms) {
                        for (const field of form.fields) {
                            // MODIFIED LPC CODE
                            switch (field.personFieldType) {
                                case RegistrationPersonFieldType.Birthdate:
                                    this.currentRegistrant.fieldValues[field.guid] = getDefaultDatePartsPickerModel();
                                    break;

                                case RegistrationPersonFieldType.AnniversaryDate:
                                    this.currentRegistrant.fieldValues[field.guid] = getDefaultDatePartsPickerModel();
                                    break;

                                case RegistrationPersonFieldType.Address:
                                    this.currentRegistrant.fieldValues[field.guid] = getDefaultAddressControlModel();
                                    break;

                                default:
                            delete this.currentRegistrant.fieldValues[field.guid];
                        }
                            // END MODIFIED LPC CODE
                        }
                    }
                }
                else {
                    // If the family member selection is made then set all form fields where use existing value is enabled
                    this.copyValuesFromFamilyMember();
                }
            }
        }
    },
    created() {
        this.copyValuesFromFamilyMember();
    },
    template: `
<div>
    <RockForm @submit="onNext" :formResetKey="formResetKey">
        <template v-if="isDataForm">
            <template v-if="currentFormIndex === 0">
                <div v-if="familyOptions.length > 1" class="well js-registration-same-family">
                    <!-- MODIFIED LPC CODE -->
                    <RadioButtonList :label="(firstName || uppercaseRegistrantTerm) + ' ' + (getLang() == 'es'? 'está en la misma familia inmediata que' : 'is in the same immediate family as')" rules='required:{"allowEmptyString": true}' v-model="currentRegistrant.familyGuid" :items="familyOptions" validationTitle="Family" />
                    <!-- END MODIFIED LPC CODE -->
                </div>
                <div v-if="familyMemberOptions.length" class="row">
                    <div class="col-md-6">
                        <!-- MODIFIED LPC CODE -->
                        <DropDownList v-model="currentRegistrant.personGuid" :items="familyMemberOptions" :label="(getLang() == 'es' ? 'Miembro de la familia para registrarse' : 'Family Member to Register')" />
                        <!-- END MODIFIED LPC CODE -->
                    </div>
                </div>
            </template>

            <ItemsWithPreAndPostHtml :items="prePostHtmlItems">
                <template v-for="field in currentFormFields" :key="field.guid" v-slot:[field.guid]>
                    <RegistrantPersonField v-if="field.fieldSource === fieldSources.personField" :field="field" :fieldValues="currentRegistrant.fieldValues" :isKnownFamilyMember="!!currentRegistrant.personGuid" />
                    <RegistrantAttributeField v-else-if="field.fieldSource === fieldSources.registrantAttribute || field.fieldSource === fieldSources.personAttribute || field.fieldSource === fieldSources.groupMemberAttribute" :field="field" :fieldValues="currentRegistrant.fieldValues" :formFields="currentFormFields" />
                    <NotificationBox alertType="danger" v-else>Could not resolve field source {{field.fieldSource}}</NotificationBox>
                </template>
            </ItemsWithPreAndPostHtml>

            <div v-if="!isWaitList && isLastForm && viewModel.fees.length" class="well registration-additional-options">
                <h4>{{pluralFeeTerm}}</h4>
                <template v-for="fee in viewModel.fees" :key="fee.guid">
                    <FeeField :fee="fee" v-model="currentRegistrant.feeItemQuantities" />
                </template>
            </div>
        </template>

        <div v-if="isSignatureForm" class="registrant-signature-document styled-scroll">
            <!-- MODIFIED LPC CODE -->
            <h2 class="signature-header" v-if="getLang() == 'es'">Por favor firma la exoneración para {{ firstName }}</h2>
            <h2 class="signature-header" v-else>Please Sign the {{ signatureDocumentTerm }} for {{ firstName }}</h2>
            <!-- END MODIFIED LPC CODE -->
            <div class="signaturedocument-container">
                <iframe src="javascript: window.frameElement.getAttribute('srcdoc');" onload="this.style.height = this.contentWindow.document.body.scrollHeight + 'px'" class="signaturedocument-iframe" border="0" frameborder="0" :srcdoc="signatureSource"></iframe>
            </div>

            <div class="well">
                <ElectronicSignature v-model="signatureData" :isDrawn="isSignatureDrawn" @signed="onSigned" :documentTerm="signatureDocumentTerm" />
            </div>
        </div>

        <div class="actions row">
            <!-- MODIFIED LPC CODE -->
            <div class="col-xs-6">
                <RockButton v-if="showPrevious" btnType="default" @click="onPrevious">
                    {{ getLang() == 'es' ? 'Anterior' : 'Previous' }}
                </RockButton>
            </div>
            <div class="col-xs-6 text-right">
                <RockButton v-if="isNextVisible" btnType="primary" type="submit" :disabled="isNextDisabled">
                  {{ getLang() == 'es' ? 'Siguiente' : 'Next' }}
                </RockButton>
            </div>
            <!-- END MODIFIED LPC CODE -->
        </div>
    </RockForm>
</div>`
});
