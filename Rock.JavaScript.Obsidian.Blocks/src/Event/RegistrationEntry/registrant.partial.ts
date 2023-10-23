﻿// <copyright>
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
import DropDownList from "@Obsidian/Controls/dropDownList.obs";
import ElectronicSignature from "@Obsidian/Controls/electronicSignature.obs";
import RadioButtonList from "@Obsidian/Controls/radioButtonList.obs";
import { getRegistrantBasicInfo } from "./utils.partial";
import StringFilter from "@Obsidian/Utility/stringUtils";
import RockButton from "@Obsidian/Controls/rockButton.obs";
import RegistrantPersonField from "./registrantPersonField.partial";
import RegistrantAttributeField from "./registrantAttributeField.partial";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import { RegistrantInfo, RegistrantsSameFamily, RegistrationEntryBlockFamilyMemberViewModel, RegistrationEntryBlockFormFieldViewModel, RegistrationEntryBlockFormFieldRuleViewModel, RegistrationEntryBlockFormViewModel, RegistrationEntryBlockViewModel, RegistrationFieldSource, RegistrationEntryState, RegistrationEntryBlockArgs } from "./types.partial";
import { areEqual, newGuid } from "@Obsidian/Utility/guid";
import RockForm from "@Obsidian/Controls/rockForm.obs";
import FeeField from "./feeField.partial.obs";
import ItemsWithPreAndPostHtml from "@Obsidian/Controls/itemsWithPreAndPostHtml.obs";
import { ItemWithPreAndPostHtml } from "@Obsidian/Types/Controls/itemsWithPreAndPostHtml";
import { useStore } from "@Obsidian/PageState";
import { CurrentPersonBag } from "@Obsidian/ViewModels/Crm/currentPersonBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { useInvokeBlockAction } from "@Obsidian/Utility/block";
import { ElectronicSignatureValue } from "@Obsidian/ViewModels/Controls/electronicSignatureValue";
import { FilterExpressionType } from "@Obsidian/Core/Reporting/filterExpressionType";
import { getFieldType } from "@Obsidian/Utility/fieldTypes";
import { smoothScrollToTop } from "@Obsidian/Utility/page";

