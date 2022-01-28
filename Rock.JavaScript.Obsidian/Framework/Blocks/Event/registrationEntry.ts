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
import Alert from "../../Elements/alert";
import CountdownTimer from "../../Elements/countdownTimer";
import JavaScriptAnchor from "../../Elements/javaScriptAnchor";
import ProgressTracker, { ProgressTrackerItem } from "../../Elements/progressTracker";
import RockButton from "../../Elements/rockButton";
import NumberFilter, { toWord } from "../../Services/number";
import StringFilter, { isNullOrWhiteSpace, toTitleCase } from "../../Services/string";
import { useStore } from "../../Store/index";
import { useConfigurationValues, useInvokeBlockAction } from "../../Util/block";
import { Guid, newGuid } from "../../Util/guid";
import { List } from "../../Util/linq";
import Page from "../../Util/page";
import { RockDateTime } from "../../Util/rockDateTime";
import { Person } from "../../ViewModels";
import RegistrationEntryIntro from "./RegistrationEntry/intro";
import { default as RegistrationEntryRegistrants } from "./RegistrationEntry/registrants";
import RegistrationEntryRegistrationEnd from "./RegistrationEntry/registrationEnd";
import { RegistrationEntryBlockArgs } from "./RegistrationEntry/registrationEntryBlockArgs";
import { RegistrantInfo, RegistrantsSameFamily, RegistrarInfo, RegistrationEntryBlockFormFieldViewModel, RegistrationEntryBlockFormViewModel, RegistrationEntryBlockSuccessViewModel, RegistrationEntryBlockViewModel, RegistrationPersonFieldType } from "./RegistrationEntry/registrationEntryBlockViewModel";
import RegistrationEntryRegistrationStart from "./RegistrationEntry/registrationStart";
import SessionRenewal from "./RegistrationEntry/sessionRenewal";
import RegistrationEntrySuccess from "./RegistrationEntry/success";
import RegistrationEntryPayment from "./RegistrationEntry/payment";
import RegistrationEntrySummary from "./RegistrationEntry/summary";

const store = useStore();

const enum Step {
    Intro = "intro",
    RegistrationStartForm = "registrationStartForm",
    PerRegistrantForms = "perRegistrantForms",
    RegistrationEndForm = "registrationEndForm",
    Review = "review",
    Payment = "payment",
    Success = "success"
}

export type RegistrantBasicInfo = {
    firstName: string;
    lastName: string;
    email: string;
    guid: Guid;
};

export type RegistrationEntryState = {
    steps: Record<Step, Step>;
    viewModel: RegistrationEntryBlockViewModel;
    currentStep: string;
    firstStep: string;
    currentRegistrantIndex: number;
    currentRegistrantFormIndex: number;
    registrants: RegistrantInfo[];
    registrationFieldValues: Record<Guid, unknown>;
    registrar: RegistrarInfo;
    gatewayToken: string;
    savedAccountGuid: Guid | null;
    discountCode: string;
    discountAmount: number;
    discountPercentage: number;
    successViewModel: RegistrationEntryBlockSuccessViewModel | null;
    amountToPayToday: number;
    sessionExpirationDateMs: number | null;
    registrationSessionGuid: Guid;
    ownFamilyGuid: Guid;
};

/** If all registrants are to be in the same family, but there is no currently authenticated person,
 *  then this guid is used as a common family guid */
const unknownSingleFamilyGuid = newGuid();

/**
 * If there is a forced family guid because of RegistrantsSameFamily setting, then this returns that guid
 * @param currentPerson
 * @param viewModel
 */
export function getForcedFamilyGuid (currentPerson: Person | null, viewModel: RegistrationEntryBlockViewModel): string | null {
    return (currentPerson && viewModel.registrantsSameFamily === RegistrantsSameFamily.Yes) ?
        (currentPerson.primaryFamilyGuid || unknownSingleFamilyGuid) :
        null;
}

/**
 * Get a default registrant object with the current family guid set.
 * @param currentPerson
 * @param viewModel
 * @param familyGuid
 */
export function getDefaultRegistrantInfo (currentPerson: Person | null, viewModel: RegistrationEntryBlockViewModel, familyGuid: Guid | null): RegistrantInfo {
    const forcedFamilyGuid = getForcedFamilyGuid(currentPerson, viewModel);

    if (forcedFamilyGuid) {
        familyGuid = forcedFamilyGuid;
    }

    // If the family is not specified, then assume the person is in their own family
    if (!familyGuid) {
        familyGuid = newGuid();
    }

    return {
        isOnWaitList: false,
        familyGuid: familyGuid,
        fieldValues: {},
        feeItemQuantities: {},
        guid: newGuid(),
        personGuid: null
    } as RegistrantInfo;
}

