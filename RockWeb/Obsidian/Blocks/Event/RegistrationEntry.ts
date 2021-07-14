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

import { defineComponent, inject, provide, reactive, ref } from 'vue';
import RockButton from '../../Elements/RockButton';
import { Guid, newGuid } from '../../Util/Guid';
import RegistrationEntryIntro from './RegistrationEntry/Intro';
import RegistrationEntryRegistrants from './RegistrationEntry/Registrants';
import { RegistrantInfo, RegistrantsSameFamily, RegistrarInfo, RegistrationEntryBlockFormViewModel, RegistrationEntryBlockSuccessViewModel, RegistrationEntryBlockViewModel, RegistrationPersonFieldType } from './RegistrationEntry/RegistrationEntryBlockViewModel';
import RegistrationEntryRegistrationStart from './RegistrationEntry/RegistrationStart';
import RegistrationEntryRegistrationEnd from './RegistrationEntry/RegistrationEnd';
import RegistrationEntrySummary from './RegistrationEntry/Summary';
import Registrants from './RegistrationEntry/Registrants';
import ProgressTracker, { ProgressTrackerItem } from '../../Elements/ProgressTracker';
import NumberFilter, { toWord } from '../../Services/Number';
import StringFilter, { isNullOrWhitespace, toTitleCase } from '../../Services/String';
import Alert from '../../Elements/Alert';
import CountdownTimer from '../../Elements/CountdownTimer';
import RegistrationEntrySuccess from './RegistrationEntry/Success';
import Page from '../../Util/Page';
import { RegistrationEntryBlockArgs } from './RegistrationEntry/RegistrationEntryBlockArgs';
import { InvokeBlockActionFunc } from '../../Controls/RockBlock';
import JavaScriptAnchor from '../../Elements/JavaScriptAnchor';
import Person from '../../ViewModels/CodeGenerated/PersonViewModel';
import SessionRenewal from './RegistrationEntry/SessionRenewal';

export enum Step
{
    'intro' = 'intro',
    'registrationStartForm' = 'registrationStartForm',
    'perRegistrantForms' = 'perRegistrantForms',
    'registrationEndForm' = 'registrationEndForm',
    'reviewAndPayment' = 'reviewAndPayment',
    'success' = 'success'
}

export type RegistrantBasicInfo = {
    FirstName: string;
    LastName: string;
    Email: string;
    Guid: Guid;
};

export type RegistrationEntryState = {
    Steps: Record<Step, Step>;
    ViewModel: RegistrationEntryBlockViewModel;
    CurrentStep: string;
    FirstStep: string;
    CurrentRegistrantIndex: number;
    CurrentRegistrantFormIndex: number;
    Registrants: RegistrantInfo[];
    RegistrationFieldValues: Record<Guid, unknown>;
    Registrar: RegistrarInfo;
    GatewayToken: string;
    DiscountCode: string;
    DiscountAmount: number;
    DiscountPercentage: number;
    SuccessViewModel: RegistrationEntryBlockSuccessViewModel | null;
    AmountToPayToday: number;
    SessionExpirationDate: Date | null;
    RegistrationSessionGuid: Guid;
};

/** If all registrants are to be in the same family, but there is no currently authenticated person,
 *  then this guid is used as a common family guid */
const unknownSingleFamilyGuid = newGuid();

/**
 * If there is a forced family guid because of RegistrantsSameFamily setting, then this returns that guid
 * @param currentPerson
 * @param viewModel
 */
export function getForcedFamilyGuid ( currentPerson: Person | null, viewModel: RegistrationEntryBlockViewModel )
{
    return ( currentPerson && viewModel.registrantsSameFamily === RegistrantsSameFamily.Yes ) ?
        ( currentPerson.primaryFamilyGuid || unknownSingleFamilyGuid ) :
        null;
}

/**
 * Get a default registrant object with the current family guid set.
 * @param currentPerson
 * @param viewModel
 * @param familyGuid
 */
