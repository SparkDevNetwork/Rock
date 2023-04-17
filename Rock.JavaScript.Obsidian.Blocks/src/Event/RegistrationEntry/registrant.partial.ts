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
import Alert from "@Obsidian/Controls/alert.vue";
import { RegistrantInfo, RegistrantsSameFamily, RegistrationEntryBlockFamilyMemberViewModel, RegistrationEntryBlockFormFieldViewModel, RegistrationEntryBlockFormViewModel, RegistrationEntryBlockViewModel, RegistrationFieldSource, RegistrationEntryState, RegistrationEntryBlockArgs } from "./types";
import { areEqual, newGuid } from "@Obsidian/Utility/guid";
import RockForm from "@Obsidian/Controls/rockForm";
import FeeField from "./feeField.partial";
import ItemsWithPreAndPostHtml, { ItemWithPreAndPostHtml } from "@Obsidian/Controls/itemsWithPreAndPostHtml";
import { useStore } from "@Obsidian/PageState";
import { PersonBag } from "@Obsidian/ViewModels/Entities/personBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { useInvokeBlockAction } from "@Obsidian/Utility/block";
import { ElectronicSignatureValue } from "@Obsidian/ViewModels/Controls/electronicSignatureValue";

const store = useStore();

export default defineComponent({
    name: "Event.RegistrationEntry.Registrant",
    components: {
        ElectronicSignature,
        RadioButtonList,
        RockButton,
        RegistrantPersonField,
        RegistrantAttributeField,
        Alert,
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
    setup () {
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
    data () {
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
        showPrevious (): boolean {
            return this.registrationEntryState.firstStep !== this.registrationEntryState.steps.perRegistrantForms;
        },
        viewModel (): RegistrationEntryBlockViewModel {
            return this.registrationEntryState.viewModel;
        },
        currentFormIndex (): number {
            return this.registrationEntryState.currentRegistrantFormIndex;
        },
        currentForm (): RegistrationEntryBlockFormViewModel | null {
            return this.formsToShow[ this.currentFormIndex ] || null;
        },
        isLastForm (): boolean {
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
        formsToShow (): RegistrationEntryBlockFormViewModel[] {
            if (!this.isWaitList) {
                return this.viewModel.registrantForms;
            }

            return this.viewModel.registrantForms.filter(form => form.fields.some(field => field.showOnWaitList));
        },

        /** The filtered fields to show on the current form */
        currentFormFields (): RegistrationEntryBlockFormFieldViewModel[] {
            return (this.currentForm?.fields || [])
                .filter(f => !this.isWaitList || f.showOnWaitList);
        },

        /** The current fields as pre-post items to allow pre-post HTML to be rendered */
        prePostHtmlItems (): ItemWithPreAndPostHtml[] {
            return this.currentFormFields
                .map(f => ({
                    preHtml: f.preHtml,
                    postHtml: f.postHtml,
                    slotName: f.guid
                }));
        },
        currentPerson (): PersonBag | null {
            return store.state.currentPerson;
        },
        pluralFeeTerm (): string {
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
                const registrant = this.registrationEntryState.registrants[ i ];
                const info = getRegistrantBasicInfo(registrant, this.viewModel.registrantForms);

                if (!usedFamilyGuids[ registrant.familyGuid ] && info?.firstName && info?.lastName) {
                    options.push({
                        text: `${info.firstName} ${info.lastName}`,
                        value: registrant.familyGuid
                    });

                    usedFamilyGuids[ registrant.familyGuid ] = true;
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
                text: "None of the above",
                value: familyGuid
            });

            return options;
        },

        /** The people that can be picked from because they are members of the same family. */
        familyMemberOptions (): ListItemBag[] {
            const selectedFamily = this.currentRegistrant.familyGuid;

            if (!selectedFamily) {
                return [];
            }

            const usedFamilyMemberGuids = this.registrationEntryState.registrants
                .filter(r => r.personGuid && r.personGuid !== this.currentRegistrant.personGuid)
                .map(r => r.personGuid);

            return this.viewModel.familyMembers
                .filter(fm =>
                    areEqual(fm.familyGuid, selectedFamily) &&
                    !usedFamilyMemberGuids.includes(fm.guid))
                .map(fm => ({
                    text: fm.fullName,
                    value: fm.guid
                }));
        },
        uppercaseRegistrantTerm (): string {
            return StringFilter.toTitleCase(this.viewModel.registrantTerm);
        },
        firstName (): string {
            return getRegistrantBasicInfo(this.currentRegistrant, this.viewModel.registrantForms).firstName;
        },
        familyMember (): RegistrationEntryBlockFamilyMemberViewModel | null {
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
    created () {
        this.copyValuesFromFamilyMember();
    },
    template: `
<div>
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
                    <Alert alertType="danger" v-else>Could not resolve field source {{field.fieldSource}}</Alert>
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