const store = useStore();

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
    setup(props) {
        const invokeBlockAction = useInvokeBlockAction();
        const registrationEntryState = inject("registrationEntryState") as RegistrationEntryState;
        const getRegistrationEntryBlockArgs = inject("getRegistrationEntryBlockArgs") as () => RegistrationEntryBlockArgs;

        const signatureData = ref<ElectronicSignatureValue | null>(null);
        const signatureSource = ref("");
        const signatureToken = ref("");
        const formResetKey = ref("");
        const isNextDisabled = ref(false);
        const isSignatureDrawn = computed((): boolean => registrationEntryState.viewModel.isSignatureDrawn);

        for (const fee of registrationEntryState.viewModel.fees) {
            for (const feeItem of fee.items) {
                if (typeof props.currentRegistrant.feeItemQuantities[feeItem.guid] !== "number") {
                    props.currentRegistrant.feeItemQuantities[feeItem.guid] = 0;
                }
            }
        }

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
    updated() {
        this.updateFeeItemsRemaining();
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
            // Allow to navigate to other registrants
            if(this.registrationEntryState.currentRegistrantIndex > 0) {
                return true;
            }

            // Allow to navigate to registration attributes
            if(this.registrationEntryState.viewModel?.registrationAttributesStart?.length > 0) {
                return true;
            }

            // Allow back to intro page if this is not an existing registration
            if(!this.registrationEntryState.viewModel.isExistingRegistration) {
                return true;
            }

            return false;
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
        currentFormFields(): RegistrationEntryBlockFormFieldViewModel[] {
            return (this.currentForm?.fields || [])
                .filter(f => !this.isWaitList || f.showOnWaitList);
        },

        /** The filtered fields to show on the current form augmented to remove pre/post HTML from non-visible fields */
        currentFormFieldsAugmented(): RegistrationEntryBlockFormFieldViewModel[] {
            const fields = JSON.parse(JSON.stringify(this.currentFormFields)) as RegistrationEntryBlockFormFieldViewModel[];

            fields.forEach(value => {
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

                    if (isVisible === false) {
                        value.preHtml = "";
                        value.postHtml = "";
                    }
                }
            });

            return fields;
        },

        /** The current fields as pre-post items to allow pre-post HTML to be rendered */
        prePostHtmlItems(): ItemWithPreAndPostHtml[] {
            return this.currentFormFieldsAugmented
                .map(f => ({
                    preHtml: f.preHtml,
                    postHtml: f.postHtml,
                    slotName: f.guid
                }));
        },
        currentPerson(): CurrentPersonBag | null {
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
            if (this.viewModel.currentPersonFamilyGuid && this.currentPerson?.fullName && !usedFamilyGuids[this.viewModel.currentPersonFamilyGuid]) {
                usedFamilyGuids[this.viewModel.currentPersonFamilyGuid] = true;
                options.push({
                    text: this.currentPerson.fullName,
                    value: this.viewModel.currentPersonFamilyGuid
                });
            }

            // Add the current person (registrant) if not already added
            const familyGuid = usedFamilyGuids[this.currentRegistrant.familyGuid] == true
                ? newGuid()
                : this.currentRegistrant.familyGuid;
            options.push({
                text: "None of the above",
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
            return StringFilter.toTitleCase(this.viewModel.registrantTerm);
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
        onPrevious(): void {
            this.clearFormErrors();

            if (this.currentFormIndex <= 0) {
                this.$emit("previous");
                return;
            }

            this.registrationEntryState.currentRegistrantFormIndex--;

            // Wait for the previous form to be rendered and then scroll to the top
            setTimeout(() => smoothScrollToTop(), 10);
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

            // Wait for the next form to be rendered and then scroll to the top
            setTimeout(() => smoothScrollToTop(), 10);
        },
        updateFeeItemsRemaining(): void {
            // calculate fee items remaining
            const combinedFeeItemQuantities: Record<Guid, number> = {};

            /* eslint-disable-next-line */
            const self = this;

            // Get all of the fee items in use for all registrants and add them to the combinedFeeItemQuantities Record
            for(const registrant of self.registrationEntryState.registrants) {
                for (const feeItemGuid in registrant.feeItemQuantities) {

                    if (registrant.feeItemQuantities[feeItemGuid] > 0) {
                        const feeItemsUsed = registrant.feeItemQuantities[feeItemGuid];
                        if(combinedFeeItemQuantities[feeItemGuid] === undefined || combinedFeeItemQuantities[feeItemGuid] === null) {
                            combinedFeeItemQuantities[feeItemGuid] = feeItemsUsed;
                        }
                        else {
                            combinedFeeItemQuantities[feeItemGuid] = combinedFeeItemQuantities[feeItemGuid] + feeItemsUsed;
                        }
                    }
                }
            }

            // No go through all of the fee items and update the usage by subtracting the the total in combinedFeeItemQuantities from the originalCountRemaining
            const fees = self.registrationEntryState.viewModel.fees;
            for(const fee of fees){
                const selfFee = fee;
                if(selfFee !== undefined && selfFee !== null && selfFee.items !== undefined && selfFee.items !== null && selfFee.items.length > 0) {
                    for(const feeItem of selfFee.items) {
                        if(feeItem.countRemaining === null || feeItem.countRemaining === undefined || feeItem.originalCountRemaining === undefined || feeItem.originalCountRemaining === null) {
                            continue;
                        }

                        const usedFeeItemCount = combinedFeeItemQuantities[feeItem.guid] ?? 0;
                        if(usedFeeItemCount !== undefined && usedFeeItemCount !== null) {
                            feeItem.countRemaining = feeItem.originalCountRemaining - usedFeeItemCount;
                        }
                    }
                }
            }
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
            if (!this.familyMember || this.registrationEntryState.navBack || this.registrationEntryState.viewModel.isExistingRegistration) {
                // Nothing to copy
                return;
            }

            // If the family member selection is made then set all form fields where use existing value is enabled
            for (const form of this.viewModel.registrantForms) {
                for (const field of form.fields) {
                    // Do not set common fields if they are of type
                    // Registrant Attribute since there is no value to set.
                    // Fixes issue #5610.
                    if (field.fieldSource === RegistrationFieldSource.RegistrantAttribute) {
                        continue;
                    }

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
        },

        async getFieldValues(): Promise<void> {
            const result = await this.invokeBlockAction<Record<Guid, unknown>>("GetDefaultAttributeFieldValues", {
                args: this.getRegistrationEntryBlockArgs(),
                forms: this.viewModel.registrantForms,
                registrantGuid: this.currentRegistrant.guid
            });

            if (result.isSuccess && result.data) {
                for (const form of this.viewModel.registrantForms) {
                    for (const field of form.fields) {
                        // Check if we gota value for the attribute
                        if (field.guid in result.data) {
                            const formFieldValue = result.data[field.guid];
                            const currentFormFieldValue = this.currentRegistrant.fieldValues[field.guid];

                            if(currentFormFieldValue === undefined || currentFormFieldValue === null || currentFormFieldValue === "") {
                                if (typeof formFieldValue === "object" && typeof currentFormFieldValue === "object") {
                                    this.currentRegistrant.fieldValues[field.guid] = { ...formFieldValue };
                                }
                                else {
                                    this.currentRegistrant.fieldValues[field.guid] = formFieldValue;
                                }
                            }
                        }
                    }
                }
            }
        },

        onUpdateRegistrantFee(values: Record<string, number>): void {
            const newValue = {...this.currentRegistrant.feeItemQuantities};

            for (const key of Object.keys(values)) {
                newValue[key] = values[key];
            }

            this.currentRegistrant.feeItemQuantities = newValue;

            this.updateFeeItemsRemaining();
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
                            // Do not touch common fields if they are of type
                            // Registrant Attribute since we don't set them when
                            // selecting a family member either.
                            // Fixes issue #5610.
                            if (field.fieldSource === RegistrationFieldSource.RegistrantAttribute) {
                                continue;
                            }

                            delete this.currentRegistrant.fieldValues[field.guid];
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
        this.getFieldValues();
        this.copyValuesFromFamilyMember();
    },
    template: `
<div class="registrationentry-registrant-details" >
    <RockForm @submit="onNext" :formResetKey="formResetKey">
        <template v-if="isDataForm">
            <template v-if="currentFormIndex === 0">
                <div v-if="familyOptions.length > 1" class="well js-registration-same-family">
                    <RadioButtonList :label="(firstName || uppercaseRegistrantTerm) + ' is in the same immediate family as'" rules='required:{"allowEmptyString": true}' v-model="currentRegistrant.familyGuid" :items="familyOptions" validationTitle="Family" />
                </div>
                <div v-if="familyMemberOptions.length" class="row">
                    <div class="col-md-6">
                        <DropDownList v-model="currentRegistrant.personGuid" :items="familyMemberOptions" label="Family Member to Register" />
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
                    <FeeField :modelValue="currentRegistrant.feeItemQuantities" :fee="fee" @update:modelValue="onUpdateRegistrantFee" />
                </template>
            </div>
        </template>

        <div v-if="isSignatureForm" class="registrant-signature-document styled-scroll">
            <h2 class="signature-header">Please Sign the {{ signatureDocumentTerm }} for {{ firstName }}</h2>
            <div class="signaturedocument-container">
                <iframe src="javascript: window.frameElement.getAttribute('srcdoc');" onload="this.style.height = this.contentWindow.document.body.scrollHeight + 'px'" class="signaturedocument-iframe" border="0" frameborder="0" :srcdoc="signatureSource"></iframe>
            </div>

            <div class="well">
                <ElectronicSignature v-model="signatureData" :isDrawn="isSignatureDrawn" @signed="onSigned" :documentTerm="signatureDocumentTerm" />
            </div>
        </div>

        <div class="actions row">
            <div class="col-xs-6">
                <RockButton v-if="showPrevious" btnType="default" @click="onPrevious">
                    Previous
                </RockButton>
            </div>
            <div class="col-xs-6 text-right">
                <RockButton v-if="isNextVisible" btnType="primary" type="submit" :disabled="isNextDisabled">
                    Next
                </RockButton>
            </div>
        </div>
    </RockForm>
</div>`
});