export function getRegistrantBasicInfo (registrant: RegistrantInfo, registrantForms: RegistrationEntryBlockFormViewModel[]): RegistrantBasicInfo {
    const fields = registrantForms?.reduce((acc, f) => acc.concat(f.fields), [] as RegistrationEntryBlockFormFieldViewModel[]) || [];

    const firstNameGuid = fields.find(f => f.personFieldType === RegistrationPersonFieldType.FirstName)?.guid || "";
    const lastNameGuid = fields.find(f => f.personFieldType === RegistrationPersonFieldType.LastName)?.guid || "";
    const emailGuid = fields.find(f => f.personFieldType === RegistrationPersonFieldType.Email)?.guid || "";

    return {
        firstName: (registrant?.fieldValues[ firstNameGuid ] || "") as string,
        lastName: (registrant?.fieldValues[ lastNameGuid ] || "") as string,
        email: (registrant?.fieldValues[ emailGuid ] || "") as string,
        guid: registrant?.guid
    };
}

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
        Alert,
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

        if (viewModel === null) {
            notFound.value = true;

            return {
                viewModel,
                notFound
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

        const staticRegistrationEntryState: RegistrationEntryState = {
            steps: steps,
            viewModel: viewModel,
            firstStep: currentStep,
            currentStep: currentStep,
            currentRegistrantFormIndex: 0,
            currentRegistrantIndex: 0,
            registrants: viewModel.session?.registrants || [getDefaultRegistrantInfo(null, viewModel, null)],
            registrationFieldValues: viewModel.session?.fieldValues || {},
            registrar: viewModel.session?.registrar || {
                nickName: "",
                lastName: "",
                email: "",
                updateEmail: true,
                familyGuid: null
            },
            gatewayToken: "",
            savedAccountGuid: null,
            discountCode: viewModel.session?.discountCode || "",
            discountAmount: viewModel.session?.discountAmount || 0,
            discountPercentage: viewModel.session?.discountPercentage || 0,
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
        /** The person currently authenticated */
        currentPerson(): Person | null {
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
            return StringFilter.toTitleCase(this.viewModel?.registrantTerm ?? "");
        },
        currentRegistrantTitle(): string {
            if (this.registrationEntryState == null) {
                return "";
            }

            const ordinal = NumberFilter.toOrdinal(this.registrationEntryState.currentRegistrantIndex + 1);
            let title = StringFilter.toTitleCase(
                this.registrants.length <= 1 ?
                    this.uppercaseRegistrantTerm :
                    ordinal + " " + this.uppercaseRegistrantTerm);

            if (this.registrationEntryState.currentRegistrantFormIndex > 0) {
                title += " (cont)";
            }

            return title;
        },
        stepTitleHtml(): string {
            if (this.currentStep === Step.RegistrationStartForm) {
                return this.viewModel?.registrationAttributeTitleStart ?? "";
            }

            if (this.currentStep === Step.PerRegistrantForms) {
                return this.currentRegistrantTitle;
            }

            if (this.currentStep === Step.RegistrationEndForm) {
                return this.viewModel?.registrationAttributeTitleEnd ?? "";
            }

            if (this.currentStep === Step.Review) {
                return "Review Registration";
            }

            if (this.currentStep === Step.Success) {
                return this.registrationEntryState?.successViewModel?.titleHtml || "Congratulations";
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
                    title: "Start",
                    subtitle: this.viewModel.registrationTerm
                });
            }

            if (this.hasPreAttributes) {
                items.push({
                    key: "Pre",
                    title: this.viewModel.registrationAttributeTitleStart,
                    subtitle: this.viewModel.registrationTerm
                });
            }

            if (!this.registrationEntryState.registrants.length) {
                items.push({
                    key: "Registrant",
                    title: toTitleCase(this.viewModel.registrantTerm),
                    subtitle: this.viewModel.registrationTerm
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
                        title: toTitleCase(this.viewModel.registrantTerm),
                        subtitle: toTitleCase(toWord(i + 1))
                    });
                }
            }

            if (this.hasPostAttributes) {
                items.push({
                    key: "Post",
                    title: this.viewModel.registrationAttributeTitleEnd,
                    subtitle: this.viewModel.registrationTerm
                });
            }

            items.push({
                key: "Finalize",
                title: "Finalize",
                subtitle: this.viewModel.registrationTerm
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

            const hasCostFees = new List(this.viewModel.fees)
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

        registrationTerm(): string {
            return (this.viewModel?.registrationTerm || "registration").toLowerCase();
        },

        registrationTermPlural(): string {
            return (this.viewModel?.pluralRegistrationTerm || "registrations").toLowerCase();
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
                Page.smoothScrollToTop();
            }
        },
        async onRegistrationStartPrevious(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                await this.persistSession(false);
                this.registrationEntryState.currentStep = Step.Intro;
                Page.smoothScrollToTop();
            }
        },
        async onRegistrationStartNext(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                await this.persistSession(false);
                this.registrationEntryState.currentStep = Step.PerRegistrantForms;
                Page.smoothScrollToTop();
            }
        },
        async onRegistrantPrevious(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                await this.persistSession(false);
                this.registrationEntryState.currentStep = this.hasPreAttributes ? Step.RegistrationStartForm : Step.Intro;
                Page.smoothScrollToTop();
            }
        },
        async onRegistrantNext(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                await this.persistSession(false);
                this.registrationEntryState.currentStep = this.hasPostAttributes ? Step.RegistrationEndForm : Step.Review;
                Page.smoothScrollToTop();
            }
        },
        async onRegistrationEndPrevious(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                await this.persistSession(false);
                this.registrationEntryState.currentStep = Step.PerRegistrantForms;
                Page.smoothScrollToTop();
            }
        },
        async onRegistrationEndNext(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                await this.persistSession(false);
                this.registrationEntryState.currentStep = Step.Review;
                Page.smoothScrollToTop();
            }
        },
        async onSummaryPrevious(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                await this.persistSession(false);
                this.registrationEntryState.currentStep = this.hasPostAttributes ? Step.RegistrationEndForm : Step.PerRegistrantForms;
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
                Page.smoothScrollToTop();
            }
        },
        async onPaymentPrevious(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                await this.persistSession(false);
                this.registrationEntryState.currentStep = Step.Review;
                Page.smoothScrollToTop();
            }
        },
        async onPaymentNext(): Promise<void> {
            if (this.persistSession && this.registrationEntryState) {
                this.registrationEntryState.currentStep = Step.Success;
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
    <Alert v-if="notFound" alertType="warning">
        <strong>Sorry</strong>
        <p>The selected registration could not be found or is no longer active.</p>
    </Alert>
    <Alert v-else-if="mustLogin" alertType="warning">
        <strong>Please log in</strong>
        <p>You must be logged in to access this registration.</p>
    </Alert>
    <Alert v-else-if="isUnauthorized" alertType="warning">
        <strong>Sorry</strong>
        <p>You are not allowed to view or edit the selected registration since you are not the one who created the registration.</p>
    </Alert>
    <Alert v-else-if="isInvalidGateway" alertType="warning">
        <strong>Incorrect Configuration</strong>
        <p>This registration has costs/fees associated with it but the configured payment gateway is not supported.</p>
    </Alert>
    <Alert v-else-if="isFull" class="text-left" alertType="warning">
        <strong>{{registrationTermTitleCase}} Full</strong>
        <p>
            There are not any more {{registrationTermPlural}} available for {{viewModel.instanceName}}.
        </p>
    </Alert>
    <template v-else>
        <h1 v-if="currentStep !== steps.intro" v-html="stepTitleHtml"></h1>
        <ProgressTracker v-if="currentStep !== steps.success" :items="progressTrackerItems" :currentIndex="progressTrackerIndex">
            <template #aside>
                <div v-if="secondsBeforeExpiration >= 0" v-show="secondsBeforeExpiration <= (30 * 60)" class="remaining-time flex-grow-1 flex-md-grow-0">
                    <Alert v-if="hasSessionRenewalSuccess" alertType="success" class="m-0 pt-3" style="position: absolute; top: 0; left: 0; right: 0; bottom: 0;">
                        <h4>Success</h4>
                    </Alert>
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
        <Alert v-else alertType="danger">Invalid State: '{{currentStep}}'</Alert>
    </template>
    <SessionRenewal :isSessionExpired="isSessionExpired" @success="onSessionRenewalSuccess" />
</div>`
});
