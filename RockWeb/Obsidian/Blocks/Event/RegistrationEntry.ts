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
import { RegistrantInfo, RegistrarInfo, RegistrationEntryBlockFormViewModel, RegistrationEntryBlockSuccessViewModel, RegistrationEntryBlockViewModel, RegistrationPersonFieldType } from './RegistrationEntry/RegistrationEntryBlockViewModel';
import RegistrationEntryRegistrationStart from './RegistrationEntry/RegistrationStart';
import RegistrationEntryRegistrationEnd from './RegistrationEntry/RegistrationEnd';
import RegistrationEntrySummary from './RegistrationEntry/Summary';
import Registrants from './RegistrationEntry/Registrants';
import ProgressBar from '../../Elements/ProgressBar';
import NumberFilter from '../../Services/Number';
import StringFilter, { isNullOrWhitespace } from '../../Services/String';
import Alert from '../../Elements/Alert';
import RegistrationEntrySuccess from './RegistrationEntry/Success';
import Page from '../../Util/Page';

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
    SuccessViewModel: RegistrationEntryBlockSuccessViewModel | null;
};

export function getDefaultRegistrantInfo ()
{
    const ownFamilyGuid = newGuid();

    return {
        IsOnWaitList: false,
        FamilyGuid: ownFamilyGuid,
        FieldValues: {},
        FeeItemQuantities: {},
        Guid: newGuid(),
        PersonGuid: '',
        OwnFamilyGuid: ownFamilyGuid
    } as RegistrantInfo;
}

