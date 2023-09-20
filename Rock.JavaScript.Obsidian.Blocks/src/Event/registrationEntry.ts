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

import { defineComponent, provide, reactive, ref } from "vue";
import NotificationBox from "@Obsidian/Controls/notificationBox.obs";
import CountdownTimer from "@Obsidian/Controls/countdownTimer";
import JavaScriptAnchor from "@Obsidian/Controls/javaScriptAnchor";
import ProgressTracker, { ProgressTrackerItem } from "@Obsidian/Controls/progressTracker";
import RockButton from "@Obsidian/Controls/rockButton";
import NumberFilter, { toWord } from "@Obsidian/Utility/numberUtils";
import StringFilter, { isNullOrWhiteSpace, toTitleCase } from "@Obsidian/Utility/stringUtils";
import { useStore } from "@Obsidian/PageState";
import { useConfigurationValues, useInvokeBlockAction } from "@Obsidian/Utility/block";
import { newGuid } from "@Obsidian/Utility/guid";
import { List } from "@Obsidian/Utility/linq";
import Page from "@Obsidian/Utility/page";
import { RockDateTime } from "@Obsidian/Utility/rockDateTime";
import { PersonBag } from "@Obsidian/ViewModels/Entities/personBag";
import RegistrationEntryIntro from "./RegistrationEntry/intro.partial";
import RegistrationEntryRegistrants from "./RegistrationEntry/registrants.partial";
import RegistrationEntryRegistrationEnd from "./RegistrationEntry/registrationEnd.partial";
import { RegistrantInfo, RegistrationEntryBlockFeeViewModel, RegistrationEntryBlockViewModel, RegistrationEntryBlockArgs, Step, RegistrationEntryState } from "./RegistrationEntry/types.partial";
import RegistrationEntryRegistrationStart from "./RegistrationEntry/registrationStart.partial";
import SessionRenewal from "./RegistrationEntry/sessionRenewal.partial";
import RegistrationEntrySuccess from "./RegistrationEntry/success.partial";
import RegistrationEntryPayment from "./RegistrationEntry/payment.partial";
import RegistrationEntrySummary from "./RegistrationEntry/summary.partial";
import { getDefaultRegistrantInfo, getForcedFamilyGuid, getRegistrantBasicInfo } from "./RegistrationEntry/utils.partial";

const store = useStore();

// LPC CODE
/** Gets the lang parameter from the query string.
 * Returns "en" or "es". Defaults to "en" if invalid. */
function getLang(): string {
    let lang = typeof store.state.pageParameters["lang"] === "string" ? store.state.pageParameters["lang"] : "";

    if (lang != "es") {
        lang = "en";
    }

    return lang;
}
// END LPC CODE