export function getDefaultRegistrantInfo ( currentPerson: Person | null, viewModel: RegistrationEntryBlockViewModel, familyGuid: Guid | null )
{
    const forcedFamilyGuid = getForcedFamilyGuid( currentPerson, viewModel );
    const ownFamilyGuid = newGuid();

    if ( forcedFamilyGuid )
    {
        familyGuid = forcedFamilyGuid;
    }

    // If the family is not specified, then assume the person is in their own family
    if ( !familyGuid )
    {
        familyGuid = ownFamilyGuid;
    }

    return {
        IsOnWaitList: false,
        FamilyGuid: familyGuid,
        FieldValues: {},
        FeeItemQuantities: {},
        Guid: newGuid(),
        PersonGuid: '',
        OwnFamilyGuid: ownFamilyGuid
    } as RegistrantInfo;
}

export function getRegistrantBasicInfo ( registrant: RegistrantInfo, registrantForms: RegistrationEntryBlockFormViewModel[] ): RegistrantBasicInfo
{
    const fields = registrantForms?.flatMap( f => f.fields ) || [];

    const firstNameGuid = fields.find( f => f.personFieldType === RegistrationPersonFieldType.FirstName )?.guid || '';
    const lastNameGuid = fields.find( f => f.personFieldType === RegistrationPersonFieldType.LastName )?.guid || '';
    const emailGuid = fields.find( f => f.personFieldType === RegistrationPersonFieldType.Email )?.guid || '';

    return {
        FirstName: ( registrant?.FieldValues[ firstNameGuid ] || '' ) as string,
        LastName: ( registrant?.FieldValues[ lastNameGuid ] || '' ) as string,
        Email: ( registrant?.FieldValues[ emailGuid ] || '' ) as string,
        Guid: registrant?.Guid
    };
}