export function getRegistrantBasicInfo ( registrant: RegistrantInfo, registrantForms: RegistrationEntryBlockFormViewModel[] ): RegistrantBasicInfo
{
    const fields = registrantForms?.flatMap( f => f.Fields ) || [];

    const firstNameGuid = fields.find( f => f.PersonFieldType === RegistrationPersonFieldType.FirstName )?.Guid || '';
    const lastNameGuid = fields.find( f => f.PersonFieldType === RegistrationPersonFieldType.LastName )?.Guid || '';
    const emailGuid = fields.find( f => f.PersonFieldType === RegistrationPersonFieldType.Email )?.Guid || '';

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
        ProgressBar,
        Alert
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

        if ( !viewModel?.RegistrationAttributesStart )
        {
            notFound.value = true;
        }

        const hasPreAttributes = viewModel.RegistrationAttributesStart?.length > 0;
        let currentStep = steps.intro;

        if ( viewModel.Session )
        {
            // This is an existing registration, start at the summary
            currentStep = steps.reviewAndPayment;
        }
        else if ( viewModel.MaxRegistrants === 1 && isNullOrWhitespace( viewModel.InstructionsHtml ) )
        {
            // There is no need to show the numer of registrants selector or instructions. Start at the second page.
            currentStep = hasPreAttributes ? steps.registrationStartForm : steps.perRegistrantForms;
        }

        const registrationEntryState = reactive( {
            Steps: steps,
            ViewModel: viewModel,
            FirstStep: currentStep,
            CurrentStep: currentStep,
            CurrentRegistrantFormIndex: 0,
            CurrentRegistrantIndex: 0,
            Registrants: viewModel.Session?.Registrants || [ getDefaultRegistrantInfo() ],
            RegistrationFieldValues: viewModel.Session?.FieldValues || {},
            Registrar: viewModel.Session?.Registrar || {
                NickName: '',
                LastName: '',
                Email: '',
                UpdateEmail: true
            },
            GatewayToken: '',
            DiscountCode: viewModel.Session?.DiscountCode || '',
            SuccessViewModel: null
        } as RegistrationEntryState );

        provide( 'registrationEntryState', registrationEntryState );

        return {
            viewModel,
            steps,
            registrationEntryState,
            notFound
        };
    },
    computed: {
        viewModel (): RegistrationEntryBlockViewModel
        {
            return this.registrationEntryState.ViewModel;
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
            return this.viewModel.RegistrationAttributesStart.length > 0;
        },
        hasPostAttributes (): boolean
        {
            return this.viewModel.RegistrationAttributesEnd.length > 0;
        },
        numberOfPages (): number
        {
            return 2 + // Intro and summary
                ( this.hasPostAttributes ? 1 : 0 ) +
                ( this.hasPreAttributes ? 1 : 0 ) +
                ( this.viewModel.RegistrantForms.length * this.registrants.length );
        },
        completionPercentDecimal (): number
        {
            if ( this.currentStep === this.steps.intro )
            {
                return 0;
            }

            if ( this.currentStep === this.steps.registrationStartForm )
            {
                return 1 / this.numberOfPages;
            }

            if ( this.currentStep === this.steps.perRegistrantForms )
            {
                const firstRegistrantPage = this.viewModel.RegistrationAttributesStart.length === 0 ? 1 : 2;
                const finishedRegistrantForms = this.registrationEntryState.CurrentRegistrantIndex * this.viewModel.RegistrantForms.length;
                return ( firstRegistrantPage + this.registrationEntryState.CurrentRegistrantFormIndex + finishedRegistrantForms ) / this.numberOfPages;
            }

            if ( this.currentStep === this.steps.registrationEndForm )
            {
                return ( this.numberOfPages - 2 ) / this.numberOfPages;
            }

            if ( this.currentStep === this.steps.reviewAndPayment )
            {
                return ( this.numberOfPages - 1 ) / this.numberOfPages;
            }

            if ( this.currentStep === this.steps.success )
            {
                return 1;
            }

            return 0;
        },
        completionPercentInt (): number
        {
            return this.completionPercentDecimal * 100;
        },
        uppercaseRegistrantTerm (): string
        {
            return StringFilter.toTitleCase( this.viewModel.RegistrantTerm );
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
                return this.viewModel.RegistrationAttributeTitleStart;
            }

            if ( this.currentStep === this.steps.perRegistrantForms )
            {
                return this.currentRegistrantTitle;
            }

            if ( this.currentStep === this.steps.registrationEndForm )
            {
                return this.viewModel.RegistrationAttributeTitleEnd;
            }

            if ( this.currentStep === this.steps.reviewAndPayment )
            {
                return 'Review Registration';
            }

            if ( this.currentStep === this.steps.success )
            {
                return this.registrationEntryState.SuccessViewModel?.TitleHtml || 'Congratulations';
            }

            return '';
        }
    },
    methods: {
        onIntroNext ()
        {
            this.registrationEntryState.CurrentStep = this.hasPreAttributes ? this.steps.registrationStartForm : this.steps.perRegistrantForms;
            Page.smoothScrollToTop();
        },
        onRegistrationStartPrevious ()
        {
            this.registrationEntryState.CurrentStep = this.steps.intro;
            Page.smoothScrollToTop();
        },
        onRegistrationStartNext ()
        {
            this.registrationEntryState.CurrentStep = this.steps.perRegistrantForms;
            Page.smoothScrollToTop();
        },
        onRegistrantPrevious ()
        {
            this.registrationEntryState.CurrentStep = this.hasPreAttributes ? this.steps.registrationStartForm : this.steps.intro;
            Page.smoothScrollToTop();
        },
        onRegistrantNext ()
        {
            this.registrationEntryState.CurrentStep = this.hasPostAttributes ? this.steps.registrationEndForm : this.steps.reviewAndPayment;
            Page.smoothScrollToTop();
        },
        onRegistrationEndPrevious ()
        {
            this.registrationEntryState.CurrentStep = this.steps.perRegistrantForms;
            Page.smoothScrollToTop();
        },
        onRegistrationEndNext ()
        {
            this.registrationEntryState.CurrentStep = this.steps.reviewAndPayment;
            Page.smoothScrollToTop();
        },
        onSummaryPrevious ()
        {
            this.registrationEntryState.CurrentStep = this.hasPostAttributes ? this.steps.registrationEndForm : this.steps.perRegistrantForms;
            Page.smoothScrollToTop();
        },
        onSummaryNext ()
        {
            this.registrationEntryState.CurrentStep = this.steps.success;
            Page.smoothScrollToTop();
        }
    },
    template: `
<div>
    <Alert v-if="notFound" alertType="warning">
        <strong>Sorry</strong>
        <p>The selected registration could not be found or is no longer active.</p>
    </Alert>
    <template v-else>
        <template v-if="currentStep !== steps.intro">
            <h1 v-html="stepTitleHtml"></h1>
            <ProgressBar :percent="completionPercentInt" />
        </template>

        <RegistrationEntryIntro v-if="currentStep === steps.intro" @next="onIntroNext" />
        <RegistrationEntryRegistrationStart v-else-if="currentStep === steps.registrationStartForm" @next="onRegistrationStartNext" @previous="onRegistrationStartPrevious" />
        <RegistrationEntryRegistrants v-else-if="currentStep === steps.perRegistrantForms" @next="onRegistrantNext" @previous="onRegistrantPrevious" />
        <RegistrationEntryRegistrationEnd v-else-if="currentStep === steps.registrationEndForm" @next="onRegistrationEndNext" @previous="onRegistrationEndPrevious" />
        <RegistrationEntrySummary v-else-if="currentStep === steps.reviewAndPayment" @next="onSummaryNext" @previous="onSummaryPrevious" />
        <RegistrationEntrySuccess v-else-if="currentStep === steps.success" />
        <Alert v-else alertType="danger">Invalid State: '{{currentStep}}'</Alert>
    </template>
</div>`
} );