export default defineComponent({
    name: "Event.RegistrationEntry",
    components: {
        RockButton,
        RegistrationEntryIntro,
        RegistrationEntryRegistrants,
        RegistrationEntryRegistrationStart,
        RegistrationEntryRegistrationEnd,
        RegistrationEntrySummary,
        RegistrationEntryPayment,
        RegistrationEntrySuccess,
        ProgressTracker,
        NotificationBox,
        CountdownTimer,
        JavaScriptAnchor,
        SessionRenewal
    },
    setup() {
        const steps: Record<Step, Step> = {
            [Step.Intro]: Step.Intro,
            [Step.RegistrationStartForm]: Step.RegistrationStartForm,
            [Step.PerRegistrantForms]: Step.PerRegistrantForms,
            [Step.RegistrationEndForm]: Step.RegistrationEndForm,
            [Step.Review]: Step.Review,
            [Step.Payment]: Step.Payment,
            [Step.Success]: Step.Success
        };

        const notFound = ref(false);
        const viewModel = useConfigurationValues<RegistrationEntryBlockViewModel | null>();
        const invokeBlockAction = useInvokeBlockAction();
        // MODIFIED LPC CODE
        const notFoundMessage = getLang() == 'es' ? "La inscripción seleccionada no se encuentra o ya no está activa." : (viewModel?.registrationInstanceNotFoundMessage || "The selected registration could not be found or is no longer active.");
        // END MODIFIED LPC CODE

        if (viewModel === null || viewModel.registrationInstanceNotFoundMessage) {
            notFound.value = true;

            return {
                viewModel,
                notFound,
                notFoundMessage
            };
        }

        if (!viewModel.registrationAttributesStart) {
            notFound.value = true;
        }

        const hasPreAttributes = viewModel.registrationAttributesStart?.length > 0;
        let currentStep = steps.intro;

        if (viewModel.successViewModel) {
            // This is after having paid via redirect gateway
            currentStep = steps.success;
        }
        else if (viewModel.session && !viewModel.startAtBeginning) {
            // This is an existing registration, start at the summary
            currentStep = steps.review;
        }
        else if (viewModel.maxRegistrants === 1 && isNullOrWhiteSpace(viewModel.instructionsHtml)) {
            // There is no need to show the number of registrants selector or instructions. Start at the second page.
            currentStep = hasPreAttributes ? steps.registrationStartForm : steps.perRegistrantForms;
        }
        else if (viewModel.isExistingRegistration && viewModel.startAtBeginning) {
            // An existing registration with StartAtBeginning set to true, go right to the attributes.
            currentStep = hasPreAttributes ? steps.registrationStartForm : steps.perRegistrantForms;
        }

        const staticRegistrationEntryState: RegistrationEntryState = {
            steps: steps,
            viewModel: viewModel,
            firstStep: currentStep,
            currentStep: currentStep,
            navBack: false,
            currentRegistrantFormIndex: 0,
            currentRegistrantIndex: 0,
            registrants: viewModel.session?.registrants || [getDefaultRegistrantInfo(null, viewModel, null)],
            registrationFieldValues: viewModel.session?.fieldValues || {},
            registrar: viewModel.session?.registrar || {
                nickName: "",
                lastName: "",
                email: "",
                // LPC CODE
                mobilePhone: "",
                preferredLanguage: "",
                // END LPC CODE
                updateEmail: true,
                familyGuid: null
            },
            gatewayToken: "",
            savedAccountGuid: null,
            discountCode: viewModel.session?.discountCode || "",
            discountAmount: viewModel.session?.discountAmount || 0,
            discountPercentage: viewModel.session?.discountPercentage || 0,
            discountMaxRegistrants: viewModel.session?.discountMaxRegistrants || 0,
            successViewModel: viewModel.successViewModel,
            amountToPayToday: 0,
            sessionExpirationDateMs: null,
            registrationSessionGuid: viewModel.session?.registrationSessionGuid || newGuid(),
            ownFamilyGuid: store.state.currentPerson?.primaryFamilyGuid || newGuid()
        };
        const registrationEntryState = reactive(staticRegistrationEntryState);

        provide("registrationEntryState", registrationEntryState);

        /** A method to get the args needed for persisting the session */
        const getRegistrationEntryBlockArgs: () => RegistrationEntryBlockArgs = () => {
            return {
                registrationSessionGuid: registrationEntryState.registrationSessionGuid,
                gatewayToken: registrationEntryState.gatewayToken,
                savedAccountGuid: registrationEntryState.savedAccountGuid,
                discountCode: registrationEntryState.discountCode,
                fieldValues: registrationEntryState.registrationFieldValues,
                registrar: registrationEntryState.registrar,
                registrants: registrationEntryState.registrants,
                amountToPayNow: registrationEntryState.amountToPayToday,
                registrationGuid: viewModel.session?.registrationGuid || null
            };
        };

        provide("getRegistrationEntryBlockArgs", getRegistrationEntryBlockArgs);

        /** A method to persist the session */
        const persistSession: (force: boolean) => Promise<void> = async (force = false) => {
            if (!force && !viewModel.timeoutMinutes) {
                return;
            }

            const response = await invokeBlockAction<{ expirationDateTime: string }>("PersistSession", {
                args: getRegistrationEntryBlockArgs()
            });

            if (response.data) {
                const expirationDate = RockDateTime.parseISO(response.data.expirationDateTime);

                registrationEntryState.sessionExpirationDateMs = expirationDate?.toMilliseconds() ?? null;
            }
        };

        provide("persistSession", persistSession);

        /** Expose these members and make them available within the rest of the component */
        return {
            viewModel,
            steps,
            registrationEntryState,
            notFound,
            notFoundMessage,
            persistSession,
            invokeBlockAction,
            getRegistrationEntryBlockArgs
        };
    },
    data() {
        return {
            secondsBeforeExpiration: -1,
            hasSessionRenewalSuccess: false
        };
    },
    computed: {
        // LPC CODE
        generalTranslationStyles(): string {
            if (getLang() == 'es') {
                return ".EnglishText { display: none !important; }";
            }
            else {
                return ".SpanishText { display: none !important; }";
            }
        },
        // END LPC CODE

        /** The person currently authenticated */
        currentPerson(): PersonBag | null {
            return store.state.currentPerson;
        },

        /** Is the session expired? */
        isSessionExpired(): boolean {
            return this.secondsBeforeExpiration === 0 && this.currentStep !== Step.Success;
        },

        mustLogin(): boolean {
            return !store.state.currentPerson && this.viewModel != null && (this.viewModel.isUnauthorized || this.viewModel.loginRequiredToRegister);
        },
        isUnauthorized(): boolean {
            return this.viewModel?.isUnauthorized ?? false;
        },
        currentStep(): string {
            return this.registrationEntryState?.currentStep ?? "";
        },
        registrants(): RegistrantInfo[] {
            return this.registrationEntryState?.registrants ?? [];
        },
        hasPreAttributes(): boolean {
            return (this.viewModel?.registrationAttributesStart?.length ?? 0) > 0;
        },
        hasPostAttributes(): boolean {
            return (this.viewModel?.registrationAttributesEnd?.length ?? 0) > 0;
        },
        progressTrackerIndex(): number {
            if (this.currentStep === Step.Intro || this.registrationEntryState == null) {
                return 0;
            }

            const stepsBeforePre = this.registrationEntryState?.firstStep === Step.Intro ? 1 : 0;

            if (this.currentStep === Step.RegistrationStartForm) {
                return stepsBeforePre;
            }

            const stepsBeforeRegistrants = stepsBeforePre + (this.hasPreAttributes ? 1 : 0);

            if (this.currentStep === Step.PerRegistrantForms) {
                return this.registrationEntryState.currentRegistrantIndex + stepsBeforeRegistrants;
            }

            const stepsToCompleteRegistrants = this.registrationEntryState.registrants.length + stepsBeforeRegistrants;

            if (this.currentStep === Step.RegistrationEndForm) {
                return stepsToCompleteRegistrants;
            }

            if (this.currentStep === Step.Review) {
                return stepsToCompleteRegistrants + (this.hasPostAttributes ? 1 : 0);
            }

            if (this.currentStep === Step.Payment) {
                return stepsToCompleteRegistrants + (this.hasPostAttributes ? 1 : 0);
            }

            return 0;
        },
        uppercaseRegistrantTerm(): string {
            // MODIFIED LPC CODE
            const defaultTerm = (this.viewModel?.registrantTerm ?? "").toLowerCase();
            return StringFilter.toTitleCase(getLang() == "es" ? "persona" : defaultTerm);
            // END MODIFIED LPC CODE
        },
        currentRegistrantTitle(): string {
            if (this.registrationEntryState == null) {
                return "";
            }

            // MODIFIED LPC CODE
            const ordinal = NumberFilter.toOrdinal(this.registrationEntryState.currentRegistrantIndex + 1, getLang());
            let title = "";
            if (getLang() == "es") {
                title = StringFilter.toTitleCase(
                    this.registrants.length <= 1 ?
                        "Persona" :
                        ordinal + " Persona");
            }
            else {
                title = StringFilter.toTitleCase(
                    this.registrants.length <= 1 ?
                        this.uppercaseRegistrantTerm :
                        ordinal + " " + this.uppercaseRegistrantTerm);
            }
            // END MODIFIED LPC CODE

            if (this.registrationEntryState.currentRegistrantFormIndex > 0) {
                title += " (cont)";
            }

            return title;
        },
        stepTitleHtml(): string {
            if (this.currentStep === Step.RegistrationStartForm) {
                // LPC CODE
                if (getLang() == "es") {
                    return "Información de Registro";
                }
                else {
                    return this.viewModel?.registrationAttributeTitleStart ?? ""; // THIS LINE IS THE ORIGINAL CODE
                }
                // END LPC CODE
            }

            if (this.currentStep === Step.PerRegistrantForms) {
                return this.currentRegistrantTitle;
            }

            if (this.currentStep === Step.RegistrationEndForm) {
                // LPC CODE
                if (getLang() == "es") {
                    return "Información de Registro";
                }
                else {
                    return this.viewModel?.registrationAttributeTitleEnd ?? ""; // THIS LINE IS THE ORIGINAL CODE
                }
                // END LPC CODE
            }

            if (this.currentStep === Step.Review) {
                // LPC CODE
                if (getLang() == "es") {
                    return "Revisión de Registro";
                }
                else {
                    return "Review Registration"; // THIS LINE IS THE ORIGINAL CODE
                }
                // END LPC CODE
            }

            if (this.currentStep === Step.Success) {
                // LPC CODE
                if (getLang() == "es") {
                    let output = "Felicidades " + store.state.currentPerson?.nickName;

                    const englishTitle = this.registrationEntryState?.successViewModel?.titleHtml || "";
                    if (englishTitle.toLowerCase().includes("almost there")) {
                        output = "Casi Terminamos, " + store.state.currentPerson?.nickName;
                    }

                    return output;
                }
                else {
                    return this.registrationEntryState?.successViewModel?.titleHtml || "Congratulations"; // THIS LINE IS THE ORIGINAL CODE
                }
                // END LPC CODE
            }

            return "";
        },

        /** The items to display in the progress tracker */
        progressTrackerItems(): ProgressTrackerItem[] {
            const items: ProgressTrackerItem[] = [];

            if (this.registrationEntryState == null) {
                return items;
            }

            if (this.registrationEntryState.firstStep === Step.Intro) {
                items.push({
                    key: "Start",
                    // MODIFIED LPC CODE
                    title: getLang() == 'es' ? 'Iniciar' : 'Start',
                    subtitle: this.registrationTermTitleCase
                    // END MODIFIED LPC CODE
                });
            }

            if (this.hasPreAttributes) {
                items.push({
                    key: "Pre",
                    // MODIFIED LPC CODE
                    title: getLang() == 'es' ? 'Información de' : this.viewModel.registrationAttributeTitleStart,
                    subtitle: this.registrationTermTitleCase
                    // END MODIFIED LPC CODE
                });
            }

            if (!this.registrationEntryState.registrants.length) {
                items.push({
                    key: "Registrant",
                    // MODIFIED LPC CODE
                    title: this.uppercaseRegistrantTerm,
                    subtitle: this.registrationTermTitleCase
                    // END MODIFIED LPC CODE
                });
            }

            for (let i = 0; i < this.registrationEntryState.registrants.length; i++) {
                const registrant = this.registrationEntryState.registrants[i];
                const info = getRegistrantBasicInfo(registrant, this.viewModel.registrantForms);

                if (info?.firstName && info?.lastName) {
                    items.push({
                        key: `Registrant-${registrant.guid}`,
                        title: info.firstName,
                        subtitle: info.lastName
                    });
                }
                else {
                    items.push({
                        key: `Registrant-${registrant.guid}`,
                        // MODIFIED LPC CODE
                        title: this.uppercaseRegistrantTerm,
                        subtitle: toTitleCase(toWord(i + 1, getLang()))
                        // END MODIFIED LPC CODE
                    });
                }
            }

            if (this.hasPostAttributes) {
                items.push({
                    key: "Post",
                    title: this.viewModel.registrationAttributeTitleEnd,
                    // MODIFIED LPC CODE
                    subtitle: this.registrationTermTitleCase
                    // END MODIFIED LPC CODE
                });
            }

            items.push({
                key: "Finalize",
                // MODIFIED LPC CODE
                title: getLang() == 'es' ? 'Finalizar' : 'Finalize',
                subtitle: this.registrationTermTitleCase
                // END MODIFIED LPC CODE
            });

            return items;
        },

        /**
         * Determines if there are any costs or fees on the registration and
         * checks if we have a valid gateway.
         */
        isInvalidGateway() {
            if (!this.viewModel) {
                return false;
            }

            const hasCostFees = new List<RegistrationEntryBlockFeeViewModel>(this.viewModel.fees)
                .any(f => new List(f.items).any(i => i.cost > 0));

            // If no cost or fees, then no gateway will be needed.
            if (this.viewModel.cost <= 0 && !hasCostFees) {
                return false;
            }

            if (this.viewModel.isRedirectGateway || this.viewModel.gatewayControl.fileUrl) {
                return false;
            }

            return true;
        },

        isFull(): boolean {
            if (!this.viewModel || this.viewModel.spotsRemaining === null) {
                return false;
            }

            return this.viewModel.spotsRemaining < 1 && !this.viewModel.waitListEnabled;
        },

        preventNewRegistration(): boolean {
            if (!this.viewModel) {
                return this.isFull;
            }

            return this.isFull && !this.viewModel.isExistingRegistration;
        },

        registrationTerm(): string {
            // MODIFIED LPC CODE
            const defaultTerm = (this.viewModel?.registrationTerm || "registration").toLowerCase();
            return getLang() == 'es' ? 'registro' : defaultTerm;
            // END MODIFIED LPC CODE
        },

        registrationTermPlural(): string {
            // MODIFIED LPC CODE
            const defaultTerm = (this.viewModel?.pluralRegistrationTerm || "registrations").toLowerCase();
            return getLang() == 'es' ? 'registros' : defaultTerm;
            // END MODIFIED LPC CODE
        },

        registrationTermTitleCase(): string {
            return toTitleCase(this.registrationTerm);
        }
    },
    methods: {
        /** The user requested an extension in time and it was granted */
        onSessionRenewalSuccess(): void {
            this.hasSessionRenewalSuccess = true;
            setTimeout(() => this.hasSessionRenewalSuccess = false, 5000);
        },

        async onIntroNext(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                await this.persistSession(false);
                this.registrationEntryState.currentStep = this.hasPreAttributes ? Step.RegistrationStartForm : Step.PerRegistrantForms;
                this.registrationEntryState.navBack = false;
                Page.smoothScrollToTop();
            }
        },
        async onRegistrationStartPrevious(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                await this.persistSession(false);
                this.registrationEntryState.currentStep = Step.Intro;
                this.registrationEntryState.navBack = true;
                Page.smoothScrollToTop();
            }
        },
        async onRegistrationStartNext(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                await this.persistSession(false);
                this.registrationEntryState.currentStep = Step.PerRegistrantForms;
                this.registrationEntryState.navBack = false;
                Page.smoothScrollToTop();
            }
        },
        async onRegistrantPrevious(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                await this.persistSession(false);
                this.registrationEntryState.currentStep = this.hasPreAttributes ? Step.RegistrationStartForm : Step.Intro;
                this.registrationEntryState.navBack = true;
                Page.smoothScrollToTop();
            }
        },
        async onRegistrantNext(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                await this.persistSession(false);
                this.registrationEntryState.currentStep = this.hasPostAttributes ? Step.RegistrationEndForm : Step.Review;
                this.registrationEntryState.navBack = false;
                Page.smoothScrollToTop();
            }
        },
        async onRegistrationEndPrevious(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                await this.persistSession(false);
                this.registrationEntryState.currentStep = Step.PerRegistrantForms;
                this.registrationEntryState.navBack = true;
                Page.smoothScrollToTop();
            }
        },
        async onRegistrationEndNext(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                await this.persistSession(false);
                this.registrationEntryState.currentStep = Step.Review;
                this.registrationEntryState.navBack = false;
                Page.smoothScrollToTop();
            }
        },
        async onSummaryPrevious(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                await this.persistSession(false);

                if (this.hasPostAttributes) {
                    this.registrationEntryState.currentStep = Step.RegistrationEndForm;
                }
                else {
                    const lastFormIndex = this.registrationEntryState.viewModel.registrantForms.length - 1;

                    this.registrationEntryState.currentRegistrantFormIndex = lastFormIndex;
                    this.registrationEntryState.currentStep = Step.PerRegistrantForms;
                }
                this.registrationEntryState.navBack = true;
                Page.smoothScrollToTop();
            }
        },
        async onSummaryNext(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                if (this.registrationEntryState.amountToPayToday) {
                    this.registrationEntryState.currentStep = Step.Payment;
                }
                else {
                    this.registrationEntryState.currentStep = Step.Success;
                }
                this.registrationEntryState.navBack = false;
                Page.smoothScrollToTop();
            }
        },
        async onPaymentPrevious(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                await this.persistSession(false);
                this.registrationEntryState.currentStep = Step.Review;
                this.registrationEntryState.navBack = true;
                Page.smoothScrollToTop();
            }
        },
        async onPaymentNext(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                this.registrationEntryState.currentStep = Step.Success;
                this.registrationEntryState.navBack = false;
                Page.smoothScrollToTop();
            }
        }
    },
    watch: {
        currentPerson: {
            immediate: true,
            handler() {
                if (this.viewModel != null && this.registrationEntryState != null) {
                    const forcedFamilyGuid = getForcedFamilyGuid(this.currentPerson, this.viewModel);

                    if (forcedFamilyGuid) {
                        for (const registrant of this.registrationEntryState.registrants) {
                            registrant.familyGuid = forcedFamilyGuid;
                        }
                    }
                }
            }
        },
        "registrationEntryState.sessionExpirationDateMs": {
            immediate: true,
            handler() {
                if (!this.registrationEntryState?.sessionExpirationDateMs) {
                    this.secondsBeforeExpiration = -1;
                    return;
                }

                const nowMs = RockDateTime.now().toMilliseconds();
                const thenMs = this.registrationEntryState.sessionExpirationDateMs;
                const diffMs = thenMs - nowMs;
                this.secondsBeforeExpiration = diffMs / 1000;
            }
        }
    },
    mounted() {
        if (this.viewModel?.loginRequiredToRegister && !store.state.currentPerson) {
            store.redirectToLogin();
        }
    },
    template: `
<div>
    <NotificationBox v-if="notFound" alertType="warning">
        <strong>Sorry</strong>
        <p>{{notFoundMessage}}</p>
    </NotificationBox>
    <NotificationBox v-else-if="mustLogin" alertType="warning">
        <strong>Please log in</strong>
        <p>You must be logged in to access this registration.</p>
    </NotificationBox>
    <NotificationBox v-else-if="isUnauthorized" alertType="warning">
        <strong>Sorry</strong>
        <p>You are not allowed to view or edit the selected registration since you are not the one who created the registration.</p>
    </NotificationBox>
    <NotificationBox v-else-if="isInvalidGateway" alertType="warning">
        <strong>Incorrect Configuration</strong>
        <p>This registration has costs/fees associated with it but the configured payment gateway is not supported.</p>
    </NotificationBox>
    <NotificationBox v-else-if="preventNewRegistration" class="text-left" alertType="warning">
        <strong>{{registrationTermTitleCase}} Full</strong>
        <p>
            There are not any more {{registrationTermPlural}} available for {{viewModel.instanceName}}.
        </p>
    </NotificationBox>
    <template v-else>
        <h1 v-if="currentStep !== steps.intro" v-html="stepTitleHtml"></h1>
        <ProgressTracker v-if="currentStep !== steps.success" :items="progressTrackerItems" :currentIndex="progressTrackerIndex">
            <template #aside>
                <div v-if="secondsBeforeExpiration >= 0" v-show="secondsBeforeExpiration <= (30 * 60)" class="remaining-time flex-grow-1 flex-md-grow-0">
                    <NotificationBox v-if="hasSessionRenewalSuccess" alertType="success" class="m-0 pt-3" style="position: absolute; top: 0; left: 0; right: 0; bottom: 0;">
                        <h4>Success</h4>
                    </NotificationBox>
                    <span class="remaining-time-title">Time left before timeout</span>
                    <p class="remaining-time-countdown">
                        <CountdownTimer v-model="secondsBeforeExpiration" />
                    </p>
                </div>
            </template>
        </ProgressTracker>
        <RegistrationEntryIntro v-if="currentStep === steps.intro" @next="onIntroNext" />
        <RegistrationEntryRegistrationStart v-else-if="currentStep === steps.registrationStartForm" @next="onRegistrationStartNext" @previous="onRegistrationStartPrevious" />
        <RegistrationEntryRegistrants v-else-if="currentStep === steps.perRegistrantForms" @next="onRegistrantNext" @previous="onRegistrantPrevious" />
        <RegistrationEntryRegistrationEnd v-else-if="currentStep === steps.registrationEndForm" @next="onRegistrationEndNext" @previous="onRegistrationEndPrevious" />
        <RegistrationEntrySummary v-else-if="currentStep === steps.review" @next="onSummaryNext" @previous="onSummaryPrevious" />
        <RegistrationEntryPayment v-else-if="currentStep === steps.payment" @next="onPaymentNext" @previous="onPaymentPrevious" />
        <RegistrationEntrySuccess v-else-if="currentStep === steps.success" />
        <NotificationBox v-else alertType="danger">Invalid State: '{{currentStep}}'</NotificationBox>
        <!-- LPC CODE -->
        <component is="style">
            .hide-label label.control-label, .SpanishLabel, .SpanishOption {
                display: none !important;
            }
            {{ generalTranslationStyles }}
        </component>
        <!-- END LPC CODE -->
    </template>
    <SessionRenewal :isSessionExpired="isSessionExpired" @success="onSessionRenewalSuccess" />
</div>`
});