export default defineComponent( {
    name: 'Event.RegistrationEntry',
    components: {
        RockButton,
        Registrants,
        RegistrationEntryIntro,
        RegistrationEntryRegistrants,
        RegistrationEntryRegistrationStart,
        RegistrationEntryRegistrationEnd,
        RegistrationEntrySummary,
        RegistrationEntrySuccess,
        ProgressTracker,
        Alert,
        CountdownTimer,
        JavaScriptAnchor,
        SessionRenewal
    },
    setup ()
    {
        const steps: Record<Step, Step> = {
            [ Step.intro ]: Step.intro,
            [ Step.registrationStartForm ]: Step.registrationStartForm,
            [ Step.perRegistrantForms ]: Step.perRegistrantForms,
            [ Step.registrationEndForm ]: Step.registrationEndForm,
            [ Step.reviewAndPayment ]: Step.reviewAndPayment,
            [ Step.success ]: Step.success
        };

        const notFound = ref( false );
        const viewModel = inject( 'configurationValues' ) as RegistrationEntryBlockViewModel;
        const invokeBlockAction = inject( 'invokeBlockAction' ) as InvokeBlockActionFunc;

        if ( !viewModel?.registrationAttributesStart )
        {
            notFound.value = true;
        }

        const hasPreAttributes = viewModel.registrationAttributesStart?.length > 0;
        let currentStep = steps.intro;

        if ( viewModel.successViewModel )
        {
            // This is after having paid via redirect gateway
            currentStep = steps.success;
        }
        else if ( viewModel.session && !viewModel.startAtBeginning )
        {
            // This is an existing registration, start at the summary
            currentStep = steps.reviewAndPayment;
        }
        else if ( viewModel.maxRegistrants === 1 && isNullOrWhitespace( viewModel.instructionsHtml ) )
        {
            // There is no need to show the number of registrants selector or instructions. Start at the second page.
            currentStep = hasPreAttributes ? steps.registrationStartForm : steps.perRegistrantForms;
        }

        const registrationEntryState = reactive( {
            Steps: steps,
            ViewModel: viewModel,
            FirstStep: currentStep,
            CurrentStep: currentStep,
            CurrentRegistrantFormIndex: 0,
            CurrentRegistrantIndex: 0,
            Registrants: viewModel.session?.registrants || [ getDefaultRegistrantInfo( null, viewModel, null ) ],
            RegistrationFieldValues: viewModel.session?.fieldValues || {},
            Registrar: viewModel.session?.registrar || {
                NickName: '',
                LastName: '',
                Email: '',
                UpdateEmail: true,
                OwnFamilyGuid: newGuid(),
                FamilyGuid: null
            },
            GatewayToken: '',
            DiscountCode: viewModel.session?.discountCode || '',
            DiscountAmount: viewModel.session?.discountAmount || 0,
            DiscountPercentage: viewModel.session?.discountPercentage || 0,
            SuccessViewModel: viewModel.successViewModel,
            AmountToPayToday: 0,
            SessionExpirationDate: null,
            RegistrationSessionGuid: viewModel.session?.registrationSessionGuid || newGuid()
        } as RegistrationEntryState );

        provide( 'registrationEntryState', registrationEntryState );

        /** A method to get the args needed for persisting the session */
        const getRegistrationEntryBlockArgs: () => RegistrationEntryBlockArgs = () =>
        {
            return {
                registrationSessionGuid: registrationEntryState.RegistrationSessionGuid,
                gatewayToken: registrationEntryState.GatewayToken,
                discountCode: registrationEntryState.DiscountCode,
                fieldValues: registrationEntryState.RegistrationFieldValues,
                registrar: registrationEntryState.Registrar,
                registrants: registrationEntryState.Registrants,
                amountToPayNow: registrationEntryState.AmountToPayToday,
                registrationGuid: viewModel.session?.registrationGuid || null
            };
        };

        provide( 'getRegistrationEntryBlockArgs', getRegistrationEntryBlockArgs );

        /** A method to persist the session */
        const persistSession: ( force: boolean ) => Promise<void> = async ( force = false ) =>
        {
            if ( !force && !viewModel.timeoutMinutes )
            {
                return;
            }

            const response = await invokeBlockAction<{ ExpirationDateTime: string }>( 'PersistSession', {
                args: getRegistrationEntryBlockArgs()
            } );

            if ( response.data )
            {
                const asDate = new Date( response.data.ExpirationDateTime );
                registrationEntryState.SessionExpirationDate = asDate;
            }
        };

        provide( 'persistSession', persistSession );

        /** Expose these members and make them available within the rest of the component */
        return {
            viewModel,
            steps,
            registrationEntryState,
            notFound,
            persistSession
        };
    },
    data ()
    {
        return {
            secondsBeforeExpiration: -1,
            hasSessionRenewalSuccess: false
        };
    },
    computed: {
        /** The person currently authenticated */
        currentPerson (): Person | null
        {
            return this.$store.state.currentPerson;
        },

        /** Is the session expired? */
        isSessionExpired (): boolean
        {
            return this.secondsBeforeExpiration === 0 && this.currentStep !== this.steps.success;
        },

        viewModel (): RegistrationEntryBlockViewModel
        {
            return this.registrationEntryState.ViewModel;
        },
        mustLogin (): boolean
        {
            return !this.$store.state.currentPerson && ( this.viewModel.isUnauthorized || this.viewModel.loginRequiredToRegister );
        },
        isUnauthorized (): boolean
        {
            return this.viewModel.isUnauthorized;
        },
        currentStep (): string
        {
            return this.registrationEntryState.CurrentStep;
        },
        registrants (): RegistrantInfo[]
        {
            return this.registrationEntryState.Registrants;
        },
        hasPreAttributes (): boolean
        {
            return this.viewModel.registrationAttributesStart.length > 0;
        },
        hasPostAttributes (): boolean
        {
            return this.viewModel.registrationAttributesEnd.length > 0;
        },
        progressTrackerIndex (): number
        {
            if ( this.currentStep === this.steps.intro )
            {
                return 0;
            }

            const stepsBeforePre = this.registrationEntryState.FirstStep === this.steps.intro ? 1 : 0;

            if ( this.currentStep === this.steps.registrationStartForm )
            {
                return stepsBeforePre;
            }

            const stepsBeforeRegistrants = stepsBeforePre + ( this.hasPreAttributes ? 1 : 0 );

            if ( this.currentStep === this.steps.perRegistrantForms )
            {
                return this.registrationEntryState.CurrentRegistrantIndex + stepsBeforeRegistrants;
            }

            const stepsToCompleteRegistrants = this.registrationEntryState.Registrants.length + stepsBeforeRegistrants;

            if ( this.currentStep === this.steps.registrationEndForm )
            {
                return stepsToCompleteRegistrants;
            }

            if ( this.currentStep === this.steps.reviewAndPayment )
            {
                return stepsToCompleteRegistrants + ( this.hasPostAttributes ? 1 : 0 );
            }

            return 0;
        },
        uppercaseRegistrantTerm (): string
        {
            return StringFilter.toTitleCase( this.viewModel.registrantTerm );
        },
        currentRegistrantTitle (): string
        {
            const ordinal = NumberFilter.toOrdinal( this.registrationEntryState.CurrentRegistrantIndex + 1 );
            let title = StringFilter.toTitleCase(
                this.registrants.length <= 1 ?
                    this.uppercaseRegistrantTerm :
                    ordinal + ' ' + this.uppercaseRegistrantTerm );

            if ( this.registrationEntryState.CurrentRegistrantFormIndex > 0 )
            {
                title += ' (cont)';
            }

            return title;
        },
        stepTitleHtml (): string
        {
            if ( this.currentStep === this.steps.registrationStartForm )
            {
                return this.viewModel.registrationAttributeTitleStart;
            }

            if ( this.currentStep === this.steps.perRegistrantForms )
            {
                return this.currentRegistrantTitle;
            }

            if ( this.currentStep === this.steps.registrationEndForm )
            {
                return this.viewModel.registrationAttributeTitleEnd;
            }

            if ( this.currentStep === this.steps.reviewAndPayment )
            {
                return 'Review Registration';
            }

            if ( this.currentStep === this.steps.success )
            {
                return this.registrationEntryState.SuccessViewModel?.titleHtml || 'Congratulations';
            }

            return '';
        },

        /** The items to display in the progress tracker */
        progressTrackerItems (): ProgressTrackerItem[]
        {
            const items: ProgressTrackerItem[] = [];

            if ( this.registrationEntryState.FirstStep === this.steps.intro )
            {
                items.push( {
                    Key: 'Start',
                    Title: 'Start',
                    Subtitle: this.viewModel.registrationTerm
                } );
            }

            if ( this.hasPreAttributes )
            {
                items.push( {
                    Key: 'Pre',
                    Title: this.viewModel.registrationAttributeTitleStart,
                    Subtitle: this.viewModel.registrationTerm
                } );
            }

            if ( !this.registrationEntryState.Registrants.length )
            {
                items.push( {
                    Key: 'Registrant',
                    Title: toTitleCase( this.viewModel.registrantTerm ),
                    Subtitle: this.viewModel.registrationTerm
                } );
            }

            for ( let i = 0; i < this.registrationEntryState.Registrants.length; i++ )
            {
                const registrant = this.registrationEntryState.Registrants[ i ];
                const info = getRegistrantBasicInfo( registrant, this.viewModel.registrantForms );

                if ( info?.FirstName && info?.LastName )
                {
                    items.push( {
                        Key: `Registrant-${registrant.Guid}`,
                        Title: info.FirstName,
                        Subtitle: info.LastName
                    } );
                }
                else
                {
                    items.push( {
                        Key: `Registrant-${registrant.Guid}`,
                        Title: toTitleCase( this.viewModel.registrantTerm ),
                        Subtitle: toTitleCase( toWord( i + 1 ) )
                    } );
                }
            }

            if ( this.hasPostAttributes )
            {
                items.push( {
                    Key: 'Post',
                    Title: this.viewModel.registrationAttributeTitleEnd,
                    Subtitle: this.viewModel.registrationTerm
                } );
            }

            items.push( {
                Key: 'Finalize',
                Title: 'Finalize',
                Subtitle: this.viewModel.registrationTerm
            } );

            return items;
        }
    },
    methods: {
        /** The user requested an extension in time and it was granted */
        onSessionRenewalSuccess ()
        {
            this.hasSessionRenewalSuccess = true;
            setTimeout( () => this.hasSessionRenewalSuccess = false, 5000 );
        },

        async onIntroNext ()
        {
            await this.persistSession( false );
            this.registrationEntryState.CurrentStep = this.hasPreAttributes ? this.steps.registrationStartForm : this.steps.perRegistrantForms;
            Page.smoothScrollToTop();
        },
        async onRegistrationStartPrevious ()
        {
            await this.persistSession( false );
            this.registrationEntryState.CurrentStep = this.steps.intro;
            Page.smoothScrollToTop();
        },
        async onRegistrationStartNext ()
        {
            await this.persistSession( false );
            this.registrationEntryState.CurrentStep = this.steps.perRegistrantForms;
            Page.smoothScrollToTop();
        },
        async onRegistrantPrevious ()
        {
            await this.persistSession( false );
            this.registrationEntryState.CurrentStep = this.hasPreAttributes ? this.steps.registrationStartForm : this.steps.intro;
            Page.smoothScrollToTop();
        },
        async onRegistrantNext ()
        {
            await this.persistSession( false );
            this.registrationEntryState.CurrentStep = this.hasPostAttributes ? this.steps.registrationEndForm : this.steps.reviewAndPayment;
            Page.smoothScrollToTop();
        },
        async onRegistrationEndPrevious ()
        {
            await this.persistSession( false );
            this.registrationEntryState.CurrentStep = this.steps.perRegistrantForms;
            Page.smoothScrollToTop();
        },
        async onRegistrationEndNext ()
        {
            await this.persistSession( false );
            this.registrationEntryState.CurrentStep = this.steps.reviewAndPayment;
            Page.smoothScrollToTop();
        },
        async onSummaryPrevious ()
        {
            await this.persistSession( false );
            this.registrationEntryState.CurrentStep = this.hasPostAttributes ? this.steps.registrationEndForm : this.steps.perRegistrantForms;
            Page.smoothScrollToTop();
        },
        async onSummaryNext ()
        {
            this.registrationEntryState.CurrentStep = this.steps.success;
            Page.smoothScrollToTop();
        }
    },
    watch: {
        currentPerson: {
            immediate: true,
            handler ()
            {
                const forcedFamilyGuid = getForcedFamilyGuid( this.currentPerson, this.viewModel );

                if ( forcedFamilyGuid )
                {
                    for ( const registrant of this.registrationEntryState.Registrants )
                    {
                        registrant.FamilyGuid = forcedFamilyGuid;
                    }
                }
            }
        },
        'registrationEntryState.SessionExpirationDate': {
            immediate: true,
            handler ()
            {
                if ( !this.registrationEntryState.SessionExpirationDate )
                {
                    this.secondsBeforeExpiration = -1;
                    return;
                }

                const nowMs = new Date().getTime();
                const thenMs = this.registrationEntryState.SessionExpirationDate.getTime();
                const diffMs = thenMs - nowMs;
                this.secondsBeforeExpiration = diffMs / 1000;
            }
        }
    },
    mounted ()
    {
        if ( this.viewModel.loginRequiredToRegister && !this.$store.state.currentPerson )
        {
            this.$store.dispatch( 'redirectToLogin' );
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
        <RegistrationEntrySummary v-else-if="currentStep === steps.reviewAndPayment" @next="onSummaryNext" @previous="onSummaryPrevious" />
        <RegistrationEntrySuccess v-else-if="currentStep === steps.success" />
        <Alert v-else alertType="danger">Invalid State: '{{currentStep}}'</Alert>
    </template>
    <SessionRenewal :isSessionExpired="isSessionExpired" @success="onSessionRenewalSuccess" />
</div>`
} );
